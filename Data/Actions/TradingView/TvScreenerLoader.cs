using System.IO;
using System.IO.Compression;
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
            Helpers.Download.DownloadPage_POST(@"https://scanner.tradingview.com/america/scan", filename, parameters);

            // Zip data
            var zipFileName = ZipUtils.ZipFile(filename);

            // Parse and save data to database
            Logger.AddMessage($"Parse and save files to database");
            var itemCount = ParseAndSaveToDb(zipFileName);

            // Remove text files
            File.Delete(filename);

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        private static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries)
                    if (entry.Length > 0)
                    {
                        var o = JsonConvert.DeserializeObject<ScreenerTradingView>(entry.GetContentOfZipEntry());
                        var items = o.data.Select(a => a.GetDbItem(entry.LastWriteTime.DateTime)).ToArray();

                        if (items.Length > 0)
                        {
                            DbUtils.ClearAndSaveToDbTable(items, "Bfr_ScreenerTradingView", "Symbol", "Exchange",
                                "Name", "Type", "Subtype", "Sector", "Industry", "Close", "MarketCap", "Volume",
                                "Recommend", "TimeStamp");
                            DbUtils.RunProcedure("pUpdateScreenerTradingView");
                        }

                        itemCount += items.Length;
                    }

            return itemCount;
        }
    }
}
