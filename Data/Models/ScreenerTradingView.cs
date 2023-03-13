using System;

namespace Data.Models
{
    public class ScreenerTradingView
    {
        public int totalCount;
        public Item[] data;

        public class Item
        {
            public string s;
            public object[] d;

            public DbItem GetDbItem(DateTime timeStamp)
            {
                var ss = s.Split(':');
                if (!string.Equals(ss[1], (string)d[1]))
                    throw new Exception("Check 'Symbol' in ScreenerTradingView.Item.");

                var item = new DbItem
                {
                    Exchange = ss[0], Symbol = ss[1], Name = CheckNull((string) d[14]),
                    Type = CheckNull((string) d[15]), Subtype = CheckNull((string) d[16]),
                    Sector = CheckNull((string) d[12]), Industry = CheckNull((string) d[13]),
                    Close = Convert.ToSingle(d[2]),
                    Volume = Convert.ToSingle(Math.Round(Convert.ToDouble(d[6])/1000000.0,3)),
                    MarketCap = d[8] == null ? (float?) null : Convert.ToSingle(Math.Round(Convert.ToDouble(d[8]) / 1000000.0, 3)),
                    Recommend = Convert.ToSingle(d[5]),
                    TimeStamp = timeStamp
                };
                return item;

                string CheckNull(string s) => string.IsNullOrEmpty(s) ? null : s;
            }
        }

        public class DbItem
        {
            public string Exchange;
            public string Symbol;
            public string Name;
            public string Type;
            public string Subtype;
            public string Sector;
            public string Industry;
            public float Close;
            public float Volume;
            public float? MarketCap;
            public float Recommend;
            public DateTime TimeStamp;
        }
    }
}
