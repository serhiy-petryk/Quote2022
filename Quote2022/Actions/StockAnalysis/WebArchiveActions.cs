using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Quote2022.Actions.StockAnalysis
{
    public class WebArchiveActions
    {
        private static string WebArchiveFolder = @"E:\Quote\WebArchive\Symbols\Stockanalysis\Actions\Recent";
        public static void Parse(Action<string> ShowStatus)
        {
            ShowStatus($"StockAnalysis.WebArchiveActions is started");

            var files = Directory.GetFiles(WebArchiveFolder, "*.html").OrderBy(a => a).ToArray();
            var items = new List<Data.Models.ActionStockAnalysis>();
            foreach (var file in files)
            {
                ShowStatus($"StockAnalysis.WebArchiveActions. Processing file {Path.GetFileName(file)}");
                var timeStamp =
                    File.GetLastWriteTime(file); // Path.GetFileNameWithoutExtension(file).Split('_')[1].Substring(0, 8);
                var content = File.ReadAllText(file);

                var i1 = content.IndexOf(">Action</th>", StringComparison.InvariantCultureIgnoreCase);
                if (i1 == -1)
                    throw new Exception("Check StockAnalysis.WebArchiveActions parser");
                var i2 = content.IndexOf("</tbody>", i1 + 12, StringComparison.InvariantCultureIgnoreCase);

                var rows = content.Substring(i1 + 12, i2 - i1 - 12).Replace("</thead>", "").Replace("<tbody>", "")
                    .Replace("</tr>", "").Replace("<!-- HTML_TAG_START -->", "").Replace("<!-- HTML_TAG_END -->", "")
                    .Split(new[] {"<tr>", "<tr "}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row)) continue;
                    var item = new Data.Models.ActionStockAnalysis(row.Trim(), timeStamp);
                    items.Add(item);
                }

                if (items.Count > 0)
                {
                    SaveToDb.ClearAndSaveToDbTable(items.Where(a => !a.IsBad), "dbQuote2023..Bfr_ActionsStockAnalysis",
                        "Date", "Type", "Symbol", "OtherSymbolOrName", "Name", "Description", "SplitRatio", "SplitK",
                        "TimeStamp");

                    SaveToDb.ClearAndSaveToDbTable(items.Where(a => a.IsBad),
                        "dbQuote2023..Bfr_ActionsStockAnalysisError", "Date", "Type", "Symbol", "OtherSymbolOrName",
                        "Description", "TimeStamp");

                    SaveToDb.RunProcedure("dbQuote2023..pUpdateActionsStockAnalysis");

                    items.Clear();
                }
            }

            ShowStatus($"StockAnalysis.WebArchiveActions finished");
        }
    }
}
