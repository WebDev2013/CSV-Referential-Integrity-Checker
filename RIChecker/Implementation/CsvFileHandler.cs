using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;

using RIChecker.Interfaces;

namespace RIChecker.Implementation
{
    public class CsvFileHandler : ICsvFileHandler
    {
        private Config folderConfig;
        private IStreamFactory streamFactory;
        private IFileSystem fileSystem;

        public CsvFileHandler(Config folderConfig, IStreamFactory streamFactory, IFileSystem fileSystem)
        {
            this.folderConfig = folderConfig;
            this.streamFactory = streamFactory;
            this.fileSystem = fileSystem;
        }

        public CsvFileHandler(Config folderConfig) :
            this(folderConfig, new StreamFactory(), new FileSystem())
        {
        }

        public IEnumerable<IEnumerable<string>> ReadExtractFile(string sourceFileName)
        {
            var sourceFilePath = fileSystem.Path.Combine(folderConfig.ExtractsFolder, sourceFileName);
            return ReadCsv(sourceFilePath);
        }

        public IEnumerable<IEnumerable<string>> ReadKeysFile(string sourceFileName)
        {
            var sourceFilePath = fileSystem.Path.Combine(folderConfig.KeysFolder, sourceFileName);
            return ReadCsv(sourceFilePath);
        }

        private IEnumerable<IEnumerable<string>> ReadCsv(string sourceFilePath, char delim = ',')
        {
            if (!fileSystem.File.Exists(sourceFilePath))
                throw new FileLoadException($"Requested keys file '{sourceFilePath}' does not exist");

            string line;
            var reader = streamFactory.GetStreamReader(sourceFilePath);
            while ((line = reader.ReadLine()) != null)
            {
                if (delim == '\n')
                {
                    yield return new List<String>();
                }
                else
                {
                    var data = line.Split(delim);
                    var currentLine = new List<String>(data);
                    yield return currentLine;
                }
            }
            reader.Dispose();
        }

        public void WriteCsv(IEnumerable<object> list, string targetFileName, char delim = '\t')
        {
            var targetFilePath = fileSystem.Path.Combine(folderConfig.KeysFolder, targetFileName);
            var writer = streamFactory.GetStreamWriter(targetFilePath);

            WriteHeaders(writer, list.First(), delim);
            WriteData(writer, list, delim);

            writer.Dispose();
        }

        private void WriteHeaders(StreamWriter writer, object first, char delim)
        {
            writer.WriteLine(ObjectProperties2Csv(first, delim));
        }

        private void WriteData(StreamWriter writer, IEnumerable<object> objects, char delim)
        {
            foreach (var o in objects) 
                writer.WriteLine(Object2Csv(o, delim));
        }

        public bool CheckIfTargetIsOutOfDate(string sourceFileName, string targetFileName)
        {
            var sourceFilePath = fileSystem.Path.Combine(folderConfig.ExtractsFolder, sourceFileName);
            if (!fileSystem.File.Exists(sourceFilePath))
                return false;

            var targetFilePath = fileSystem.Path.Combine(folderConfig.KeysFolder, targetFileName);
            if (!fileSystem.File.Exists(targetFilePath))
                return true;

            return fileSystem.File.GetLastWriteTime(sourceFilePath) > fileSystem.File.GetLastWriteTime(targetFilePath);
        }

        private string ObjectProperties2Csv(object o, char delim = '\t')
        {
            var result = "";
            foreach (PropertyInfo propertyInfo in o.GetType().GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    var Value = propertyInfo.Name;
                    result = result + Value + delim;
                }
            }
            result = result.Substring(0, result.Length - 1);
            return result;
        }

        private string Object2Csv(object o, char delim = '\t')
        {
            var result = "";
            foreach (PropertyInfo propertyInfo in o.GetType().GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    var Value = propertyInfo.GetValue(o, null);
                    result = result + Value + delim;
                }
            }
            result = result.Substring(0, result.Length - 1);
            return result;
        }


    }
}
