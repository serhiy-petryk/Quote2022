using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Actions.Polygon;
using Data.Helpers;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Data.Actions.Polygon2003
{
    public static class PolygonDailyLoader2003
    {
        private const string Folder = @"E:\Quote\WebData\Daily\Polygon2003\Data\";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            // Define the dates to download
            var dates = new List<DateTime>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 150;
                /*cmd.CommandText = "select a.Date from (select date from dbPolygon2003..TradingDays " +
                                  "where date >= isnull((select min(date) from dbPolygon2003..DayPolygon), '2003-09-01')) a " +
                                  "left join dbPolygon2003..DayPolygon b on a.Date = b.Date where b.Date is null order by 1";*/
                cmd.CommandText = "select a.Date from dbPolygon2003..TradingDays a "+
                                  "left join (select distinct date from dbPolygon2003..DayPolygon) b on a.Date = b.Date " +
                                  "where b.Date is null and a.Date >= '2003-09-10'";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        dates.Add((DateTime)rdr["Date"]);
            }

            // Download data
            var cnt = 0;
            var itemCount = 0;
            var filesCount = 0;
            var filesSize = 0;
            foreach (var date in dates)
            {
                filesCount++;
                Logger.AddMessage($"Downloaded {cnt++} files from {dates.Count}");
                var zipFileName = Folder + $"DayPolygon_{date:yyyyMMdd}.zip";
                var url = $@"https://api.polygon.io/v2/aggs/grouped/locale/us/market/stocks/{date:yyyy-MM-dd}?adjusted=false&apiKey={PolygonCommon.GetApiKey2003()}";
                if (!File.Exists(zipFileName))
                {
                    var o = Download.DownloadToString(url);
                    if (o is Exception ex)
                        throw new Exception($"PolygonDailyLoader: Error while download from {url}. Error message: {ex.Message}");

                    var entry = new VirtualFileEntry($"DayPolygon_{date:yyyyMMdd}.json", (string)o);
                    ZipUtils.ZipVirtualFileEntries(zipFileName, new[] {entry});
                }

                itemCount += ParseAndSaveToDb(zipFileName);
                filesSize += CsUtils.GetFileSizeInKB(zipFileName);
            }

            // Logger.AddMessage($"Refresh summary data (~10 minutes)");
            // DbUtils.RunProcedure("dbQ2023..pUpdateDayPolygon");

            Logger.AddMessage($"!Finished. Loaded quotes into DayPolygon table. Quotes: {itemCount:N0}. Number of files: {filesCount}. Size of files: {filesSize:N0}KB");
        }

        public static void ParseAndSaveToDbAllFiles()
        {
            var folder = @"E:\Quote\WebData\Daily\Polygon2003\Data";
            var files = Directory.GetFiles(folder, "*.zip");
            var itemCnt = 0;
            var fileCnt = 0;
            foreach (var file in files)
            {
                Logger.AddMessage($"Parsed {fileCnt++} files from {files.Length}");
                itemCnt += ParseAndSaveToDb(file);
            }

            Logger.AddMessage($"Refresh summary data");
            DbUtils.RunProcedure("dbPolygon2003..pUpdateDayPolygon");

            Logger.AddMessage($"Finished! Parsed {fileCnt++} files from {files.Length}");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            Logger.AddMessage($"Parsed and saved to database {zipFileName}");

            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var oo = JsonConvert.DeserializeObject<cRoot>(entry.GetContentOfZipEntry());
                    if (oo.status != "OK" || oo.count != oo.queryCount || oo.count != oo.resultsCount ||
                        oo.adjusted)
                        throw new Exception($"Bad file: {zipFileName}");
                    
                    if (oo.resultsCount == 0) continue; // No data

                    var a1 = oo.results[0].Date;
                    itemCount += oo.results.Length;

                    // Save data to buffer table of data server
                    DbUtils.SaveToDbTable(oo.results, "dbPolygon2003..DayPolygon", "Symbol", "Date", "Open", "High",
                        "Low", "Close", "Volume", "WeightedVolume", "TradeCount");
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
            public string T;
            public long v;
            public float vw;
            public float o;
            public float h;
            public float l;
            public float c;
            public long t;
            public int n;

            public string Symbol => PolygonCommon.GetMyTicker(T);
            public DateTime Date => CsUtils.GetEstDateTimeFromUnixSeconds(t / 1000);
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
