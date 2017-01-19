using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RIChecker;
using RIChecker.Model;
using RIChecker.Implementation;

namespace TestRiChecker
{
    [TestClass]
    public class SampleRIChecker_MATSummary_Only
    {
        [TestMethod]
        [TestCategory("Sample")]
        public void SampleRIChecker_MATSummary_WithIncorrectParser_Throws()
        {
            Func<List<string>, object> mpiParser = r => new { UHPI = r[0] };
            Func<List<string>, object> matParser = r => new { UHPI = r[0], PregnancyId = r[1] };

            Func<dynamic, string> mpiKeySelector = x => x.UHPI;
            Func<dynamic, string> matKeySelector = x => $"{x.UHPI}:{x.PregnancyId}";   

            var schemas = new Schemas
            {
                new Schema("MPI", mpiParser),
                new Schema("MATSummary", matParser)
            };

            var relations = new Relations();
            relations.Add("Maternity records vs Patient record",
                parentSchema: schemas["MPI"],
                relationKeySelector: mpiKeySelector,
                keyDescriptor: "UHPI",
                childSchemas: new [] 
                {
                    schemas["MATSummary"],
                }
            );

            var config = new Config(mffSet: "MATSummary", loadNo: "1.01")
            {
                ExtractsFolder = @"..\..\data",
                KeysFolder = @"..\..\data",
                ResultsFolder = @"..\..\results"
            };
            var riChecker = new RiChecker(relations, config);
            riChecker.Check();
            var results = riChecker.Results;
            var expectedMessage = "Error in parsing file: 'MATSummary_Keys.csv', 1040 errors out of 1040 lines";
            Assert.AreEqual(expectedMessage, results[0].ErrorMessage);
        }
    }
}
