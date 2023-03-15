using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Data.Helpers;
using Newtonsoft.Json;

namespace Data.Actions.Nasdaq
{
    public class NasdaqScreenerLoader
    {
        private static string stockUrl = @"https://api.nasdaq.com/api/screener/stocks?tableonly=true&download=true";
        private static string etfUrl = @"https://api.nasdaq.com/api/screener/etf?tableonly=true&download=true";

        public static void Start(Action<string> logEvent)
        {
            logEvent($"NasdaqScreenerLoader started");

            var timeStamp = CsUtils.GetTimeStamp();
            var folder = $@"E:\Quote\WebData\Screener\Nasdaq\NasdaqScreener_{timeStamp.Item2}\";

            // Download data
            var stockFile =  folder + $@"ScreenerStock_{timeStamp.Item2}.json";
            logEvent($"NasdaqScreenerLoader. Download STOCK data from {stockUrl} to {stockFile}");
            Helpers.Download.DownloadPage(stockUrl, stockFile, true);

            var etfFile = folder + $@"ScreenerEtf_{timeStamp.Item2}.json";
            logEvent($"NasdaqScreenerLoader. Download ETF data from {etfUrl} to {etfFile}");
            Helpers.Download.DownloadPage(etfUrl, etfFile, true);

            // Parse and save data to database
            logEvent($"NasdaqScreenerLoader. Parse and save files to database");
            var itemsCount = Parse(stockFile, timeStamp.Item1);
            itemsCount += Parse(etfFile, timeStamp.Item1);

            // Zip data and remove text files
            var zipFilename = CsUtils.ZipFolder(folder);
            Directory.Delete(folder);

            logEvent($"!NasdaqScreenerLoader finished. Filename: {zipFilename} with {itemsCount} items");
        }

        private static int Parse(string filename, DateTime timeStamp)
        {
            var deserializerSettings = new JsonSerializerSettings
            {
                Culture = System.Globalization.CultureInfo.InvariantCulture
            };

            var stockItems = new List<cStockRow>();
            var etfItems = new List<cEtfRow>();

            if (Path.GetFileName(filename).EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
            {
                string lastLine = null;
                foreach (var line in File.ReadAllLines(filename))
                {
                    if (lastLine == null)
                    {
                        if (!Equals(line, "Symbol,Name,Last Sale,Net Change,% Change,Market Cap,Country,IPO Year,Volume,Sector,Industry"))
                            throw new Exception($"Invalid Nasdaq stock screener file structure! {Path.GetFileName(filename)}");
                        lastLine = line;
                        continue;
                    }
                    if (Equals(lastLine, line)) continue;

                    stockItems.Add(new cStockRow(line));
                    lastLine = line;
                }
            }

            else if (Path.GetFileNameWithoutExtension(filename)
                         .IndexOf("stock", StringComparison.InvariantCultureIgnoreCase) != -1 &&
                     Path.GetExtension(filename) == ".json")
            {
                var oStock = JsonConvert.DeserializeObject<cStockRoot>(File.ReadAllText(filename), deserializerSettings);
                stockItems = oStock.data.rows.ToList();
            }

            else if (Path.GetFileNameWithoutExtension(filename)
                         .IndexOf("etf", StringComparison.InvariantCultureIgnoreCase) != -1 &&
                     Path.GetExtension(filename) == ".json")
            {
                var oEtf = JsonConvert.DeserializeObject<cEtfRoot>(File.ReadAllText(filename), deserializerSettings);
                etfItems = oEtf.data.data.rows.ToList();
            }
            else
                throw new Exception($"Nasdaq.ScreenerLoader.Parse. '{Path.GetFileName(filename)}' file is invalid");

            if (stockItems.Count > 0)
            {
                foreach (var item in stockItems)
                    item.TimeStamp = timeStamp;

                DbUtils.ClearAndSaveToDbTable(stockItems, "Bfr_ScreenerNasdaqStock", "symbol", "Name", "LastSale",
                    "Volume", "netChange", "Change", "MarketCap", "Country", "ipoYear", "Sector", "Industry", "TimeStamp");
                DbUtils.RunProcedure("pUpdateScreenerNasdaqStock", new Dictionary<string, object> { { "@Date", timeStamp } });
            }
            if (etfItems.Count > 0)
            {
                foreach (var item in etfItems)
                    item.TimeStamp = timeStamp;

                DbUtils.ClearAndSaveToDbTable(etfItems, "Bfr_ScreenerNasdaqEtf", "symbol", "Name",
                    "LastSale", "netChange", "Change", "TimeStamp");
                DbUtils.RunProcedure("pUpdateScreenerNasdaqEtf", new Dictionary<string, object> { { "@Date", timeStamp } });
            }

            return stockItems.Count + etfItems.Count;
        }

        #region =========  Json classes  ============
        //========================
        private static CultureInfo culture = new CultureInfo("en-US");
        public class cStockRoot
        {
            public cStockData data;
            public object message;
            public cStatus status;
        }
        public class cStockData
        {
            public cStockRow[] rows;
        }
        public class cStockRow
        {
            // "symbol", "name", "LastSale", "Volume", "netChange", "Change", "MarketCap", "country", "ipoYear", "sector", "industry", "timeStamp"
            // github: symbol, name, lastsale, volume, netchange, pctchange, marketCap, country, ipoyear, industry, sector, url
            public string symbol;
            public string name;
            public string lastSale;
            public long volume;
            public float? netChange;
            public string pctChange;
            public float? marketCap;
            public string country;
            public short? ipoYear;
            public string sector;
            public string industry;
            public DateTime TimeStamp;

            public string Exchange;
            public string Name => NullCheck(name);
            public float? LastSale => lastSale == "NA" ? (float?)null : float.Parse(lastSale, NumberStyles.Any, culture);
            public float Volume => Convert.ToSingle(Math.Round(volume / 1000000.0, 3));
            public float? Change => string.IsNullOrEmpty(pctChange) ? (float?)null : float.Parse(pctChange.Replace("%", ""), culture);
            public float? MarketCap => marketCap.HasValue
                ? Convert.ToSingle(Math.Round(marketCap.Value / 1000000.0, 3))
                : (float?)null;
            public string Country => NullCheck(country);
            public string Sector => NullCheck(sector);
            public string Industry => NullCheck(industry);

            public cStockRow() { }

            public cStockRow(string fileLine)
            {
                var ss = fileLine.Split(',');
                symbol = NullCheck(ss[0]);
                name = ss[1];
                lastSale = ss[2];
                marketCap = NullCheck(ss[5]) == null ? (long?)null : Convert.ToInt64(decimal.Parse(ss[5], culture));
                country = ss[6];
                ipoYear = NullCheck(ss[7]) == null ? (short?)null : short.Parse(ss[7]);
                volume = long.Parse(ss[8], culture);
                sector = ss[9];
                industry = ss[10];
            }
        }

        private static string NullCheck(string s) => string.IsNullOrEmpty(s) ? null : s.Trim();

        //========================
        public class cEtfRoot
        {
            public cEtfData data;
            public object message;
            public cStatus status;
        }
        public class cEtfData
        {
            public string dataAsOf;
            public cEtfData2 data;
        }
        public class cEtfData2
        {
            public cEtfRow[] rows;
        }
        public class cEtfRow
        {
            // "symbol", "name", "LastSale", "netChange", "Change", "TimeStamp"
            public string symbol;
            public string companyName;
            public string lastSalePrice;
            public float netChange;
            public string percentageChange;
            public DateTime TimeStamp;
            public string Name => NullCheck(companyName);
            public float LastSale => float.Parse(lastSalePrice, NumberStyles.Any, culture);
            public float? Change => string.IsNullOrEmpty(percentageChange) ? (float?)null : float.Parse(percentageChange.Replace("%", ""), culture);
        }

        //=====================
        public class cStatus
        {
            public int rCode;
            public object bCodeMessage;
            public string developerMessage;
        }

        #endregion
    }
}
