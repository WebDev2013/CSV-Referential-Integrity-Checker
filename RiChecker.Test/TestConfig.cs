using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JasmineNET;
using JasmineExtensions;

using Moq;
using RIChecker.Interfaces;
using RIChecker.Implementation;

using System.IO.Abstractions.TestingHelpers;

namespace TestRiChecker
{
    [TestClass]
    public class TestConfig : JasmineExtended
    {
        [TestMethod]
        [TestCategory("Integration: Config")]
        [TestCategory("Feature: Config is initialized from config.json")]
        public void IntegrationTestConfig_ReadFromConfigJson()
        {
            Describe("Config is initialized from config.json", () =>
            {
                //Act
                var config = Config.FromSettings("TestMff", "1.01");

                //Assert
                It("should use the values in config.json", () =>
                {
                    Assert.AreEqual(@"path/to/production/extracts", config.ExtractsFolder);
                    Assert.AreEqual(@"path/to/production/keys", config.KeysFolder);
                    Assert.AreEqual(@"path/to/production/results", config.ResultsFolder);
                    Assert.AreEqual(5, config.SampleSize);
                });
            });
        }

        [TestMethod]
        [TestCategory("Integration: Config")]
        [TestCategory("Feature: Config set (Dev/Prod/Etc) to be used can be specified upon setup")]
        public void IntegrationTestConfig_CanChooseConfigSet()
        {
            Describe("Feature: Config set (Dev/Prod/Etc) to be used can be specified upon setup", () =>
            {
                //Act
                var config = Config.FromSettings("TestMff", "1.01", "Development");

                //Assert
                It("should use the override config set values in config.json", () =>
                {
                    Assert.AreEqual(@"path/to/development/extracts", config.ExtractsFolder);
                    Assert.AreEqual(@"path/to/development/keys", config.KeysFolder);
                    Assert.AreEqual(@"path/to/development/results", config.ResultsFolder);
                    Assert.AreEqual(0, config.SampleSize);
                });
            });
        }

        [TestMethod]
        [TestCategory("Integration: Config")]
        [TestCategory("Boundary condition: Config")]
        public void IntegrationTestConfig_InvalidConfigSet()
        {
            Describe("When config set override is invalid", () =>
            {
                ItThrows("should throw an exception", typeof(Exception), () =>
                {
                    var config = Config.FromSettings("TestMff", "1.01", "NotAConfigSet");
                });
            });
        }
    }
}
