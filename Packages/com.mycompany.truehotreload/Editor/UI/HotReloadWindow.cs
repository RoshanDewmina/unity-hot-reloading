using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TrueHotReload.Editor.UI
{
    /// <summary>
    /// Editor window for monitoring and controlling hot reload operations.
    /// Accessible via Window > True Hot Reload > Hot Reload Monitor.
    /// </summary>
    public class HotReloadWindow : EditorWindow
    {
        private Vector2 m_ScrollPosition;
        private List<LogEntry> m_RecentLogs = new List<LogEntry>();
        private const int MaxLogEntries = 50;

        private class LogEntry
        {
            public DateTime Timestamp;
            public string Message;
            public LogType Type;
            public int MethodCount;

            public override string ToString()
            {
                var timeStr = Timestamp.ToString("HH:mm:ss");
                var methodInfo = MethodCount > 0 ? $" ({MethodCount} methods)" : "";
                return $"[{timeStr}] {Message}{methodInfo}";
            }
        }

        [MenuItem("Window/True Hot Reload/Hot Reload Monitor")]
        public static void ShowWindow()
        {
            var window = GetWindow<HotReloadWindow>("Hot Reload Monitor");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            // Refresh on enable
            Repaint();
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(10);
            DrawStatusPanel();
            EditorGUILayout.Space(10);
            DrawActionsPanel();
            EditorGUILayout.Space(10);
            DrawStatisticsPanel();
            EditorGUILayout.Space(10);
            DrawActivePatchesPanel();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("True Hot Reload Monitor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Monitor hot reload operations and view active patches. Edit C# scripts during Play Mode to see changes applied instantly.",
                MessageType.Info
            );
        }

        private void DrawStatusPanel()
        {
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                // Play Mode status
                var playModeStatus = HotReloadOrchestrator.IsInPlayMode ? "Playing" : "Stopped";
                var playModeColor = HotReloadOrchestrator.IsInPlayMode ? "green" : "grey";
                EditorGUILayout.LabelField("Play Mode:", $"<color={playModeColor}>{playModeStatus}</color>", CreateRichTextStyle());

                // Hot Reload enabled status
                var hotReloadStatus = HotReloadOrchestrator.IsEnabled ? "Active" : "Inactive";
                var hotReloadColor = HotReloadOrchestrator.IsEnabled ? "green" : "grey";
                EditorGUILayout.LabelField("Hot Reload:", $"<color={hotReloadColor}>{hotReloadStatus}</color>", CreateRichTextStyle());

                // Auto-apply setting
                var settings = HotReloadSettings.Instance;
                var autoApplyStatus = settings.autoApplyOnSave ? "Enabled" : "Manual";
                EditorGUILayout.LabelField("Auto-Apply:", autoApplyStatus);
            }
        }

        private void DrawActionsPanel()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.enabled = HotReloadOrchestrator.IsInPlayMode;
                    
                    if (GUILayout.Button("Clear All Patches", GUILayout.Height(30)))
                    {
                        if (EditorUtility.DisplayDialog(
                            "Clear Patches",
                            "Clear all active patches and restore original methods?",
                            "Clear",
                            "Cancel"))
                        {
                            HotReloadOrchestrator.ClearAllPatches();
                            AddLog("All patches cleared manually", LogType.Log, 0);
                        }
                    }

                    GUI.enabled = true;

                    if (GUILayout.Button("Open Settings", GUILayout.Height(30)))
                    {
                        SettingsService.OpenProjectSettings("Project/True Hot Reload");
                    }
                }

                EditorGUILayout.HelpBox(
                    HotReloadOrchestrator.IsInPlayMode 
                        ? "Edit and save C# scripts to trigger hot reload" 
                        : "Enter Play Mode to enable hot reload",
                    MessageType.None
                );
            }
        }

        private void DrawStatisticsPanel()
        {
            EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var patchCount = HotReloadOrchestrator.SessionPatchCount;
                var activePatchCount = HotReloadOrchestrator.GetActivePatchInfo().Count;

                EditorGUILayout.LabelField("Session Patches:", patchCount.ToString());
                EditorGUILayout.LabelField("Active Patches:", activePatchCount.ToString());

                if (patchCount > 0)
                {
                    var avgTime = "N/A"; // Would need to track this in orchestrator
                    EditorGUILayout.LabelField("Avg. Reload Time:", avgTime);
                }
            }
        }

        private void DrawActivePatchesPanel()
        {
            EditorGUILayout.LabelField("Active Patches", EditorStyles.boldLabel);

            var patches = HotReloadOrchestrator.GetActivePatchInfo();

            if (patches.Count == 0)
            {
                EditorGUILayout.HelpBox("No active patches", MessageType.None);
                return;
            }

            using (var scrollScope = new EditorGUILayout.ScrollViewScope(m_ScrollPosition, EditorStyles.helpBox, GUILayout.Height(200)))
            {
                m_ScrollPosition = scrollScope.scrollPosition;

                foreach (var patch in patches)
                {
                    var methodName = $"{patch.Key.DeclaringType?.Name}.{patch.Key.Name}";
                    EditorGUILayout.LabelField($"• {methodName}", EditorStyles.miniLabel);
                }
            }
        }

        private GUIStyle CreateRichTextStyle()
        {
            var style = new GUIStyle(EditorStyles.label);
            style.richText = true;
            return style;
        }

        private void AddLog(string message, LogType type, int methodCount)
        {
            m_RecentLogs.Add(new LogEntry
            {
                Timestamp = DateTime.Now,
                Message = message,
                Type = type,
                MethodCount = methodCount
            });

            // Keep only recent logs
            if (m_RecentLogs.Count > MaxLogEntries)
            {
                m_RecentLogs.RemoveAt(0);
            }

            Repaint();
        }

        // Auto-refresh the window periodically
        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
