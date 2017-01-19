using System;
using System.Collections.Generic;

namespace RIChecker.Interfaces
{
    public interface ISchema
    {
        string Name { get; set; }
        string SchemaDescription { get; }
        Func<List<string>, object> LineParserExpression { get; set; }
        IFileParser FileParser { get; set; }
        List<ISchema> GetSourceSchemas();
    }
}