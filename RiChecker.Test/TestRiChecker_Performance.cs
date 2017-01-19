using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JasmineNET;
using Moq;
using TestRiChecker.MocksAndBuilders;

using RIChecker;
using RIChecker.Interfaces;
using RIChecker.Implementation;

namespace TestRiChecker
{
    [TestClass]
    public class TestRiChecker_Performance : TestRiCheckerBase
    {
        [TestMethod]
        [TestCategory("Performance: Iterates files only once")]
        public void TestRiChecker_EnumeratesParentFile_OnlyOnce()
        {
            Describe("When the RiChecker.Check() is called", () => {
                // Arrange
                var relations = Builder_Relations.BuildRelations_OneParentWithTwoChildren();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_OnePartParentKey();

                mockKeyFactory.Setup(x => x.GetKeys("ParentMff", null)).Returns(EnumeratorCallMonitor_ParentKeysList());
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);

                // Act
                riChecker.Check();

                // Assert
                It("should only call the parent file enumerator once", () => {
                    Assert.AreEqual(1, timesEnumeratorCalled);
                });
            });
        }

        [TestMethod]
        [TestCategory("Performance: Iterates files only once")]
        public void TestRiChecker_EnumeratesChildFile_OnlyOnce()
        {
            Describe("When the RiChecker.Check() is called", () => {
                // Arrange
                var relations = Builder_Relations.BuildRelations_OneParentWithTwoChildren();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_OnePartParentKey();

                mockKeyFactory.Setup(x => x.GetKeys("ChildMff1", null)).Returns(EnumeratorCallMonitor_ChildKeysList());
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, (new Mock<IFileParser>()).Object, mockRiReporter.Object, config);

                // Act
                riChecker.Check();

                // Assert
                It("should only call the child file enumerator once", () => {
                    Assert.AreEqual(1, timesEnumeratorCalled);
                });
            });
        }

    }
}
