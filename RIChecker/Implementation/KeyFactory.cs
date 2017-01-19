using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using RIChecker.Model;
using RIChecker.Utils;
using RIChecker.Interfaces;

namespace RIChecker.Implementation
{
    public class KeyFactory : IKeyFactory
    {
        private Schemas schemas;
        private ICsvFileHandler csvFileHandler;
        private IRiReporter reporter;
        private IFileParser fileParser;

        public KeyFactory(Schemas schemas, IFileParser fileParser, ICsvFileHandler csvFileHandler, IRiReporter reporter = null)
        {
            if (schemas == null || !schemas.Any())
                throw new ArgumentNullException("schemas");
            if (fileParser == null)
                throw new ArgumentNullException("fileParser");
            if (csvFileHandler == null)
                throw new ArgumentNullException("csvFileHandler");
            if (reporter == null)
                reporter = new NullRiReporter();

            this.schemas = schemas;
            this.csvFileHandler = csvFileHandler;
            this.reporter = reporter;
            this.fileParser = fileParser;
        }
        public KeyFactory(Schemas schemas, ICsvFileHandler csvFileHandler, IRiReporter reporter = null)
        {
            if (schemas == null || !schemas.Any())
                throw new ArgumentNullException("schemas");
            if (csvFileHandler == null)
                throw new ArgumentNullException("csvFileHandler");
            if (reporter == null)
                reporter = new NullRiReporter();

            this.schemas = schemas;
            this.csvFileHandler = csvFileHandler;
            this.reporter = reporter;
            //this.fileParser = fileParser;
        }

        public IEnumerable<object> GetKeys(string mff, Func<dynamic, bool> filter = null)
        {
            if (string.IsNullOrWhiteSpace(mff))
                throw new ArgumentNullException("Error in Keys.GetKeys: parameter must not be null");
            if (!schemas.ContainsKey(mff))
                throw new Exception($"Error in Keys.GetKeys: No schema found for mff: '{mff}'");

            var extractFileName = mff + ".txt";
            var keysFileName = mff + "_Keys.csv";

            CreateKeysIfOutOfDate(extractFileName, keysFileName, mff);

            var fileParser = schemas[mff].FileParser;
            fileParser.Reset();
            try
            {
                var fileDataWithoutHeaders = csvFileHandler.ReadKeysFile(keysFileName).Skip(1);
                //fileParser.ParseAndTest(schemas[mff].LineParser, keysFileName, fileDataWithoutHeaders, filter);
                fileParser.ParseAndTest(keysFileName, fileDataWithoutHeaders, filter);
                var parseResults = fileParser.ParseResults.ToList();

                if (parseResults.Any(pr => !pr.Success))
                    ReportAndFail(parseResults, keysFileName);

                return parseResults
                    .Where(pr => pr.Success)
                    .AsList(pr => pr.ParsedObject);
            }
            catch (FileLoadException)
            {
                reporter.WriteMessage(string.Format($"Error in GetKeys(), file: {keysFileName} was not found"));
                throw;
            }
        }

        private void ReportAndFail(List<FileParseResult> results, string fileName)
        {
            results
                .Where(r => !r.Success)
                .AsList(r => r.ExceptionMessage)
                .ForEach(m => { reporter.WriteMessage(m); });
            throw new Exception($"Error in parsing file: '{fileName}', {results.Where(r => !r.Success).Count()} errors out of {results.Count} lines");
        }

        private void CreateKeysIfOutOfDate(string sourceFileName, string keysFileName, string mff)
        {
            if(csvFileHandler.CheckIfTargetIsOutOfDate(sourceFileName, keysFileName))
                CreateKeys(mff);
        }

        private void CreateKeys(string mff)
        {
            var sourceFileName = mff + ".txt";
            var targetFileName = mff + "_Keys.csv";

            reporter.WriteMessage(": ...creating keys");
            fileParser.Reset();
            try
            {
                var fileData = csvFileHandler.ReadExtractFile(sourceFileName);
                var fileDataWithoutHeaders = fileData.Skip(1);
                fileParser.ParseAndTest(sourceFileName, fileDataWithoutHeaders);

                csvFileHandler.WriteCsv(fileParser.ParseResults.Select(pr=> pr.ParsedObject), targetFileName, delim: ',');
            }
            catch (FileLoadException)
            {
                reporter.WriteMessage(string.Format($"Error in CreateKeys(), file: {sourceFileName} was not found"));
                throw;
            }
            catch (Exception e)
            {
                reporter.WriteMessage($"Error in Keys.CreateKeys, file: {sourceFileName}, {e.Message}");
            }
        }

    }
}
