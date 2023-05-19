﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.Compression;
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

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var tradingDays = new List<DateTime>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT [date] FROM TradingDays WHERE [date] > DATEADD(day,-30, GETDATE())";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        tradingDays.Add((DateTime) rdr["Date"]);
            }

            var missingFiles = new List<Tuple<string, string>>();
            foreach (var date in tradingDays)
            {
                var fileTimeStamp = date.ToString("yyyyMMdd", CultureInfo.InstalledUICulture);
                foreach (var exchange in _exchanges)
                {
                    var filename = $"{FILE_FOLDER}{exchange}_{fileTimeStamp}.zip";
                    if (!File.Exists(filename))
                        missingFiles.Add(Tuple.Create(exchange, fileTimeStamp));
                }
            }

            if (missingFiles.Count > 0)
            {
                // Prepare cookies
                var cookieContainer = CookiesGoogle.GetCookies(URL_FOR_COOKIES);
                if (cookieContainer.Count == 0)
                    throw new Exception("Check login to www.eoddata.com in Chrome browser");

                // Get k parameter of eoddata url
                var o = Download.DownloadToString(URL_HOME, false, cookieContainer);
                if (o is Exception ex)
                    throw new Exception($"EoddataDailyLoader.Start. Error while download from {URL_HOME}. Error message: {ex.Message}");

                var s = (string)o;
                var i1 = s.IndexOf("/data/filedownload.aspx?e=", StringComparison.InvariantCulture);
                var i2 = s.IndexOf("\"", i1 + 20, StringComparison.InvariantCulture);
                var kParameter = s.Substring(i1 + 20, i2 - i1 - 20).Split('&').FirstOrDefault(a => a.StartsWith("k="));
                if (kParameter == null)
                    throw new Exception("Can not define 'k' parameter of url request for www.eoddata.com");

                // Download and zip missing files
                foreach (var fileId in missingFiles)
                {
                    Logger.AddMessage($"Download Eoddata daily data for {fileId.Item1} and {fileId.Item2}");
                    var url = string.Format(URL_TEMPLATE, fileId.Item1, fileId.Item2, kParameter.Substring(2));
                    o = Download.DownloadToString(url, false, cookieContainer);
                    if (o is Exception ex2)
                        throw new Exception($"EoddataDailyLoader.Start. Error while download from {url}. Error message: {ex2.Message}");

                    var zipFileName = $"{FILE_FOLDER}{fileId.Item1}_{fileId.Item2}.zip";
                    var entry = new VirtualFileEntry($"{Path.GetFileNameWithoutExtension(zipFileName)}.txt", (string)o);
                    ZipUtils.ZipVirtualFileEntries(zipFileName, new[]{entry});
                }

                Logger.AddMessage($"!Downloaded {missingFiles.Count} files");
            }

            // Get missing quotes in database
            Logger.AddMessage($"Get existing data in database");
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
            var fileSize = 0;
            foreach (var file in files)
            {
                if (!existingQuotes.ContainsKey(file))
                {
                    newFileCount++;
                    Logger.AddMessage($"Save to database quotes from file {Path.GetFileName(file)}");
                    itemCount += ParseAndSaveToDb(file);
                    fileSize += CsUtils.GetFileSizeInKB(file);
                }
            }

            if (newFileCount > 0)
            {
                Logger.AddMessage($"Update data in database ('pUpdateDayEoddata' procedure)");
                DbUtils.RunProcedure("pUpdateDayEoddata");
            }

            Logger.AddMessage($"!Finished. Loaded quotes into DayEoddata table. Quotes: {itemCount:N0}. Number of files: {newFileCount}. Size of files: {fileSize:N0}KB");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var exchange = Path.GetFileNameWithoutExtension(zipFileName).Split('_')[0].Trim().ToUpper();
            var quotes = new List<DayEoddata>();
            string[] lines = null;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
            {
                if (zip.Entries.Count != 1)
                    throw new Exception($"Error in zip file structure: {zipFileName}");
                lines = zip.Entries[0].GetLinesOfZipEntry().ToArray();
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

            DbUtils.SaveToDbTable(quotes, "DayEoddata", "Symbol", "Exchange", "Date", "Open", "High", "Low", "Close", "Volume");
            return itemCount;
        }

    }
}
