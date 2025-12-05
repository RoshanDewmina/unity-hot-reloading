using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TrueHotReload.Editor
{
    /// <summary>
    /// Integrates TrueHotReload with Unity Editor lifecycle and events.
    /// </summary>
    [InitializeOnLoad]
    public static class UnityEditorIntegration
    {
        static UnityEditorIntegration()
        {
            // Subscribe to editor update for driving async operations
            EditorApplication.update += OnEditorUpdate;
            
            // Check Unity settings on startup
            EditorApplication.delayCall += CheckUnitySettings;
            
            HotReloadLogger.LogVerbose("Unity Editor integration initialized");
        }

        private static void OnEditorUpdate()
        {
            // The orchestrator will hook into this to drive the ScriptChangeDetector
            HotReloadOrchestrator.OnEditorUpdate();
        }

        private static void CheckUnitySettings()
        {
            var settings = HotReloadSettings.Instance;
            if (!settings.checkUnitySettings)
                return;

            bool hasWarnings = false;

            // Check Enter Play Mode settings
            if (!EditorSettings.enterPlayModeOptionsEnabled)
            {
                hasWarnings = true;
                HotReloadLogger.LogWarning(
                    "Enter Play Mode Options are disabled. For best hot reload experience, " +
                    "go to Project Settings > True Hot Reload and enable them."
                );
            }
            else
            {
                var options = EditorSettings.enterPlayModeOptions;
                bool domainReloadDisabled = (options & EnterPlayModeOptions.DisableDomainReload) != 0;
                bool sceneReloadDisabled = (options & EnterPlayModeOptions.DisableSceneReload) != 0;

                if (!domainReloadDisabled || !sceneReloadDisabled)
                {
                    hasWarnings = true;
                    HotReloadLogger.LogWarning(
                        "Domain Reload or Scene Reload is enabled. This may interfere with hot reload. " +
                        "Go to Project Settings > True Hot Reload to fix."
                    );
                }
            }

            if (!hasWarnings)
            {
                HotReloadLogger.Log("Unity settings are optimized for hot reload ✓");
            }
        }

        /// <summary>
        /// Open the TrueHotReload settings in Project Settings.
        /// </summary>
        [MenuItem("Window/True Hot Reload/Open Settings")]
        public static void OpenSettings()
        {
            SettingsService.OpenProjectSettings("Project/True Hot Reload");
        }
    }
}
