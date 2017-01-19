using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RIChecker.Model;
using RIChecker.Interfaces;
using RIChecker.Implementation;

namespace TestRiChecker
{
    [TestClass]
    public class SampleRIChecker_MAT
    {
        [TestMethod]
        [TestCategory("Sample")]
        public void SampleRiChecker_MAT()
        {
            Func<List<string>, object> mpiParser = r => new { UHPI = r[0] };
            Func<List<string>, object> matParser = r => new { UHPI = r[0], PregnancyId = r[1] };

            Func<dynamic, string> mpiKeySelector = x => x.UHPI;
            Func<dynamic, string> matKeySelector = x => $"{x.UHPI}:{x.PregnancyId}";   

            var schemas = new Schemas
            {
                new Schema("MPI", mpiParser),
                new Schema("MATSummary", mpiParser),
                new Schema("MATCurrentPregnancy", matParser),
                new Schema("MATPreviousPregnancies", matParser),
                new SchemaMultiFile("AllPregnancies", 
                    sourceSchemas: new List<ISchema> {
                        new Schema("MATCurrentPregnancy", matParser),
                        new Schema("MATPreviousPregnancies", matParser)
                }),
                new Schema("MATPreviousBabies", matParser),
                new Schema("MATAntenatalComplications", matParser),
                new Schema("MATDeliveryComplications", matParser),
                new Schema("MATWaitingLists", matParser)
            };

            var relations = new Relations();
            relations.Add("Maternity records vs Patient record",
                parentSchema: schemas["MPI"],
                relationKeySelector: mpiKeySelector,
                keyDescriptor: "UHPI",
                childSchemas: new [] 
                {
                    schemas["MATSummary"],
                    schemas["MATCurrentPregnancy"],
                    schemas["MATPreviousPregnancies"],
                    schemas["MATPreviousBabies"],
                    schemas["MATAntenatalComplications"],
                    schemas["MATDeliveryComplications"],
                    schemas["MATWaitingLists"],
                }
            );
            relations.Add("Previous pregnancy associated records",
                parentSchema: schemas["MATPreviousPregnancies"],
                relationKeySelector: matKeySelector,
                keyDescriptor: "UHPI:PregnancyId",
                childSchemas: new []
                {
                    schemas["MATPreviousBabies"],
                    schemas["MATDeliveryComplications"],
                    schemas["MATAntenatalComplications"],
                }
            );
            relations.Add("Antenatal complications for all pregnancy types",
                parentSchema: schemas["AllPregnancies"],
                relationKeySelector: matKeySelector,
                keyDescriptor: "UHPI:PregnancyId",
                childSchemas: new []
                {
                    schemas["MATAntenatalComplications"],
                }
            );

            var config = new Config(mffSet: "MAT", loadNo: "1.01")
            {
                ExtractsFolder = @"..\..\data",
                KeysFolder = @"..\..\data",
                ResultsFolder = @"..\..\results"
            };
            var riChecker = new RIChecker.RiChecker(relations, config);
            riChecker.Check();
            var results = riChecker.Results;
            Assert.AreEqual(78, results[9].TotalOrphanCount);
        }
    }
}
