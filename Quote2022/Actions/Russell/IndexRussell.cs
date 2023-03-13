using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Quote2022.Helpers;

namespace Quote2022.Actions.Russell
{
    public static class IndexRussell
    {
        public class ParseItem
        {
            public string[] Lines;
            public string Filename;
            public ParseItem(ZipReaderItem item)
            {
                Lines= item.AllLines.ToArray();
                Filename = item.FileNameWithoutExtension;
            }
        }

        public static void Parse(string zipFile, Action<string> showStatusAction)
        {

        using (var zip = new ZipReader(zipFile))
            {
                var items = zip
                    .Where(a => a.Length > 0 &&
                                a.FullName.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase))
                    .ToDictionary(a => a.FileNameWithoutExtension, a => a.AllLines.ToArray());
            }

            using (var zip = new ZipReader(zipFile))
            {
                var sourceItems =
                    zip.Where(a =>
                            a.Length > 0 && a.FullName.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase))
                        .Select(a => new ParseItem(a)).ToArray();

                foreach (var sourceItem in sourceItems.OrderBy(a=>a.Filename))
                {
                    showStatusAction($"IndexRussell.Parse. Processing '{sourceItem.Filename}'");
                    var ss = sourceItem.Filename.Split('_');
                    var indexName = ss[0].ToUpper();
                    var timeStamp = DateTime.ParseExact(ss[2], "yyyyMMdd", CultureInfo.InvariantCulture);
                    var items = new Dictionary<string, Models.IndexDbItem>();

                    var lines = sourceItem.Lines.Where(a => !string.IsNullOrEmpty(a)).ToArray();
                    if (lines[0] != "Company\tTicker")
                        throw new Exception($"IndexRussell.Parse. Invalid header in {sourceItem.Filename}");
                    for (var k = 1; k < lines.Length; k++)
                    {
                        ss = lines[k].Split('\t');
                        var item = new Models.IndexDbItem{Index = indexName, Symbol = ss[1].Trim(), Name = ss[0].Trim(), TimeStamp = timeStamp};
                        if (string.IsNullOrWhiteSpace(item.Symbol) || string.IsNullOrWhiteSpace(item.Name))
                            throw new Exception($"Check data in {sourceItem.Filename}");

                        if (!items.ContainsKey(item.Symbol))
                            items.Add(item.Symbol, item);
                    }

                    Models.IndexDbItem.SaveToDb(indexName, timeStamp, items.Values);
                }
            }

            showStatusAction($"IndexRussell.Parse finished");
        }

    }
    }
