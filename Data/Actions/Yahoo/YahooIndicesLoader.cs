using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.Actions.Yahoo
{
    public static class YahooIndicesLoader
    {
        public static void Start(Action<string> showStatus)
        {
            var timeStamp = csUtils.GetTimeStamp();
            var folder = $@"E:\Quote\WebData\Screener\Nasdaq\NasdaqScreener_{timeStamp.Item2}\";

            /*// Download data
            var stockFile = folder + $@"ScreenerStock_{timeStamp.Item2}.json";
            showStatus($"Nasdaq.ScreenerLoader. Download STOCK data from {stockUrl} to {stockFile}");
            Helpers.Download.DownloadPage(stockUrl, stockFile, true);

            var etfFile = folder + $@"ScreenerEtf_{timeStamp.Item2}.json";
            showStatus($"Nasdaq.ScreenerLoader. Download ETF data from {etfUrl} to {etfFile}");
            Helpers.Download.DownloadPage(etfUrl, etfFile, true);

            // Parse and save data to database
            showStatus($"Nasdaq.ScreenerLoader. Parse and save files to database");
            Parse(stockFile, timeStamp.Item1);
            Parse(etfFile, timeStamp.Item1);

            // Zip data and remove text files
            var zipFilename = csUtils.ZipFolder(folder);
            Directory.Delete(folder);

            showStatus($"Nasdaq.ScreenerLoader finished. Filename: {zipFilename}");*/
        }


    }
}
