using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIChecker.Model
{
    public class FileParseResult
    {
        public bool Success { get; set; }
        public object ParsedObject { get; set; }
        public ParserException Exception { get; set; }

        public string ExceptionMessage
        {
            get
            {
                if (Exception == null)
                    return null;
                var inner = Exception.InnerException != null ? $", ({Exception.InnerException.Message})" : "";
                return Exception.Message + inner;
            }
        }
    }

    public class ParserException : Exception
    {
        public ParserException() : base() { }
        public ParserException(string message) : base(message) { }
        public ParserException(string message, Exception inner) : base(message, inner) { }
    }
}
