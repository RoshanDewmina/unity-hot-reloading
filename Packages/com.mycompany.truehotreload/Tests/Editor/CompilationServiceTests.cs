using NUnit.Framework;
using System.Linq;
using TrueHotReload.Editor;

namespace TrueHotReload.EditorTests
{
    /// <summary>
    /// Edit mode tests for HotReloadCompilationService.
    /// Tests Roslyn compilation and method detection.
    /// </summary>
    public class CompilationServiceTests
    {
        private HotReloadCompilationService m_Service;

        [SetUp]
        public void Setup()
        {
            m_Service = new HotReloadCompilationService();
        }

        [Test]
        public void CanCreateCompilationService()
        {
            Assert.IsNotNull(m_Service);
        }

        [Test]
        public void CompilationFailsWithEmptyPaths()
        {
            var result = m_Service.CompileChangedScripts(new string[] { });
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void CompilationFailsWithNonExistentFile()
        {
            var result = m_Service.CompileChangedScripts(new[] { "Assets/NonExistent.cs" });
            Assert.IsFalse(result.Success);
        }

        // Note: More comprehensive tests would require:
        // 1. Creating temporary .cs files with known content
        // 2. Compiling them
        // 3. Verifying the compiled assembly
        // 4. Checking method descriptors
        // This is complex and would require a test harness
    }
}
