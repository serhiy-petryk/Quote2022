using System;

namespace Quote2022.Models
{
    public class SymbolsQuantumonline
    {
        public string SymbolKey;
        public string Symbol;
        public string Exchange;
        public string Name;
        public string HtmlName;
        public string Url;
        public bool IsDead => SymbolKey.EndsWith("*");
        public DateTime TimeStamp;
        public int MinLevel = int.MaxValue;

        public SymbolsQuantumonline(string symbolKey, string exchange, string htmlName, string url, DateTime timeStamp)
        {
            SymbolKey = symbolKey;
            Symbol = SymbolKey;
            while (Symbol.EndsWith("*"))
                Symbol = Symbol.Substring(0, Symbol.Length - 1);
            Exchange = exchange;
            HtmlName = htmlName;
            Url = url;
            TimeStamp = timeStamp;

            var ss = HtmlName.Split(new[] { "More results" }, StringSplitOptions.RemoveEmptyEntries);
            Name = ss[ss.Length - 1].Trim();
        }

        public override string ToString()
        {
            return $"{Symbol}^{Exchange}^{Name}^{IsDead}";
        }
    }
}
