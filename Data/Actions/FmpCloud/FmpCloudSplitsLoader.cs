using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Newtonsoft.Json;

namespace Data.Actions.FmpCloud
{
    public static class FmpCloudSplitsLoader
    {
        private const string UrlTemplate = "https://fmpcloud.io/api/v3/stock_split_calendar?from={0}&to={1}&apikey={2}";
        public static void Start()
        {
            Logger.AddMessage($"Started");

            var fromDate = DateTime.Today.AddDays(-1);
            var folder = $@"E:\Quote\WebData\Splits\FmpCloud\Data\FmpCloudSplits_{fromDate:yyyyMMdd}\";

            while (fromDate > new DateTime(2000, 1, 1))
            {
                Logger.AddMessage($"Download splits for {fromDate:yyyy-MM-dd}");
                var filename = $"{folder}FmpCloudSplits_{fromDate:yyyyMMdd}.json";
                var url = string.Format(UrlTemplate, fromDate.AddMonths(-6).ToString("yyyy-MM-dd"),
                    fromDate.ToString("yyyy-MM-dd"), FmpCloudCommon.GetApiKey());
                if (!File.Exists(filename))
                {
                    Download.DownloadPage(url, filename);
                    if (!File.Exists(filename))
                    {
                    }
                }
                var items = JsonConvert.DeserializeObject<cItem[]>(File.ReadAllText(filename)).ToArray();

                var nextDate = items.Min(a => a.date);
                if (nextDate >= fromDate)
                    throw new Exception("Check");

                fromDate = nextDate;
            }

            var zipFileName = ZipUtils.ZipFolder(folder);

            Logger.AddMessage($"Finished");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var items = new Dictionary<Tuple<string, DateTime>, cItem>();
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var cItems = JsonConvert.DeserializeObject<cItem[]>(entry.GetContentOfZipEntry()).ToArray();
                    foreach (var item in cItems)
                    {
                        if (!FmpCloudCommon.IsValidSymbol(item.symbol))
                            continue;

                        item.TimeStamp = entry.LastWriteTime.DateTime;
                        var key = new Tuple<string, DateTime>(item.symbol, item.date);
                        if (!items.ContainsKey(key))
                            items.Add(key, item);
                    }

                }

            var K = items.Values.Select(a => a.K).ToArray();
            // Save data to buffer table of data server
            Helpers.DbUtils.ClearAndSaveToDbTable(items.Values, "dbQ2023..SplitFmpCloud", "symbol", "date", "Ratio",
                "K", "TimeStamp");
            return items.Count;
        }

        private class cItem
        {
            public string symbol;
            public DateTime date;
            public string label;
            public float numerator;
            public float denominator;

            public string Ratio => $"{numerator}:{denominator}";
            public float? K => numerator < 0.00001f || denominator<0.00001f ? (float?) null : numerator / denominator;
            public DateTime TimeStamp;
        }
    }
    }
