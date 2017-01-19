using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIChecker.Model
{
    public class RiResult 
    {
        public string Description { get; set; }
        public int ParentCount { get; set; }
        public int ChildCount { get; set; }
        public int TotalOrphanCount { get; set; }
        public IEnumerable<object> OrphansSample { get; set; }
        public string ErrorMessage { get; set; }

        public RiResult()
        {
            OrphansSample = new List<object>();
        }
    }
}
