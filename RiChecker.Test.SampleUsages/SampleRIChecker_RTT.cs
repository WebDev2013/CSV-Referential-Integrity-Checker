using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RIChecker.Interfaces;
using RIChecker.Model;
using RIChecker.Implementation;

namespace TestRiChecker
{
    [TestClass]
    public class SampleRIChecker_RTT
    {
        [TestMethod]
        [TestCategory("Sample")]
        public void SampleRiChecker_RTT()
        {
            Func<List<string>, object> mpiParser = r => new { UHPI = r[0] };
            Func<List<string>, object> waitinglistParser = r => new { UHPI = r[0], WaitinglistNo = r[1], AdmissionNo = r[2], TreatmentPathwayId = r[3] };
            Func<List<string>, object> rttParser = r => new { UHPI = r[0], TreatmentPathwayId = r[1] };

            Func<dynamic, string> mpiKeySelector = x => x.UHPI;
            Func<dynamic, string> treatmentPathwayKeySelector = x => $"{x.UHPI}:{x.TreatmentPathwayId}";
            
            var schemas = new Schemas
            {
                new Schema("MPI", mpiParser),
                new Schema("IPWaitingLists", waitinglistParser),
                new Schema("OPWaitingLists", waitinglistParser),
                new SchemaMultiFile("AllWaitingLists",
                    sourceSchemas: new List<ISchema> {
                        new Schema("IPWaitingLists", waitinglistParser),
                        new Schema("OPWaitingLists", waitinglistParser)
                }),
                new Schema("RTTStatus", rttParser)
            };

            var relations = new Relations();
            relations.Add("Referral To Treat (RTT) Status records vs Master Patient Record (MPI)",
                parentSchema: schemas["MPI"],
                relationKeySelector: mpiKeySelector,
                keyDescriptor: "UHPI",
                childSchemas: new [] {
                    schemas["RTTStatus"]
                }
            );
            relations.Add("Referral To Treat (RTT) Status records vs all waiting lists",
                parentSchema: schemas["AllWaitingLists"],
                relationKeySelector: treatmentPathwayKeySelector,
                keyDescriptor: "UHPI:TreatmentPathwayId",
                childSchemas: new [] {
                    schemas["RTTStatus"]
                }
            );

            var config = new Config(mffSet: "RTT", loadNo: "1.01")
            {
                ExtractsFolder = @"..\..\data",
                KeysFolder = @"..\..\data",
                ResultsFolder = @"..\..\results"
            };
            var riChecker = new RIChecker.RiChecker(relations, config);
            riChecker.Check();

        }
    }
}
