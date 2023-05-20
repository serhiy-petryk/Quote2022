using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.Actions.WebArchive.Eoddata
{
    public class WA_EoddataSymbolsLoader
    {
        private const string UrlTemplate = @"https://web.archive.org/cdx/search/cdx?url=eoddata.com/stockquote/{0}/&matchType=prefix&limit=200000";
        private const string BaseFolder = @"E:\Quote\WebArchive\Eoddata\Symbols";
        private static string[] Exchanges = new[] { "AMEX", "NASDAQ", "NYSE", "OTCBB" };

        public static void Start()
        {
            Logger.AddMessage($"Started");

            // Download web archive list of url
            var dataFolder = BaseFolder + $@"\WA_Eoddata_Symbols_{CsUtils.GetTimeStamp().Item2}\";
            /*foreach (var exchange in Exchanges)
            {
                Logger.AddMessage($"Download web archive list of url for {exchange}");
                var url = string.Format(UrlTemplate, exchange);
                // var filename = string.Format(listFileTemplate, exchange);
                var filename = $@"{dataFolder}UrlList\{exchange}.txt";
                Download.DownloadPage(url, filename);
            }*/

            foreach (var exchange in Exchanges)
            {
                var filename = $@"{dataFolder}UrlList\{exchange}.txt";
                var items = File.ReadAllLines(filename).Select(a => new JsonItem(a)).Where(a => a.Type == "text/html" && a.Status == 200).ToArray();
                var cnt = 0;
                Parallel.ForEach(items, new ParallelOptions { MaxDegreeOfParallelism = 8 }, item =>
                {
                    Logger.AddMessage($"Downloaded {cnt++} from {items.Length} pages for {exchange}");
                    var url = $"https://web.archive.org/web/{item.TimeStamp}/{item.Url}";
                    var fn = $@"{dataFolder}{exchange}\{exchange}_{item.Symbol}_{item.TimeStamp}.html";
                    if (!File.Exists(fn))
                    {
                        Download.DownloadToFile(url, fn);
                        if (!File.Exists(fn))
                        {

                        }
                    }
                });
            }

            Logger.AddMessage($"Finished");
        }

        public static void ParseAndSaveToDb(string zipFileName)
        {
            var ss = Path.GetFileNameWithoutExtension(zipFileName).Split('_');
            var exchange = ss[ss.Length - 2];

            DbUtils.ExecuteSql($"DELETE from dbQ2023..HSymbolsEoddataDetails WHERE Exchange='{exchange}' AND [Source]='quote'");

            var items = new Dictionary<Tuple<string, DateTime>, DbItem>();
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0 && a.FullName.EndsWith(".html", StringComparison.InvariantCultureIgnoreCase)))
                {
                    var content = entry.GetContentOfZipEntry();
                    var item = new DbItem(exchange, entry.LastWriteTime.DateTime, content);
                    if (item.IsBad)
                    {

                        var i11 = content.IndexOf("Page Not Found (Error:404)", StringComparison.InvariantCulture);
                        if (i11 > 0)
                            continue;
                        throw new Exception($"Check file {entry.FullName} in {zipFileName}");
                    }
                    else
                    {
                        var key = Tuple.Create(item.Symbol, item.TimeStamp);
                        if (!items.ContainsKey(key))
                            items.Add(key, item);
                    }

                    //if (items.Count > 100)
                    //  break;
                }

            // var aa = items.Where(a=>a.Symbol == "WLFC").ToList();
            // Save data to buffer table of data server
            DbUtils.ClearAndSaveToDbTable(items.Values, "dbQ2023..HSymbolsEoddataDetails", "Symbol", "TimeStamp", "Source",
                "Exchange", "Name", "Date", "Last", "Open", "High", "Low", "Volume");
        }

        private static string GetCellContent(string cell)
        {
            var s = System.Net.WebUtility.HtmlDecode(cell).Replace("</b>", "").Replace("<b>", "").Replace("</font>", "").Trim();
            if (s.EndsWith("]"))
            {
                var i1 = s.LastIndexOf("[", StringComparison.InvariantCulture);
                s = s.Substring(0, i1);
            }

            var i2 = s.LastIndexOf(">", StringComparison.InvariantCulture);
            var s1 = s.Substring(i2 + 1).Trim();
            return string.IsNullOrEmpty(s1) ? null : s1;
        }

        #region =========  SubClasses  ===========

        private class DbItem
        {
            public string Symbol;
            public DateTime Date;
            public string Source => "quote";
            public string Exchange;
            public string Name;
            public float Open;
            public float High;
            public float Low;
            public float Last;
            public long Volume;
            public DateTime TimeStamp;

            public bool IsBad;

            public DbItem(string exchange, DateTime timeStamp, string content)
            {
                Exchange = exchange;
                TimeStamp = timeStamp;

                var i1 = content.IndexOf(">LAST:<", StringComparison.InvariantCulture);
                if (i1 == -1)
                {
                    IsBad = true;
                    return;
                }

                var i22 = content.LastIndexOf("<table ", i1, StringComparison.InvariantCulture);
                var i32 = content.IndexOf("</table>", i22, StringComparison.InvariantCulture);
                var rows2 = content.Substring(i22 + 7, i32 - i22 - 7).Trim().Split(new[] { "</tr>" }, StringSplitOptions.RemoveEmptyEntries);

                var i21 = content.LastIndexOf("<table ", i22 - 7, StringComparison.InvariantCulture);
                var i31 = content.IndexOf("</table>", i21, StringComparison.InvariantCulture);
                var rows1 = content.Substring(i21 + 7, i31 - i21 - 7).Trim().Split(new[] { "</tr>" }, StringSplitOptions.RemoveEmptyEntries);

                var cells = rows1[0].Trim().Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                Symbol = GetCellContent(cells[0]);
                Name = GetCellContent(cells[1]);
                var sDate = GetCellContent(cells[2]);
                Date = DateTime.Parse(sDate, CultureInfo.InvariantCulture);

                cells = rows2[0].Trim().Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                Last = float.Parse(GetCellContent(cells[0]), NumberStyles.Any, CultureInfo.InvariantCulture);
                Open = float.Parse(GetCellContent(cells[2]), NumberStyles.Any, CultureInfo.InvariantCulture);
                High = float.Parse(GetCellContent(cells[3]), NumberStyles.Any, CultureInfo.InvariantCulture);
                Volume = long.Parse(GetCellContent(cells[5]), NumberStyles.Any, CultureInfo.InvariantCulture);

                cells = rows2[1].Trim().Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                Low = float.Parse(GetCellContent(cells[2]), NumberStyles.Any, CultureInfo.InvariantCulture);
            }
        }

        private class JsonItem
        {
            public string Url;
            public string TimeStamp;
            public string Type;
            public int Status;
            public string Symbol => Path.GetFileNameWithoutExtension(Url.Split('?')[0]).ToUpper();

            public JsonItem(string line)
            {
                var ss = line.Split(' ');
                TimeStamp = ss[1];
                Url = ss[2];
                Type = ss[3];
                Status = int.Parse(ss[4]);
            }
        }
        #endregion
    }
}
