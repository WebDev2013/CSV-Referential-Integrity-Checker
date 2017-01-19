using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RIChecker.Model;

namespace TestRiChecker.MocksAndBuilders
{
    public static class Builder_Schemas
    {
        public static Schemas BuildSingleSchema(Func<List<string>, object> parser)
        {
            return new Schemas
            {
                new Schema("someRecordType", parser)
            };
        }
    }
}
