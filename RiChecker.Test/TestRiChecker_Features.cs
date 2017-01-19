using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;
using RIChecker.Interfaces;
using TestRiChecker.MocksAndBuilders;

using RIChecker;

namespace TestRiChecker
{
    [TestClass]
    public class TestRiChecker_Features : TestRiCheckerBase
    {
        [TestMethod]
        [TestCategory("Feature: Provides extract file row count figures")]
        public void TestRiChecker_ShouldRecordNumberOfParentAndChildKeys()
        {
            Describe("RiChecker should record the number of records found", () =>
            {
                // Arrange
                var relations = Builder_Relations.BuildRelations_OneParentWithTwoChildren();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_OnePartParentKey();

                //Act
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);
                riChecker.Check();
                var results = riChecker.Results;

                //Assert
                It("should record number of parent records", () =>
                {
                    Assert.AreEqual(2, results.First().ParentCount, "Parent count");
                });
                It("should record number of child records", () =>
                {
                    Assert.AreEqual(2, results.First().ChildCount, "Child key count");
                });
            });
        }

        [TestMethod]
        [TestCategory("Feature: Two part parent keys (Grandchild relation)")]
        public void TestRiChecker_RelationHasTwoPartKeys_WhenParentKeyAndChildKeyMatch_FindsNoOrphans()
        {
            Describe("When testing grandchild realtion, using two part key check to ensure correct Parent/Child/Grandchild relation", () => {
                // Arrange
                var relations = Builder_Relations.BuildRelations_TwoPartKeys();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_TwoPartKey();

                //Act
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);
                riChecker.Check();
                var results = riChecker.Results;
                var orphans = results.SelectMany(r => r.OrphansSample);

                //Assert
                It("should have one result for each relation", () => {
                    Assert.AreEqual(relations.Count(), results.Count, "Expect to have one RiResult per relation");
                });
                It("should have no orphans", () => {
                    Assert.AreEqual(0, orphans.Count(), "Expect to have NO Orphans");
                });
            });
        }

        [TestMethod]
        [TestCategory("Feature: Two part parent keys (Grandchild relation)")]
        public void TestRiChecker_RelationHasTwoPartKeys_WhenParentKeyMismatches_FindsOrphans()
        {
            Describe("When some grandchildren have a invalid parent key", () => {
                // Arrange
                var relations = Builder_Relations.BuildRelations_TwoPartKeys();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_TwoPartKey();

                mockKeyFactory.Setup(x => x.GetKeys("GrandchildMff2", null))
                    .Returns(new List<object> {
                    new { ParentKey = "NOTparentKey2", ChildKey = "childKey2", GrandchildKey = "grandchildKey3" },
                    new { ParentKey = "parentKey2", ChildKey = "childKey2", GrandchildKey = "grandchildKey4" }
                    });

                //Act
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);
                riChecker.Check();
                var orphans = riChecker.Results.SelectMany(r => r.OrphansSample);

                //Assert
                It("should find orphans", () => {
                    Assert.AreEqual(1, orphans.Count());
                });
            });
        }

        [TestMethod]
        [TestCategory("Feature: Two part parent keys (Grandchild relation)")]
        public void TestRiChecker_RelationHasTwoPartKeys_WhenChildKeyMismatches_FindsOrphans()
        {
            Describe("When some grandchildren have a invalid child key", () => {
                // Arrange
                var relations = Builder_Relations.BuildRelations_TwoPartKeys();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_TwoPartKey();

                mockKeyFactory.Setup(x => x.GetKeys("GrandchildMff2", null))
                    .Returns(new List<object> {
                    new { ParentKey = "parentKey2", ChildKey = "NOTchildKey2", GrandchildKey = "grandchildKey3" },
                    new { ParentKey = "parentKey2", ChildKey = "childKey2", GrandchildKey = "grandchildKey4" }
                    });

                //Act
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);
                riChecker.Check();
                var orphans = riChecker.Results.SelectMany(r => r.OrphansSample);

                //Assert
                It("should find orphans", () => {
                    Assert.AreEqual(1, orphans.Count());
                });
            });
        }

        [TestMethod]
        [TestCategory("Feature: Groups relations with same parent to re-use key hashset")]
        public void TestRiChecker_WhenSameParentInMultipleRelations_FetchesParentKeysOnlyOnce()
        {
            Describe("When the same parent occurs in multiple relations", () => {
                // Arrange
                var relations = Builder_Relations.BuildRelations_OneParentWithTwoChildren();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_OnePartParentKey();

                //Act
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);
                riChecker.Check();

                //Assert
                It("should read the parent keys only once", () => {
                    var relationsWithSameParent = relations.Where(r => r.ParentSchema.Name == "ParentMff").Count();
                    Assert.AreEqual(2, relationsWithSameParent);
                    mockKeyFactory.Verify(x => x.GetKeys("ParentMff", null), Times.Once());
                });
            });
        }

        [TestMethod]
        [TestCategory("Feature: Multiple parent source files (Join two key sets)")]
        public void TestRiChecker_ParentSchemaHasTwoSourceFiles_CreatesHashOfAllKeysFromBothFiles()
        {
            Describe("When the relation requires parent keys from two extract files", () => {
                // Arrange
                var relations = Builder_Relations.BuildRelations_MultiSourceParentSchema();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_MultiSourceParentSchema();

                //Act
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);
                riChecker.Check();
                //var results = riChecker.Results;
                //var orphans = results.SelectMany(r => r.OrphansSample);

                //Assert
                It("should call the key factory for each source file", () => {
                    mockKeyFactory.Verify(mock => mock.GetKeys("Parent1", null), Times.Once());
                    mockKeyFactory.Verify(mock => mock.GetKeys("Parent2", null), Times.Once());
                    //Assert.AreEqual(relations.Count, results.Count, "Expect to have one RiResult per relation");
                });
            });
        }

        [TestMethod]
        [TestCategory("Feature: Sample size config limits the number of orphans reported")]
        public void TestRiChecker_WhenSampleSizeIsZero_ReportsAllOrphans()
        {
            Describe("When the SampleSize config is set to zero", () => {
                // Arrange
                var relations = Builder_Relations.BuildRelations_OneParentWithTwoChildren();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_OnePartParentKey();

                mockKeyFactory.Setup(x => x.GetKeys("ChildMff2", null))  // Override std test fixture with a parent key that does not exist
                    .Returns(new List<object> {
                    new { ParentKey = "notParentKey", ChildKey = "childKey3" },
                    new { ParentKey = "notParentKey", ChildKey = "childKey4" },
                    new { ParentKey = "notParentKey", ChildKey = "childKey3" },
                    new { ParentKey = "notParentKey", ChildKey = "childKey6" }
                    });
                config.SampleSize = 0;

                //Act
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);
                riChecker.Check();
                var results = riChecker.Results;
                var orphans = results.SelectMany(r => r.OrphansSample);

                //Assert
                It("should report all orphans that occur", () => {
                    Assert.AreEqual(4, orphans.Count(), "Expect to have 4 Orphans");
                    Assert.AreEqual(4, results.Last().ChildCount, "Expect to count 4 Children");
                    Assert.AreEqual(4, results.Last().TotalOrphanCount, "Expect to count 4 Orphans");
                });
            });
        }

        [TestMethod]
        [TestCategory("Feature: Sample size config limits the number of orphans reported")]
        public void TestRiChecker_WhenSampleSizeIsNonZero_LimitsTheNumberOfOrphansReturned()
        {
            Describe("When the SampleSize config is set to a non-zero value", () => {
                // Arrange
                var relations = Builder_Relations.BuildRelations_OneParentWithTwoChildren();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_OnePartParentKey();

                mockKeyFactory.Setup(x => x.GetKeys("ChildMff2", null))  // Override std test fixture with a parent key that does not exist
                    .Returns(new List<object> {
                    new { ParentKey = "notParentKey", ChildKey = "childKey3" },
                    new { ParentKey = "notParentKey", ChildKey = "childKey4" },
                    new { ParentKey = "notParentKey", ChildKey = "childKey5" },
                    new { ParentKey = "notParentKey", ChildKey = "childKey6" }
                    });
                config.SampleSize = 2;

                //Act
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);
                riChecker.Check();
                var results = riChecker.Results;
                var orphans = results.SelectMany(r => r.OrphansSample);

                //Assert
                It("should contain only SampleSize number of orphans", () => {
                    Assert.AreEqual(2, orphans.Count(), "Expect to have 2 Orphans");
                });
                It("should report the correct number of children", () => {
                    Assert.AreEqual(4, results.Last().ChildCount, "Expect to count 4 Children");
                });
                It("should report the correct number of total orphans", () => {
                    Assert.AreEqual(4, results.Last().TotalOrphanCount, "Expect to count 4 Orphans");
                });
            });
        }

        [TestMethod]
        [TestCategory("Feature: Relation can be filtered on child key")]
        public void TestRiChecker_ChildFilter()
        {
            Describe("When the KeyFactory is called to get child keys", () => {
                // Arrange
                var relations = Builder_Relations.BuildRelations_OneParentWithTwoChildren();
                var mockKeyFactory = Builder_MockKeyFactory.BuildMockKeyFactory_OnePartParentKey();

                Func<dynamic, bool> filter = x => x == "123";
                relations[0].RelationChildFilter = filter;
                relations[1].RelationChildFilter = filter;

                // Act
                var riChecker = new RiChecker(relations, mockKeyFactory.Object, mockParser.Object, mockRiReporter.Object, config);
                riChecker.Check();

                // Assert
                It("should pass in Realtion.RelationChildFilter when fetching child keys", () => {
                    mockKeyFactory.Verify(mock => mock.GetKeys("ChildMff1", Moq.It.IsAny<Func<dynamic, bool>>()), Times.Once());
                    mockKeyFactory.Verify(mock => mock.GetKeys("ChildMff2", Moq.It.IsAny<Func<dynamic, bool>>()), Times.Once());
                });
                It("should NOT pass in a filter when fetching parent keys", () => {
                    mockKeyFactory.Verify(mock => mock.GetKeys("ParentMff", null), Times.Once());
                });
            });
        }

    }
}
