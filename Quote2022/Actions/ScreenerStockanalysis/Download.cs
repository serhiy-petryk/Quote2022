using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;

namespace Quote2022.Actions.ScreenerStockAnalysis
{
    public static class ScreenerStockAnalysis_Download
    {
        private static Dictionary<string, Action<object, Models.ScreenerStockAnalysis>> parts =
            new Dictionary<string, Action<object, Models.ScreenerStockAnalysis>>
            {
                {"exchange", (o, analysis) => analysis.Exchange = (string) o},
                {"n", (o, analysis) => analysis.Name = (string) o},
                {"sector", (o, analysis) => analysis.Sector = (string) o},
                {"industry", (o, analysis) => analysis.Industry = (string) o},
                {"marketCapCategory", (o, analysis) => analysis.CapGroup = (string) o},
                {"price", (o, analysis) => analysis.Price = Convert.ToSingle(o, CultureInfo.InvariantCulture)},
                {"volume", (o, analysis) => analysis.Volume = Convert.ToSingle(Math.Round(Convert.ToDouble(o, CultureInfo.InvariantCulture) / 1000000.0,3))},
                {"marketCap", (o, analysis) => analysis.MarketCap = Convert.ToSingle(Math.Round(Convert.ToDouble(o, CultureInfo.InvariantCulture) / 1000000.0,3))},
                {"peForward", (o, analysis) => analysis.PeForward = Convert.ToSingle(o, CultureInfo.InvariantCulture)},
                {"dividendYield", (o, analysis) => analysis.DivYield = Convert.ToSingle(o, CultureInfo.InvariantCulture)
                }
            };

        private static string urlTemplate = @"https://stockanalysis.com/api/screener/s/d/{0}.json";
        private static string fileTemplate = @"E:\Quote\WebData\Screener\StockAnalysis\ScreenerStockAnalysis_{1}\{0}_{1}.json";

        public static void Start(Action<string> showStatusAction)
        {
            showStatusAction($"ScreenerStockAnalysis_Download. Started.");
            var timeStamp = DateTime.Now.AddHours(-12).Date.ToString("yyyyMMdd");
            var urlsAndFilename = new List<Tuple<string, string>>();
            /*foreach (var kvp in xx)
            {
                var url = string.Format(urlTemplate, kvp.Key);
                var filename = string.Format(fileTemplate, kvp.Key, timeStamp);
                var folder = Path.GetDirectoryName(filename);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                Download.DownloadPage(url, filename);
            }*/

            var items = new Dictionary<string, Models.ScreenerStockAnalysis>();
            foreach (var kvp in parts)
            {
                var filename = string.Format(fileTemplate, kvp.Key, timeStamp);
                var oo = JsonConvert.DeserializeObject<Models.ScreenerStockAnalysis.cRawData>(File.ReadAllText(filename));
                var fileTimeStamp = File.GetCreationTime(filename);
                foreach (var o in oo.data.data)
                {
                    var symbol = (string) o[0];
                    if (!items.ContainsKey(symbol))
                        items.Add(symbol, new Models.ScreenerStockAnalysis {Symbol = symbol, TimeStamp = fileTimeStamp});

                    kvp.Value(o[1], items[symbol]);
                    if (!Equals(o[2], "N"))
                        throw new Exception("Check");
                }
            }

            if (items.Count > 0)
            {
                SaveToDb.ClearAndSaveToDbTable(items.Values, "dbQuote2023..ScreenerStockAnalysis", "Symbol", "Exchange",
                    "Name", "Sector", "Industry", "CapGroup", "Price", "Volume", "MarketCap", "PeForward", "DivYield",
                    "TimeStamp");
                items.Clear();
            }


            showStatusAction($"ScreenerStockAnalysis_Download. Finished.");
        }

    }
    }
