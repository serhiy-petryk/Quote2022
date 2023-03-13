using System;
using Quote2022.Helpers;

namespace Quote2022.Models
{
    public class SymbolsNanex
    {
        public string Symbol;
        public string Exchange;
        public string Type;
        public string Name;
        public long? Activity;
        public DateTime? LastQuoteDate;
        public DateTime? LastTradeDate;
        public bool IsTest = false;
        public string YahooID; // Yahoo Symbol
        public string EoddataID; // Eoddata Symbol
        public string MsnID; // MoneyCentral Symbol
        public string GoogleID;
        public DateTime? Created = null;

        public SymbolsNanex(string exchange, string type, string symbol, string name, string activity, string lastQuoteDate, string lastTradeDate, DateTime created)
        {
            this.Exchange = exchange.ToUpper(); this.Type = type.ToLower();
            this.Symbol = symbol.ToUpper(); this.Name = name; this.Created = created;
            if (!string.IsNullOrEmpty(activity)) this.Activity = Int64.Parse(activity, Settings.fiNumberInvariant);
            if (!string.IsNullOrEmpty(lastQuoteDate)) this.LastQuoteDate = DateTime.Parse(lastQuoteDate, Settings.fiNumberInvariant);
            if (!string.IsNullOrEmpty(lastTradeDate)) this.LastTradeDate = DateTime.Parse(lastTradeDate, Settings.fiNumberInvariant);

            if ((name.ToLower().Contains(" test stock") || name.ToLower().Contains(" test symbol") || name.ToLower().Contains(" listing test")) && symbol.StartsWith("Z") && symbol.Length >= 3)
                IsTest = true;
            else if (name.ToLower().Contains("test") && symbol.EndsWith("TEST") && symbol.Length == 5)
                IsTest = true;

            if (!this.IsTest)
            {
                this.YahooID = GetYahooSymbol(this.Symbol);
                this.EoddataID = GetEoddataSymbol(this.Symbol);
                this.MsnID = GetMsnSymbol(this.Symbol);
                this.GoogleID = GetGoogleSymbol(this.Symbol);
            }
        }

        public override string ToString()
        {
            return this.Symbol + "\t" + this.Exchange + "\t" + this.Name + "\t" + this.Activity + "\t" +
              CsUtils.GetString(this.LastQuoteDate) + "\t" +
              CsUtils.GetString(this.LastTradeDate) + "\t" +
              this.YahooID + "\t" + this.EoddataID + "\t" + this.MsnID + "\t" + this.GoogleID + "\t" +
              this.Type + "\t" + CsUtils.GetString(this.Created);
        }

        private static string GetYahooSymbol(string symbol)
        {
            // complex
            if (symbol.IndexOf(".RT/WI") > 0) symbol = symbol.Replace(".RT/WI", "-RWI");
            if (symbol.IndexOf(".PR") > 0) symbol = symbol.Replace(".PR", "-P");
            if (symbol.IndexOf(".WS") > 0) symbol = symbol.Replace(".WS", "-WT");
            if (symbol.IndexOf(".RT") > 0) symbol = symbol.Replace(".RT", "-RI");
            if (symbol.IndexOf("/CL") > 0) symbol = symbol.Replace("/CL", "-CL");
            if (symbol.IndexOf("/WI") > 0) symbol = symbol.Replace("/WI", "-WI");
            if (symbol.IndexOf("/WD") > 0) symbol = symbol.Replace("/WD", "-WD");
            int i1 = symbol.IndexOf(".");
            int i2 = symbol.IndexOf("-");
            int i3 = symbol.Length;
            if (i1 == -1) return symbol;// no dot == do not need to change
            if (i1 > -1 && i2 > -1 && i1 == (i2 - 2))
            {// dot before "-"
                symbol = symbol.Substring(0, i1) + "-" + symbol.Substring(i1 + 1);// BR.T/WI
                return symbol;
            }
            if (i1 > -1 && i2 == -1 && (i1 + 2) == i3)
            {
                symbol = symbol.Substring(0, i1) + "-" + symbol.Substring(i1 + 1);// symbol.<letter>
                return symbol;
            }
            if (i1 > i2)
            {
                symbol = symbol.Substring(0, i1) + symbol.Substring(i1 + 1);// symbol-WT.<letter>*
                return symbol;
            }
            return symbol;
        }

        private static string GetEoddataSymbol(string symbol)
        {
            // complex
            if (symbol.EndsWith(".RT/WI")) return symbol.Replace(".RT/WI", ".V"); // or .V or .R
            if (symbol.EndsWith(".PR")) return symbol.Replace(".PR", ".P");
            if (symbol.EndsWith(".WS")) return symbol.Replace(".WS", ".W");
            if (symbol.EndsWith(".RT")) return symbol.Replace(".RT", ".R");
            if (symbol.IndexOf(".PR.") > 0) return symbol.Replace(".PR.", "-");
            if (symbol.IndexOf(".WS.") > 0) return symbol.Replace(".WS.", ".");
            return symbol;
        }

        private static string GetMsnSymbol(string symbol)
        {
            // complex
            if (symbol.EndsWith(".WS")) return "?";
            if (symbol.IndexOf(".WS.") > 0) return "?";
            if (symbol.EndsWith("/WI")) return symbol.Replace("/WI", "*");
            if (symbol.EndsWith(".PR")) return symbol.Replace(".PR", "-");
            if (symbol.IndexOf(".PR.") > 0) return symbol.Replace(".PR.", "-");
            if (symbol.EndsWith(".RT")) return symbol.Replace(".RT", "_RT");
            if (symbol.Contains("\\")) return "?";
            return symbol;
        }

        private static string GetGoogleSymbol(string symbol)
        {
            if (symbol.EndsWith(".PR")) symbol = symbol.Replace(".PR", "-");
            if (symbol.Contains(".PR.")) symbol = symbol.Replace(".PR.", "-");
            if (symbol.EndsWith(".RT")) { };// .RT or/and .RT*
            if (symbol.EndsWith(".WS")) return "?";
            if (symbol.Contains(".WS.")) return "?";
            if (symbol.EndsWith("/WI")) symbol = symbol.Replace("/WI", "*");
            if (symbol.Contains("/WI.")) return "?";
            if (symbol.EndsWith("/WD")) symbol = symbol.Replace("/WD", ".WD");
            if (symbol.EndsWith(".PR/WD")) symbol = symbol.Replace(".PR/WD", "-WD");
            if (symbol.EndsWith("/CL"))
            {
                if (symbol.Contains("-")) symbol = symbol.Replace("/CL", ".CL");
                else symbol = symbol.Replace("/CL", "-CL");
            }
            if (symbol.Contains("\\")) return "?";
            return symbol;
        }
    }
}
