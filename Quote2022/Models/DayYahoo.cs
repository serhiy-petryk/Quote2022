using System;
using System.Globalization;

namespace Quote2022.Models
{
    public class DayYahoo
    {
        public string Symbol;
        public DateTime Date;
        public decimal Open;
        public decimal High;
        public decimal Low;
        public decimal Close;
        public long Volume;
        public decimal AdjClose;

        public DayYahoo(string symbol, string[] ss)
        {
            Symbol = symbol;
            Date = DateTime.Parse(ss[0].Trim(), CultureInfo.InvariantCulture);
            Open = Decimal.Parse(ss[1].Trim(), CultureInfo.InvariantCulture);
            High = Decimal.Parse(ss[2].Trim(), CultureInfo.InvariantCulture);
            Low = Decimal.Parse(ss[3].Trim(), CultureInfo.InvariantCulture);
            Close = Decimal.Parse(ss[4].Trim(), CultureInfo.InvariantCulture);
            Volume = long.Parse(ss[6].Trim(), CultureInfo.InvariantCulture);
            AdjClose = Decimal.Parse(ss[5].Trim(), CultureInfo.InvariantCulture);
        }
    }
}
