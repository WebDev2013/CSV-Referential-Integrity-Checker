using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RIChecker;
using RIChecker.Interfaces;
using RIChecker.Implementation;
using System.Collections.Generic;
using JasmineExtensions;

using Moq;
using RIChecker.Model;
using TestRiChecker.MocksAndBuilders;

namespace TestRiChecker
{
    [TestClass]
    public class TestKeyFactory : JasmineExtended
    {
        private Mock<ICsvFileHandler> mockCsvFileHandler;
        private Mock<IRiReporter> mockRiReporter;
        private Mock<IFileParser> mockFileParser;
        private Schemas schema;

        [TestInitialize]
        public void testInit()
        {
            mockFileParser = new Mock<IFileParser>();
            mockCsvFileHandler = new Mock<ICsvFileHandler>();
            mockCsvFileHandler.Setup(x => x.CheckIfTargetIsOutOfDate("anyString", "anyString")).Returns(false);
            mockRiReporter = new Mock<IRiReporter>();
            schema = Builder_Schemas.BuildSingleSchema(parser: r => new { key1 = r[0] });
        }

        [TestMethod]
        [TestCategory("Expected behaviour: KeyFactory.GetKeys")]
        public void TestGetKeys_HandsFileDataToParser()
        {
            Describe("When the CsvFleHandler returns file data", () =>
            {
                //Arrange
                var fileData = new List<List<string>> {
                            new List<string> { "header1", "header2", "header3" },
                            new List<string> { "value1", "value2", "value3" }
                        };
                var fileDataStrippedOfHeaders = fileData.Skip(1);
                mockCsvFileHandler.Setup(x => x.ReadKeysFile("someRecordType_Keys.csv"))
                    .Returns(fileData);
                schema = Builder_Schemas.BuildSingleSchema(parser: r => new { key1 = r[0], key2 = r[1] });
                schema["someRecordType"].FileParser = mockFileParser.Object;

                //Act
                //var keys = new KeyFactory(schema, mockFileParser.Object, mockCsvFileHandler.Object, mockRiReporter.Object);
                var keys = new KeyFactory(schema, mockCsvFileHandler.Object, mockRiReporter.Object);
                List<dynamic> result = keys.GetKeys("someRecordType", null).ToList();

                // Assert
                It("should call Parser.ParseAndTest with the file data stripped of headers", () =>
                {
                    mockFileParser.Verify(mock => mock.ParseAndTest("someRecordType_Keys.csv", fileDataStrippedOfHeaders, null), Times.Once());
                });

            });
        }

        [TestMethod]
        [TestCategory("Expected behaviour: KeyFactory.GetKeys")]
        public void TestGetKeys_ReturnsAllParserData()
        {
            Describe("When the CSV file has two rows and schema defines two keys", () =>
            {
                var parseResults = new List<FileParseResult> {
                    new FileParseResult { Success = true, ParsedObject = new { key1 = "value1", key2 = "value2" } },
                    new FileParseResult { Success = true, ParsedObject = new { key1 = "value4", key2 = "value5" } }
                };
                mockFileParser.Setup(mock => mock.ParseResults)
                    .Returns(parseResults);
                schema = Builder_Schemas.BuildSingleSchema(parser: r => new { key1 = r[0], key2 = r[1] });
                schema["someRecordType"].FileParser = mockFileParser.Object;

                //Act
                var keys = new KeyFactory(schema, mockFileParser.Object, mockCsvFileHandler.Object, mockRiReporter.Object);
                List<dynamic> result = keys.GetKeys("someRecordType", null).ToList();

                // Assert
                It("should return two rows", () =>
                {
                    Assert.AreEqual(2, result.Count());
                });
                It("should return two keys", () =>
                {
                    var firstResult = result.First();
                    Assert.AreEqual("value1", firstResult.key1);
                    Assert.AreEqual("value2", firstResult.key2);

                    var secondResult = result.Last();
                    Assert.AreEqual("value4", secondResult.key1);
                    Assert.AreEqual("value5", secondResult.key2);
                });
            });
        }

        [TestMethod]
        [TestCategory("Expected behaviour: KeyFactory.GetKeys")]
        public void TestGetKeys_WhenAnyParserErrorsOccur_FailsTheFile()
        {
            Describe("When the parser reports any rows failed", () =>
            {
                var parseResults = new List<FileParseResult> {
                    new FileParseResult { Success = true, ParsedObject = new { key1 = "value1", key2 = "value2" } },
                    new FileParseResult { Success = false, Exception= new ParserException("some error") },
                    new FileParseResult { Success = true, ParsedObject = new { key1 = "value4", key2 = "value5" } }
                };
                mockFileParser.Setup(mock => mock.ParseResults)
                    .Returns(parseResults);
                schema = Builder_Schemas.BuildSingleSchema(parser: r => new { key1 = r[0], key2 = r[1] });
                schema["someRecordType"].FileParser = mockFileParser.Object;

                // Assert
                ItThrows("should fail the file and raise an exception", typeof(Exception), () =>
                {
                    var keys = new KeyFactory(schema, mockFileParser.Object, mockCsvFileHandler.Object, mockRiReporter.Object);
                    var result = keys.GetKeys("someRecordType");
                });
            });
        }

        [TestMethod]
        [TestCategory("Feature: Keys file is (re)created when does not exist or is older than extract file")]
        public void TestGetKeys_KeysFileDoesNotExistOrIsOutOfDate_CreatesAndReturnsKeys()
        {
            Describe("When the keys file is out of date or missing", () =>
            {
                //Arrange
                mockCsvFileHandler.Setup(x => x.ReadExtractFile("someRecordType.txt"))
                    .Returns(new List<List<string>> {
                            new List<string> { "header1", "header2", "header3" },
                            new List<string> { "value1", "value2", "value3" },
                            new List<string> { "value4", "value5", "value6" }
                    });
                mockCsvFileHandler.Setup(x => x.CheckIfTargetIsOutOfDate("someRecordType.txt", "someRecordType_Keys.csv")).Returns(true);
                var parseResults = new List<FileParseResult> {
                    new FileParseResult { Success = true, ParsedObject = new { key1 = "value1", key2 = "value2" } },
                    new FileParseResult { Success = true, ParsedObject = new { key1 = "value4", key2 = "value5" } }
                };
                mockFileParser.Setup(mock => mock.ParseResults)
                    .Returns(parseResults);
                schema = Builder_Schemas.BuildSingleSchema(parser: r => new { key1 = r[0], key2 = r[1] });

                //Act
                var keys = new KeyFactory(schema, mockFileParser.Object, mockCsvFileHandler.Object, mockRiReporter.Object);
                IEnumerable<dynamic> result = keys.GetKeys("someRecordType");

                // Assert
                It("should check if the files are out of date", () =>
                {
                    mockCsvFileHandler.Verify(mock => mock.CheckIfTargetIsOutOfDate("someRecordType.txt", "someRecordType_Keys.csv"), Times.Once());
                });
                It("should read the source file", () =>
                {
                    mockCsvFileHandler.Verify(mock => mock.ReadExtractFile("someRecordType.txt"), Times.Once());
                });
                It("should write the keys file", () =>
                {
                    var keysToWrite = parseResults.Select(pr => pr.ParsedObject);
                    mockCsvFileHandler.Verify(mock => mock.WriteCsv(keysToWrite, "someRecordType_Keys.csv", ','), Times.Once());
                });
                It("should read the keys file", () =>
                {
                    mockCsvFileHandler.Verify(mock => mock.ReadKeysFile("someRecordType_Keys.csv"), Times.Once());
                });
            });
        }


        // Boundary conditions
        [TestMethod]
        [TestCategory("Boundary condition: KeyFactory.GetKeys")]
        public void TestGetKeys_WhenCsvWithoutDataRows_ReturnsEmptyResultEnumerable()
        {
            Describe("When the CSV file has no data or header", () =>
            {
                //Arrange
                mockCsvFileHandler.Setup(x => x.ReadKeysFile("someRecordType_Keys.csv"))
                    .Returns(
                        new List<List<string>> {
                            new List<string> { "header1", "header2", "header3" }
                        });
                schema = Builder_Schemas.BuildSingleSchema(parser: r => new { key1 = r[0], key2 = r[1] });

                //Act
                var keys = new KeyFactory(schema, mockFileParser.Object, mockCsvFileHandler.Object, mockRiReporter.Object);
                IEnumerable<dynamic> result = keys.GetKeys("someRecordType", null);

                // Assert
                It("should return and empty result set", () =>
                {
                    Assert.IsTrue(result.Count() == 0);
                });
            });
        }

        [TestMethod]
        [TestCategory("Boundary condition: KeyFactory.GetKeys")]
        public void TestGetKeys_WhenCsvWithoutDataOrHeaderRows_ReturnsEmptyResultEnumerable()
        {
            Describe("When the CSV file has no data or header", () =>
            {
                //Arrange
                var csvWithoutDataOrHeaderRows = new List<List<string>>();
                mockCsvFileHandler.Setup(x => x.ReadKeysFile("someRecordType_Keys.csv"))
                    .Returns(csvWithoutDataOrHeaderRows);
                schema = Builder_Schemas.BuildSingleSchema(parser: r => new { key1 = r[0], key2 = r[1] });

                //Act
                var keys = new KeyFactory(schema, mockFileParser.Object, mockCsvFileHandler.Object, mockRiReporter.Object);
                IEnumerable<dynamic> result = keys.GetKeys("someRecordType", null);

                // Assert
                It("should return and empty result set", () =>
                {
                    Assert.IsTrue(result.Count() == 0);
                });
            });
        }

        [TestMethod]
        [TestCategory("Boundary condition: KeyFactory.GetKeys")]
        public void TestGetKeys_WhenCsvDoesNotExist_CatchAndWriteToReporterAndFail()
        {
            Describe("When there is an error reading the Keys Csv file", () =>
            {
                //Arrange
                mockCsvFileHandler.Setup(x => x.ReadKeysFile("someRecordType_Keys.csv"))
                    .Throws(new FileLoadException());
                schema = Builder_Schemas.BuildSingleSchema(parser: r => new { key1 = r[0], key2 = r[1] });

                ItThrows("should catch the error and pass it on to the reporter", typeof(FileLoadException), () =>
                {
                    //Act
                    var keys = new KeyFactory(schema, mockFileParser.Object, mockCsvFileHandler.Object, mockRiReporter.Object);
                    var result = keys.GetKeys("someRecordType", null);

                    // Assert
                    Assert.IsNull(result);
                    mockRiReporter.Verify(mock => mock.WriteMessage(Moq.It.IsAny<string>()), Times.Once());
                });
            });
        }

        [TestMethod]
        [TestCategory("Boundary condition: KeyFactory.GetKeys")]
        public void TestGetKeys_MalformedCsv_CatchErrorAndWriteToReporter()
        {
            Describe("When an error occurs in the parser", () =>
            {
                //Arrange
                var parseResults = new List<FileParseResult> {
                    new FileParseResult { Success = true, ParsedObject = new { key1 = "value1", key2 = "value2" } },
                    new FileParseResult { Success = false, Exception = new ParserException("An error occurred") },
                    new FileParseResult { Success = true, ParsedObject = new { key1 = "value5", key2 = "value6" } },
                    new FileParseResult { Success = false, Exception = new ParserException("An error occurred") },
                    new FileParseResult { Success = true, ParsedObject = new { key1 = "value8", key2 = "value9" } }
                };
                mockFileParser.Setup(mock => mock.ParseResults)
                    .Returns(parseResults);

                schema = Builder_Schemas.BuildSingleSchema(parser: r => new { key1 = r[0], key2 = r[1] });
                schema["someRecordType"].FileParser = mockFileParser.Object;

                //Act
                try
                {
                    var keys = new KeyFactory(schema, mockFileParser.Object, mockCsvFileHandler.Object, mockRiReporter.Object);
                    var result = keys.GetKeys("someRecordType");
                }
                catch { } // Consume the 'file failed' exception

                // Assert
                It("should catch the error and pass it on to the reporter", () =>
                {
                    mockRiReporter.Verify(mock => mock.WriteMessage(Moq.It.IsAny<string>()), Times.Exactly(2));
                });
            });
        }

        [TestMethod]
        [TestCategory("Boundary condition: KeyFactory.GetKeys")]
        public void TestGetKeys_ParameterChecking()
        {
            Describe("Keys.GetKeys method", () => {

                ItThrows("when mff paramater is null, should throw an error", typeof(ArgumentNullException), () =>
                {
                    var keys = new KeyFactory(schema, mockFileParser.Object, mockCsvFileHandler.Object, mockRiReporter.Object);
                    keys.GetKeys(null);
                });
                ItThrows("when mff paramater is empty, should throw an error", typeof(ArgumentNullException), () =>
                {
                    var keys = new KeyFactory(schema, mockFileParser.Object, mockCsvFileHandler.Object, mockRiReporter.Object);
                    keys.GetKeys(" ");
                });
            });
        }

        [TestMethod]
        [TestCategory("Boundary condition: KeyFactory.GetKeys")]
        public void TestKeyFactory_Constructor_ParameterChecking()
        {
            Describe("Keys constructor", () =>
            {
                ItThrows("when schema paramater is null, should throw an error", typeof(ArgumentNullException), () =>
                {
                    var keys = new KeyFactory(null, mockFileParser.Object, mockCsvFileHandler.Object, mockRiReporter.Object);
                });
                ItThrows("when parser paramater is null, should throw an error", typeof(ArgumentNullException), () =>
                {
                    var keys = new KeyFactory(new Schemas(), null, mockCsvFileHandler.Object, mockRiReporter.Object);
                });

                ItThrows("when records paramater is empty, should throw an error", typeof(ArgumentNullException), () =>
                {
                    var keys = new KeyFactory(new Schemas(), mockFileParser.Object, mockCsvFileHandler.Object, mockRiReporter.Object);
                });
                ItThrows("when csvFileHandler paramater is null, should throw an error", typeof(ArgumentNullException), () =>
                {
                    var keys = new KeyFactory(schema, mockFileParser.Object, null, mockRiReporter.Object);
                });
                It("when reporter paramater is null, should NOT throw an error", () =>
                {
                    var keys = new KeyFactory(schema, mockFileParser.Object, mockCsvFileHandler.Object, null);
                });
            });
        }
    }
}
