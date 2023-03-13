using System;

namespace Quote2022.Actions.Nasdaq
{
    public static class ScreenerNasdaq_Download
    {
        private static string stockUrl = @"https://api.nasdaq.com/api/screener/stocks?tableonly=true&download=true";
        private static string etfUrl = @"https://api.nasdaq.com/api/screener/etf?tableonly=true&download=true";
        public static void Start(Action<string> showStatusAction)
        {

            var timeStamp = DateTime.Now.AddHours(-10).Date.ToString("yyyyMMdd");


            var stockFile = Settings.ScreenerNasdaqFolder + $"\\Screener_{timeStamp}.json";
            showStatusAction($"Download STOCK data from {stockUrl} to {stockFile}");
            Actions.Download.DownloadPage(stockUrl, stockFile, true);

            var etfFile = Settings.ScreenerNasdaqFolder + $"\\EtfScreener_{timeStamp}.json";
            showStatusAction($"Download ETF data from {etfUrl} to {etfFile}");
            Actions.Download.DownloadPage(etfUrl, etfFile, true);

            showStatusAction($"Data is ready! Files: {stockFile}, {etfFile}");
        }
    }
}
