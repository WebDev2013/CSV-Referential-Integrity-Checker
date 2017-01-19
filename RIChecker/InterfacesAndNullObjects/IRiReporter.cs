using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RIChecker.Model;

namespace RIChecker.Interfaces
{
    public interface IRiReporter
    {
        void OnInit(int relationsCount);
        void OnNextItem();
        void WriteMessages(List<string> messages);
        void WriteMessage(string message);
        void OnComplete(List<RiResult> Results);
    }
}
