using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using RIChecker.Interfaces;

namespace RIChecker.Implementation
{
    public class Config : IConfig
    {
        public const string InitMessage = "Checking relations...";

        public string ExtractsFolder { get; set; }
        public string KeysFolder { get; set; }
        public string ResultsFolder { get; set; }

        public int SampleSize { get; set; } = 5;

        public string Title { get; set; }

        public Config(string mffSet, string loadNo)
        {
            var load = loadNo ?? $"Load: {DateTime.Today}";
            Title = $"{mffSet} RI checks for LoadNo: {load}";
        }

        public static Config FromSettings(string mffSet = null, string loadNo = null, string configSetRequested = null)
        {
            var configFilePath = @".\config.json";
            if (!File.Exists(configFilePath))
                return new Config(mffSet, loadNo);

            var json = File.ReadAllText(configFilePath);
            var jobject = JObject.Parse(json);

            var configSetToUse = configSetRequested ?? (string)jobject["ConfigSetToUse"];
            var configSet = jobject[configSetToUse];
            if (configSet == null)
                throw new Exception($"Config set name not found or is invalid: '{configSet}'");

            return new Config(mffSet, loadNo)
            {
                ExtractsFolder = (string)configSet["ExtractsFolder"],
                KeysFolder = (string)configSet["KeysFolder"],
                ResultsFolder = (string)configSet["ResultsFolder"],
                SampleSize = (int)(configSet["SampleSize"] ?? 0)
            };
        }
    }
}
