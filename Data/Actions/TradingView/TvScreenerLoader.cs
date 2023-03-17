using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data.Helpers;
using Data.Models;
using Newtonsoft.Json;

namespace Data.Actions.TradingView
{
    public class TvScreenerLoader
    {
        private const string parameters = @"{""filter"":[{""left"":""exchange"",""operation"":""in_range"",""right"":[""AMEX"",""NASDAQ"",""NYSE""]}],""options"":{""lang"":""en""},""markets"":[""america""],""symbols"":{""query"":{""types"":[]},""tickers"":[]},""columns"":[""minmov"",""name"",""close"",""change"",""change_abs"",""Recommend.All"",""volume"",""Value.Traded"",""market_cap_basic"",""price_earnings_ttm"",""earnings_per_share_basic_ttm"",""number_of_employees"",""sector"",""industry"",""description"",""type"",""subtype""],""sort"":{""sortBy"":""name"",""sortOrder"":""asc""},""range"":[0,20000]}";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            // Download
            var timeStamp = Helpers.CsUtils.GetTimeStamp();
            var filename = $@"E:\Quote\WebData\Screener\TradingView\TVScreener_{timeStamp.Item2}.json";

            Logger.AddMessage($"Download data to {filename}");
            if (!File.Exists(filename))
              Helpers.Download.DownloadPage_POST(@"https://scanner.tradingview.com/america/scan", filename, parameters);

            // Parse and save data to database
            Logger.AddMessage($"Parse and save files to database");
            var itemCount = Parse(File.ReadAllText(filename), File.GetLastWriteTime(filename));

            // Zip data and remove text files
            var zipFilename = CsUtils.ZipFile(filename);
            File.Delete(filename);

            Logger.AddMessage($"!Finished. Filename: {zipFilename} with {itemCount} items");
        }

        private static int Parse(string content, DateTime timeStamp)
        {
            var o = JsonConvert.DeserializeObject<ScreenerTradingView>(content);
            var items = o.data.Select(a => a.GetDbItem(timeStamp)).ToArray();

            if (items.Length > 0)
            {
                DbUtils.ClearAndSaveToDbTable(items, "Bfr_ScreenerTradingView", "Symbol", "Exchange", "Name",
                    "Type", "Subtype", "Sector", "Industry", "Close", "MarketCap", "Volume", "Recommend",
                    "TimeStamp");
                DbUtils.RunProcedure("pUpdateScreenerTradingView", new Dictionary<string, object> { { "@Date", timeStamp } });
            }

            return items.Length;
        }
    }
}
