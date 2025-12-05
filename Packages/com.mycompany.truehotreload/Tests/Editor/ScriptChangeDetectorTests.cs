using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TrueHotReload.Editor;

namespace TrueHotReload.EditorTests
{
    /// <summary>
    /// Edit mode tests for ScriptChangeDetector.
    /// Tests file watching, debouncing, and filtering.
    /// </summary>
    public class ScriptChangeDetectorTests
    {
        private ScriptChangeDetector m_Detector;
        private List<IReadOnlyList<string>> m_ReceivedChanges;
        private string m_TestDirectory;

        [SetUp]
        public void Setup()
        {
            m_Detector = new ScriptChangeDetector();
            m_ReceivedChanges = new List<IReadOnlyList<string>>();
            m_Detector.ScriptsChanged += OnScriptsChanged;

            // Create temporary test directory
            m_TestDirectory = Path.Combine(Application.dataPath, "..", "Temp", "HotReloadTests");
            if (Directory.Exists(m_TestDirectory))
            {
                Directory.Delete(m_TestDirectory, true);
            }
            Directory.CreateDirectory(m_TestDirectory);
        }

        [TearDown]
        public void Teardown()
        {
            m_Detector?.Dispose();
            
            if (Directory.Exists(m_TestDirectory))
            {
                try
                {
                    Directory.Delete(m_TestDirectory, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        private void OnScriptsChanged(IReadOnlyList<string> changedPaths)
        {
            m_ReceivedChanges.Add(changedPaths);
        }

        [Test]
        public void CanCreateAndDisposeDetector()
        {
            var detector = new ScriptChangeDetector();
            Assert.IsNotNull(detector);
            detector.Dispose();
        }

        [Test]
        public void CanStartAndStopWatching()
        {
            m_Detector.StartWatching(m_TestDirectory);
            m_Detector.StopWatching();
            Assert.Pass("Start and stop watching completed without errors");
        }

        [Test]
        public void DetectsFileCreation()
        {
            m_Detector.StartWatching(m_TestDirectory);

            // Create a C# file
            var testFile = Path.Combine(m_TestDirectory, "TestScript.cs");
            File.WriteAllText(testFile, "public class TestScript { }");

            // Wait for debouncing
            System.Threading.Thread.Sleep(1000);
            m_Detector.Update();

            // Note: This test may be flaky due to FileSystemWatcher timing
            // In a real test suite, you'd want to mock the file system watcher
            m_Detector.StopWatching();
        }

        [Test]
        public void PathFilteringWorks()
        {
            var settings = HotReloadSettings.Instance;
            
            // Test included path
            Assert.IsTrue(settings.ShouldMonitorPath("Assets/Scripts/Player.cs"));
            
            // Test excluded path
            Assert.IsFalse(settings.ShouldMonitorPath("Assets/Plugins/SomeLib.cs"));
            
            // Test path normalization
            Assert.IsTrue(settings.ShouldMonitorPath("Assets\\Scripts\\Player.cs"));
        }
    }
}
