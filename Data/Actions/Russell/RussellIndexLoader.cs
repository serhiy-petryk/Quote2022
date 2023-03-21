using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.Russell
{
    public static class RussellIndexLoader
    {
        private const string Folder = @"E:\Quote\WebData\Indices\Russell";

        public static int Parse(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries)
                    if (entry.Length > 0 && entry.FullName.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var lines = entry.GetLinesOfZipEntry().ToArray();

                        Logger.AddMessage($"Processing '{entry.Name}' in '{zipFileName}' file");

                        var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                        var indexName = ss[0].ToUpper();
                        var timeStamp = DateTime.ParseExact(ss[2], "yyyyMMdd", CultureInfo.InvariantCulture);
                        var items = new Dictionary<string, Models.IndexDbItem>();

                        // var lines = sourceItem.Lines.Where(a => !string.IsNullOrEmpty(a)).ToArray();
                        if (lines[0] != "Company\tTicker")
                            throw new Exception($"IndexRussell.Parse. Invalid header in {entry.Name}");
                        for (var k = 1; k < lines.Length; k++)
                        {
                            ss = lines[k].Split('\t');
                            var item = new Models.IndexDbItem { Index = indexName, Symbol = ss[1].Trim(), Name = ss[0].Trim(), TimeStamp = timeStamp };
                            if (string.IsNullOrWhiteSpace(item.Symbol) || string.IsNullOrWhiteSpace(item.Name))
                                throw new Exception($"Check data in {entry.Name}");

                            if (!items.ContainsKey(item.Symbol))
                                items.Add(item.Symbol, item);
                        }

                        Models.IndexDbItem.SaveToDb(items.Values);

                        itemCount += items.Count;
                    }

            Logger.AddMessage($"!Parser finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");

            return itemCount;
        }
    }
}
