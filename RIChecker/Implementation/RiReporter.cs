using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using LINQPad;
using RIChecker.Model;
using RIChecker.Interfaces;

namespace RIChecker.Implementation
{
    public class RiReporter: IRiReporter
    {
        private Config config;

        private int relationsCount { get; set; }
        private int countDone { get; set; }

        private object progressBar;

        public RiReporter(Config config)
        {
            this.config = config;
        }

        public void OnInit(int relationsCount)
        {
            this.relationsCount = relationsCount;
            progressBar = new Util.ProgressBar(Config.InitMessage);
            progressBar.Dump();
        }

        public void OnNextItem()
        {
            countDone++;
            Util.Progress = countDone / relationsCount * 100;
        }

        public void WriteMessages(List<string> messages)
        {
            messages.ForEach(m=> WriteMessage(m));
        }

        public void WriteMessage(string message)
        {
            message.Dump();
        }

        public void OnComplete(List<RiResult> Results)
        {
            var title = config.Title;
            Results.Dump(title);
            var totalOrphans = Results.Select(r => r.OrphansSample.Count()).Sum();
            if (totalOrphans > 0 || Results.Select(r=> r.ErrorMessage).Any())
                Results.Dump2Html(title, config.ResultsFolder);
        }
    }
}
