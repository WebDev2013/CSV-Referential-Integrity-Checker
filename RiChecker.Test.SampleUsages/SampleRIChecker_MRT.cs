using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RIChecker.Model;
using RIChecker.Implementation;

namespace TestRiChecker
{
    [TestClass]
    public class SampleRIChecker_MRT
    {
        [TestMethod]
        [TestCategory("Sample")]
        public void SampleRiChecker_MRT()
        {
            Func<List<string>, object> mpiParser = r => new { UHPI = r[0] };
            Func<List<string>, object> mrtParser = r => new { UHPI = r[0], MedicalRecordNo = r[1], MedicalRecordType = r[2] };

            Func<dynamic, string> mpiKeySelector = x => x.UHPI;
            Func<dynamic, string> mrtKeySelector = x => $"{x.UHPI}:{x.MedicalRecordNo}";
            
            var schemas = new Schemas
            {
                new Schema("MPI", mpiParser),
                new Schema("MRT", mrtParser),
                new Schema("MRTVolumes", mrtParser),
                new Schema("MRTMovements", mrtParser)
            };

            var relations = new Relations();
            var allSchemasExceptMPI = schemas.Skip(1).Select(x=> x.Value).ToArray();
            relations.Add("Medical Records vs Master Patient Record (MPI)",
                parentSchema: schemas["MPI"],
                relationKeySelector: mpiKeySelector,
                keyDescriptor: "UHPI",
                childSchemas: allSchemasExceptMPI
            );
            relations.Add("MRT associated records",
                parentSchema: schemas["MRT"],
                relationKeySelector: mpiKeySelector,
                keyDescriptor: "UHPI",
                childSchemas: new[]
                {
                    schemas["MRTVolumes"],
                    schemas["MRTMovements"]
                }
            );

            var config = new Config(mffSet: "MRT", loadNo: "1.01")
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
