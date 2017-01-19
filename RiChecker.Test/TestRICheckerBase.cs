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
    public abstract class TestRiCheckerBase : Jasmine
    {
        internal Mock<IRiReporter> mockRiReporter;
        internal Config config;
        internal int timesEnumeratorCalled = 0;
        internal Mock<IFileParser> mockParser;

        internal IEnumerable<object> EnumeratorCallMonitor_ChildKeysList()
        {
            timesEnumeratorCalled += 1;
            yield return new { ParentKey = "ParentKey", ChildKey = "childKey1" };
            yield return new { ParentKey = "ParentKey", ChildKey = "childKey2" };
        }

        internal IEnumerable<object> EnumeratorCallMonitor_ParentKeysList()
        {
            timesEnumeratorCalled += 1;
            yield return new { ParentKey = "ParentKey" };
            yield return new { ParentKey = "ParentKey" };
        }

        [TestInitialize]
        public void testInit()
        {
            mockRiReporter = Builder_MockRiReporter.Build();
            config = new Config(mffSet: "TestSet", loadNo: "1.01");
            timesEnumeratorCalled = 0;
            mockParser = new Mock<IFileParser>();
        }
    }
}
