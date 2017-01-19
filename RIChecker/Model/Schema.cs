using System;
using System.Collections.Generic;

using RIChecker.Interfaces;
using RIChecker.Implementation;

namespace RIChecker.Model
{
    public class Schema : ISchema, IKeyHashsetProviderRole
    {
        public string Name { get; set; }
        public string SchemaDescription { get { return Name; } }
        public Func<List<string>, object> LineParserExpression { get; set; }
        public IFileParser FileParser{ get; set; }

        //public Schema()
        //{
        //    FileParser = new FileParser(new LineParser(LineParserExpression));
        //}

        public Schema(string name, Func<List<string>, object> lineParser)
        {
            Name = name;
            LineParserExpression = lineParser;
            FileParser = new FileParser(new LineParser(LineParserExpression));
        }

        public List<ISchema> GetSourceSchemas()
        {
            return new List<ISchema> { this };
        }
    }
}
