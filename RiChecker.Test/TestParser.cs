using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JasmineExtensions;
using TestRiChecker.MocksAndBuilders;

using RIChecker.Implementation;

namespace TestRiChecker
{
    [TestClass]
    public class TestParser : JasmineExtended
    {
        [TestMethod]
        [TestCategory("Expected behaviour: Parser.ParseAndTest")]
        public void TestParseAndTest_WhenKeysExist_ReturnsOneRowOneKey()
        {
            Describe("When the CSV file has one rows and schema defines two keys", () =>
            {
                var fileDataStrippedOfHeaders = new List<List<string>> {
                    new List<string> { "value1", "value2", "value3" }
                };
                Func<List<string>, object> lineParserExpression = r => new { key1 = r[0] };

                //Act
                var fileParser = new FileParser(new LineParser(lineParserExpression));
                fileParser.ParseAndTest("someRecordType_Keys.csv", fileDataStrippedOfHeaders, filter: null);
                List<dynamic> result = fileParser.ParseResults.Select(pr => pr.ParsedObject).ToList();

                // Assert
                It("should return one row", () =>
                {
                    Assert.AreEqual(1, result.Count());
                });
                It("should return exactly one key", () =>
                {
                    Assert.AreEqual("value1", result.First().key1);

                    var properties = result.First().GetType().GetProperties();
                    Assert.AreEqual(1, properties.Length);
                });
            });
        }

        [TestMethod]
        [TestCategory("Expected behaviour: Parser.ParseAndTest")]
        public void TestParseAndTest_WhenKeysExist_ReturnsOneRowTwoKeys()
        {
            Describe("When the CSV file has one row and schema defines two keys", () =>
            {
                var fileDataStrippedOfHeaders = new List<List<string>> {
                    new List<string> { "value1", "value2", "value3" }
                };
                Func<List<string>, object> lineParserExpression = r => new { key1 = r[0], key2 = r[1] };

                //Act
                var fileParser = new FileParser(new LineParser(lineParserExpression));
                fileParser.ParseAndTest("someRecordType_Keys.csv", fileDataStrippedOfHeaders, filter: null);
                List<dynamic> result = fileParser.ParseResults.Select(pr => pr.ParsedObject).ToList();

                // Assert
                It("should return one row", () =>
                {
                    Assert.AreEqual(1, result.Count());
                });
                It("should return exactly two keys", () =>
                {
                    var firstResult = result.First();
                    Assert.AreEqual("value1", firstResult.key1);
                    Assert.AreEqual("value2", firstResult.key2);

                    var properties = result.First().GetType().GetProperties();
                    Assert.AreEqual(2, properties.Length);
                });
            });
        }

        [TestMethod]
        [TestCategory("Expected behaviour: Parser.ParseAndTest")]
        public void TestParseAndTest_WhenKeysExist_ReturnsTwoRowsWithTwoKeys()
        {
            Describe("When the CSV file has two rows and schema defines two keys", () =>
            {
                var fileDataStrippedOfHeaders = new List<List<string>> {
                    new List<string> { "value1", "value2", "value3" },
                    new List<string> { "value4", "value5", "value6" }
                };
                Func<List<string>, object> lineParserExpression = r => new { key1 = r[0], key2 = r[1] };

                //Act
                var fileParser = new FileParser(new LineParser(lineParserExpression));
                fileParser.ParseAndTest("someRecordType_Keys.csv", fileDataStrippedOfHeaders, filter: null);
                List<dynamic> results = fileParser.ParseResults.Select(pr => pr.ParsedObject).ToList();

                // Assert
                It("should return two rows", () =>
                {
                    Assert.AreEqual(2, results.Count());
                });
                It("should return two keys", () =>
                {
                    var firstResult = results.First();
                    Assert.AreEqual("value1", firstResult.key1);
                    Assert.AreEqual("value2", firstResult.key2);
                    Assert.AreEqual(2, firstResult.GetType().GetProperties().Length);

                    var secondResult = results.Last();
                    Assert.AreEqual("value4", secondResult.key1);
                    Assert.AreEqual("value5", secondResult.key2);
                    Assert.AreEqual(2, secondResult.GetType().GetProperties().Length);
                });
            });
        }

        [TestMethod]
        [TestCategory("Feature: Relation can be filtered on child key")]
        public void TestParseAndTest_Filter_ReturnsSubsetOfKeys()
        {
            Describe("When a filter is passed to ParseAndTest()", () =>
            {
                var fileData = new List<List<string>> {
                    new List<string> { "header1", "header2", "header3" },
                    new List<string> { "value1", "value2", "value3" },
                    new List<string> { "value4", "FILTERME", "value6" },
                    new List<string> { "value7", "value8", "value9" }
                };
                Func<List<string>, object> lineParserExpression = r => new { key1 = r[0], key2 = r[1] };
                Func< dynamic, bool> filter = (x) => x.key2 == "FILTERME";

                //Act
                var fileParser = new FileParser(new LineParser(lineParserExpression));
                fileParser.ParseAndTest("someRecordType_Keys.csv", fileData, filter);
                List<dynamic> results = fileParser.ParseResults.Select(pr => pr.ParsedObject).ToList();

                // Assert
                It("should return a subset of keys", () =>
                {
                    Assert.AreEqual(1, results.Count());
                    var firstResult = results.First();
                    Assert.AreEqual("value4", firstResult.key1);
                    Assert.AreEqual("FILTERME", firstResult.key2);
                });
            });
        }

        [TestMethod]
        [TestCategory("Boundary condition: Parser.ParseAndTest")]
        public void TestParseAndTest_MalformedCsv_CatchesErrors()
        {
            Describe("When there are not enough columns in SOURCE row", () =>
            {
                //Arrange
                var fileDataStrippedOfHeaders = new List<List<string>> {
                    new List<string> { "value1", "value2" },
                    new List<string> { "value4" },
                    new List<string> { "value5", "value6" },
                    new List<string> { "value7" },
                    new List<string> { "value8", "value9" },
                };
                Func<List<string>, object> lineParserExpression = r => new { key1 = r[0], key2 = r[1] };

                //Act
                var fileParser = new FileParser(new LineParser(lineParserExpression));
                fileParser.ParseAndTest("someRecordType_Keys.csv", fileDataStrippedOfHeaders, filter: null);
                var results = fileParser.ParseResults.ToList();

                // Assert
                It("should catch the error", () =>
                {
                    Assert.IsFalse(results[1].Success);
                    Assert.IsFalse(results[3].Success);
                });
                It("should report line number in source file where the error occurs", () =>
                {
                    Assert.IsTrue(results[1].Exception.Message.Contains("line 2"));
                });
                It("should catch the error and pass it on to the reporter, and continue to the next row", () =>
                {
                    Assert.IsTrue(results[3].Exception.Message.Contains("line 4"));
                });
            });
        }

    }
}
