using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Quote2022.Actions.Nasdaq;

namespace Quote2022.Actions
{
    public static class Github
    {
        private static string _rootUrl = "https://github.com/rreichel3/US-Stock-Symbols/";
        public static void NasdaqScreener(Action<string> ShowStatus)
        {
            var commits = new List<Tuple<DateTime, string>>();
            //GetCommitList(commits, ShowStatus);
            //DownloadFiles(commits, ShowStatus);
            ParseAndSaveFiles(ShowStatus);

            ShowStatus($"Github.NasdaqScreener finished");
        }

        private static void ParseAndSaveFiles(Action<string> ShowStatus)
        {
            var folder = @"E:\Quote\WebArchive\Screener\NasdaqGithub\Json\";
            var files = Directory.GetFiles(folder, "*.json").OrderBy(a=> SortKey(a)).ToArray();

            var deserializerSettings = new JsonSerializerSettings
            {
                Culture = System.Globalization.CultureInfo.InvariantCulture
            };
            foreach (var file in files)
            {
                ShowStatus($"Github.NasdaqScreener. Parse and save file {Path.GetFileNameWithoutExtension(file)}");

                var ss = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file)).Split('_');
                var exchange = ss[1].ToUpper();
                var timeStamp = DateTime.ParseExact(ss[2], "yyyyMMdd", CultureInfo.InvariantCulture);
                var content = File.ReadAllText(file);
                var items = JsonConvert.DeserializeObject<ScreenerNasdaq_Parse.cStockRow[]>(content, deserializerSettings);
                foreach (var item in items)
                {
                    item.Exchange = exchange;
                    item.TimeStamp = timeStamp;
                }

                SaveToDb.ClearAndSaveToDbTable(items, "Bfr_ScreenerNasdaqStock2", "symbol", "Exchange", "Name",
                    "LastSale", "Volume", "netChange", "Change", "MarketCap", "Country", "ipoYear", "Sector",
                    "Industry", "TimeStamp");

                SaveToDb.RunProcedure("pUpdateScreenerNasdaqStock2",
                    new Dictionary<string, object> {{"@Exchange", exchange}, {"@Date", timeStamp}});
            }

            string SortKey(string filename)
            {
                var ss = Path.GetFileNameWithoutExtension(filename).Split('_');
                return ss[2] + ss[1];
            }
        }

        private static void GetCommitList(List<Tuple<DateTime, string>> commits, Action<string> ShowStatus)
        {
            // !!! use request: https://api.github.com/repos/rreichel3/US-Stock-Symbols/commits
            ShowStatus($"Github.NasdaqScreener. Prepare commit list");

            var url = _rootUrl + "commits/main";
            var filenameTemplate = @"E:\Quote\WebArchive\Screener\NasdaqGithub\Html\NG_{0}.html";
            var cnt = 0;
            while (url != null)
            {
                var filename = string.Format(filenameTemplate, cnt.ToString());
                if (!File.Exists(filename))
                    Download.DownloadPage(url, filename);

                var content = File.ReadAllText(filename);
                url = ParsePage(content, commits);
                cnt++;
            }

        }

        private static void DownloadFiles(List<Tuple<DateTime, string>> commits, Action<string> ShowStatus)
        {
            var exchanges = new[] { "Amex", "Nasdaq", "Nyse" };
            var jsonTemplate = @"E:\Quote\WebArchive\Screener\NasdaqGithub\Json\NG_{1}_{0}.json";
            var urlTemplate = "https://raw.githubusercontent.com/rreichel3/US-Stock-Symbols/{0}/{1}/{1}_full_tickers.json";
            foreach (var item in commits)
            {
                ShowStatus($"Github.NasdaqScreener. Download file for {item.Item1:yyyy-MM-dd}");

                foreach (var exchange in exchanges)
                {
                    if (item.Item1 < new DateTime(2021, 2, 1)) continue;

                    var timeStamp = item.Item1.ToString("yyyyMMdd");
                    var filename = string.Format(jsonTemplate, timeStamp, exchange);
                    if (!File.Exists(filename))
                    {
                        var url = string.Format(urlTemplate, item.Item2, exchange.ToLower());
                        Download.DownloadPage(url, filename, true);
                    }
                }
            }
        }

        private static string ParsePage(string content, List<Tuple<DateTime, string>> items)
        {
            var ss = content.Split(new[] {"class=\"TimelineItem-body\""}, StringSplitOptions.RemoveEmptyEntries);
            for (var k1 = 1; k1 < ss.Length; k1++)
            {
                var i1 = ss[k1].IndexOf("</ol>", StringComparison.InvariantCulture);
                var s = ss[k1].Substring(0, i1);

                i1 = s.IndexOf(">Commits on ", StringComparison.InvariantCulture);
                var i2 = s.IndexOf("<", i1+12, StringComparison.InvariantCulture);
                var sDate = s.Substring(i1 + 12, i2 - i1 - 12).Trim();
                var date = DateTime.Parse(sDate, CultureInfo.InvariantCulture);

                i1 = s.IndexOf("/rreichel3/US-Stock-Symbols/tree/", StringComparison.InvariantCulture);
                i2 = s.IndexOf("\"", i1 + 30, StringComparison.InvariantCulture);
                var id = s.Substring(i1 + 33, i2 - i1 - 33);
                items.Add(new Tuple<DateTime, string>(date, id));
            }

            var j2 = ss[ss.Length - 1].IndexOf(">Older</a>", StringComparison.InvariantCulture);
            if (j2 == -1) return null; // last page

            var j1 = ss[ss.Length - 1].LastIndexOf("href=\"", j2, StringComparison.InvariantCulture);
            j2 = ss[ss.Length - 1].IndexOf("\"", j1 + 6, StringComparison.InvariantCulture);
            var nextUrl = ss[ss.Length - 1].Substring(j1 + 6, j2 - j1 - 6);
            var nUrl = System.Net.WebUtility.HtmlDecode(nextUrl);
            return nUrl;
        }
    }
}
