using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RIChecker.Utils;
using RIChecker.Model;
using RIChecker.Interfaces;

namespace RIChecker.Implementation
{
    public class FileParser : IFileParser
    {
        public IEnumerable<FileParseResult> ParseResults { get; set; }
        public int LineCount { get; set; }
        public IEnumerable<object> Keys
        {
            get
            {
                return ParseResults
                    .Where(r => r.Success)
                    .Select(p => p.ParsedObject);
            }
        }
        public IEnumerable<string> Errors
        {
            get
            {
                return ParseResults
                    .Where(r => r.Exception != null)
                    .Select(p => $"{p.Exception.Message}, ({p.Exception.InnerException.Message})");
            }
        }

        private LineParser lineParser;

        public FileParser(LineParser lineParser)
        {
            this.lineParser = lineParser;
        }

        public void ParseAndTest(string fileName, IEnumerable<IEnumerable<string>> fileData, Func<dynamic, bool> filter = null)
        {
            ParseResults = new List<FileParseResult>();
            ParseResults = fileData
                .IncrementCounter(() => LineCount++)
                .Select(data => lineParser.TestParseLine(data, fileName, LineCount))
                .Where(r => filter != null ? filter(r.ParsedObject) : true);
        }

        public void Reset()
        {
            LineCount = 0;
            ParseResults = new List<FileParseResult>();
        }
    }
}
