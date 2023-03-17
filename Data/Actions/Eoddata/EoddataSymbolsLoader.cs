using System;
using System.Collections.Generic;
using System.IO;
using Data.Helpers;
using Data.Models;

namespace Data.Actions.Eoddata
{
    public class EoddataSymbolsLoader
    {
        private static string[] _exchanges = new string[] {"AMEX", "NASDAQ", "NYSE", "OTCBB"};
        private static string _urlTemplate = @"https://www.eoddata.com/Data/symbollist.aspx?e={0}";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var timeStamp = CsUtils.GetTimeStamp();
            var folder = $@"E:\Quote\WebData\Symbols\Eoddata\SymbolsEoddata_{timeStamp.Item2}\";

            // Prepare cookies
            var urlForCookie = "https://www.eoddata.com/";
            var cookieContainer = Helpers.CookiesGoogle.GetCookies(urlForCookie);
            if (cookieContainer.Count == 0)
                throw new Exception("Check login to www.eoddata.com in Chrome browser");

            // Download data
            foreach (var exchange in _exchanges)
            {
                Logger.AddMessage($"Download Symbols data for {exchange}");
                var url = string.Format(_urlTemplate, exchange);
                var filename = $"{folder}{exchange}_{timeStamp.Item2}.txt";
                Helpers.Download.DownloadPage(url, filename, false, cookieContainer);
            }

            // Parse and save data to database
            var itemsCount = 0;
            foreach (var filename in Directory.GetFiles(folder))
            {
                Logger.AddMessage($"'{Path.GetFileName(filename)}' file is parsing");
                itemsCount += Parse(filename);
            }
            Helpers.DbUtils.RunProcedure("pUpdateSymbolsXref");


            // Zip data and remove text files
            var zipFilename = CsUtils.ZipFolder(folder);
            Directory.Delete(folder);

            Logger.AddMessage($"!Finished. Filename: {zipFilename} with {itemsCount} items");
        }

        private static int Parse(string filename)
        {
            var items = new List<SymbolsEoddata>();
            var ss = Path.GetFileNameWithoutExtension(filename).Split('_');
            var exchange = ss[0].Trim().ToUpper();
            var date = File.GetLastWriteTime(filename);
            var lines = File.ReadAllLines(filename);
            var firstLine = true;

            // Add data to array (items)
            foreach (var line in lines)
            {
                if (firstLine)
                {
                    if (!string.Equals(line, "Symbol\tDescription"))
                        throw new Exception($"SymbolsEoddata_Parse error! Please, check the first line of {filename} file");
                    firstLine = false;
                }
                else if (!string.IsNullOrEmpty(line))
                    items.Add(new SymbolsEoddata(exchange, date, line.Split('\t')));
            }

            // Save data to buffer table of data server
            Helpers.DbUtils.ClearAndSaveToDbTable(items, "Bfr_SymbolsEoddata", "Symbol", "Exchange", "Name", "Created");
            Helpers.DbUtils.RunProcedure("pUpdateSymbolsEoddata", new Dictionary<string, object> { { "@Exchange", exchange }, { "@Date", date } });

            return items.Count;
        }

    }
}
