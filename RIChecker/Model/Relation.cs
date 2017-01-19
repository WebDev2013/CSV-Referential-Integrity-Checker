using System;

using RIChecker.Interfaces;

namespace RIChecker.Model
{
    public class Relation 
    {
        public ISchema ParentSchema { get; set; }
        public ISchema ChildSchema { get; set; }
        public Func<object, string> RelationKeySelector { get; set; }
        public Func<dynamic, bool> RelationChildFilter { get; set; }

        public string RelationDescriptor { get; set; }
        public string SchemasDescription { get { return $"{ChildSchema.SchemaDescription} vs {ParentSchema.SchemaDescription}"; } }
        public string KeyDescriptor { get; set; }

        public int ParentKeyCount { get; set; }
        public int ChildKeyCount { get; set; }

        public override string ToString()
        {
            return $"Description: '{RelationDescriptor}', Schemas: '{SchemasDescription}', Relation key: '{KeyDescriptor ?? "Not given"}'";
        }
    }
}
