using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RIChecker.Model;
using RIChecker.Implementation;
using RIChecker.Interfaces;

namespace TestRiChecker
{
    public class MockRiReporter : IRiReporter
    {
        public void OnComplete(List<RiResult> Results) {}
        public void OnInit(int relationsCount) {}
        public void OnNextItem() {}
        public void WriteMessages(List<string> messages) { }
        public void WriteMessage(string message) {}
    }
}
