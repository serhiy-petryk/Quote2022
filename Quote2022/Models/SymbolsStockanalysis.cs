using System;

namespace Quote2022.Models
{
    public class SymbolsStockanalysisExchanges
    {
        public string Symbol;
        public string Exchange;
        public DateTime Created;

        public SymbolsStockanalysisExchanges(string symbol, string exchange, DateTime created)
        {
            Symbol = symbol;
            Exchange = exchange;
            Created = created;
        }
    }
}
