using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using Data.Helpers;
using Newtonsoft.Json;

namespace Data.Actions.Polygon
{
    public static class PolygonSymbolsLoader
    {
        private const string StartUrlTemplate = "https://api.polygon.io/v3/reference/tickers?active=true&limit=1000";
        private const string UrlTemplate = "https://api.polygon.io/v3/reference/tickers?date={0}&active=true&limit=1000";
        private const string ZipFileNameTemplate = @"E:\Quote\WebData\Symbols\Polygon\Data\SymbolsPolygon_{0}.zip";

        public static bool DbCheck()
        {
            Logger.AddMessage($"Check symbols/dates in SymbolsPolygon table of database.");

            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 300;
                cmd.CommandText = "select top 1 c.Mindate, a.* from dbQ2023..DayPolygon a " +
                                  "left join dbQ2023..SymbolsPolygon b on a.Symbol = b.Symbol and a.Date between b.Date and isnull(b.[To], GetDate()) " +
                                  "inner join(select symbol, min(date) MinDate from dbQ2023..SymbolsPolygon group by symbol) c on a.Symbol = c.Symbol " +
                                  "where b.Symbol is null";
                var i = cmd.ExecuteScalar();

                if (!Equals(i, null))
                {
                    Debug.Print($"Please, check missing dates in SymbolsPolygon table of database:{Environment.NewLine}{cmd.CommandText}");
                    MessageBox.Show(@"Please, check missing dates in SymbolsPolygon table of database. See output window");
                    return false;
                }

                cmd.CommandText = "select top 1 * from dbQ2023..SymbolsPolygon a " +
                                  "inner join dbQ2023..SymbolsPolygon b on a.Symbol = b.Symbol and b.Date between a.Date and isnull(a.[To], GetDate()) " +
                                  "where a.Date<>b.Date";
                i = cmd.ExecuteScalar();

                if (!Equals(i, null))
                {
                    Debug.Print($"Please, check date duplicates in SymbolsPolygon table of database:{Environment.NewLine}{cmd.CommandText}");
                    MessageBox.Show(@"Please, check date duplicates in SymbolsPolygon table of database. See output window");
                    return false;
                }

                return true;
            }
        }

        public static void Start()
        {
            Logger.AddMessage($"Started");

            // Define list of dates
            var dates = new List<DateTime>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 300;
                cmd.CommandText = "select date from dbQuote2022..TradingDays where date>=(select min(date) from dbQ2023..DayPolygon) order by 1";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        dates.Add((DateTime)rdr["Date"]);
            }

            var itemCount = 0;
            var fileCount = 0;
            foreach (var date in dates)
            {
                var zipFileName = string.Format(ZipFileNameTemplate, date.ToString("yyyyMMdd"));
                var virtualFileEntries = new List<VirtualFileEntry>();
                if (!File.Exists(zipFileName))
                {
                    fileCount++;
                    var folder = zipFileName.Substring(0, zipFileName.Length - 4);
                    var url = string.Format(UrlTemplate, date.ToString("yyyy-MM-dd"));
                    var cnt = 0;
                    while (url != null)
                    {
                        url = url + "&apiKey=" + PolygonCommon.GetApiKey();
                        var filename = folder + $@"\SymbolsPolygon_{cnt:D2}_{date:yyyyMMdd}.json";
                        Logger.AddMessage($"Downloading {cnt} data chunk into {Path.GetFileName(filename)} for {date:yyyy-MM-dd}");

                        var o = Download.DownloadToString(url);
                        if (o is Exception ex)
                            throw new Exception($"PolygonSymbolsLoader: Error while download from {url}. Error message: {ex.Message}");

                        var entry = new VirtualFileEntry($@"SymbolsPolygon_{cnt:D2}_{date:yyyyMMdd}.json", (string) o);
                        virtualFileEntries.Add(entry);

                        var oo = JsonConvert.DeserializeObject<cRoot>((string)o);
                        if (oo.status != "OK")
                            throw new Exception("Wrong status of response!");

                        url = oo.next_url;
                        cnt++;
                    }

                    ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);
                    itemCount += ParseAndSaveToDb(zipFileName);
                }
            }

            Logger.AddMessage($"!Finished. Processed {fileCount} files with {itemCount:N0} items");
        }

        public static void ParseAllZip()
        {
            var folder = Path.GetDirectoryName(ZipFileNameTemplate);
            var files = Directory.GetFiles(folder, "*.zip").OrderBy(a => a).ToArray();
            foreach (var zipFileName in files)
            {
                var items = new List<cItem>();
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        var oo = JsonConvert.DeserializeObject<cRoot>(entry.GetContentOfZipEntry());

                        var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                        var date = DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMdd", CultureInfo.InstalledUICulture);
                        foreach (var item in oo.results)
                        {
                            item.Date = date;
                            item.TimeStamp = entry.LastWriteTime.DateTime;
                        }

                        items.AddRange(oo.results.Where(a => a.IsValidTicker && a.market == "stocks"));
                    }

                DbUtils.SaveToDbTable(items, "dbQ2023..SymbolsPolygonDetails", "Symbol", "Date", "primary_exchange",
                    "name", "type", "cik", "composite_figi", "share_class_figi", "last_updated_utc", "TimeStamp");
            }
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            Logger.AddMessage($"Parsing and saving to database: {Path.GetFileName(zipFileName)}");

            var items = new List<cItem>();
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var oo = JsonConvert.DeserializeObject<cRoot>(entry.GetContentOfZipEntry());

                    var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                    var date = DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMdd", CultureInfo.InstalledUICulture);
                    foreach (var item in oo.results)
                    {
                        item.Date = date;
                        item.TimeStamp = entry.LastWriteTime.DateTime;
                    }

                    items.AddRange(oo.results.Where(a => a.IsValidTicker && a.market == "stocks"));
                }

            DbUtils.ClearAndSaveToDbTable(items, "dbQ2023..Bfr_SymbolsPolygon", "Symbol", "Date", "primary_exchange",
                "name", "type", "cik", "composite_figi", "share_class_figi", "last_updated_utc", "TimeStamp");

            DbUtils.RunProcedure("dbQ2023..pUpdateSymbolsPolygon");

            return items.Count;
        }

        #region ===========  Json SubClasses  ===========

        private class cRoot
        {
            public int count;
            public string next_url;
            public string request_id;
            public cItem[] results;
            public string status;
        }
        private class cItem
        {
            public string ticker;
            public string name;
            public string market;
            public string locale;
            public string primary_exchange;
            public string type;
            public bool active;
            public string currency_name;
            public string cik;
            public string composite_figi;
            public string share_class_figi;
            public DateTime last_updated_utc;
            public DateTime delisted_utc;

            public bool IsValidTicker => !(ticker.StartsWith("X:", StringComparison.InvariantCultureIgnoreCase) ||
                                           ticker.StartsWith("C:", StringComparison.InvariantCultureIgnoreCase) ||
                                           ticker.StartsWith("I:", StringComparison.InvariantCultureIgnoreCase) ||
                                           PolygonCommon.IsTestTicker(Symbol));
            public string Symbol => PolygonCommon.GetMyTicker(ticker);
            public DateTime Date;
            public string Name => string.IsNullOrEmpty(name) ? null : name;
            public DateTime TimeStamp;
        }
        #endregion

    }
}
