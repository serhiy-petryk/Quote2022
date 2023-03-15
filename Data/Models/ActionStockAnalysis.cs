using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class ActionStockAnalysis
    {
        public enum Action { None, Listed, Delisted, Split, Change, Spinoff, Bankruptcy, Acquisition };

        private static Dictionary<string, Action> _keys = new Dictionary<string, Action>
            {
                {" ticker symbol changed to ", Action.Change}, {" Listed - ", Action.Listed},
                {" was listed", Action.Listed}, {" Delisted - ", Action.Delisted}, {" was delisted", Action.Delisted},
                {" stock split: ", Action.Split}, {" spun off from ", Action.Spinoff},
                {" was acquired by ", Action.Acquisition}, {" was liquidated due to bankruptcy.", Action.Bankruptcy}
            };

        public DateTime Date;
        public Action Type;
        public string Symbol;
        public string OtherSymbolOrName = "";
        public string Description;
        public string SplitRatio;
        public double? SplitK;
        public DateTime TimeStamp;
        public bool IsBad;
        public Action DescriptionAction;

        public ActionStockAnalysis(string row, DateTime timestamp)
        {
            TimeStamp = timestamp;

            var cells = row.Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries);
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
                Type = Action.Change;
                OtherSymbolOrName = GetFirstWord(Description);
                Symbol = GetLastWord(Description);
            }
            else if (action == "Listed")
            {
                Type = Action.Listed;
                if (Description.IndexOf(" was listed", StringComparison.InvariantCulture) == -1)
                    Symbol = GetFirstWord(Description);
                else
                    Symbol = symbol;
            }
            else if (action == "Delisted")
            {
                Type = Action.Delisted;
                if (Description.IndexOf(" was delisted", StringComparison.InvariantCulture) == -1)
                    Symbol = GetFirstWord(Description);
                else
                    Symbol = symbol;
            }
            else if (action == "Stock Split")
            {
                Type = Action.Split;
                if (DescriptionAction == Action.Split)
                {
                    Symbol = GetFirstWord(Description);
                    var ss = Description.Split(new[] { " stock split: ", " for " }, StringSplitOptions.None);
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
                Type = Action.Spinoff;
                OtherSymbolOrName = GetFirstWord(Description);
                Symbol = GetLastWord(Description);
            }
            else if (action == "Acquisition")
            {
                Type = Action.Acquisition;
                Symbol = GetFirstWord(Description);
                var i1 = Description.LastIndexOf(" was acquired by ", StringComparison.InvariantCulture);
                OtherSymbolOrName = Description.Substring(i1 + 17);
                if (OtherSymbolOrName.EndsWith(".") && !OtherSymbolOrName.EndsWith(".."))
                    OtherSymbolOrName = OtherSymbolOrName.Substring(0, OtherSymbolOrName.Length - 1);
            }
            else if (action == "Bankruptcy")
            {
                Type = Action.Bankruptcy;
                Symbol = GetFirstWord(Description);
            }
            else
            {
                throw new Exception("Check action");
            }

            if (Type != DescriptionAction)
            {
                IsBad = true;
                return;
            }

            if (string.IsNullOrEmpty(Symbol))
                throw new Exception("Check symbol");
            if (!string.IsNullOrEmpty(symbol) && !string.Equals(symbol, Symbol))
            {
                if (Type == Action.Split)
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
            return System.Net.WebUtility.HtmlDecode(cell.Substring(i1 + 1).Trim());
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
}
