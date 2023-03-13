using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Data.Helpers;
using Data.Models;
using Newtonsoft.Json;

namespace Data.Actions.TradingView
{
    public class ScreenerLoader
    {
        private const string parameters = @"{""filter"":[{""left"":""exchange"",""operation"":""in_range"",""right"":[""AMEX"",""NASDAQ"",""NYSE""]}],""options"":{""lang"":""en""},""markets"":[""america""],""symbols"":{""query"":{""types"":[]},""tickers"":[]},""columns"":[""minmov"",""name"",""close"",""change"",""change_abs"",""Recommend.All"",""volume"",""Value.Traded"",""market_cap_basic"",""price_earnings_ttm"",""earnings_per_share_basic_ttm"",""number_of_employees"",""sector"",""industry"",""description"",""type"",""subtype""],""sort"":{""sortBy"":""name"",""sortOrder"":""asc""},""range"":[0,20000]}";

        public static void Start(Action<string> showStatus)
        {
            showStatus($"TradingView.ScreenerLoader started");

            // Download
            var timeStamp = Helpers.csUtils.GetTimeStamp();
            var filename = $@"E:\Quote\WebData\Screener\TradingView\TVScreener_{timeStamp.Item2}.json";

            showStatus($"TradingView.ScreenerLoader. Download data to {filename}");
            if (!File.Exists(filename))
              Helpers.Download.DownloadPage_POST(@"https://scanner.tradingview.com/america/scan", filename, parameters);

            // Parse and save data to database
            showStatus($"TradingView.ScreenerLoader. Parse and save files to database");
            Parse(File.ReadAllText(filename), File.GetCreationTime(filename));

            // Zip data and remove text files
            var zipFilename = csUtils.ZipFile(filename);
            File.Delete(filename);

            showStatus($"TradingView.ScreenerLoader finished. Filename: {zipFilename}");
        }

        private static void Parse(string content, DateTime timeStamp)
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
        }
    }
}
