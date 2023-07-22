﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Data.Models;

namespace Data.Actions.Eoddata
{
    public class EoddataSymbolsLoader
    {
        private static readonly string[] Exchanges = new string[] {"AMEX", "NASDAQ", "NYSE", "OTCBB"};
        private const string UrlTemplate = @"https://www.eoddata.com/Data/symbollist.aspx?e={0}";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var timeStamp = CsUtils.GetTimeStamp();
            var virtualFileEntries = new List<VirtualFileEntry>();
            var zipFileName = $@"E:\Quote\WebData\Symbols\Eoddata\SymbolsEoddata_{timeStamp.Item2}.zip";

            // Prepare cookies
            var urlForCookie = "https://www.eoddata.com/";
            var cookieContainer = CookiesGoogle.GetCookies(urlForCookie);
            if (cookieContainer.Count == 0)
                throw new Exception("Check login to www.eoddata.com in Chrome browser");

            // Download data
            foreach (var exchange in Exchanges)
            {
                Logger.AddMessage($"Download Symbols data for {exchange}");
                var url = string.Format(UrlTemplate, exchange);
                var o = Download.DownloadToString(url, false, cookieContainer);
                if (o is Exception ex)
                    throw new Exception($"EoddataSymbolsLoader: Error while download from {url}. Error message: {ex.Message}");

                var entry = new VirtualFileEntry($@"{Path.GetFileNameWithoutExtension(zipFileName)}\{exchange}_{timeStamp.Item2}.txt", (string)o);
                virtualFileEntries.Add(entry);
            }

            // Save to zip file
            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);

            // Parse and save data to database
            Logger.AddMessage($"'{zipFileName}' file is parsing");
            var itemCount = ParseAndSaveToDb(zipFileName);

            Logger.AddMessage($"Run sql procedure: pUpdateSymbolsXref");
            DbUtils.RunProcedure("dbQ2023Others..pUpdateSymbolsXref");

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var lines = entry.GetLinesOfZipEntry().ToArray();

                    var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                    var exchange = ss[0].Trim().ToUpper();
                    var date = entry.LastWriteTime.DateTime;

                    if (lines.Length == 0 || !string.Equals(lines[0], "Symbol\tDescription"))
                        throw new Exception($"SymbolsEoddata_Parse error! Please, check the first line of {entry.Name} file in {zipFileName}");

                    var items = lines.Skip(1).Select(line => new SymbolsEoddata(exchange, date, line.Split('\t')))
                        .ToArray();
                    foreach (var item in items)
                        item.TimeStamp = entry.LastWriteTime.DateTime;

                    itemCount += items.Length;

                    // Save data to buffer table of data server
                    DbUtils.ClearAndSaveToDbTable(items, "dbQ2023Others..Bfr_SymbolsEoddata", "Symbol", "Exchange", "Name",
                        "TimeStamp");
                    DbUtils.RunProcedure("dbQ2023Others..pUpdateSymbolsEoddata");
                }

            return itemCount;
        }
    }
}
