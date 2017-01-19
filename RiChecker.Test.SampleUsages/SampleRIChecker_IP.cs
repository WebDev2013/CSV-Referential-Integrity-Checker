using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RIChecker.Model;
using RIChecker.Implementation;

namespace TestRiChecker
{
    [TestClass]
    public class SampleRIChecker_IP
    {
        [TestMethod]
        [TestCategory("Sample")]
        public void SampleRiChecker_IP()
        {
            Func<List<string>, object> mpiParser = r => new { UHPI = r[0] };
            Func<List<string>, object> inpatientsParser = r => new { UHPI = r[0], AdmissionNo = r[1] };
            Func<List<string>, object> transactionParser = r => new { UHPI = r[0], AdmissionNo = r[1], TransactionId = r[2] };
            Func<List<string>, object> waitingListParser = r => new { UHPI = r[0], WaitinglistNo = r[1], AdmissionNo = r[2], TreatmentPathwayId = r[3] };
            Func<List<string>, object> waitingListOfferParser = r => new { UHPI = r[0], WaitinglistNo = r[1] };

            Func<dynamic, string> mpiKeySelector = x => x.UHPI;
            Func<dynamic, string> admissionKeySelector = x => $"{x.UHPI}:{x.AdmissionNo}";   
            Func<dynamic, string> transactionKeySelector = x => $"{x.UHPI}:{x.TransactionId}";
            Func<dynamic, string> waitingListKeySelector = x => $"{x.UHPI}:{x.WaitinglistNo}";

            var schemas = new Schemas
            {
                new Schema("MPI", mpiParser),
                new Schema("Inpatients", inpatientsParser),
                new Schema("IPTransactions", transactionParser),
                new Schema("IPProcedures", transactionParser),
                new Schema("IPDiagnoses", transactionParser),
                new Schema("IPWaitingLists", waitingListParser),
                new Schema("IPWaitingListTCI", waitingListParser),
                new Schema("IPWaitingListOffers", waitingListOfferParser),
                new Schema("IPWaitingListNotAvailable", waitingListOfferParser)
            };

            var relations = new Relations();
            relations.Add( "Inpatient related records vs Patient record",
                parentSchema: schemas["MPI"], 
                relationKeySelector: mpiKeySelector, 
                keyDescriptor: "UHPI",
                childSchemas: new []
                {
                    schemas["Inpatients"],
                    schemas["IPTransactions"],
                    schemas["IPProcedures"],
                    schemas["IPDiagnoses"],
                    schemas["IPWaitingLists"],
                    schemas["IPWaitingListTCI"]
                }
            );
            relations.Add( "Transactions associated with admission",
                parentSchema: schemas["Inpatients"],
                relationKeySelector: admissionKeySelector,
                keyDescriptor: "AdmissionNo",
                childSchemas: new []
                {
                    schemas["IPTransactions"],
                    schemas["IPProcedures"],
                    schemas["IPDiagnoses"]
                }
            );
            relations.Add( "Waiting lists that have been admitted",
                parentSchema: schemas["Inpatients"],
                relationKeySelector: admissionKeySelector,
                keyDescriptor: "AdmissionNo",
                childSchemas: new []
                {
                    schemas["IPWaitingLists"],
                    schemas["IPWaitingListTCI"]
                },
                childFilter: x => !string.IsNullOrWhiteSpace(x.AdmissionNo)
            );
            relations.Add("Transactions associated records",
                parentSchema: schemas["IPTransactions"],
                relationKeySelector: transactionKeySelector,
                keyDescriptor: "TransactionId",
                childSchemas: new []
                {
                    schemas["IPProcedures"],
                    schemas["IPDiagnoses"]
                }
            );
            relations.Add("Waiting list associated records",
                parentSchema: schemas["IPWaitingLists"],
                relationKeySelector: waitingListKeySelector,
                keyDescriptor: "WaitinglistNo",
                childSchemas: new []
                {
                    schemas["IPWaitingListTCI"],
                    schemas["IPWaitingListOffers"],
                    schemas["IPWaitingListNotAvailable"]
                }
            );

            var config = new Config(mffSet: "IP", loadNo: "1.01")
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
