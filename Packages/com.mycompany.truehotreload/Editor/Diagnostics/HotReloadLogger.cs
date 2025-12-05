using System;
using System.IO;
using UnityEngine;

namespace TrueHotReload.Editor
{
    /// <summary>
    /// Centralized logging for TrueHotReload system.
    /// Handles console output and optional file logging with performance metrics.
    /// </summary>
    public static class HotReloadLogger
    {
        private const string LogPrefix = "[TrueHotReload]";
        private static StreamWriter s_FileWriter;
        private static bool s_FileWriterInitialized;

        /// <summary>
        /// Log an informational message.
        /// </summary>
        public static void Log(string message)
        {
            var formattedMessage = $"{LogPrefix} {message}";
            Debug.Log(formattedMessage);
            WriteToFile(formattedMessage);
        }

        /// <summary>
        /// Log a warning message.
        /// </summary>
        public static void LogWarning(string message)
        {
            var formattedMessage = $"{LogPrefix} {message}";
            Debug.LogWarning(formattedMessage);
            WriteToFile($"WARNING: {formattedMessage}");
        }

        /// <summary>
        /// Log an error message.
        /// </summary>
        public static void LogError(string message)
        {
            var formattedMessage = $"{LogPrefix} {message}";
            Debug.LogError(formattedMessage);
            WriteToFile($"ERROR: {formattedMessage}");
        }

        /// <summary>
        /// Log a verbose/debug message (only if verbose logging is enabled).
        /// </summary>
        public static void LogVerbose(string message)
        {
            if (!HotReloadSettings.Instance.verboseLogging)
                return;

            var formattedMessage = $"{LogPrefix} [VERBOSE] {message}";
            Debug.Log($"<color=#888888>{formattedMessage}</color>");
            WriteToFile(formattedMessage);
        }

        /// <summary>
        /// Log compilation performance metrics.
        /// </summary>
        public static void LogCompilationTime(float seconds, int fileCount)
        {
            var message = $"Compiled {fileCount} file(s) in {seconds:F3}s ({seconds / fileCount:F3}s per file)";
            Log($"<color=#00FF00>{message}</color>");
        }

        /// <summary>
        /// Log patching performance metrics.
        /// </summary>
        public static void LogPatchTime(float seconds, int methodCount)
        {
            var message = $"Patched {methodCount} method(s) in {seconds:F3}s";
            Log($"<color=#00FF00>{message}</color>");
        }

        /// <summary>
        /// Log a successful hot reload.
        /// </summary>
        public static void LogSuccess(int methodCount, float totalSeconds)
        {
            var message = $"✓ Hot reload successful: {methodCount} method(s) patched in {totalSeconds:F3}s";
            Log($"<color=#00FF00><b>{message}</b></color>");
        }

        /// <summary>
        /// Log a failed hot reload with error details.
        /// </summary>
        public static void LogFailure(string reason)
        {
            var message = $"✗ Hot reload failed: {reason}";
            LogError($"<color=#FF0000><b>{message}</b></color>");
        }

        /// <summary>
        /// Write a message to the log file if file logging is enabled.
        /// </summary>
        private static void WriteToFile(string message)
        {
            if (!HotReloadSettings.Instance.logToFile)
                return;

            try
            {
                if (!s_FileWriterInitialized)
                {
                    InitializeFileWriter();
                }

                if (s_FileWriter != null)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    s_FileWriter.WriteLine($"[{timestamp}] {message}");
                    s_FileWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Failed to write to log file: {ex.Message}");
                s_FileWriter?.Dispose();
                s_FileWriter = null;
            }
        }

        /// <summary>
        /// Initialize the file writer for log output.
        /// </summary>
        private static void InitializeFileWriter()
        {
            s_FileWriterInitialized = true;

            try
            {
                var settings = HotReloadSettings.Instance;
                var logPath = Path.Combine(Application.dataPath, "..", settings.logFilePath);
                var logDirectory = Path.GetDirectoryName(logPath);

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                s_FileWriter = new StreamWriter(logPath, append: true)
                {
                    AutoFlush = true
                };

                s_FileWriter.WriteLine($"\n========== TrueHotReload Session Started: {DateTime.Now} ==========\n");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Failed to initialize log file: {ex.Message}");
                s_FileWriter = null;
            }
        }

        /// <summary>
        /// Close the file writer and release resources.
        /// </summary>
        public static void Shutdown()
        {
            if (s_FileWriter != null)
            {
                try
                {
                    s_FileWriter.WriteLine($"\n========== TrueHotReload Session Ended: {DateTime.Now} ==========\n");
                    s_FileWriter.Flush();
                    s_FileWriter.Dispose();
                }
                catch
                {
                    // Ignore errors during shutdown
                }
                finally
                {
                    s_FileWriter = null;
                    s_FileWriterInitialized = false;
                }
            }
        }
    }
}
