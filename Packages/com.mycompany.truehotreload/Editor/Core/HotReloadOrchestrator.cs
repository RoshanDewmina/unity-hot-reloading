using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TrueHotReload.Editor
{
    /// <summary>
    /// Main orchestrator for TrueHotReload system.
    /// Coordinates all subsystems and manages the hot reload pipeline.
    /// </summary>
    [InitializeOnLoad]
    public static class HotReloadOrchestrator
    {
        private static ScriptChangeDetector s_Detector;
        private static IHotReloadCompilationService s_Compiler;
        private static MethodPatchEngine s_Patcher;
        private static bool s_IsEnabled;
        private static bool s_IsInPlayMode;
        private static int s_SessionPatchCount;

        static HotReloadOrchestrator()
        {
            // Subscribe to Play Mode state changes
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            
            // Initialize subsystems
            InitializeSubsystems();
            
            // Register cleanup on domain reload (if it happens)
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            
            HotReloadLogger.Log("TrueHotReload orchestrator initialized");
        }

        private static void InitializeSubsystems()
        {
            try
            {
                s_Detector = new ScriptChangeDetector();
                s_Compiler = new HotReloadCompilationService();
                s_Patcher = new MethodPatchEngine();
                s_Patcher.Initialize();

                // Subscribe to script change events
                s_Detector.ScriptsChanged += OnScriptsChanged;

                HotReloadLogger.LogVerbose("All subsystems initialized");
            }
            catch (Exception ex)
            {
                HotReloadLogger.LogError($"Failed to initialize subsystems: {ex.Message}");
            }
        }

        /// <summary>
        /// Called from Unity Editor integration on EditorApplication.update.
        /// </summary>
        public static void OnEditorUpdate()
        {
            s_Detector?.Update();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    OnEnteredPlayMode();
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    OnExitingPlayMode();
                    break;
            }
        }

        private static void OnEnteredPlayMode()
        {
            s_IsInPlayMode = true;
            s_SessionPatchCount = 0;

            var settings = HotReloadSettings.Instance;
            if (!settings.autoApplyOnSave)
            {
                HotReloadLogger.Log("Hot reload is in manual mode. Use the Hot Reload Window to apply changes.");
                return;
            }

            // Start watching for script changes
            var projectPath = Application.dataPath.Replace("/Assets", "");
            s_Detector?.StartWatching(projectPath);
            s_IsEnabled = true;

            HotReloadLogger.Log("<color=#00FF00><b>Hot reload enabled! Edit scripts and save to apply changes.</b></color>");
        }

        private static void OnExitingPlayMode()
        {
            s_IsInPlayMode = false;
            s_IsEnabled = false;

            // Stop watching
            s_Detector?.StopWatching();

            // Clear all patches
            s_Patcher?.ClearAllPatches();

            if (s_SessionPatchCount > 0)
            {
                HotReloadLogger.Log($"Play Mode session ended. Total patches applied: {s_SessionPatchCount}");
            }
        }

        private static void OnBeforeAssemblyReload()
        {
            // Clean up before domain reload
            HotReloadLogger.LogVerbose("Assembly reload detected, cleaning up...");
            
            s_Detector?.StopWatching();
            s_Patcher?.Dispose();
            HotReloadLogger.Shutdown();
        }

        private static void OnScriptsChanged(IReadOnlyList<string> changedScriptPaths)
        {
            if (!s_IsEnabled || !s_IsInPlayMode)
            {
                HotReloadLogger.LogWarning("Scripts changed but hot reload is not enabled");
                return;
            }

            var settings = HotReloadSettings.Instance;
            if (!settings.autoApplyOnSave)
            {
                HotReloadLogger.Log("Scripts changed. Use 'Apply Changes' button to hot reload.");
                return;
            }

            // Perform hot reload
            PerformHotReload(changedScriptPaths);
        }

        /// <summary>
        /// Manually trigger hot reload for all changed scripts.
        /// Can be called from UI.
        /// </summary>
        public static void ManuallyApplyChanges(IReadOnlyList<string> scriptPaths)
        {
            if (!s_IsInPlayMode)
            {
                HotReloadLogger.LogWarning("Hot reload can only be applied during Play Mode");
                return;
            }

            PerformHotReload(scriptPaths);
        }

        private static void PerformHotReload(IReadOnlyList<string> changedScriptPaths)
        {
            var totalStartTime = Time.realtimeSinceStartup;

            try
            {
                HotReloadLogger.Log($"<color=#FFFF00>Starting hot reload for {changedScriptPaths.Count} file(s)...</color>");

                // Step 1: Compile changed scripts
                var compilationResult = s_Compiler.CompileChangedScripts(changedScriptPaths);

                if (!compilationResult.Success)
                {
                    HotReloadLogger.LogFailure("Compilation failed");
                    return;
                }

                if (compilationResult.ChangedMethods.Count == 0)
                {
                    HotReloadLogger.LogWarning("No changed methods detected. Scripts may contain only whitespace changes.");
                    return;
                }

                // Step 2: Apply patches
                s_Patcher.ApplyPatches(compilationResult.ChangedMethods);

                // Step 3: Report success
                var totalTime = Time.realtimeSinceStartup - totalStartTime;
                s_SessionPatchCount += compilationResult.ChangedMethods.Count;
                
                HotReloadLogger.LogSuccess(compilationResult.ChangedMethods.Count, totalTime);
                HotReloadLogger.Log($"Session total: {s_SessionPatchCount} method(s) patched");
            }
            catch (Exception ex)
            {
                HotReloadLogger.LogFailure($"Exception: {ex.Message}");
                HotReloadLogger.LogError(ex.StackTrace);
            }
        }

        /// <summary>
        /// Get current hot reload status.
        /// </summary>
        public static bool IsEnabled => s_IsEnabled;

        /// <summary>
        /// Get current Play Mode status.
        /// </summary>
        public static bool IsInPlayMode => s_IsInPlayMode;

        /// <summary>
        /// Get session patch count.
        /// </summary>
        public static int SessionPatchCount => s_SessionPatchCount;

        /// <summary>
        /// Get active patch information.
        /// </summary>
        public static IReadOnlyDictionary<System.Reflection.MethodBase, string> GetActivePatchInfo()
        {
            return s_Patcher?.GetActivePatchInfo() ?? new Dictionary<System.Reflection.MethodBase, string>();
        }

        /// <summary>
        /// Manually clear all patches (for debugging).
        /// </summary>
        [MenuItem("Window/True Hot Reload/Clear All Patches")]
        public static void ClearAllPatches()
        {
            s_Patcher?.ClearAllPatches();
            s_SessionPatchCount = 0;
            HotReloadLogger.Log("All patches cleared manually");
        }
    }
}
