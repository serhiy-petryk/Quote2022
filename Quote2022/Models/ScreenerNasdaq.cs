using System;
using System.Globalization;

namespace Quote2022.Models
{
    public class ScreenerNasdaq
    {
        private static CultureInfo culture = new CultureInfo("en-US");
        private const NumberStyles numberStyles = NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;
        private const NumberStyles numberStyles2 = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;
        private const NumberStyles numberStyles3 = NumberStyles.Any;

        //Symbol,Name,Last Sale,Net Change,% Change,Market Cap,Country,IPO Year,Volume,Sector,Industry
        public DateTime TimeStamp;
        public string Symbol;
        public string Name;
        public float LastSale;
        public float NetChange;
        public float? Change;
        public long? MarketCap;
        public string Country;
        public short? IPOYear;
        public long Volume;
        public string Sector;
        public string Industry;

        public ScreenerNasdaq(DateTime timeStamp, string fileLine)
        {
            TimeStamp = timeStamp;
            var ss = fileLine.Split(',');
            Symbol = NullCheck(ss[0]);
            Name = NullCheck(ss[1]);
            LastSale = float.Parse(ss[2], numberStyles3, culture);
            NetChange = float.Parse(ss[3], numberStyles3, culture);
            Change = NullCheck(ss[4]) == null ? (float?)null : float.Parse(ss[4].Replace("%",""), numberStyles3, culture);
            MarketCap = NullCheck(ss[5]) == null ? (long?)null : Convert.ToInt64(decimal.Parse(ss[5], culture));
            Country = NullCheck(ss[6]);
            IPOYear = NullCheck(ss[7]) ==null ? (short?)null : short.Parse(ss[7]);
            Volume = long.Parse(ss[8], culture);
            Sector = NullCheck(ss[9]);
            Industry = NullCheck(ss[10]);
        }

        private static string NullCheck(string s) => string.IsNullOrEmpty(s) ? null : s.Trim();
    }
}
