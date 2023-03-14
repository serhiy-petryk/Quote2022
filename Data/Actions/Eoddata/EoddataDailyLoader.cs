using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using Data.Helpers;
using Data.Models;

namespace Data.Actions.Eoddata
{
    public class EoddataDailyLoader
    {
        private const string FILE_FOLDER = @"E:\Quote\WebData\Daily\Eoddata\";
        private static string[] _exchanges = new string[] {"AMEX", "NASDAQ", "NYSE"};
        private const string URL_FOR_COOKIES = "https://www.eoddata.com/";
        private const string URL_HOME = "https://www.eoddata.com/download.aspx";
        private const string URL_TEMPLATE = "https://www.eoddata.com/data/filedownload.aspx?e={0}&sd={1}&ea=1&ed={1}&d=9&p=0&o=d&k={2}";
        private const string ZIP_FILE_TEMPLATE = FILE_FOLDER + @"{0}_{1}.zip";
        private const string TEXT_FILE_TEMPLATE = FILE_FOLDER + @"Temp\{0}_{1}.txt";

        public static void Start(Action<string> showStatus)
        {
            var timeStamp = csUtils.GetTimeStamp();

            var tradingDays = new List<DateTime>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT [date] FROM TradingDays WHERE [date] > DATEADD(day,-30, GETDATE())";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        tradingDays.Add((DateTime)rdr["Date"]);
                    }
            }

            var missingFiles = new List<Tuple<string, string>>();
            foreach (var date in tradingDays)
            {
                var fileTimeStamp = date.ToString("yyyyMMdd", CultureInfo.InstalledUICulture);
                foreach (var exchange in _exchanges)
                {
                    var filename = string.Format(ZIP_FILE_TEMPLATE, exchange, fileTimeStamp);
                    if (!File.Exists(filename))
                        missingFiles.Add(new Tuple<string, string>(exchange,fileTimeStamp));
                }
            }

            if (missingFiles.Count > 0)
            {
                // Prepare cookies
                var cookies = Helpers.CookiesGoogle.GetCookies(URL_FOR_COOKIES);
                if (cookies.Count == 0)
                    throw new Exception("Check login to www.eoddata.com in Chrome browser");
                var cookieContainer = new System.Net.CookieContainer();
                foreach (var cookie in cookies)
                    cookieContainer.Add(cookie);

                // Get k parameter of eoddata url
                var tempFn = FILE_FOLDER + @"Temp\download.html";
                Helpers.Download.DownloadPage(URL_HOME, tempFn, false, cookieContainer);

                var s = File.ReadAllText(tempFn);
                var i1 = s.IndexOf("/data/filedownload.aspx?e=", StringComparison.InvariantCulture);
                var i2 = s.IndexOf("\"", i1 + 20, StringComparison.InvariantCulture);
                var kParameter = s.Substring(i1 + 20, i2 - i1 - 20).Split('&').FirstOrDefault(a => a.StartsWith("k="));
                if (kParameter == null)
                    throw new Exception("Can not define 'k' parameter of url request for www.eoddata.com");

                // Download text files
                foreach (var fileId in missingFiles)
                {
                    showStatus($"EoddataDailyLoader. Download Eoddata daily data for {fileId.Item1} and {fileId.Item2}");
                    var url = string.Format(URL_TEMPLATE, fileId.Item1, fileId.Item2, kParameter.Substring(2));
                    var textFilename = string.Format(TEXT_FILE_TEMPLATE, fileId.Item1, fileId.Item2);
                    Helpers.Download.DownloadPage(url, textFilename, false, cookieContainer);
                }

                var d = 0;

                /*// Download data
                foreach (var exchange in _exchanges)
                {
                    showStatus($"EoddataDailyLoader. Download Symbols data for {exchange}");
                    var url = string.Format(_urlTemplate, exchange);
                    var filename = $"{folder}{exchange}_{timeStamp.Item2}.txt";
                    Helpers.Download.DownloadPage(url, filename, false, cookieContainer);
                }
    
                // Parse and save data to database
                foreach (var filename in Directory.GetFiles(folder))
                {
                    showStatus($"Eoddata.SymbolsLoader. '{Path.GetFileName(filename)}' file is parsing");
                    Parse(filename, timeStamp.Item1);
                }*/

            }

            showStatus($"EoddataDailyLoader finished");
        }

        private static void Parse(string filename, DateTime timeStamp)
        {
        }

    }
}
