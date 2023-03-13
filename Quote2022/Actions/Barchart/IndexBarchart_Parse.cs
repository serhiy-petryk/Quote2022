using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Quote2022.Actions.Barchart
{
    public static class IndexBarchart_Parse
    {

        public static void ParseSP600()
        {
            var filename = @"E:\Quote\WebData\Indices\Barchart\SP600\BarchartSP600_20230301.txt";
            var settings = new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture
            };

            var oo = JsonConvert.DeserializeObject<cRoot>(File.ReadAllText(filename), settings);
            var timeStamp = File.GetCreationTime(filename);
            var key = Path.GetFileName(Path.GetDirectoryName(filename));
            foreach (var o in oo.data.Select(a => a.raw))
            {
                o.Screener = key;
                o.TimeStamp = timeStamp;
            }

            SaveToDb.SaveToDbTable(oo.data.Select(a => a.raw), "dbQuote2023..ScreenerBarchart", "Screener",
                "symbol", "exchange", "Name", "High", "Low", "lastPrice", "Volume", "High1y", "Low1y", "Volume20d",
                "MarketCap", "opinion", "symbolCode", "symbolType", "TradeTime", "TimeStamp");
        }

        public static void ParseRussell3000()
        {
            var filename = @"E:\Quote\WebData\Indices\Barchart\Russell3000\BarchartRussell3000_20230227.txt";
            var settings = new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture
            };

            var oo = JsonConvert.DeserializeObject<cRoot>(File.ReadAllText(filename), settings);
            var timeStamp = File.GetCreationTime(filename);
            foreach (var o in oo.data.Select(a => a.raw))
            {
                o.Screener = "Russell3000";
                o.TimeStamp = timeStamp;
            }

            SaveToDb.SaveToDbTable(oo.data.Select(a => a.raw), "dbQuote2023..ScreenerBarchart", "Screener",
                "symbol", "exchange", "Name", "High", "Low", "lastPrice", "Volume", "High1y", "Low1y", "Volume20d",
                "MarketCap", "opinion", "symbolCode", "symbolType", "TradeTime", "TimeStamp");
        }

        public static void ParseScreener()
        {
            // var filename = @"E:\Quote\WebData\Indices\Barchart\Russell3000\Barchart.Russell3000.Resonse.txt";
            var filenames = Directory.GetFiles(@"E:\Quote\WebData\Indices\Barchart\Russell3000", "BarchartScreenerFrom2010_20230227_page*.txt");
            foreach (var filename in filenames)
            {
                //   var filename =@"E:\Quote\WebData\Indices\Barchart\Russell3000\BarchartScreenerFrom2010_20230227_page4.txt";
                var settings = new JsonSerializerSettings
                {
                    Culture = CultureInfo.InvariantCulture
                };
                var oo = JsonConvert.DeserializeObject<cRoot>(File.ReadAllText(filename), settings);
                var o1 = oo.data.Where(a => a.symbolCode != "STK").ToList();
                var o2 = oo.data.Where(a => a.symbolType != 1).ToList();

                var timeStamp = File.GetCreationTime(filename);
                foreach (var o in oo.data.Select(a => a.raw))
                {
                    o.Screener = "Screener";
                    o.TimeStamp = timeStamp;
                }

                SaveToDb.SaveToDbTable(oo.data.Select(a => a.raw), "dbQuote2023..ScreenerBarchart", "Screener",
                    "symbol", "exchange", "Name", "High", "Low", "lastPrice", "Volume", "High1y", "Low1y", "Volume20d",
                    "MarketCap", "opinion", "symbolCode", "symbolType", "TradeTime", "TimeStamp");
            }
        }

        public class DbItemRaw: cItemRaw
        {
            public string screener;
            public DateTime timeStamp;
        }

        public class cRoot
        {
            public int count;
            public int total;
            public cItem[] data;
            public object error;
        }

        public class cItem
        {
            public string symbol;
            public string exchange;
            public string symbolName;
            public string highPrice;
            public string lowPrice;
            public float lastPrice;
            public string volume;
            public string highPrice1y;
            public string lowPrice1y;
            public string averageVolume20d;
            public string marketCap;
            public string opinion;
            public string symbolCode;
            public int symbolType;
            public string tradeTime;
            public cItemRaw raw;
        }
        public class cItemRaw
        {
            public string Screener;
            public string symbol;
            public string exchange;
            public string symbolName;
            public string Name => symbolName;
            public float highPrice;
            public float High => highPrice;
            public float lowPrice;
            public float Low => lowPrice;
            public float lastPrice;
            public long? volume;
            public float? Volume => volume.HasValue ? Convert.ToSingle(Math.Round(volume.Value / 1000000.0, 3)) : (float?)null;
            public float highPrice1y;
            public float High1y => highPrice1y;
            public float lowPrice1y;
            public float Low1y => lowPrice1y;
            public long? averageVolume20d;
            public float? Volume20d => averageVolume20d.HasValue ? Convert.ToSingle(Math.Round(averageVolume20d.Value / 1000000.0, 3)) : (float?)null;
            public long? marketCap;
            public float? MarketCap => marketCap.HasValue ? Convert.ToSingle(Math.Round(marketCap.Value / 1000000.0, 3)) : (float?)null;
            public short? opinion;
            public string symbolCode;
            public int symbolType;
            public long tradeTime;
            public DateTime TradeTime => new DateTime(1970, 1, 1).AddSeconds(tradeTime - 18000);
            public DateTime TimeStamp;
        }
    }
}
