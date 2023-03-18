using System;

namespace Data.Models
{
    public class SymbolsEoddata
    {
        public string Symbol;
        public string Exchange;
        public string Name;
        public DateTime TimeStamp;

        public SymbolsEoddata(string exchange, DateTime timeStamp, string[] ss)
        {
            this.Exchange = exchange.ToUpper();
            Symbol = ss[0].Trim().ToUpper();
            Name = ss[1].Trim();
            if (string.IsNullOrEmpty(Name)) Name = null;
            TimeStamp = timeStamp;
        }

        // public override string ToString() => this.Symbol + "\t" + this.Exchange + "\t" + this.Name + "\t" + CsUtils.GetString(this.Created);
    }
}
