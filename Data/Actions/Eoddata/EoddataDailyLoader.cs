using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
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

        public static void Start(Action<string> logEvent)
        {
            logEvent($"EoddataDailyLoader started");

            var timeStamp = CsUtils.GetTimeStamp();
            var tempFolder =  FILE_FOLDER + timeStamp.Item2 + @"\";
            var tempZipFileNameTemplate = tempFolder + @"{0}_{1}.zip";
            var textFileNameTemplate = tempFolder + @"{0}_{1}.txt";

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
                    var filename = $"{FILE_FOLDER}{exchange}_{fileTimeStamp}.zip";
                    if (!File.Exists(filename))
                        missingFiles.Add(new Tuple<string, string>(exchange,fileTimeStamp));
                }
            }

            if (missingFiles.Count > 0)
            {
                // Prepare cookies
                var cookieContainer = Helpers.CookiesGoogle.GetCookies(URL_FOR_COOKIES);
                if (cookieContainer.Count == 0)
                    throw new Exception("Check login to www.eoddata.com in Chrome browser");

                // Get k parameter of eoddata url
                var tempFn = tempFolder + @"download.html";
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
                    logEvent($"EoddataDailyLoader. Download Eoddata daily data for {fileId.Item1} and {fileId.Item2}");
                    var url = string.Format(URL_TEMPLATE, fileId.Item1, fileId.Item2, kParameter.Substring(2));
                    var textFilename = string.Format(textFileNameTemplate, fileId.Item1, fileId.Item2);
                    Helpers.Download.DownloadPage(url, textFilename, false, cookieContainer);
                }

                // Zip data and remove text files
                foreach (var fileId in missingFiles)
                {
                    var textFilename = string.Format(textFileNameTemplate, fileId.Item1, fileId.Item2);
                    var newZipFilename = Helpers.CsUtils.ZipFile(textFilename);
                    var destinationZipFileName = $"{FILE_FOLDER}{fileId.Item1}_{fileId.Item2}.zip";
                    File.Move(newZipFilename, destinationZipFileName);
                }

                logEvent($"!EoddataDailyLoader. Downloaded {missingFiles.Count} files");
            }

            // Get missing quotes in database
            logEvent($"EoddataDailyLoader. Get existing data in database");
            var existingQuotes = new Dictionary<string, object>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 150;
                    cmd.CommandText = "SELECT distinct Exchange, date from DayEoddata";
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                            existingQuotes.Add($"{FILE_FOLDER}{(string)rdr["Exchange"]}_{(DateTime)rdr["Date"]:yyyyMMdd}.zip", null);
                }
            }

            // Parse and save new items to database
            var files = Directory.GetFiles(FILE_FOLDER, "*.zip", SearchOption.TopDirectoryOnly);
            var newFileCount = 0;
            var itemCount = 0;
            foreach (var file in files)
            {
                if (!existingQuotes.ContainsKey(file))
                {
                    newFileCount++;
                    logEvent($"EoddataDailyLoader. Save to database quotes from file {Path.GetFileName(file)}");
                    itemCount += Parse(file);
                }
            }

            if (newFileCount > 0)
            {
                logEvent($"EoddataDailyLoader. Update data in database ('pUpdateDayEoddata' procedure)");
                Helpers.DbUtils.RunProcedure("pUpdateDayEoddata");
            }

            logEvent($"!EoddataDailyLoader finished. Loaded data from {newFileCount} files into database. Total {itemCount:N0} quotes");
        }

        private static int Parse(string filename)
        {
            var exchange = Path.GetFileNameWithoutExtension(filename).Split('_')[0].Trim().ToUpper();
            var quotes = new List<DayEoddata>();
            string[] lines = null;
            using (var _zip = new ZipReader(filename))
            {
                var fileContents = _zip.Select(a => a.AllLines.ToArray()).ToArray();
                if (fileContents.Length == 1)
                    lines = fileContents[0];
                else
                    throw new Exception($"Error in zip file structure: {filename}");
            }

            var itemCount = 0;
            string prevLine = null; // To ignore duplicates
            foreach (var line in lines)
            {
                if (!string.Equals(line, prevLine))
                {
                    itemCount++;
                    prevLine = line;
                    quotes.Add(new DayEoddata(exchange, line.Split(',')));
                }
            }

            Helpers.DbUtils.SaveToDbTable(quotes, "DayEoddata", "Symbol", "Exchange", "Date", "Open", "High", "Low", "Close", "Volume");
            return itemCount;
        }

    }
}
