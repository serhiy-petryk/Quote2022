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

            var virtualFileEntries = new List<VirtualFileEntry>();
            var fromDate = DateTime.Today.AddDays(-1);
            var zipFileName = $@"E:\Quote\WebData\Splits\FmpCloud\Data\FmpCloudSplits_{fromDate:yyyyMMdd}.zip";
            var itemsCount = 0;

            while (fromDate > new DateTime(2000, 1, 1))
            {
                Logger.AddMessage($"Download splits for {fromDate:yyyy-MM-dd}");
                var url = string.Format(UrlTemplate, fromDate.AddMonths(-6).ToString("yyyy-MM-dd"),
                    fromDate.ToString("yyyy-MM-dd"), FmpCloudCommon.GetApiKey());
                var o = Download.DownloadToString(url);
                if (o is Exception ex)
                    throw new Exception($"FmpCloudSplitsLoader.Start. Error while download from {url}. Error message: {ex.Message}");

                var entry = new VirtualFileEntry($@"{Path.GetFileNameWithoutExtension(zipFileName)}\FmpCloudSplits_{fromDate:yyyyMMdd}.json", (string)o);
                virtualFileEntries.Add(entry);
                var items = JsonConvert.DeserializeObject<cItem[]>((string)o).ToArray();
                itemsCount += items.Count();

                var nextDate = items.Min(a => a.date);
                if (nextDate >= fromDate)
                    throw new Exception("Check");

                fromDate = nextDate;
            }

            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);

            Logger.AddMessage($"!Finished. Items: {itemsCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
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
                        var key = Tuple.Create(item.symbol, item.date);
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
