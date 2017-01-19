using System;
using System.Collections.Generic;

using RIChecker.Interfaces;

namespace RIChecker.Model
{
    public class Schemas : Dictionary<string, ISchema>
    {
        public void AddRange(List<ISchema> schemas)
        {
            schemas.ForEach(schema => this.Add(schema));
        }

        public void Add(ISchema schema)
        {
            if (this.ContainsKey(schema.Name))
                return;
            this.Add(schema.Name, schema);
        }
    }
}
