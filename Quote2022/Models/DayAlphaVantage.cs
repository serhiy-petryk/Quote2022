using System;
using System.Globalization;

namespace Quote2022.Models
{
    public class DayAlphaVantage
    {
        public string Symbol;
        public DateTime Date;
        public float Open;
        public float High;
        public float Low;
        public float Close;
        public long Volume;
        public float? Dividend;
        public float? Split;

        public DayAlphaVantage(string symbol, string line)
        {
            Symbol = symbol;
            var ss = line.Split(',');
            Date = DateTime.Parse(ss[0], CultureInfo.InvariantCulture);
            Open = float.Parse(ss[1], CultureInfo.InvariantCulture);
            High = float.Parse(ss[2], CultureInfo.InvariantCulture);
            Low = float.Parse(ss[3], CultureInfo.InvariantCulture);
            Close = float.Parse(ss[4], CultureInfo.InvariantCulture);
            Volume = long.Parse(ss[6], CultureInfo.InvariantCulture);
            Dividend = float.Parse(ss[7], CultureInfo.InvariantCulture);
            if (Math.Abs(Dividend.Value) < 0.000001)
                Dividend = null;
            Split = float.Parse(ss[8], CultureInfo.InvariantCulture);
            if (Math.Abs(Split.Value - 1.0) < 0.000001)
                Split = null;
        }
    }
}
