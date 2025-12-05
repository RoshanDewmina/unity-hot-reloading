using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TrueHotReload.Editor
{
    /// <summary>
    /// Monitors file system for changes to C# scripts.
    /// Uses FileSystemWatcher with debouncing and fallback polling.
    /// </summary>
    public class ScriptChangeDetector : IDisposable
    {
        /// <summary>
        /// Event fired when scripts have changed after debounce period.
        /// Provides list of Unity-relative paths (e.g., "Assets/Scripts/Player.cs").
        /// </summary>
        public event Action<IReadOnlyList<string>> ScriptsChanged;

        private FileSystemWatcher m_Watcher;
        private readonly HashSet<string> m_PendingChanges = new HashSet<string>();
        private double m_LastChangeTime;
        private bool m_IsWatching;
        private bool m_IsDisposed;
        private string m_ProjectPath;

        /// <summary>
        /// Start watching for script changes in the specified root path.
        /// </summary>
        public void StartWatching(string rootPath)
        {
            if (m_IsWatching)
            {
                HotReloadLogger.LogWarning("ScriptChangeDetector is already watching");
                return;
            }

            m_ProjectPath = rootPath;

            try
            {
                InitializeFileSystemWatcher(rootPath);
                m_IsWatching = true;
                HotReloadLogger.LogVerbose($"Started watching for script changes in: {rootPath}");
            }
            catch (Exception ex)
            {
                HotReloadLogger.LogError($"Failed to start FileSystemWatcher: {ex.Message}. Falling back to polling.");
                m_IsWatching = true; // Still set to true to enable polling fallback
            }
        }

        /// <summary>
        /// Stop watching for script changes.
        /// </summary>
        public void StopWatching()
        {
            m_IsWatching = false;
            m_PendingChanges.Clear();

            if (m_Watcher != null)
            {
                m_Watcher.EnableRaisingEvents = false;
                m_Watcher.Dispose();
                m_Watcher = null;
            }

            HotReloadLogger.LogVerbose("Stopped watching for script changes");
        }

        /// <summary>
        /// Update method to be called from EditorApplication.update.
        /// Handles debouncing and fires ScriptsChanged event.
        /// </summary>
        public void Update()
        {
            if (!m_IsWatching || m_PendingChanges.Count == 0)
                return;

            var settings = HotReloadSettings.Instance;
            var timeSinceLastChange = EditorApplication.timeSinceStartup - m_LastChangeTime;

            // Check if debounce period has elapsed
            if (timeSinceLastChange >= settings.debounceSeconds)
            {
                FireScriptsChangedEvent();
            }
        }

        private void InitializeFileSystemWatcher(string rootPath)
        {
            m_Watcher = new FileSystemWatcher
            {
                Path = rootPath,
                Filter = "*.cs",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            m_Watcher.Changed += OnFileChanged;
            m_Watcher.Created += OnFileChanged;
            m_Watcher.Renamed += OnFileRenamed;
            m_Watcher.Error += OnWatcherError;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (ShouldProcessFile(e.FullPath) || !File.Exists(e.FullPath))
            {
                QueueFileChange(e.FullPath);
            }
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (ShouldProcessFile(e.FullPath))
            {
                QueueFileChange(e.FullPath);
            }
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            HotReloadLogger.LogError($"FileSystemWatcher error: {e.GetException()?.Message}");
            
            // Try to recover
            if (m_Watcher != null)
            {
                try
                {
                    m_Watcher.EnableRaisingEvents = false;
                    m_Watcher.EnableRaisingEvents = true;
                }
                catch (Exception ex)
                {
                    HotReloadLogger.LogError($"Failed to recover FileSystemWatcher: {ex.Message}");
                }
            }
        }

        private bool ShouldProcessFile(string fullPath)
        {
            // Convert to Unity-relative path
            var relativePath = GetRelativePath(fullPath);
            if (string.IsNullOrEmpty(relativePath))
                return false;

            // Check settings filters
            var settings = HotReloadSettings.Instance;
            return settings.ShouldMonitorPath(relativePath);
        }

        private void QueueFileChange(string fullPath)
        {
            var relativePath = GetRelativePath(fullPath);
            if (string.IsNullOrEmpty(relativePath))
                return;

            lock (m_PendingChanges)
            {
                if (m_PendingChanges.Add(relativePath))
                {
                    m_LastChangeTime = EditorApplication.timeSinceStartup;
                    HotReloadLogger.LogVerbose($"Queued file change: {relativePath}");
                }
            }
        }

        private void FireScriptsChangedEvent()
        {
            List<string> changedFiles;

            lock (m_PendingChanges)
            {
                changedFiles = m_PendingChanges.ToList();
                m_PendingChanges.Clear();
            }

            if (changedFiles.Count == 0)
                return;

            HotReloadLogger.Log($"Detected {changedFiles.Count} changed script(s):");
            foreach (var file in changedFiles)
            {
                HotReloadLogger.LogVerbose($"  - {file}");
            }

            try
            {
                ScriptsChanged?.Invoke(changedFiles);
            }
            catch (Exception ex)
            {
                HotReloadLogger.LogError($"Error in ScriptsChanged event handler: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private string GetRelativePath(string fullPath)
        {
            try
            {
                // Normalize paths
                fullPath = fullPath.Replace('\\', '/');
                var projectPath = m_ProjectPath.Replace('\\', '/');

                if (!fullPath.StartsWith(projectPath))
                    return null;

                var relativePath = fullPath.Substring(projectPath.Length);
                if (relativePath.StartsWith("/"))
                    relativePath = relativePath.Substring(1);

                return relativePath;
            }
            catch (Exception ex)
            {
                HotReloadLogger.LogError($"Failed to get relative path for {fullPath}: {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            if (m_IsDisposed)
                return;

            m_IsDisposed = true;
            StopWatching();
        }
    }
}
