using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TrueHotReload.Editor
{
    /// <summary>
    /// Settings provider for TrueHotReload in Project Settings window.
    /// Accessible via Edit > Project Settings > True Hot Reload.
    /// </summary>
    public class HotReloadSettingsProvider : SettingsProvider
    {
        private SerializedObject m_SerializedSettings;
        private HotReloadSettings m_Settings;

        private const string SettingsPath = "Project/True Hot Reload";

        public HotReloadSettingsProvider(string path, SettingsScope scopes)
            : base(path, scopes)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            m_Settings = HotReloadSettings.Instance;
            m_SerializedSettings = new SerializedObject(m_Settings);
        }

        public override void OnDeactivate()
        {
            if (m_Settings != null)
            {
                m_Settings.Save();
            }
        }

        public override void OnGUI(string searchContext)
        {
            if (m_SerializedSettings == null || m_SerializedSettings.targetObject == null)
            {
                m_Settings = HotReloadSettings.Instance;
                m_SerializedSettings = new SerializedObject(m_Settings);
            }

            m_SerializedSettings.Update();

            EditorGUILayout.Space(10);
            
            // Header
            EditorGUILayout.LabelField("True Hot Reload Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure hot reload behavior for editing C# scripts during Play Mode without domain reload.",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            // Unity Settings Health Check
            if (m_Settings.checkUnitySettings)
            {
                DrawUnitySettingsHealthCheck();
                EditorGUILayout.Space(10);
            }

            // Path Filtering
            EditorGUILayout.LabelField("Path Filtering", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("includedPaths"));
            EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("excludedPaths"));
            EditorGUILayout.Space(5);

            // Assembly Filtering
            EditorGUILayout.LabelField("Assembly Filtering", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("includedAssemblies"));
            EditorGUILayout.HelpBox(
                "Leave empty to monitor all assemblies. Add assembly names (e.g., 'Assembly-CSharp') to filter.",
                MessageType.None
            );
            EditorGUILayout.Space(5);

            // Behavior
            EditorGUILayout.LabelField("Behavior", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("autoApplyOnSave"));
            EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("debounceSeconds"));
            EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("checkUnitySettings"));
            EditorGUILayout.Space(5);

            // Diagnostics
            EditorGUILayout.LabelField("Diagnostics", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("verboseLogging"));
            EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("logToFile"));
            
            if (m_Settings.logToFile)
            {
                EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("logFilePath"));
                
                if (GUILayout.Button("Open Log File", GUILayout.Width(150)))
                {
                    OpenLogFile();
                }
            }

            EditorGUILayout.Space(10);

            // Actions
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Reset to Defaults", GUILayout.Width(150)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Reset Settings",
                        "Reset all TrueHotReload settings to defaults?",
                        "Reset",
                        "Cancel"))
                    {
                        m_Settings.ResetToDefaults();
                        m_SerializedSettings.Update();
                    }
                }

                GUILayout.FlexibleSpace();
            }

            if (m_SerializedSettings.ApplyModifiedProperties())
            {
                m_Settings.Save();
            }
        }

        private void DrawUnitySettingsHealthCheck()
        {
            EditorGUILayout.LabelField("Unity Settings Health Check", EditorStyles.boldLabel);

            bool hasIssues = false;

            // Check Enter Play Mode settings
            var enterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            var enterPlayModeOptions = EditorSettings.enterPlayModeOptions;

            if (!enterPlayModeOptionsEnabled)
            {
                hasIssues = true;
                EditorGUILayout.HelpBox(
                    "⚠️ Enter Play Mode Options are disabled. For best hot reload experience, enable them.",
                    MessageType.Warning
                );
                
                if (GUILayout.Button("Enable Enter Play Mode Options", GUILayout.Width(250)))
                {
                    EditorSettings.enterPlayModeOptionsEnabled = true;
                    EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
                }
            }
            else
            {
                bool domainReloadDisabled = (enterPlayModeOptions & EnterPlayModeOptions.DisableDomainReload) != 0;
                bool sceneReloadDisabled = (enterPlayModeOptions & EnterPlayModeOptions.DisableSceneReload) != 0;

                if (!domainReloadDisabled || !sceneReloadDisabled)
                {
                    hasIssues = true;
                    EditorGUILayout.HelpBox(
                        "⚠️ Domain Reload and Scene Reload should be disabled for hot reload to preserve game state.",
                        MessageType.Warning
                    );

                    if (GUILayout.Button("Disable Domain & Scene Reload", GUILayout.Width(250)))
                    {
                        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
                    }
                }
            }

            if (!hasIssues)
            {
                EditorGUILayout.HelpBox("✓ Unity settings are optimized for hot reload.", MessageType.Info);
            }
        }

        private void OpenLogFile()
        {
            var logPath = System.IO.Path.Combine(Application.dataPath, "..", m_Settings.logFilePath);
            
            if (System.IO.File.Exists(logPath))
            {
                EditorUtility.RevealInFinder(logPath);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Log File Not Found",
                    $"Log file does not exist yet: {logPath}",
                    "OK"
                );
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new HotReloadSettingsProvider(SettingsPath, SettingsScope.Project)
            {
                keywords = new[] 
                { 
                    "hot", "reload", "live", "coding", "runtime", "patch",
                    "true hot reload", "hot reloading"
                }
            };

            return provider;
        }
    }
}
