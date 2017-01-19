using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using RIChecker.Interfaces;

namespace RIChecker.Implementation
{
    public class StreamFactory : IStreamFactory
    {
        public StreamReader GetStreamReader(string sourceFilePath)
        {
            return new StreamReader(new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }

        public StreamWriter GetStreamWriter(string targetFilePath)
        {
            return new StreamWriter(targetFilePath);
        }
    }
}
