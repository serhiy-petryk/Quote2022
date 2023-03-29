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
    public static class PolygonDailyLoader
    {
        private const string Folder = @"E:\Quote\WebData\Daily\Polygon\Data\";
        private static readonly string ApiKey = CsUtils.GetApiKeys("polygon.io")[1];

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var dates = new List<DateTime>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 150;
                cmd.CommandText = "SELECT date from TradingDays WHERE date between '2018-03-27' and '2023-03-31' order by date desc";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        dates.Add((DateTime) rdr["Date"]);
            }

            var cnt = 0;
            foreach (var date in dates)
            {
                Logger.AddMessage($"Downloaded {cnt++} files from {dates.Count}");
                var jsonFileName = Folder + $"DayPolygon_{date:yyyyMMdd}.json";
                var zipFileName = Path.ChangeExtension(jsonFileName, ".zip");
                var url = $@"https://api.polygon.io/v2/aggs/grouped/locale/us/market/stocks/{date:yyyy-MM-dd}?adjusted=false&apiKey={ApiKey}";
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
