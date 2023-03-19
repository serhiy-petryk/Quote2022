using System;
using System.IO;
using System.Linq;
using Data.Helpers;
using Data.Models;

namespace Data.Actions.Eoddata
{
    public class EoddataSymbolsLoader
    {
        private static readonly string[] Exchanges = new string[] {"AMEX", "NASDAQ", "NYSE", "OTCBB"};
        private const string UrlTemplate = @"https://www.eoddata.com/Data/symbollist.aspx?e={0}";
        private const  string FolderTemplate = @"E:\Quote\WebData\Symbols\Eoddata\SymbolsEoddata_{0}\";

        public static void Start()
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
            var zipFileName = CsUtils.ZipFolder(folder);

            // Parse and save data to database
            Logger.AddMessage($"'{zipFileName}' file is parsing");
            var itemCount = ParseAndSaveToDb(zipFileName);

            Logger.AddMessage($"Run sql procedure: pUpdateSymbolsXref");
            Helpers.DbUtils.RunProcedure("pUpdateSymbolsXref");

            // Zip data
            Directory.Delete(folder, true);

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = new ZipReader(zipFileName))
                foreach (var zipItem in zip)
                    if (zipItem.Length > 0)
                    {
                        var lines = zipItem.AllLines.ToArray();

                        var ss = zipItem.FileNameWithoutExtension.Split('_');
                        var exchange = ss[0].Trim().ToUpper();
                        var date = zipItem.Created;

                        if (lines.Length == 0 || !string.Equals(lines[0], "Symbol\tDescription"))
                            throw new Exception($"SymbolsEoddata_Parse error! Please, check the first line of {zipItem.FileNameWithoutExtension} file in {zipFileName}");
                        
                        var items = lines.Skip(1).Select(line => new SymbolsEoddata(exchange, date, line.Split('\t')))
                            .ToArray();

                        itemCount += items.Length;

                        // Save data to buffer table of data server
                        Helpers.DbUtils.ClearAndSaveToDbTable(items, "Bfr_SymbolsEoddata", "Symbol", "Exchange", "Name",
                            "TimeStamp");
                        Helpers.DbUtils.RunProcedure("pUpdateSymbolsEoddata");
                    }

            return itemCount;
        }
    }
}
