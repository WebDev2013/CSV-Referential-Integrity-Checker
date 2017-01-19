using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RIChecker.Model;

namespace RIChecker.Implementation
{
    public class LineParser
    {
        private Func<List<string>, object> parser;
        
        public LineParser(Func<List<string>, object> parser)
        {
            this.parser = parser;
        }

        public FileParseResult TestParseLine(IEnumerable<string> data, string sourceFileName, int line)
        {
            var result = new FileParseResult();
            try
            {
                result.ParsedObject = parser(data.ToList());
                result.Success = true;
            }
            catch (ArgumentOutOfRangeException e)
            {
                result.Success = false;
                result.Exception = new ParserException($"Malformed source file at line {line}, file: {sourceFileName}", e);
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Exception = new ParserException($"Error in source file at line {line}, file: {sourceFileName}", e);
            }
            return result;
        }

    }
}
