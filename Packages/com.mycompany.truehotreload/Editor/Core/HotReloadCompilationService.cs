using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace TrueHotReload.Editor
{
    /// <summary>
    /// Result of a hot reload compilation.
    /// </summary>
    public class HotReloadCompilationResult
    {
        public bool Success { get; set; }
        public Assembly CompiledAssembly { get; set; }
        public IReadOnlyList<Diagnostic> Diagnostics { get; set; }
        public IReadOnlyList<HotReloadMethodDescriptor> ChangedMethods { get; set; }

        public HotReloadCompilationResult()
        {
            Diagnostics = Array.Empty<Diagnostic>();
            ChangedMethods = Array.Empty<HotReloadMethodDescriptor>();
        }
    }

    /// <summary>
    /// Descriptor for a method that has changed and needs patching.
    /// </summary>
    public class HotReloadMethodDescriptor
    {
        public string AssemblyName { get; set; }
        public string FullTypeName { get; set; }
        public string MethodName { get; set; }
        public string MethodSignature { get; set; }
        public MethodInfo OldMethodInfo { get; set; }
        public MethodInfo NewMethodInfo { get; set; }

        public override string ToString()
        {
            return $"{FullTypeName}.{MethodName}";
        }
    }

    /// <summary>
    /// Interface for hot reload compilation service.
    /// </summary>
    public interface IHotReloadCompilationService
    {
        HotReloadCompilationResult CompileChangedScripts(IReadOnlyList<string> changedScriptPaths);
    }

    /// <summary>
    /// Roslyn-based compilation service for hot reload.
    /// Compiles changed scripts into in-memory assemblies and identifies changed methods.
    /// </summary>
    public class HotReloadCompilationService : IHotReloadCompilationService
    {
        private readonly Dictionary<string, Assembly> m_AssemblyCache = new Dictionary<string, Assembly>();
        private int m_CompilationCounter = 0;

        public HotReloadCompilationResult CompileChangedScripts(IReadOnlyList<string> changedScriptPaths)
        {
            var startTime = Time.realtimeSinceStartup;
            var result = new HotReloadCompilationResult();

            try
            {
                HotReloadLogger.LogVerbose($"Starting compilation of {changedScriptPaths.Count} script(s)");

                // Parse syntax trees from changed files
                var syntaxTrees = ParseSyntaxTrees(changedScriptPaths);
                if (syntaxTrees.Count == 0)
                {
                    result.Success = false;
                    HotReloadLogger.LogWarning("No valid syntax trees parsed from changed scripts");
                    return result;
                }

                // Get references to all Unity assemblies
                var references = GetMetadataReferences();

                // Create compilation
                var assemblyName = $"TrueHotReload.Dynamic.{m_CompilationCounter++}";
                var compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees: syntaxTrees,
                    references: references,
                    options: new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        optimizationLevel: OptimizationLevel.Debug,
                        allowUnsafe: true
                    )
                );

                // Emit to memory stream
                using (var ms = new MemoryStream())
                {
                    var emitResult = compilation.Emit(ms);
                    result.Diagnostics = emitResult.Diagnostics.ToList();

                    if (!emitResult.Success)
                    {
                        result.Success = false;
                        LogCompilationErrors(result.Diagnostics);
                        return result;
                    }

                    // Load assembly
                    ms.Seek(0, SeekOrigin.Begin);
                    var assemblyBytes = ms.ToArray();
                    var newAssembly = Assembly.Load(assemblyBytes);

                    result.CompiledAssembly = newAssembly;
                    result.Success = true;

                    // Find changed methods
                    var changedMethods = FindChangedMethods(changedScriptPaths, newAssembly);
                    result.ChangedMethods = changedMethods;

                    var elapsedTime = Time.realtimeSinceStartup - startTime;
                    HotReloadLogger.LogCompilationTime(elapsedTime, changedScriptPaths.Count);
                    HotReloadLogger.Log($"Found {changedMethods.Count} changed method(s)");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                HotReloadLogger.LogError($"Compilation exception: {ex.Message}\n{ex.StackTrace}");
            }

            return result;
        }

        private List<SyntaxTree> ParseSyntaxTrees(IReadOnlyList<string> scriptPaths)
        {
            var syntaxTrees = new List<SyntaxTree>();

            foreach (var relativePath in scriptPaths)
            {
                try
                {
                    var fullPath = Path.Combine(Application.dataPath, "..", relativePath);
                    
                    if (!File.Exists(fullPath))
                    {
                        HotReloadLogger.LogWarning($"Script file not found: {fullPath}");
                        continue;
                    }

                    var sourceCode = File.ReadAllText(fullPath);
                    var syntaxTree = CSharpSyntaxTree.ParseText(
                        sourceCode,
                        path: fullPath,
                        encoding: System.Text.Encoding.UTF8
                    );

                    syntaxTrees.Add(syntaxTree);
                    HotReloadLogger.LogVerbose($"Parsed: {relativePath}");
                }
                catch (Exception ex)
                {
                    HotReloadLogger.LogError($"Failed to parse {relativePath}: {ex.Message}");
                }
            }

            return syntaxTrees;
        }

        private List<MetadataReference> GetMetadataReferences()
        {
            var references = new List<MetadataReference>();

            // Add all currently loaded assemblies
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in loadedAssemblies)
            {
                try
                {
                    // Skip dynamic assemblies
                    if (assembly.IsDynamic)
                        continue;

                    // Skip assemblies without location
                    if (string.IsNullOrEmpty(assembly.Location))
                        continue;

                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
                catch
                {
                    // Ignore assemblies we can't reference
                }
            }

            HotReloadLogger.LogVerbose($"Added {references.Count} assembly references for compilation");
            return references;
        }

        private List<HotReloadMethodDescriptor> FindChangedMethods(
            IReadOnlyList<string> changedScriptPaths,
            Assembly newAssembly)
        {
            var changedMethods = new List<HotReloadMethodDescriptor>();

            // Get all types from the new assembly
            var newTypes = newAssembly.GetTypes();

            foreach (var newType in newTypes)
            {
                try
                {
                    // Find the corresponding old type in the current domain
                    var oldType = FindOldType(newType.FullName);
                    if (oldType == null)
                    {
                        HotReloadLogger.LogVerbose($"New type detected (not yet supported): {newType.FullName}");
                        continue;
                    }

                    // Compare methods
                    var newMethods = newType.GetMethods(
                        BindingFlags.Public | BindingFlags.NonPublic | 
                        BindingFlags.Instance | BindingFlags.Static | 
                        BindingFlags.DeclaredOnly
                    );

                    foreach (var newMethod in newMethods)
                    {
                        // Skip compiler-generated methods (except we want to patch them too for some cases)
                        if (newMethod.Name.Contains("<") && !newMethod.Name.Contains("<>"))
                        {
                            // This is likely a compiler-generated method, but not a backing field
                            continue;
                        }

                        // Find corresponding old method
                        var oldMethod = FindMatchingMethod(oldType, newMethod);
                        
                        if (oldMethod == null)
                        {
                            HotReloadLogger.LogVerbose($"New method detected: {newType.FullName}.{newMethod.Name}");
                            // For now, we don't support adding entirely new methods at runtime
                            continue;
                        }

                        // Check if method implementation has changed
                        // For simplicity, we'll assume all methods in changed files have changed
                        // A more sophisticated approach would compare IL or method body hashes
                        var descriptor = new HotReloadMethodDescriptor
                        {
                            AssemblyName = oldType.Assembly.GetName().Name,
                            FullTypeName = oldType.FullName,
                            MethodName = newMethod.Name,
                            MethodSignature = GetMethodSignature(newMethod),
                            OldMethodInfo = oldMethod,
                            NewMethodInfo = newMethod
                        };

                        changedMethods.Add(descriptor);
                        HotReloadLogger.LogVerbose($"Method changed: {descriptor}");
                    }
                }
                catch (Exception ex)
                {
                    HotReloadLogger.LogError($"Error processing type {newType.FullName}: {ex.Message}");
                }
            }

            return changedMethods;
        }

        private Type FindOldType(string fullTypeName)
        {
            // Search all loaded assemblies for the type
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    // Skip our dynamic assemblies
                    if (assembly.FullName.Contains("TrueHotReload.Dynamic"))
                        continue;

                    var type = assembly.GetType(fullTypeName);
                    if (type != null)
                        return type;
                }
                catch
                {
                    // Ignore errors accessing assembly types
                }
            }

            return null;
        }

        private MethodInfo FindMatchingMethod(Type oldType, MethodInfo newMethod)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | 
                             BindingFlags.Instance | BindingFlags.Static | 
                             BindingFlags.DeclaredOnly;

            var oldMethods = oldType.GetMethods(bindingFlags);

            foreach (var oldMethod in oldMethods)
            {
                if (oldMethod.Name != newMethod.Name)
                    continue;

                // Compare parameter types
                var oldParams = oldMethod.GetParameters();
                var newParams = newMethod.GetParameters();

                if (oldParams.Length != newParams.Length)
                    continue;

                bool parametersMatch = true;
                for (int i = 0; i < oldParams.Length; i++)
                {
                    if (oldParams[i].ParameterType.FullName != newParams[i].ParameterType.FullName)
                    {
                        parametersMatch = false;
                        break;
                    }
                }

                if (parametersMatch)
                    return oldMethod;
            }

            return null;
        }

        private string GetMethodSignature(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var paramStrings = parameters.Select(p => $"{p.ParameterType.Name} {p.Name}");
            return $"{method.ReturnType.Name} {method.Name}({string.Join(", ", paramStrings)})";
        }

        private void LogCompilationErrors(IReadOnlyList<Diagnostic> diagnostics)
        {
            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            var warnings = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ToList();

            HotReloadLogger.LogError($"Compilation failed with {errors.Count} error(s) and {warnings.Count} warning(s)");

            foreach (var error in errors)
            {
                var location = error.Location.GetLineSpan();
                HotReloadLogger.LogError(
                    $"{location.Path}({location.StartLinePosition.Line + 1},{location.StartLinePosition.Character + 1}): " +
                    $"{error.Id} {error.GetMessage()}"
                );
            }

            foreach (var warning in warnings.Take(5)) // Limit warnings to avoid spam
            {
                var location = warning.Location.GetLineSpan();
                HotReloadLogger.LogWarning(
                    $"{location.Path}({location.StartLinePosition.Line + 1},{location.StartLinePosition.Character + 1}): " +
                    $"{warning.Id} {warning.GetMessage()}"
                );
            }

            if (warnings.Count > 5)
            {
                HotReloadLogger.LogWarning($"... and {warnings.Count - 5} more warning(s)");
            }
        }
    }
}
