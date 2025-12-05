#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TrueHotReload.Runtime
{
    /// <summary>
    /// Runtime bootstrap for TrueHotReload system.
    /// Initializes when Unity Editor loads and manages hot reload lifecycle.
    /// </summary>
    [InitializeOnLoad]
    public static class HotReloadRuntimeBootstrap
    {
        static HotReloadRuntimeBootstrap()
        {
            // Subscribe to Play Mode state changes
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            
            Debug.Log("[TrueHotReload] Runtime bootstrap initialized");
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
            Debug.Log("[TrueHotReload] Entered Play Mode - hot reload ready");
            // Editor orchestrator will handle the actual initialization
        }

        private static void OnExitingPlayMode()
        {
            Debug.Log("[TrueHotReload] Exiting Play Mode - cleaning up patches");
            // Editor orchestrator will handle cleanup
        }
    }
}
#endif
