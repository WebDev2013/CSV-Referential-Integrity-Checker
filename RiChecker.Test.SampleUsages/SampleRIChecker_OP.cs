using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RIChecker.Model;
using RIChecker.Implementation;

namespace TestRiChecker
{
    [TestClass]
    public class SampleRIChecker_OP
    {
        [TestMethod]
        [TestCategory("Sample")]
        public void SampleRiChecker_OP()
        {
            Func<List<string>, object> mpiParser = r => new { UHPI = r[0] };
            Func<List<string>, object> appointmentsParser = r => new { UHPI = r[0], AppointmentId = r[1] };
            Func<List<string>, object> waitinglistParser = r => new { UHPI = r[0], WaitinglistNo = r[1], AppointmentId = r[2], TreatmentPathwayId = r[3] };

            Func<dynamic, string> mpiKeySelector = x => x.UHPI;
            Func<dynamic, string> appointmentsKeySelector = x => $"{x.UHPI}:{x.AppointmentId}";

            Func<dynamic, bool> appointmentsKeyFilter = x => x.AppointmentId != "";

            var schemas = new Schemas
            {
                new Schema("MPI", mpiParser),
                new Schema("OPAppointments", appointmentsParser),
                new Schema("OPAppOutcomes", appointmentsParser),
                new Schema("OPAppProcedures", appointmentsParser),
                new Schema("OPWaitingLists", waitinglistParser)
            };

            var relations = new Relations();
            relations.Add( "Outpatient related records vs Patient record",
                parentSchema: schemas["MPI"], 
                relationKeySelector: mpiKeySelector, 
                keyDescriptor: "UHPI",
                childSchemas: new []
                {
                    schemas["OPAppointments"],
                    schemas["OPAppOutcomes"],
                    schemas["OPAppProcedures"],
                    schemas["OPWaitingLists"]
                }
            );
            relations.Add( "Oppointment associated records",
                parentSchema: schemas["OPAppointments"],
                relationKeySelector: appointmentsKeySelector,
                keyDescriptor: "AppointmentId",
                childSchemas: new []
                {
                    schemas["OPAppOutcomes"],
                    schemas["OPAppProcedures"]
                }
            );
            relations.Add( "Waiting lists that have been converted to appointment",
                parentSchema: schemas["OPAppointments"],
                relationKeySelector: appointmentsKeySelector,
                keyDescriptor: "AppointmentId",
                childSchemas: new []
                {
                    schemas["OPWaitingLists"]
                },
                childFilter: x => !string.IsNullOrWhiteSpace(x.AppointmentId)
            );

            var config = new Config(mffSet: "OP", loadNo: "1.01")
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
