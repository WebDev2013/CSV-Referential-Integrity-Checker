using System;
using System.Collections.Generic;

using RIChecker.Model;

namespace RIChecker.Interfaces
{
    public interface IFileParser
    {
        IEnumerable<string> Errors { get; }
        IEnumerable<object> Keys { get; }
        int LineCount { get; set; }
        IEnumerable<FileParseResult> ParseResults { get; set; }

        //void ParseAndTest(Func<List<string>, object> parser, string fileName, IEnumerable<IEnumerable<string>> fileData, System.Func<dynamic, bool> filter = null);
        void ParseAndTest(string fileName, IEnumerable<IEnumerable<string>> fileData, System.Func<dynamic, bool> filter = null);
        void Reset();
    }
}