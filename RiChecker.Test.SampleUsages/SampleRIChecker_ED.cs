using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RIChecker.Model;
using RIChecker.Implementation;

namespace TestRiChecker
{
    [TestClass]
    public class SampleRIChecker_ED
    {
        [TestMethod]
        [TestCategory("Sample")]
        public void SampleRiChecker_ED()
        {
            Func<List<string>, object> mpiParser = r => new { UHPI = r[0] };
            Func<List<string>, object> edAttendParser = r => new { UHPI = r[0], EpisodeNo = r[1] };

            Func<dynamic, string> mpiKeySelector = x => x.UHPI;
            Func<dynamic, string> episodeKeySelector = x => $"{x.UHPI}:{x.EpisodeNo}";   // Strengthen the grandchild key check

            var schemas = new Schemas
            {
                new Schema("MPI", mpiParser ),
                new Schema("EDAttendances", edAttendParser ),
                new Schema("EDDischargeDiagnoses", edAttendParser ),
                new Schema("EDAttendClinicians", edAttendParser ),
                new Schema("EDDischargeInvestigations", edAttendParser ),
                new Schema("EDDischargeTreatments", edAttendParser ),
            };

            var relations = new Relations();
            relations.Add("Emergency related records vs Patient record",
                parentSchema: schemas["MPI"],
                relationKeySelector: mpiKeySelector,
                keyDescriptor: "UHPI",
                childSchemas: new []
                {
                    schemas["EDAttendances"],
                    schemas["EDAttendClinicians"],
                    schemas["EDDischargeInvestigations"],
                    schemas["EDDischargeDiagnoses"],
                    schemas["EDDischargeTreatments"]
                }
            );
            relations.Add("Emergency episodes associated records",
                parentSchema: schemas["EDAttendances"],
                relationKeySelector: episodeKeySelector,
                keyDescriptor: "EpisodeNo",
                childSchemas: new []
                {
                    schemas["EDAttendClinicians"],
                    schemas["EDDischargeInvestigations"],
                    schemas["EDDischargeDiagnoses"],
                    schemas["EDDischargeTreatments"]
                }
            );

            var config = new Config(mffSet: "ED", loadNo: "1.01")
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
