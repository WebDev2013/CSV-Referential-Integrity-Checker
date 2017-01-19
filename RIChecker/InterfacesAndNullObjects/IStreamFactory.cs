using System.IO;

namespace RIChecker.Interfaces
{
    public interface IStreamFactory
    {
        StreamReader GetStreamReader(string sourceFilePath);
        StreamWriter GetStreamWriter(string targetFilePath);
    }
}