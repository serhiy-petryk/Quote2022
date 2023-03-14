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

        public static void Start(Action<string> showStatus)
        {
            var timeStamp = csUtils.GetTimeStamp();
            var folder = $@"E:\Quote\WebData\Symbols\Eoddata\SymbolsEoddata_{timeStamp.Item2}\";

            // Prepare cookies
            var urlForCookie = "https://www.eoddata.com/";
            var cookies = Helpers.CookiesGoogle.GetCookies(urlForCookie);
            if (cookies.Count == 0)
                throw new Exception("Check login to www.eoddata.com in Chrome browser");
            var cookieContainer = new System.Net.CookieContainer();
            foreach (var cookie in cookies)
                cookieContainer.Add(cookie);

            // Download data
            foreach (var exchange in _exchanges)
            {
                showStatus($"Eoddata.SymbolsLoader. Download Symbols data for {exchange}");
                var url = string.Format(_urlTemplate, exchange);
                var filename = $"{folder}{exchange}_{timeStamp.Item2}.txt";
                Helpers.Download.DownloadPage(url, filename, false, cookieContainer);
            }

            // Parse and save data to database
            foreach (var filename in Directory.GetFiles(folder))
            {
                showStatus($"Eoddata.SymbolsLoader. '{Path.GetFileName(filename)}' file is parsing");
                Parse(filename, timeStamp.Item1);
            }
            Helpers.DbUtils.RunProcedure("pUpdateSymbolsXref");


            // Zip data and remove text files
            var zipFilename = csUtils.ZipFolder(folder);
            Directory.Delete(folder);

            showStatus($"Eoddata.SymbolsLoader finished. Filename: {zipFilename}");
        }

        private static void Parse(string filename, DateTime timeStamp)
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
            // Helpers.DbUtils.ClearAndSaveToDbTable(items, "Bfr_SymbolsEoddata", "Symbol", "Exchange", "Name", "Created");
            // Helpers.DbUtils.RunProcedure("pUpdateSymbolsEoddata", new Dictionary<string, object> { { "@Exchange", exchange }, { "@Date", date } });
        }

    }
}
