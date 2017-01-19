using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RIChecker.Model;
using RIChecker.Implementation;

namespace TestRiChecker
{
    [TestClass]
    public class SampleRIChecker_MPI
    {
        [TestMethod]
        [TestCategory("Sample")]
        public void SampleRiChecker_MPI()
        {
            Func<List<string>, object> mpiParser = r => new { UHPI = r[0] };
            Func<dynamic, string> mpiKeySelector = x => x.UHPI;

            var schemas = new Schemas
            {
                new Schema("MPI", mpiParser),
                new Schema("MPIOtherNumbers", mpiParser),
                new Schema("MPIAlias", mpiParser),
                new Schema("MPIOtherAddresses", mpiParser),
                new Schema("MPINOK", mpiParser),
                new Schema("MPIAlerts", mpiParser)
            };

            var relations = new Relations();
            var allChildren = schemas.Skip(1).Select(x=> x.Value).ToArray();
            relations.Add("MPI related records vs Master Patient Record (MPI)",
                parentSchema: schemas["MPI"],
                relationKeySelector: mpiKeySelector,
                keyDescriptor: "UHPI",
                childSchemas: allChildren
            );

            var config = new Config(mffSet: "MPI", loadNo: "1.01")
            {
                ExtractsFolder = @"..\..\data",
                KeysFolder = @"..\..\data",
                ResultsFolder = @"..\..\results"
            };
            var riChecker = new RIChecker.RiChecker(relations, config);
            riChecker.Check();

            var results = riChecker.Results;

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(0, results.Sum(x=> x.TotalOrphanCount));
        }
    }
}
