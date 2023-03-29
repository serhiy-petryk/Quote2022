using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Newtonsoft.Json;

namespace Data.Actions.Polygon
{
    public static class PolygonMinuteLoader
    {
        public static void Start()
        {
            Logger.AddMessage($"Started");

            var api = CsUtils.GetApiKeys("polygon.io")[1];
            var folder = $@"E:\Quote\WebData\Minute\Polygon\Data\20230327\";

            var symbolAndDates = new List<Tuple<string, DateTime,DateTime>>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 150;
                cmd.CommandText = "select symbol, min(Date) MinDate, max(Date) MaxDate from dbQ2023..Daypolygon where [Close]*Volume>5000000 group by symbol";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        symbolAndDates.Add(new Tuple<string, DateTime, DateTime>((string)rdr["Symbol"], (DateTime)rdr["MinDate"], (DateTime)rdr["MaxDate"]));
            }

            var cnt = 0;
            foreach (var item in symbolAndDates)
            {
                Logger.AddMessage($"Downloaded {cnt++} tickers from {symbolAndDates.Count}");

                var currentDate = item.Item2;
                while (currentDate <= item.Item3)
                {
                    var endDate = currentDate.AddMonths(3);
                    if (endDate > DateTime.Today)
                        endDate = DateTime.Today.AddDays(-1);

                    var jsonFileName = $"{folder}pMin_{item.Item1}_{currentDate:yyyyMMdd}.json";
                    var zipFileName = Path.ChangeExtension(jsonFileName, ".zip");
                    var urlTicker = item.Item1.Replace("+", "");
                    var url =
                        $"https://api.polygon.io/v2/aggs/ticker/{urlTicker}/range/1/minute/{currentDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}?adjusted=false&sort=asc&limit=50000&apiKey={api}";
                    if (!File.Exists(zipFileName))
                    {
                        Download.DownloadPage(url, jsonFileName);
                        if (File.Exists(jsonFileName))
                        {
                            var zipFileName2 = Helpers.ZipUtils.ZipFile(jsonFileName);
                            if (File.Exists(zipFileName2))
                                File.Delete(jsonFileName);
                        }
                        else
                        {
                            // ! error
                        }
                    }

                    currentDate = endDate;
                }
            }

            Logger.AddMessage($"!Finished. ");//Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static int ParseAndSaveToDbAllFiles()
        {
            var folder = @"E:\Quote\WebData\Daily\Polygon\Data";
            var files = Directory.GetFiles(folder, "*.zip");
            var itemCnt = 0;
            var fileCnt = 0;
            foreach (var file in files)
            {
                Logger.AddMessage($"Parsed {fileCnt++} files from {files.Length}");
                itemCnt += ParseAndSaveToDb(file);
            }

            Logger.AddMessage($"Finished! Parsed {fileCnt++} files from {files.Length}");
            return itemCnt;
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var oo = JsonConvert.DeserializeObject<cRoot>(entry.GetContentOfZipEntry());
                    if (oo.status != "OK" || oo.count != oo.queryCount || oo.count != oo.resultsCount ||
                        oo.adjusted)
                        throw new Exception($"Bad file: {zipFileName}");
                    var a1 = oo.results[0].Date;
                    itemCount += oo.results.Length;

                    foreach (var item in oo.results)
                        if (item.T.Any(char.IsLower))
                            item.T = item.T + "+";

                    // Save data to buffer table of data server
                    Helpers.DbUtils.SaveToDbTable(oo.results, "dbQ2023..DayPolygon", "Symbol", "Date", "Open",
                        "High", "Low", "Close", "Volume", "WeightedVolume", "TradeCount");
                }

            return itemCount;
        }

        #region ===========  Json SubClasses  ===========

        private class cRoot
        {
            public int queryCount;
            public int resultsCount;
            public bool adjusted;
            public cItem[] results;
            public string status;
            public string request_id;
            public int count;
        }

        private class cItem
        {
            private static DateTime webDateTime = new DateTime(1970, 1,1);
            public string T;
            public long v;
            public float vw;
            public float o;
            public float h;
            public float l;
            public float c;
            public long t;
            public int n;

            public string Symbol => T;
            public DateTime Date => webDateTime.AddMilliseconds(t).Date;

            public float Open => o;
            public float High => h;
            public float Low => l;
            public float Close => c;
            public long Volume => v;
            public float WeightedVolume => vw;
            public int TradeCount => n;
        }
        #endregion
    }
}
