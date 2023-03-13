using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace Quote2022.Actions.TradingView
{
    public static class WebArchive_Profile
    {
        #region =============  Download Data  ===============
        public static void DownloadData(Action<string> showStatusAction)
        {
            var urlTemplate = "https://www.tradingview.com/symbols/{0}/";
            var filenameTemplate = @"E:\Quote\WebArchive\Screener\TradingView\Profiles\TvProfile_{0}_{1}.html";

            var ids = new Dictionary<string, object>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                // cmd.CommandText = "select * from SymbolsEoddata where (TvType is null or ((isnull(TvType,'') not in ('warrant','right','dr', 'structured') and TvSubtype is null))) and TradingViewSymbol is not null";
                cmd.CommandText = "select * from SymbolsEoddata where "+
                                  "(TvType is null or (isnull(TvType,'') not in ('warrant','right') and TvSector is null)) and TradingViewSymbol is not null";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        var id = (string)rdr["Exchange"] + "-" + (string)rdr["TradingViewSymbol"];
                        ids.Add(id, null);
                    }
            }

            foreach (var kvp in ids)
            {
                var exchangeAndSymbol = kvp.Key.Replace("-", "_").Replace(@"/","^");
                var url = string.Format(urlTemplate, kvp.Key);
                var sourceUrl = System.Net.WebUtility.UrlEncode(url);
                var requestUrl = @"https://web.archive.org/__wb/sparkline?output=json&url=" + sourceUrl + @"&collection=web";

                string yearsJson = null;
                while (yearsJson == null)
                {
                    yearsJson = Actions.Download.GetResponse(requestUrl);
                    if (yearsJson == null)
                        Thread.Sleep(30000);
                }

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
                                    var filename = string.Format(filenameTemplate, exchangeAndSymbol, id);
                                    if (!File.Exists(filename))
                                    {
                                        requestUrl = $"https://web.archive.org/web/{id}/{sourceUrl}";

                                        showStatusAction($"TradingView.WebArchive_Profile.DownloadData. Download to {Path.GetFileName(filename)}");
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

            }

            showStatusAction($"TradingView.WebArchive_Profile.DownloadData finished for {Path.GetFileName(filenameTemplate)}");
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
            public string pro_symbol;
            public string short_name;
            public string description;
            public string local_description;
            public string type;
            public string[] typespecs;

            public string Symbol;
            public string Exchange;
            public string Name;
            public string Type;
            public string Subtype;
            public string Sector;
            public string Industry;
            public DateTime TimeStamp = DateTime.MinValue;
        }

        public static void ParseData(Action<string> showStatusAction)
        {
            var dbItems = new Dictionary<string, DbItem>();

            var folder = @"E:\Quote\WebArchive\Screener\TradingView\Profiles\";
            var files = Directory.GetFiles(folder, "*.html", SearchOption.AllDirectories).OrderBy(a=>a).ToArray();
            var cnt = 0;
            var symbols = new Dictionary<string, int>();
            foreach (var file in files)
            {
                cnt++;
                if (cnt % 10 == 0)
                    showStatusAction($"TradingView.WebArchive_Profile.ParseData. Processed {cnt} files from {files.Length}");

                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var exchange = ss[1];
                var symbol = ss[2];
                if (symbol == "WBT")
                {

                }
                var timeStamp = DateTime.ParseExact(ss[3], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                if (timeStamp < new DateTime(2018, 1, 1))
                    continue;

                var s = File.ReadAllText(file);
                var i1 = s.IndexOf("window.initData.symbolInfo", StringComparison.InvariantCultureIgnoreCase);
                var i2 = s.IndexOf("{", i1 + 26, StringComparison.InvariantCultureIgnoreCase);
                var i3 = s.IndexOf("};", i2 + 1, StringComparison.InvariantCultureIgnoreCase);
                var json = s.Substring(i2, i3 - i2 + 1);
                var oo = JsonConvert.DeserializeObject<DbItem>(json);

                i1 = s.IndexOf(">Sector:</span>", i3, StringComparison.InvariantCultureIgnoreCase);
                if (i1 >0)
                {
                    i2 = s.IndexOf("</span>", i1 + 15, StringComparison.InvariantCultureIgnoreCase);
                    i1 = s.Substring(0, i2).LastIndexOf(">", StringComparison.InvariantCultureIgnoreCase);
                    var sector = System.Net.WebUtility.HtmlDecode(s.Substring(i1 + 1, i2 - i1 - 1));
                    oo.Sector = sector;
                }

                i1 = s.IndexOf(">Industry:</span>", i2, StringComparison.InvariantCultureIgnoreCase);
                if (i1 > 0)
                {
                    i2 = s.IndexOf("</span>", i1 + 15, StringComparison.InvariantCultureIgnoreCase);
                    i1 = s.Substring(0, i2).LastIndexOf(">", StringComparison.InvariantCultureIgnoreCase);
                    var industry = System.Net.WebUtility.HtmlDecode(s.Substring(i1 + 1, i2 - i1 - 1));
                    oo.Industry = industry;
                }

                if (oo.typespecs.Length == 2)
                {
                    if (oo.typespecs[0] == "fund")
                    {
                        oo.Type = string.IsNullOrEmpty(oo.typespecs[0]) ? null : oo.typespecs[0];
                        oo.Subtype = string.IsNullOrEmpty(oo.typespecs[1]) ? null : oo.typespecs[1];
                    }
                    else
                    {
                        oo.Type = string.IsNullOrEmpty(oo.typespecs[1]) ? null : oo.typespecs[1];
                        oo.Subtype = string.IsNullOrEmpty(oo.typespecs[0]) ? null : oo.typespecs[0];
                    }
                }
                else if (oo.typespecs.Length == 1)
                {
                    oo.Type = string.IsNullOrEmpty(oo.type) ? null : oo.type;
                    oo.Subtype = string.IsNullOrEmpty(oo.typespecs[0]) ? null : oo.typespecs[0];
                }
                else
                    throw new Exception("Check c# code");

                if (oo.Subtype == "dr")
                {
                    oo.Type = "dr";
                    oo.Subtype = null;
                }

                if (oo.Type == "bond" && oo.Subtype == "convertible") // Nasdaq:GECCM
                {
                    oo.Type = "stock";
                    oo.Subtype = "preferred";
                }

                oo.Symbol = symbol;
                oo.Exchange = exchange;
                oo.Name = oo.local_description;
                oo.TimeStamp = timeStamp;

                ss = oo.pro_symbol.Split(':');
                if (!Equals(oo.Exchange, ss[0]) || !Equals(oo.Symbol, ss[1]))
                    throw new Exception("Check exchange/symbol");

                if (dbItems.ContainsKey(oo.pro_symbol))
                {
                    var oldItem = dbItems[oo.pro_symbol];
                    if (string.IsNullOrEmpty(oo.Name) && !string.IsNullOrEmpty(oldItem.Name))
                        oo.Name = oldItem.Name;
                    if (string.IsNullOrEmpty(oo.Sector) && !string.IsNullOrEmpty(oldItem.Sector))
                        oo.Sector = oldItem.Sector;
                    if (string.IsNullOrEmpty(oo.Industry) && !string.IsNullOrEmpty(oldItem.Industry))
                        oo.Industry = oldItem.Industry;
                    dbItems[oo.pro_symbol] = oo;
                }
                else
                    dbItems.Add(oo.pro_symbol, oo);

            }

            showStatusAction($"TradingView.WebArchive_Profile.ParseData. Saving data to database ...");
            Actions.SaveToDb.ClearAndSaveToDbTable(dbItems.Values, "dbQuote2023..HProfileTradingView", "Symbol", "Exchange",
                "Name", "Type", "Subtype", "Sector", "Industry", "TimeStamp");

            showStatusAction($"TradingView.WebArchive_Profile.ParseData finished");
        }
        #endregion


    }
}
