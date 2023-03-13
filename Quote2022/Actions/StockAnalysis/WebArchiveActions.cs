using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Quote2022.Actions.StockAnalysis
{
    public class WebArchiveActions
    {

        public enum Action { None, Listed, Delisted, Split, Change, Spinoff, Bankruptcy, Acquisition };

        public class DbItem
        {
            private static Dictionary<string, Action> _keys = new Dictionary<string, Action>
            {
                {" ticker symbol changed to ", Action.Change}, {" Listed - ", Action.Listed},
                {" was listed", Action.Listed}, {" Delisted - ", Action.Delisted}, {" was delisted", Action.Delisted},
                {" stock split: ", Action.Split}, {" spun off from ", Action.Spinoff},
                {" was acquired by ", Action.Acquisition}, {" was liquidated due to bankruptcy.", Action.Bankruptcy}
            };

            public DateTime Date;
            public Action Action;
            public string Symbol;
            public string OtherSymbolOrName = "";
            public string Description;
            public string SplitRatio;
            public double? SplitK;
            public DateTime TimeStamp;
            public bool IsBad;
            public Action DescriptionAction;

            public DbItem(string row, DateTime timestamp)
            {
                TimeStamp = timestamp;

                var cells = row.Split(new[] {"</td>"}, StringSplitOptions.RemoveEmptyEntries);
                if (cells.Length != 3 && cells.Length != 4)
                    throw new Exception("Check StockAnalysis.WebArchiveActions parser");

                Date = DateTime.Parse(GetCellContent(cells[0]), CultureInfo.InvariantCulture);
                var symbol = cells.Length == 4 ? GetCellContent(cells[1]) : null;
                var action = GetCellContent(cells[cells.Length - 2]);
                Description = GetCellContent(cells[cells.Length - 1]);

                if (Description == "") DescriptionAction = Action.None;
                else
                {
                    foreach (var kvp in _keys)
                        if (Description.Contains(kvp.Key))
                        {
                            DescriptionAction = kvp.Value;
                            break;
                        }
                }

                if (action == "Symbol Change")
                {
                    Action = Action.Change;
                    OtherSymbolOrName = GetFirstWord(Description);
                    Symbol = GetLastWord(Description);
                }
                else if (action == "Listed")
                {
                    Action = Action.Listed;
                    if (Description.IndexOf(" was listed", StringComparison.InvariantCulture) == -1)
                        Symbol = GetFirstWord(Description);
                    else
                        Symbol = symbol;
                }
                else if (action == "Delisted")
                {
                    Action = Action.Delisted;
                    if (Description.IndexOf(" was delisted", StringComparison.InvariantCulture) == -1)
                        Symbol = GetFirstWord(Description);
                    else
                        Symbol = symbol;
                }
                else if (action == "Stock Split")
                {
                    Action = Action.Split;
                    if (DescriptionAction == Action.Split)
                    {
                        Symbol = GetFirstWord(Description);
                        var ss = Description.Split(new[] {" stock split: ", " for "}, StringSplitOptions.None);
                        SplitRatio = ss[1] + ":" + ss[2];
                        var d1 = double.Parse(ss[1], CultureInfo.InvariantCulture);
                        var d2 = double.Parse(ss[2], CultureInfo.InvariantCulture);
                        SplitK = d1 / d2;
                    }
                    else
                        Symbol = symbol;
                }
                else if (action == "Spinoff")
                {
                    Action = Action.Spinoff;
                    OtherSymbolOrName = GetFirstWord(Description);
                    Symbol = GetLastWord(Description);
                }
                else if (action == "Acquisition")
                {
                    Action = Action.Acquisition;
                    Symbol = GetFirstWord(Description);
                    var i1 = Description.LastIndexOf(" was acquired by ", StringComparison.InvariantCulture);
                    OtherSymbolOrName = Description.Substring(i1 + 17);
                    if (OtherSymbolOrName.EndsWith(".") && !OtherSymbolOrName.EndsWith(".."))
                        OtherSymbolOrName = OtherSymbolOrName.Substring(0, OtherSymbolOrName.Length - 1);
                }
                else if (action == "Bankruptcy")
                {
                    Action = Action.Bankruptcy;
                    Symbol = GetFirstWord(Description);
                }
                else
                {
                    throw new Exception("Check action");
                }

                if (Action != DescriptionAction)
                {
                    IsBad = true;
                    return;
                }

                if (string.IsNullOrEmpty(Symbol))
                    throw new Exception("Check symbol");
                if (!string.IsNullOrEmpty(symbol) && !string.Equals(symbol, Symbol))
                {
                    if (Action == Action.Split)
                    {
                        IsBad = true;
                        return;
                    }

                    throw new Exception("Check symbol");
                }
            }

            string GetCellContent(string cell)
            {
                cell = cell.Replace("</a>", "");
                var i1 = cell.IndexOf("<a", StringComparison.InvariantCultureIgnoreCase);
                while (i1 != -1)
                {
                    var i2 = cell.IndexOf(">", i1 + 2);
                    cell = cell.Substring(0, i1) + cell.Substring(i2 + 1);
                    i1 = cell.IndexOf("<a", StringComparison.InvariantCultureIgnoreCase);
                }

                i1 = cell.LastIndexOf(">", StringComparison.InvariantCultureIgnoreCase);
                return System.Net.WebUtility.HtmlDecode(cell.Substring(i1+1).Trim());
            }

            string GetFirstWord(string s)
            {
                var i1 = s.IndexOf(' ');
                return s.Substring(0, i1).Trim();
            }
            string GetLastWord(string s)
            {
                var i1 = s.LastIndexOf(' ');
                return s.Substring(i1).Trim();
            }
        }

        private static string WebArchiveFolder = @"E:\Quote\WebArchive\Symbols\Stockanalysis\Actions\Recent";
        public static void Parse(Action<string> ShowStatus)
        {
            ShowStatus($"StockAnalysis.WebArchiveActions is started");

            var files = Directory.GetFiles(WebArchiveFolder, "*.html").OrderBy(a => a).ToArray();
            var items = new List<DbItem>();
            foreach (var file in files)
            {
                ShowStatus($"StockAnalysis.WebArchiveActions. Processing file {Path.GetFileName(file)}");
                var timeStamp =
                    File.GetCreationTime(file); // Path.GetFileNameWithoutExtension(file).Split('_')[1].Substring(0, 8);
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
                    var item = new DbItem(row.Trim(), timeStamp);
                    items.Add(item);
                }

                if (items.Count > 0)
                {
                    SaveToDb.ClearAndSaveToDbTable(items.Where(a => !a.IsBad), "dbQuote2023..Bfr_ActionsStockAnalysis",
                        "Date", "Action", "Symbol", "OtherSymbolOrName", "Description", "SplitRatio", "SplitK",
                        "TimeStamp");

                    SaveToDb.ClearAndSaveToDbTable(items.Where(a => a.IsBad),
                        "dbQuote2023..Bfr_ActionsStockAnalysisError", "Date", "Action", "Symbol", "OtherSymbolOrName",
                        "Description", "TimeStamp");

                    SaveToDb.RunProcedure("dbQuote2023..pUpdateActionsStockAnalysis");

                    items.Clear();
                }
            }

            ShowStatus($"StockAnalysis.WebArchiveActions finished");
        }
    }
}
