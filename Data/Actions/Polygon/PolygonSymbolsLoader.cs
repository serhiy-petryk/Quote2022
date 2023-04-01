using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Newtonsoft.Json;

namespace Data.Actions.Polygon
{
    public static class PolygonSymbolsLoader
    {
        private const string StartUrlTemplate = "https://api.polygon.io/v3/reference/tickers?active=true&limit=1000";

        public static void Start()
        {
            Logger.AddMessage($"Started");
            var timeStamp = CsUtils.GetTimeStamp().Item2;
            var folder = $@"E:\Quote\WebData\Symbols\Polygon\Data\SymbolsPolygon_{timeStamp}\";

            var url = StartUrlTemplate;
            var cnt = 0;
            while (url != null)
            {
                url = url + "&apiKey=" + PolygonCommon.GetApiKey();
                var filename = folder + $"SymbolsPolygon_{cnt:D2}_{timeStamp}.json";
                if (!File.Exists(filename))
                {
                    Logger.AddMessage($"Downloading {cnt} data chunk into {Path.GetFileName(filename)}");

                    var result = Download.DownloadPage(url, filename);
                    if (result != null)
                        throw new Exception("Error!");
                }

                var oo = JsonConvert.DeserializeObject<cRoot>(File.ReadAllText(filename));
                if (oo.status != "OK")
                    throw new Exception("Wrong status of response!");

                url = oo.next_url;
                cnt++;
            }

            var zipFileName = ZipUtils.ZipFolder(folder);

            var itemCount = ParseAndSaveToDb(zipFileName);

            Directory.Delete(folder, true);

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var items = new List<cItem>();
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var oo = JsonConvert.DeserializeObject<cRoot>(entry.GetContentOfZipEntry());

                    foreach (var item in oo.results)
                        item.TimeStamp = entry.LastWriteTime.DateTime;

                    items.AddRange(oo.results.Where(a => a.IsValidTicker));
                }

            Helpers.DbUtils.SaveToDbTable(items, "dbQ2023..Bfr_SymbolsPolygon", "Symbol", "primary_exchange",
                "name", "type", "market", "cik", "composite_figi", "share_class_figi", "active", "last_updated_utc",
                "TimeStamp");

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

            public DateTime TimeStamp;
        }
        #endregion

    }
}
