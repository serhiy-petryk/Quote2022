using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.AlphaVantage
{
    public static class MavCopyZipInfoToDb
    {
        private const string ZipFolder = @"E:\Quote\WebData\Minute\AlphaVantage\Data";

        public static void Start()
        {
            Logger.AddMessage("Started");

            var files = Directory.GetFiles(ZipFolder, "*.zip");

            // Clear data base table
            var items = new List<Item>();
            DbUtils.ClearAndSaveToDbTable(items, "dbQuote2023..ZipLogMinuteAlphaVantage", "Symbol", "Date", "Lines");
            var cnt = 0;
            foreach (var file in files)
            {
                cnt++;
                Logger.AddMessage($"Processed {cnt} files from {files.Length}");

                using (var zipArchive = ZipFile.Open(file, ZipArchiveMode.Read))
                    foreach (var entry in zipArchive.Entries.Where(a => a.Name.EndsWith(".csv")))
                    {
                        var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                        var symbol = ss[0];
                        var date = DateTime.ParseExact(ss[1], "yyyyMMdd", CultureInfo.InvariantCulture);
                        var lines = Convert.ToInt16(entry.GetLinesOfZipEntry().Count() - 1);
                        items.Add(new Item { Symbol = symbol, Date = date, Lines = lines });
                    }

                if (items.Count > 100000)
                {
                    Logger.AddMessage("Save to database ...");
                    DbUtils.SaveToDbTable(items, "dbQuote2023..ZipLogMinuteAlphaVantage", "Symbol", "Date", "Lines");
                    items.Clear();
                }
            }

            if (items.Count > 0)
                DbUtils.SaveToDbTable(items, "dbQuote2023..ZipLogMinuteAlphaVantage", "Symbol", "Date", "Lines");

            Logger.AddMessage("!Finished");
        }

        private class Item
        {
            public string Symbol;
            public DateTime Date;
            public short Lines;
        }
    }
}
