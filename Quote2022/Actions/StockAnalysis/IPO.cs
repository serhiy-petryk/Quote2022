using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Quote2022.Actions.StockAnalysis
{
    public class IPO
    {
        public static void Start(bool saveToFile, Action<string> ShowStatus)
        {
            ShowStatus($"StockAnalysis.IPO is started");

            var timeStamp = Helpers.CsUtils.GetTimeStamp();
            var url = "https://stockanalysis.com/api/screener/s/f?m=ipoDate&s=desc&c=ipoDate,s,n,ipoPrice,ippc,exchange&cn=5000&i=histip";
            var content = Download.GetResponse(url);
            var filename = $@"E:\Quote\WebData\Symbols\Stockanalysis\IPO\Ipo_{timeStamp.Item2}.json";
            if (saveToFile)
            {
                File.WriteAllText(filename, content);
            }
            var oo = JsonConvert.DeserializeObject<cRoot>(content);
            foreach (var item in oo.data.data)
                item.TimeStamp = timeStamp.Item1;

            SaveToDb.ClearAndSaveToDbTable(oo.data.data, "dbQuote2023..Bfr_IPOStockAnalysis", "Symbol", "Date", "Exchange",
                "Name", "IpoPrice", "CurrentPrice", "TimeStamp");

            SaveToDb.RunProcedure("dbQuote2023..pUpdateIPOStockAnalysis", new Dictionary<string, object> { { "@TimeStamp", timeStamp.Item1 } });

            if (saveToFile)
                ShowStatus($"StockAnalysis.IPO finished. File: {filename}");
            else
                ShowStatus($"StockAnalysis.IPO finished. No file");
        }

        public class cRoot
        {
            public int status;
            public cData data;
        }

        #region ==========  Json classes  ============
        public class cData
        {
            public int resultsCount;
            public cItem[] data;
        }
        public class cItem
        {
            public string s;
            public DateTime ipoDate;
            public string exchange;
            public string n;
            public float ipoPrice;
            public float ippc;

            public string Symbol => s;
            public DateTime Date => ipoDate;
            public string Exchange => exchange;
            public string Name => n;
            public float IpoPrice => ipoPrice;
            public float CurrentPrice => ippc;
            public DateTime TimeStamp;
        }
        #endregion
    }
}
