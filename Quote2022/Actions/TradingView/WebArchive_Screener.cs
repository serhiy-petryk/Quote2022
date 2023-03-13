using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Quote2022.Actions.TradingView
{
    public static class WebArchive_Screener
    {
        #region =============  Download Html Data  ===============
        public static void DownloadHtmlData(Action<string> showStatusAction)
        {
            var url = @"https://scanner.tradingview.com/america/scan";
            //            var filenameTemplate =$"E:\\Quote\\WebArchive\\Symbols\\Eoddata\\{exchange}\\{exchange}_{letter}_{{0}}.htm";
            var filenameTemplate = @"E:\Quote\WebArchive\Screener\TradingView\America_Scan\Html\TvScreener_{0}.htm";

            var sourceUrl = System.Net.WebUtility.UrlEncode(url);

            var requestUrl = @"https://web.archive.org/__wb/sparkline?output=json&url=" + sourceUrl + @"&collection=web";
            var yearsJson = Actions.Download.GetResponse(requestUrl);
            var ooYearList = JsonConvert.DeserializeObject<cYearList>(yearsJson);

            var years = ooYearList.status.Keys.OrderBy(a => a).ToArray();
            foreach (var year in years)
            {
                requestUrl = $"https://web.archive.org/__wb/calendarcaptures/2?url={sourceUrl}&date={year}&groupby=day";
                var daysJson = Actions.Download.GetResponse(requestUrl);
                var ooDayList = JsonConvert.DeserializeObject<cDayList>(daysJson.Replace("\"-\"", "200"));

                foreach (var day in ooDayList.items)
                {
                    if (day[1] == 200)
                    {
                        var timeStamp = $"{year}{day[0]:0000}";
                        requestUrl = $"https://web.archive.org/__wb/calendarcaptures/2?url={sourceUrl}&date={timeStamp}";
                        var dayItemsJson = Actions.Download.GetResponse(requestUrl);
                        var dayItems = JsonConvert.DeserializeObject<cDayList>(dayItemsJson.Replace("\"-\"", "200"));

                        foreach (var dayItem in dayItems.items)
                        {
                            if (dayItem[1] == 200)
                            {
                                var id = $"{timeStamp}{dayItem[0]:000000}";
                                var filename = string.Format(filenameTemplate, id);
                                if (!File.Exists(filename))
                                {
                                    requestUrl = $"https://web.archive.org/web/{id}/{sourceUrl}";

                                    showStatusAction($"TradingView.WebArchive_Screener.DownloadData. Download to {Path.GetFileName(filename)}");
                                    Actions.Download.DownloadPage(requestUrl, filename);
                                    if (File.Exists(filename))
                                    {
                                        var fileTime = DateTime.ParseExact($"{id}", "yyyyMMddHHmmss",
                                            CultureInfo.InvariantCulture);
                                        File.SetCreationTime(filename, fileTime);
                                        File.SetLastWriteTime(filename, fileTime);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            showStatusAction($"TradingView.WebArchive_Screener finished for {Path.GetFileName(filenameTemplate)}");
        }

        public class cYearList
        {
            public Dictionary<string, object> status;
            public string first_ts;
            public string last_ts;
        }
        public class cDayList
        {
            public int[][] items;
        }
        #endregion

        #region =============  Download Json Data  ===============
        public static void DownloadJsonData(Action<string> showStatusAction)
        {
            var sourceFolder = @"E:\Quote\WebArchive\Screener\TradingView\America_Scan\Html\";
            var destinationFolder = @"E:\Quote\WebArchive\Screener\TradingView\America_Scan\Json\";

            showStatusAction($"TradingView.WebArchive_Screener.DownloadJsonData. Prepare urls and file names");

            var files = Directory.GetFiles(sourceFolder, "*.htm", SearchOption.AllDirectories);
            var urlsAndFilenames = new List<Tuple<string,string>>();
            foreach (var file in files)
            {
                var s = File.ReadAllText(file);
                var i1 = s.IndexOf("id=\"playback\"", StringComparison.InvariantCultureIgnoreCase);
                var i2 = s.IndexOf("src=\"", i1 + 14, StringComparison.InvariantCultureIgnoreCase);
                var i3 = s.IndexOf("\"", i2 + 6, StringComparison.InvariantCultureIgnoreCase);
                var url = s.Substring(i2 + 5, i3 - i2 - 5);
                var filename = destinationFolder + Path.GetFileNameWithoutExtension(file) + ".json";
                urlsAndFilenames.Add(new Tuple<string, string>(url, filename));
            }

            var cnt = 0;
            foreach (var urlAndFilename in urlsAndFilenames)
            {
                cnt++;

                if (cnt%10 == 0)
                    showStatusAction($"TradingView.WebArchive_Screener.DownloadJsonData. {cnt} from {urlsAndFilenames.Count} items are ready");

                if (!File.Exists(urlAndFilename.Item2))
                    Download.DownloadPage(urlAndFilename.Item1, urlAndFilename.Item2);
            }
            showStatusAction($"TradingView.WebArchive_Screener.DownloadJsonData finished");
        }

        #endregion

        #region =============  Parse Data  ===============
        public class cRoot
        {
            public int totalCount;
            public cData[] data;
        }

        public class cData
        {
            public string s;
            public object[] d;
        }

        public class DbItem
        {
            public string Symbol;
            public string Exchange;
            public string Name;
            public string Type;
            public string Subtype;
            public string Sector;
            public DateTime TimeStamp=DateTime.MinValue;

            public DbItem(string key)
            {
                var ss = key.Split(':');
                Symbol = ss[1];
                Exchange = ss[0];
            }
        }

        // A-name, B-type, C-subType, D-Sector, S-symbol, X-exchange
        private static string[] _masks = new[]
        {
            "", "SnnnnnnnnnDASBCsnnbn", "sSnnnnnnnnnDABCsnnbn", "sSnnnnnnnnnDASBCsnnbn", new string('n', 49),
            new string('n', 82), new string('n', 47), new string('n', 48), new string('n', 81),
            new string('n', 27) + "SNEsnn", new string('n', 26) + "DsnnSA", "sSnnnnnnnnnDABCsnnbnss",
            new string('n', 27) + "DnnSA", "sSnnnnnnnnnnDABCsnnbnss", new string('n', 8), "SAsB", "AsB",
            new string('n', 31) + "DsnnSA"
        };

        public static void ParseData(Action<string> showStatusAction)
        {
            var masks = _masks.GroupBy(a=>a.Length).ToDictionary(a => a.Key, a => a.ToArray());
            var dbItems = new Dictionary<string, DbItem>();

            var folder = @"E:\Quote\WebArchive\Screener\TradingView\America_Scan\json\";
            var files = Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories);
            var cnt = 0;
            var symbols = new Dictionary<string, int>();
            foreach (var file in files)
            {
                cnt++;
                if (cnt % 10 == 0)
                    showStatusAction($"TradingView.WebArchive_Screener.ParseData. Processed {cnt} files from {files.Length}");

                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var timeStamp = DateTime.ParseExact(ss[1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                var oo = JsonConvert.DeserializeObject<cRoot>(File.ReadAllText(file));
                foreach (var o in oo.data)
                {
                    if (!symbols.ContainsKey(o.s))
                        symbols.Add(o.s, 0);
                    symbols[o.s]++;

                    var mm = masks[o.d.Length];
                    string mask = null;
                    for (var k1 = 0; k1 < mm.Length; k1++)
                    {
                        var flag = true;
                        string name = null;
                        string type = null;
                        string subtype = null;
                        string sector = null;
                        for (var k2 = 0; k2 < o.d.Length; k2++)
                        {
                            switch (mm[k1][k2])
                            {
                                case 'b':
                                    flag = Equals(o.d[k2], "true") || Equals(o.d[k2], "false");
                                    break;
                                case 'n':
                                    flag = o.d[k2] is long || o.d[k2] is double || Equals(o.d[k2], null);
                                    break;
                                case 's':
                                    flag = o.d[k2] is string || Equals(o.d[k2], null);
                                    break;
                                case 'S':
                                    flag = o.s.EndsWith(":" + o.d[k2]);
                                    break;
                                case 'X':
                                    flag = o.s.StartsWith(o.d[k2] + ":");
                                    break;
                                case 'A':
                                    flag = o.d[k2] is string;
                                    name = (o.d[k2] ?? "").ToString();
                                    break;
                                case 'B':
                                    flag = o.d[k2] is string;
                                    type = (o.d[k2] ?? "").ToString();
                                    break;
                                case 'C':
                                    flag = o.d[k2] is string;
                                    subtype = (o.d[k2] ?? "").ToString();
                                    break;
                                case 'D':
                                    flag = o.d[k2] is string;
                                    sector = (o.d[k2] ?? "").ToString();
                                    break;
                            }

                            if (!flag)
                                break;
                        }

                        if (flag)
                        {
                            mask = mm[k1];
                            if (!Equals(name, null) || !Equals(type, null) || !Equals(subtype, null) ||
                                !Equals(sector, null))
                            {
                                if (!dbItems.ContainsKey(o.s))
                                    dbItems.Add(o.s, new DbItem(o.s));

                                var dbItem = dbItems[o.s];
                                if (timeStamp>dbItem.TimeStamp)dbItem.TimeStamp = timeStamp;
                                if (!string.IsNullOrEmpty(name)) dbItem.Name = name;
                                if (!string.IsNullOrEmpty(type)) dbItem.Type = type;
                                if (!string.IsNullOrEmpty(subtype)) dbItem.Subtype = subtype;
                                if (!string.IsNullOrEmpty(sector)) dbItem.Sector = sector;
                            }
                            break;
                        }
                    }

                    if (mask == null)
                    {
                        throw new Exception(
                            $"Define mask for values: {string.Join(",", o.d.Select(a => a == null ? "" : a.ToString()))}");
                    }
                }
            }

            showStatusAction($"TradingView.WebArchive_Screener.ParseData. Saving data to database ...");
            Actions.SaveToDb.SaveToDbTable(dbItems.Values, "dbQuote2023..HScreenerTradingView", "Symbol", "Exchange",
                "Name", "Type", "Subtype", "Sector", "TimeStamp");

            showStatusAction($"TradingView.WebArchive_Screener.ParseData finished");
        }
        #endregion

    }
}
