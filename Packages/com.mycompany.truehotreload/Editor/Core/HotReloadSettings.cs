using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TrueHotReload.Editor
{
    /// <summary>
    /// Settings for TrueHotReload system.
    /// Stored as a ScriptableObject in ProjectSettings.
    /// </summary>
    public class HotReloadSettings : ScriptableObject
    {
        private const string SettingsPath = "ProjectSettings/TrueHotReloadSettings.asset";
        private static HotReloadSettings s_Instance;

        [Header("Path Filtering")]
        [Tooltip("Paths to include for hot reload monitoring (relative to project root)")]
        public List<string> includedPaths = new List<string> { "Assets/" };

        [Tooltip("Paths to exclude from hot reload monitoring")]
        public List<string> excludedPaths = new List<string> 
        { 
            "Assets/Plugins/",
            "Assets/Editor/",
            "Assets/TextMesh Pro/"
        };

        [Header("Assembly Filtering")]
        [Tooltip("Only hot reload scripts in these assemblies (empty = all assemblies)")]
        public List<string> includedAssemblies = new List<string> { "Assembly-CSharp" };

        [Header("Behavior")]
        [Tooltip("Automatically apply hot reload when scripts are saved")]
        public bool autoApplyOnSave = true;

        [Tooltip("Debounce interval in seconds before applying changes (avoids rapid recompiles)")]
        [Range(0.1f, 5f)]
        public float debounceSeconds = 0.5f;

        [Tooltip("Check Unity Editor settings and warn if suboptimal for hot reload")]
        public bool checkUnitySettings = true;

        [Header("Diagnostics")]
        [Tooltip("Enable verbose logging for debugging hot reload issues")]
        public bool verboseLogging = false;

        [Tooltip("Write logs to file in addition to Console")]
        public bool logToFile = false;

        [Tooltip("Path to log file (relative to project root)")]
        public string logFilePath = "Logs/TrueHotReload.log";

        /// <summary>
        /// Singleton accessor for settings.
        /// Creates the settings asset if it doesn't exist.
        /// </summary>
        public static HotReloadSettings Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = LoadOrCreateSettings();
                }
                return s_Instance;
            }
        }

        private static HotReloadSettings LoadOrCreateSettings()
        {
            // Try to load existing settings
            if (File.Exists(SettingsPath))
            {
                var loaded = InternalEditorUtility.LoadSerializedFileAndForget(SettingsPath);
                if (loaded != null && loaded.Length > 0)
                {
                    return loaded[0] as HotReloadSettings;
                }
            }

            // Create new settings
            var settings = CreateInstance<HotReloadSettings>();
            settings.hideFlags = HideFlags.HideAndDontSave;

            // Ensure directory exists
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Save to disk
            InternalEditorUtility.SaveToSerializedFileAndForget(
                new Object[] { settings },
                SettingsPath,
                true
            );

            return settings;
        }

        /// <summary>
        /// Save settings to disk.
        /// </summary>
        public void Save()
        {
            if (this == null) return;

            var directory = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            InternalEditorUtility.SaveToSerializedFileAndForget(
                new Object[] { this },
                SettingsPath,
                true
            );
        }

        /// <summary>
        /// Reset settings to defaults.
        /// </summary>
        public void ResetToDefaults()
        {
            includedPaths = new List<string> { "Assets/" };
            excludedPaths = new List<string> 
            { 
                "Assets/Plugins/",
                "Assets/Editor/",
                "Assets/TextMesh Pro/"
            };
            includedAssemblies = new List<string> { "Assembly-CSharp" };
            autoApplyOnSave = true;
            debounceSeconds = 0.5f;
            checkUnitySettings = true;
            verboseLogging = false;
            logToFile = false;
            logFilePath = "Logs/TrueHotReload.log";
            
            Save();
        }

        /// <summary>
        /// Check if a file path should be monitored for hot reload.
        /// </summary>
        public bool ShouldMonitorPath(string path)
        {
            // Normalize path separators
            path = path.Replace('\\', '/');

            // Check if included
            bool included = false;
            foreach (var includePath in includedPaths)
            {
                if (path.StartsWith(includePath.Replace('\\', '/')))
                {
                    included = true;
                    break;
                }
            }

            if (!included) return false;

            // Check if excluded
            foreach (var excludePath in excludedPaths)
            {
                if (path.StartsWith(excludePath.Replace('\\', '/')))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
