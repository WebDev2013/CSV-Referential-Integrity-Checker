using System;
using System.Collections.Generic;

using RIChecker.Interfaces;

namespace RIChecker.Model
{
    public class SchemaMultiFile : ISchema, IKeyHashsetProviderRole
    {
        public string Name { get; set; }
        public string SchemaDescription { get { return string.Join(" || ", SourceSchemas); } }
        public Func<List<string>, object> LineParserExpression { get; set; }
        public IFileParser FileParser { get; set; }

        public List<ISchema> SourceSchemas { get; set; }

        public SchemaMultiFile(string name, List<ISchema> sourceSchemas)
        {
            Name = name;
            SourceSchemas = sourceSchemas;
        }

        public List<ISchema> GetSourceSchemas()
        {
            return SourceSchemas;
        }
    }
}
