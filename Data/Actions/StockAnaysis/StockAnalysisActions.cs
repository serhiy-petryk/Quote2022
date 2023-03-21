using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Data.Models;
using Newtonsoft.Json;

namespace Data.Actions.StockAnaysis
{
    public class StockAnalysisActions
    {
        private const string Url = @"https://stockanalysis.com/actions/";
        private const string Folder = @"E:\Quote\WebData\Splits\StockAnalysis\Actions\";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var timeStamp = CsUtils.GetTimeStamp();
            var htmlFileName = Folder + $"StockAnalysisActions_{timeStamp.Item2}.html";
            var zipFileName = Folder + $"StockAnalysisActions_{timeStamp.Item2}.zip";

            // Download data to html file
            Helpers.Download.DownloadPage(Url, htmlFileName);

            // Zip data
            Helpers.ZipUtils.ZipFile(htmlFileName, zipFileName);

            // Parse and save to database
            var itemCount = ParseAndSaveToDb(zipFileName);

            File.Delete(htmlFileName);

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries)
                    if (entry.Length > 0)
                    {
                        var items = new List<Models.ActionStockAnalysis>();
                        Parse(entry.GetContentOfZipEntry(), items, entry.LastWriteTime.DateTime);
                        itemCount += items.Count;
                        // Save data to database
                        if (items.Count > 0)
                        {
                            DbUtils.ClearAndSaveToDbTable(items.Where(a => !a.IsBad), "dbQuote2023..Bfr_ActionsStockAnalysis",
                                "Date", "Type", "Symbol", "OtherSymbolOrName", "Name", "Description", "SplitRatio", "SplitK",
                                "TimeStamp");

                            DbUtils.ClearAndSaveToDbTable(items.Where(a => a.IsBad),
                                "dbQuote2023..Bfr_ActionsStockAnalysisError", "Date", "Type", "Symbol", "OtherSymbolOrName",
                                "Description", "TimeStamp");

                            DbUtils.RunProcedure("dbQuote2023..pUpdateActionsStockAnalysis");
                        }
                    }

            return itemCount;
        }

        #region ==========  Private section  ==========
        private static void Parse(string content, List<Models.ActionStockAnalysis> items, DateTime fileTimeStamp)
        {
            if (!TryToParseAsJson(content, items, fileTimeStamp))
                ParseAsHtml(content, items, fileTimeStamp);
        }

        private static bool TryToParseAsJson(string content, List<Models.ActionStockAnalysis> items, DateTime fileTimeStamp)
        {
            var i1 = content.IndexOf("const data =", StringComparison.InvariantCulture);
            if (i1 == -1) return false;
            var i2 = content.IndexOf("}}]", i1 + 12, StringComparison.InvariantCulture);
            var s = content.Substring(i1 + 12, i2 - i1 - 12 + 3).Trim();
            var i12 = s.IndexOf("{\"type\":", StringComparison.InvariantCulture);
            i12 = s.IndexOf("{\"type\":", i12 + 8, StringComparison.InvariantCulture);
            var s2 = s.Substring(i12, s.Length - i12 - 1);

            var oo = JsonConvert.DeserializeObject<cRoot>(s2);
            foreach (var item in oo.data.data)
                items.Add(new ActionStockAnalysis(item, fileTimeStamp));

            return true;
        }

        private static void ParseAsHtml(string content, List<Models.ActionStockAnalysis> items, DateTime fileTimeStamp)
        {
            var i1 = content.IndexOf(">Action</th>", StringComparison.InvariantCultureIgnoreCase);
            if (i1 == -1)
                throw new Exception("Check StockAnalysis.WebArchiveActions parser");
            var i2 = content.IndexOf("</tbody>", i1 + 12, StringComparison.InvariantCultureIgnoreCase);

            var rows = content.Substring(i1 + 12, i2 - i1 - 12).Replace("</thead>", "").Replace("<tbody>", "")
                .Replace("</tr>", "").Replace("<!-- HTML_TAG_START -->", "").Replace("<!-- HTML_TAG_END -->", "")
                .Split(new[] { "<tr>", "<tr " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row)) continue;
                var item = new Data.Models.ActionStockAnalysis(row.Trim(), fileTimeStamp);
                items.Add(item);
            }
        }
        #endregion

        #region =======  Json subclasses  =============
        public class cRoot
        {
            public string type;
            public cData data;
        }
        public class cData
        {
            public string action;
            public string type;
            public object props;
            public cItem[] data;
            public int fullCount;
        }
        public class cItem
        {
            public string date;
            public string type;
            public string symbol;
            public string name;
            public string other;
            public string text;

            public DateTime Date => DateTime.Parse(date, CultureInfo.InvariantCulture);
            public string Symbol => symbol.StartsWith("$") ? symbol.Substring(1) : symbol;
            public string Other => other == "N/A" || string.IsNullOrEmpty(other) ? null : other;
            public string Name => string.IsNullOrEmpty(name) ? null : name;

            public override string ToString()
            {
                return $"{Date:yyyy-MM-dd}, {Symbol}, {type}, {Other}, {text}";
            }
        }
        #endregion
    }
}
