using System;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Newtonsoft.Json;

namespace Data.Actions.Finnhub
{
    public class FinnhubSymbolsLoader
    {
       /* private static readonly string[] Exchanges = new string[] {"AMEX", "NASDAQ", "NYSE", "OTCBB"};
        private const string UrlTemplate = @"https://www.eoddata.com/Data/symbollist.aspx?e={0}";
        private const  string FolderTemplate = @"E:\Quote\WebData\Symbols\Eoddata\SymbolsEoddata_{0}\";*/

        /*public static void Start()
        {
            Logger.AddMessage($"Started");

            var timeStamp = CsUtils.GetTimeStamp();
            var folder = string.Format(FolderTemplate, timeStamp.Item2);

            // Prepare cookies
            var urlForCookie = "https://www.eoddata.com/";
            var cookieContainer = Helpers.CookiesGoogle.GetCookies(urlForCookie);
            if (cookieContainer.Count == 0)
                throw new Exception("Check login to www.eoddata.com in Chrome browser");

            // Download data
            foreach (var exchange in Exchanges)
            {
                Logger.AddMessage($"Download Symbols data for {exchange}");
                var url = string.Format(UrlTemplate, exchange);
                var filename = $"{folder}{exchange}_{timeStamp.Item2}.txt";
                Helpers.Download.DownloadPage(url, filename, false, cookieContainer);
            }

            // Zip data
            var zipFileName = ZipUtils.ZipFolder(folder);

            // Parse and save data to database
            Logger.AddMessage($"'{zipFileName}' file is parsing");
            var itemCount = ParseAndSaveToDb(zipFileName);

            Logger.AddMessage($"Run sql procedure: pUpdateSymbolsXref");
            Helpers.DbUtils.RunProcedure("pUpdateSymbolsXref");

            // Zip data
            Directory.Delete(folder, true);

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }*/

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var items = JsonConvert.DeserializeObject<cItem[]>(entry.GetContentOfZipEntry());
                    var timeStamp = entry.LastWriteTime.DateTime;
                    foreach (var item in items)
                        item.TimeStamp = timeStamp;

                    itemCount += items.Length;

                    // Save data to buffer table of data server
                    Helpers.DbUtils.ClearAndSaveToDbTable(items, "dbQuote2023..Bfr_SymbolsFinnhub", "symbol",
                        "Exchange", "Type", "Name", "Figi", "ShareClassFigi", "TimeStamp");
                    // Helpers.DbUtils.RunProcedure("pUpdateSymbolsEoddata");
                }

            return itemCount;
        }

        #region =======  Json subclassess  =========

        private class cItem
        {
            public string currency; // USD or empty
            public string description; // is null
            public string displaySymbol; // always symbol
            public string figi; // is null
            // public string isin; // always null
            public string mic; // not null
            public string shareClassFIGI;
            public string symbol;
            public string symbol2; // always blank
            public string type;

            public string Exchange => mic;
            public string Name => string.IsNullOrEmpty(description) ? null : description;
            public string Type => string.IsNullOrEmpty(type) ? null : type;
            public string Figi => string.IsNullOrEmpty(figi) ? null : figi;
            public string ShareClassFigi => string.IsNullOrEmpty(shareClassFIGI) ? null : shareClassFIGI;
            public DateTime TimeStamp;
        }
        #endregion =================================
    }
}
