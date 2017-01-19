using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JasmineExtensions;

using Moq;
using RIChecker.Interfaces;
using RIChecker.Implementation;

using System.IO.Abstractions.TestingHelpers;

namespace TestRiChecker
{
    [TestClass]
    public class TestCsvFileHandler : JasmineExtended
    {
        Mock<IStreamFactory> mockStreamFactory;
        Config folderConfig;
        MockFileSystem mockFileSystem;

        [TestInitialize]
        public void TestInit()
        {
            mockStreamFactory = new Mock<IStreamFactory>();
            folderConfig = new Config(mffSet: "NameOfSet", loadNo: "ID of Load")
            {
                KeysFolder = @"path\to\data",
                ExtractsFolder = @"path\to\extracts",
                ResultsFolder = @"path\to\results"
            };
            mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"path\to\extracts\someFile.txt", new MockFileData("Extract file content") },
                { @"path\to\data\someFile_Keys.csv", new MockFileData("Keys file content") }
            });
        }

        [TestMethod]
        [TestCategory("Expected behaviour: CsvFileHandler")]
        public void TestCsvFileHandler_ReadCsv()
        {
            Describe("CsvFileHandler when reading a CSV file ", () =>
            {
                //Arrange
                var fileContent = $"header1,header2,header3{Environment.NewLine}value1,value2,value3{Environment.NewLine}value4,value5,value6";
                var streamReader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(fileContent)));
                mockStreamFactory.Setup(x => x.GetStreamReader(Moq.It.IsAny<string>()))
                                 .Returns(streamReader);
                var csvFileHandler = new CsvFileHandler(folderConfig, mockStreamFactory.Object, mockFileSystem);

                //Act
                var result = csvFileHandler.ReadExtractFile("someFile.txt").ToList();
                streamReader.Dispose();

                //Assert
                var expected = new List<List<string>> {
                    new List<string> { "header1", "header2", "header3" },
                    new List<string> { "value1", "value2", "value3" },
                    new List<string> { "value4", "value5", "value6" }
                };
                var headers = result.ToList()[0].ToList();
                var row1 = result.ToList()[1].ToList();
                var row2 = result.ToList()[2].ToList();
                It("should read all rows", () =>
                {
                    CollectionAssert.AreEqual(expected[0], headers);
                    CollectionAssert.AreEqual(expected[1], row1);
                    CollectionAssert.AreEqual(expected[2], row2);
                });
            });
        }

        [TestMethod]
        [TestCategory("Expected behaviour: CsvFileHandler")]
        public void TestCsvFileHandler_WriteCsv()
        {
            Describe("When CsvFileHandler.WriteCsv() is called", () =>
            {
                //Arrange
                var objectsToWrite = new List<object> {
                    new { header1 = "value1", header2 = "value2", header3 = "value3" },
                    new { header1 = "value4", header2 = "value5", header3 = "value6" }
                };
                var memoryStream = new MemoryStream();
                var streamWriter = new StreamWriter(memoryStream);
                mockStreamFactory.Setup(x => x.GetStreamWriter(Moq.It.IsAny<string>()))
                                 .Returns(streamWriter);
                var csvFileHandler = new CsvFileHandler(folderConfig, mockStreamFactory.Object, mockFileSystem);

                //Act
                csvFileHandler.WriteCsv(objectsToWrite, "someFile", ',');
                var result = Encoding.UTF8.GetString(memoryStream.ToArray());
                streamWriter.Dispose();

                //Assert
                var expected = $"header1,header2,header3{Environment.NewLine}value1,value2,value3{Environment.NewLine}value4,value5,value6{Environment.NewLine}";
                It("should write all objects", () =>
                {
                    Assert.AreEqual(expected, result);
                });
            });
        }

        [TestMethod]
        [TestCategory("Feature: Keys file is (re)created when does not exist or is older than extract file")]
        public void TestCsvFileHandler_CheckIfTargetIsOutOfDate_SourceFileDoesNotExist_ReturnsFalse()
        {
            Describe("When source file does not exist", () =>
            {
                //Arrange
                mockFileSystem.RemoveFile(@"path\to\extracts\someFile.txt");
                var csvFileHandler = new CsvFileHandler(folderConfig, mockStreamFactory.Object, mockFileSystem);

                //Act
                var result = csvFileHandler.CheckIfTargetIsOutOfDate("fileDoesNotExist.txt", "someFile_Keys.csv");

                //Assert
                It("should return false (target file cannot be generated)", () =>
                {
                    Assert.IsFalse(result);
                });
            });
        }

        [TestMethod]
        [TestCategory("Expected behaviour: CsvFileHandler")]
        public void TestCsvFileHandler_CheckIfTargetIsOutOfDate_TargetFileDoesNotExist_ReturnsTrue()
        {
            Describe("When target keys file does not exist", () =>
            {
                //Arrange
                mockFileSystem.RemoveFile(@"path\to\data\someFile_Keys.csv");
                var csvFileHandler = new CsvFileHandler(folderConfig, mockStreamFactory.Object, mockFileSystem);

                //Act
                var result = csvFileHandler.CheckIfTargetIsOutOfDate("someFile.txt", "fileDoesNotExist_Keys.csv");

                //Assert
                It("should return true (target file should be generated)", () =>
                {
                    Assert.IsTrue(result);
                });
            });
        }

        [TestMethod]
        [TestCategory("Feature: Keys file is (re)created when does not exist or is older than extract file")]
        public void TestCsvFileHandler_CheckIfTargetIsOutOfDate_TargetFileIsOlderThanSourceFile_ReturnsTrue()
        {
            Describe("When target keys file is older than source extracts file", () =>
            {
                //Arrange
                // Ensure keys file is dated later than extract file
                var file = new MockFile(mockFileSystem);
                var lastWriteTime = new DateTime(2010, 6, 4, 13, 26, 42);
                file.SetLastWriteTime(@"path\to\extracts\someFile.txt", lastWriteTime);
                file.SetLastWriteTime(@"path\to\data\someFile_Keys.csv", lastWriteTime.AddDays(-1));

                //Act
                var csvFileHandler = new CsvFileHandler(folderConfig, mockStreamFactory.Object, mockFileSystem);
                var result = csvFileHandler.CheckIfTargetIsOutOfDate("someFile.txt", "someFile_Keys.csv");

                //Assert
                It("should return true (indicates target file should be generated)", () =>
                {
                    Assert.IsTrue(result);
                });
            });
        }

        // Boundary conditions
        [TestMethod]
        [TestCategory("Boundary condition: CsvFileHandler")]
        public void TestCsvFileHandler_SourceFilePathIsCorrect()
        {
            Describe("When GetStreamReader() is called, check the parameter to the call", () =>
            {
                var streamReader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes("")));
                string pathCalled = "";
                mockStreamFactory
                    .Setup(x => x.GetStreamReader(Moq.It.IsAny<string>()))  // Assign the called path into a local variable
                    .Callback((string sourceFilePath) => { pathCalled = sourceFilePath; })
                    .Returns(streamReader);

                //Act
                var csvFileHandler = new CsvFileHandler(folderConfig, mockStreamFactory.Object, mockFileSystem);
                var result = csvFileHandler.ReadExtractFile("someFile.txt").ToList();
                streamReader.Dispose();

                //Assert
                It("should be called with the correct SourceFilePath", () =>
                {
                    Assert.AreEqual(@"path\to\extracts\someFile.txt", pathCalled);
                    //Alternatively
                    mockStreamFactory.Verify(x => x.GetStreamReader(@"path\to\extracts\someFile.txt"), Times.Once());  // Does not require .Callback() step
                });
            });
        }

        [TestMethod]
        [TestCategory("Boundary condition: CsvFileHandler")]
        public void TestCsvFileHandler_ExtractFileDoesNotExist_ShouldThrowException()
        {
            Describe("CsvFileHandler.ReadExtractFile method, when file requested does not exist", () =>
            {
                ItThrows("should throw an exception", typeof(FileLoadException), () =>
                {
                    var csvFileHandler = new CsvFileHandler(folderConfig, mockStreamFactory.Object, mockFileSystem);
                    var result = csvFileHandler.ReadExtractFile("fileDoesNotExist.txt");
                    var enumerated = result.ToList();
                });
            });
        }

        [TestMethod]
        [TestCategory("Boundary condition: CsvFileHandler")]
        public void TestCsvFileHandler_KeysFileDoesNotExist_ShouldThrowException()
        {
            Describe("CsvFileHandler.ReadKeysFile method, when file requested does not exist", () =>
            {
                ItThrows("should throw an exception", typeof(FileLoadException), () =>
                {
                    var csvFileHandler = new CsvFileHandler(folderConfig, mockStreamFactory.Object, mockFileSystem);
                    var result = csvFileHandler.ReadKeysFile("fileDoesNotExist_Keys.csv");
                    var enumerated = result.ToList();
                });
            });
        }

        // Integration Tests
        [TestMethod]
        [TestCategory("Integration: TestCsvFileHandler")]
        public void IntegrationTestCsvFileHandler_ConfirmUsesSystemIO()
        {
            Describe("CsvFileHandler.ReadCsv() integration test", () =>
            {
                // Arrange
                folderConfig.ExtractsFolder = @"..\..\";
                var csvFileHandler = new CsvFileHandler(folderConfig);

                // Act
                var result = csvFileHandler.ReadExtractFile(@"data\TestDataForCsvFileHandlerIntegrationTest.csv");
                var rows = result.ToList();
                var headers = string.Join(",", rows[0]);
                var row1 = string.Join(",", rows[1]);
                var row2 = string.Join(",", rows[2]);

                // Assert
                It("should read the actual file", () =>
                {
                    Assert.AreEqual("header1,header2,header3", headers);
                    Assert.AreEqual("value1,value2,value3", row1);
                    Assert.AreEqual("value4,value5,value6", row2);
                });
            });
        }

    }
}
