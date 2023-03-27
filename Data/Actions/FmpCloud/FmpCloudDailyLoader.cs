using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Newtonsoft.Json;

namespace Data.Actions.FmpCloud
{
    public static class FmpCloudDailyLoader
    {
        public static void Start()
        {
            Logger.AddMessage($"Started");

            var api = CsUtils.GetApiKeys("fmpcloud.io")[3];
            var folder = $@"E:\Quote\WebData\Daily\FmpCloud\Data\";

            var dates = new List<DateTime>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 150;
                cmd.CommandText = "SELECT date from TradingDays WHERE date between '2019-01-01' and '2023-03-31' order by date desc";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        dates.Add((DateTime) rdr["Date"]);
            }

            var cnt = 0;
            foreach (var date in dates)
            {
                Logger.AddMessage($"Downloaded {cnt++} files from {dates.Count}");
                var jsonFileName = folder + $"DayFmpCloud_{date:yyyyMMdd}.json";
                var zipFileName = Path.ChangeExtension(jsonFileName, ".zip");
                var url = $@"https://fmpcloud.io/api/v3/batch-request-end-of-day-prices?date={date:yyyy-MM-dd}&apikey={api}";
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
            var folder = @"E:\Quote\WebData\Daily\FmpCloud\Data";
            var files = Directory.GetFiles(folder, "*12.zip");
            var itemCnt = 0;
            var fileCnt = 0;
            foreach (var file in files)
            {
                Logger.AddMessage($"Parsed {fileCnt++} files from {files.Length}");
                itemCnt += ParseAndSaveToDb(file);
            }

            return itemCnt;
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries)
                    if (entry.Length > 0)
                    {
                        var items = JsonConvert.DeserializeObject<cItem[]>(entry.GetContentOfZipEntry()).ToArray();
                        itemCount += items.Length;

                        // Save data to buffer table of data server
                        Helpers.DbUtils.SaveToDbTable(items.Where(a=>CheckSymbol(a.symbol)), "dbQuote2023..DayFmpCloud", "symbol", "date", "open",
                            "high", "low", "close", "volume", "adjClose");
                    }

            return itemCount;
        }

        private static bool CheckSymbol(string symbol)
        {
            var chars = symbol.ToArray();
            for (var k = 0; k < chars.Length; k++)
            {
                var c = chars[k];
                if (char.IsLower(c) || char.IsDigit(c) || c == '=' || c == '&') return false;
            }

            var i = symbol.IndexOf('.');
            if (i > 0)
            {
                if (i < symbol.Length - 2) // like XX.TO, 2 or more chars after dot
                    return false;
                else
                    return !(symbol.EndsWith(".L") || symbol.EndsWith(".F") || symbol.EndsWith(".V"));
            }
            return !symbol.EndsWith("-USD");
        }

        #region ===========  Json SubClasses  ===========

        private class cItem
        {
            public string symbol;
            public DateTime date;
            public float open;
            public float high;
            public float low;
            public float close;
            public float adjClose;
            public long volume;

        }
        #endregion

    }
}
