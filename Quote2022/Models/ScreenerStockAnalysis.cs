using System;

namespace Quote2022.Models
{
    public class ScreenerStockAnalysis
    {
        public string Symbol;
        public string Exchange;
        public string Name;
        public string Sector;
        public string Industry;
        public string CapGroup;
        public float? Price;
        public float? Volume;
        public float? MarketCap;
        public float? PeForward;
        public float? DivYield;
        public DateTime TimeStamp;

        public class cRawData
        {
            public int status;
            public cData data;

        }
        public class cData
        {
            public object[][] data;

        }
    }
}
