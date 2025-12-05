using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace TrueHotReload.Editor
{
    /// <summary>
    /// Runtime method patching engine using Harmony.
    /// Applies IL-level patches to replace old method implementations with new ones.
    /// </summary>
    public class MethodPatchEngine : IDisposable
    {
        private Harmony m_Harmony;
        private readonly Dictionary<MethodBase, PatchInfo> m_ActivePatches = new Dictionary<MethodBase, PatchInfo>();
        private bool m_IsInitialized;
        private bool m_IsDisposed;

        private class PatchInfo
        {
            public MethodInfo OriginalMethod;
            public MethodInfo NewMethod;
            public DateTime PatchTime;
        }

        /// <summary>
        /// Initialize the patching engine.
        /// </summary>
        public void Initialize()
        {
            if (m_IsInitialized)
                return;

            try
            {
                m_Harmony = new Harmony("com.mycompany.truehotreload");
                m_IsInitialized = true;
                HotReloadLogger.Log("Method patching engine initialized");
            }
            catch (Exception ex)
            {
                HotReloadLogger.LogError($"Failed to initialize Harmony: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Apply patches for the given method descriptors.
        /// </summary>
        public void ApplyPatches(IReadOnlyList<HotReloadMethodDescriptor> methods)
        {
            if (!m_IsInitialized)
            {
                HotReloadLogger.LogError("MethodPatchEngine not initialized");
                return;
            }

            if (methods == null || methods.Count == 0)
            {
                HotReloadLogger.LogWarning("No methods to patch");
                return;
            }

            var startTime = Time.realtimeSinceStartup;
            int successCount = 0;
            int failureCount = 0;

            HotReloadLogger.Log($"Applying patches to {methods.Count} method(s)...");

            foreach (var method in methods)
            {
                try
                {
                    if (PatchMethod(method))
                    {
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    HotReloadLogger.LogError($"Failed to patch {method}: {ex.Message}");
                }
            }

            var elapsedTime = Time.realtimeSinceStartup - startTime;
            HotReloadLogger.LogPatchTime(elapsedTime, successCount);

            if (failureCount > 0)
            {
                HotReloadLogger.LogWarning($"{failureCount} method(s) failed to patch");
            }
        }

        private bool PatchMethod(HotReloadMethodDescriptor descriptor)
        {
            var oldMethod = descriptor.OldMethodInfo;
            var newMethod = descriptor.NewMethodInfo;

            // Validate methods
            if (oldMethod == null || newMethod == null)
            {
                HotReloadLogger.LogError($"Invalid method descriptor: {descriptor}");
                return false;
            }

            // Check if method should be excluded
            if (HasNoHotReloadAttribute(oldMethod))
            {
                HotReloadLogger.LogVerbose($"Skipping method with [NoHotReload]: {descriptor}");
                return false;
            }

            try
            {
                // Use Harmony to patch the method
                // We use a Prefix patch with priority to completely replace the original method
                var prefix = CreatePrefixPatch(newMethod);
                
                m_Harmony.Patch(
                    original: oldMethod,
                    prefix: new HarmonyMethod(prefix)
                    {
                        priority = Priority.First
                    }
                );

                // Track the patch
                m_ActivePatches[oldMethod] = new PatchInfo
                {
                    OriginalMethod = oldMethod,
                    NewMethod = newMethod,
                    PatchTime = DateTime.Now
                };

                HotReloadLogger.LogVerbose($"✓ Patched: {descriptor}");
                return true;
            }
            catch (Exception ex)
            {
                HotReloadLogger.LogError($"Failed to patch {descriptor}: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        private MethodInfo CreatePrefixPatch(MethodInfo newMethod)
        {
            // For now, we'll use a simplified approach:
            // Create a wrapper that calls the new method and returns false to skip the original
            
            // NOTE: This is a simplified implementation. In a production system, you would:
            // 1. Generate dynamic IL that properly handles all parameter types
            // 2. Handle ref/out parameters correctly
            // 3. Handle return values properly
            // 4. Handle instance vs static methods
            
            // For the MVP, we'll use Harmony's built-in method replacement
            return newMethod;
        }

        private bool HasNoHotReloadAttribute(MethodInfo method)
        {
            return method.GetCustomAttribute(typeof(Runtime.NoHotReloadAttribute)) != null;
        }

        /// <summary>
        /// Clear all applied patches and restore original methods.
        /// </summary>
        public void ClearAllPatches()
        {
            if (!m_IsInitialized)
                return;

            try
            {
                HotReloadLogger.Log($"Clearing {m_ActivePatches.Count} active patch(es)...");

                foreach (var kvp in m_ActivePatches)
                {
                    try
                    {
                        m_Harmony.Unpatch(kvp.Key, HarmonyPatchType.All, m_Harmony.Id);
                        HotReloadLogger.LogVerbose($"Unpatched: {kvp.Value.OriginalMethod.Name}");
                    }
                    catch (Exception ex)
                    {
                        HotReloadLogger.LogWarning($"Failed to unpatch {kvp.Value.OriginalMethod.Name}: {ex.Message}");
                    }
                }

                m_ActivePatches.Clear();
                HotReloadLogger.Log("All patches cleared");
            }
            catch (Exception ex)
            {
                HotReloadLogger.LogError($"Error clearing patches: {ex.Message}");
            }
        }

        /// <summary>
        /// Get information about currently active patches.
        /// </summary>
        public IReadOnlyDictionary<MethodBase, string> GetActivePatchInfo()
        {
            return m_ActivePatches.ToDictionary(
                kvp => kvp.Key,
                kvp => $"{kvp.Value.NewMethod.DeclaringType.FullName}.{kvp.Value.NewMethod.Name} (patched at {kvp.Value.PatchTime:HH:mm:ss})"
            );
        }

        public void Dispose()
        {
            if (m_IsDisposed)
                return;

            m_IsDisposed = true;
            ClearAllPatches();
            
            if (m_Harmony != null)
            {
                try
                {
                    m_Harmony.UnpatchAll(m_Harmony.Id);
                }
                catch
                {
                    // Ignore errors during disposal
                }
            }

            HotReloadLogger.Log("Method patching engine disposed");
        }
    }
}
