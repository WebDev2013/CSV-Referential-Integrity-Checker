using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIChecker.Interfaces
{
    public interface IKeyFactory
    {
        IEnumerable<object> GetKeys(string mff, Func<dynamic, bool> filter = null);
    }
}
