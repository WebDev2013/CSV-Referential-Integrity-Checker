using System;
using System.Linq;
using System.Collections.Generic;

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
    public class TestRiChecker_ExpectedBehaviour : TestRiCheckerBase
    {
        [TestMethod]
        [TestCategory("Expected behaviour: RiChecker")]
        public void TestRiChecker_CallsGetKeysWithParentMffAndChildMff()
        {
            Describe("RiChecker Check() method", () =>
            {
                // Arrange
                var relations = Builder_Relations.BuildRelations_OneParentWithTwoChildren();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_OnePartParentKey();

                //Act
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);
                riChecker.Check();

                //Assert
                It("should call Keys.GetKeys() with parent mff", () =>
                {
                    mockKeyFactory.Verify(x => x.GetKeys("ParentMff", null));
                });
                It("should call Keys.GetKeys() with child1 mff", () =>
                {
                    mockKeyFactory.Verify(x => x.GetKeys("ChildMff1", null));
                });
                It("should call Keys.GetKeys() with child2 mff", () =>
                {
                    mockKeyFactory.Verify(x => x.GetKeys("ChildMff2", null));
                });
            });
        }

        [TestMethod]
        [TestCategory("Expected behaviour: RiChecker")]
        public void TestRiChecker_WhenParentKeysMatch_FindsNoOrphans()
        {
            Describe("RiChecker Check() method finds no orphans when all children have a valid parent key", () =>
            {
                // Arrange
                var relations = Builder_Relations.BuildRelations_OneParentWithTwoChildren();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_OnePartParentKey();

                //Act
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);
                riChecker.Check();
                var results = riChecker.Results;
                var orphans = results.SelectMany(r => r.OrphansSample);

                //Assert
                It("should have one result for each relation", () =>
                {
                    Assert.AreEqual(relations.Count(), results.Count, "Expect to have one RiResult per relation");
                });
                It("should have no orphans", () =>
                {
                    Assert.AreEqual(0, orphans.Count(), "Expect to have NO Orphans");
                });
            });
        }

        [TestMethod]
        [TestCategory("Expected behaviour: RiChecker")]
        public void TestRiChecker_WhenParentKeyMismatches_FindsOrphans()
        {
            Describe("When some children have a invalid parent key", () =>
            {
                // Arrange
                var relations = Builder_Relations.BuildRelations_OneParentWithTwoChildren();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_OnePartParentKey();

                mockKeyFactory.Setup(x => x.GetKeys("ChildMff2", null))  // Override std test fixture with a parent key that does not exist
                    .Returns(new List<object> {
                    new { ParentKey = "notParentKey2", ChildKey = "childKey3" },
                    new { ParentKey = "parentKey2", ChildKey = "childKey4" }
                    });

                //Act
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);
                riChecker.Check();
                var orphans = riChecker.Results.SelectMany(r => r.OrphansSample);

                //Assert
                It("should find orphans", () =>
                {
                    Assert.AreEqual(1, orphans.Count());
                });
            });
        }

        [TestMethod]
        [TestCategory("Expected behaviour: RiChecker")]
        public void TestRiChecker_CallsRiReporter()
        {
            // Arrange
            var relations = Builder_Relations.BuildRelations_OneParentWithTwoChildren();
            var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_OnePartParentKey();

            // Act
            var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);
            riChecker.Check();

            // Assert
            mockRiReporter.Verify(mock => mock.OnInit(2), Times.Once());
            mockRiReporter.Verify(mock => mock.OnNextItem(), Times.Exactly(2));
            mockRiReporter.Verify(mock => mock.OnComplete(riChecker.Results), Times.Once());
        }
    }
}
