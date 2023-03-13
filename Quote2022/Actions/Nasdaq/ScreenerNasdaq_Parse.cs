using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Quote2022.Helpers;

namespace Quote2022.Actions.Nasdaq
{
    public static class ScreenerNasdaq_Parse
    {
        private static CultureInfo culture = new CultureInfo("en-US");
        public static void Start(string zipFileName, Action<string> showStatusAction)
        {
            var deserializerSettings = new JsonSerializerSettings
            {
                Culture = System.Globalization.CultureInfo.InvariantCulture
            };

            showStatusAction($"ScreenerNasdaq_Parse started. File: {zipFileName}");

            using (var zip = new ZipReader(zipFileName))
                foreach (var entry in zip)
                {
                    if (entry.Length > 0)
                    {
                        var ss = entry.FileNameWithoutExtension.Split('_');
                        var stockItems = new List<cStockRow>();
                        var etfItems = new List<cEtfRow>();
                        var timeStamp = DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMdd", CultureInfo.InvariantCulture);
                        if (entry.FullName.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string lastLine = null;
                            foreach (var line in entry.AllLines)
                            {
                                if (lastLine == null)
                                {
                                    if (!Equals(line, "Symbol,Name,Last Sale,Net Change,% Change,Market Cap,Country,IPO Year,Volume,Sector,Industry"))
                                        throw new Exception($"Invalid Nasdaq stock screener file structure! {entry.FullName}");
                                    lastLine = line;
                                    continue;
                                }
                                if (Equals(lastLine, line)) continue;

                                stockItems.Add(new cStockRow(line));
                                lastLine = line;
                            }
                        }

                        else if (entry.FileNameWithoutExtension.IndexOf("stock",
                                     StringComparison.InvariantCultureIgnoreCase) != -1 &&
                                 entry.FullName.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var oStock = JsonConvert.DeserializeObject<cStockRoot>(entry.Content, deserializerSettings);
                            stockItems = oStock.data.rows.ToList();
                        }

                        else if (entry.FileNameWithoutExtension.IndexOf("etf",
                                     StringComparison.InvariantCultureIgnoreCase) != -1 &&
                                 entry.FullName.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var oEtf = JsonConvert.DeserializeObject<cEtfRoot>(entry.Content, deserializerSettings);
                            etfItems = oEtf.data.data.rows.ToList();
                        }
                        else
                            throw new Exception($"ScreenerNasdaq_Parse. '{entry.FileNameWithoutExtension}' filename is invalid in '{Path.GetFileName(zipFileName)}' zip file ");

                        if (stockItems.Count > 0)
                        {
                            foreach (var item in stockItems)
                                item.TimeStamp = timeStamp;

                            SaveToDb.ClearAndSaveToDbTable(stockItems, "Bfr_ScreenerNasdaqStock", "symbol", "Name", "LastSale",
                                "Volume", "netChange", "Change", "MarketCap", "Country", "ipoYear", "Sector", "Industry", "TimeStamp");
                            SaveToDb.RunProcedure("pUpdateScreenerNasdaqStock", new Dictionary<string, object> { { "@Date", timeStamp } });
                        }
                        if (etfItems.Count > 0)
                        {
                            foreach (var item in etfItems)
                                item.TimeStamp = timeStamp;

                            SaveToDb.ClearAndSaveToDbTable(etfItems, "Bfr_ScreenerNasdaqEtf", "symbol", "Name",
                                "LastSale", "netChange", "Change", "TimeStamp");
                            SaveToDb.RunProcedure("pUpdateScreenerNasdaqEtf", new Dictionary<string, object> { { "@Date", timeStamp } });
                        }
                    }
                }

            showStatusAction($"ScreenerNasdaq_Parse: Finished! File: {zipFileName}");
        }

        //========================
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
            public float? LastSale => lastSale == "NA" ? (float?)null :float.Parse(lastSale, NumberStyles.Any, culture);
            public float Volume => Convert.ToSingle(Math.Round(volume / 1000000.0, 3));
            public float? Change => string.IsNullOrEmpty(pctChange) ? (float?)null : float.Parse(pctChange.Replace("%", ""), culture);
            public float? MarketCap => marketCap.HasValue
                ? Convert.ToSingle(Math.Round(marketCap.Value / 1000000.0, 3))
                : (float?) null;
            public string Country => NullCheck(country);
            public string Sector => NullCheck(sector);
            public string Industry => NullCheck(industry);

            public cStockRow(){}

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
    }
}
