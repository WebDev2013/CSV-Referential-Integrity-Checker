using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIChecker.Interfaces
{
    public interface ICsvFileHandler
    {
        IEnumerable<IEnumerable<String>> ReadExtractFile(string filePath);
        IEnumerable<IEnumerable<String>> ReadKeysFile(string filePath);
        void WriteCsv(IEnumerable<object> list, string file, char delim = '\t');
        bool CheckIfTargetIsOutOfDate(string sourceFilePath, string targetFilePath);
    }
}
