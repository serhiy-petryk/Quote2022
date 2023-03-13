using System;
using Quote2022.Helpers;

namespace Quote2022.Models
{
    public class SymbolsEoddata
    {
        public string Symbol;
        public string Exchange;
        public string Name;
        public DateTime Created;

        public SymbolsEoddata(string exchange, DateTime created, string[] ss)
        {
            this.Exchange = exchange.ToUpper();
            Symbol = ss[0].Trim().ToUpper();
            Name = ss[1].Trim();
            if (string.IsNullOrEmpty(Name)) Name = null;
            Created = created;
        }

        public override string ToString() => this.Symbol + "\t" + this.Exchange + "\t" + this.Name + "\t" + CsUtils.GetString(this.Created);

    }
}
