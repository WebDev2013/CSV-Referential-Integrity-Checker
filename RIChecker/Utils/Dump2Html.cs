using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using LINQPad;

namespace RIChecker
{
    public static class Dump2HtmlClass
    {
        public static T Dump2Html<T>(this T o, string title)
        {
            return o.Dump2Html<T>(title, null);
        }

        public static T Dump2Html<T>(this T o, string title, string path, bool timestamped = true)
        {
            var timestamp = timestamped == true ? " (" + string.Format("{0:dd MMM yyyy - HH.mm}", DateTime.Now) + ")" : "";
            var titleClean = title.Replace(":", "=");
            var pathToWriteTo = path + @"\" + titleClean + timestamp + ".html";

            using (var writer = LINQPad.Util.CreateXhtmlWriter(true))
            {
                if (title != null)
                {
                    var titleHtml = Util.RawHtml("<div style='color:green; font-weight: bold;'>" + title + "</div>");
                    writer.Write(titleHtml);
                }
                writer.Write(o);
                File.WriteAllText(pathToWriteTo, writer.ToString());
            }
            return o;
        }
    }
}
