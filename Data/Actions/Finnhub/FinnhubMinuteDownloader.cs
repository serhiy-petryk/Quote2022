using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Data.Helpers;

namespace Data.Actions.Finnhub
{
    public static class FinnhubMinuteDownloader
    {
        private const string filenameTemplate = @"E:\Quote\WebData\Minute\Finnhub\Data\Finnhub_20230325\fMin_{0}_{1}.json";
        private static string apiKey = CsUtils.GetApiKeys("finnhub.io")[0];

        public static void Start(string[] symbols)
        {
            if (symbols.Length == 0)
            {
                MessageBox.Show("No symbols in file");
                return;
            }

            // SetUrlsAndFilenamesLastYear(symbols);
            Logger.AddMessage($"Define urls and filenames to download.");
            var urlsAndFilenames = new List<Tuple<string, string>>();
            var dates = new DateTime[12];
            for (var k = 0; k < 12; k++)
                dates[k] = DateTime.Today.AddYears(-1).AddDays(30 * k);

            foreach (var symbol in symbols)
                for (var k = 0; k < dates.Length; k++)
                {
                    if (string.IsNullOrEmpty(symbol)) continue;

                    var date = dates[k];
                    var from = CsUtils.GetWebDateTime(date);
                    var to = CsUtils.GetWebDateTime(date.AddDays(30));

                    var filename = string.Format(filenameTemplate, "M" + (k+1).ToString("D2"), symbol);
                    if (!File.Exists(filename))
                    {
                        var url = string.Format(@"https://finnhub.io/api/v1/stock/candle?symbol={0}&resolution=1&from={1}&to={2}&token={3}", symbol, from, to, apiKey);
                        urlsAndFilenames.Add(new Tuple<string, string>(url, filename));
                    }
                }

            if (MessageBox.Show($"Буде {urlsAndFilenames.Count} завантажень для {symbols.Length} символів у папку {Path.GetDirectoryName(filenameTemplate)}", "", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                return;

            var downloadedItems = 0;
            var urlOffset = 0;
            while (true)
            {
                for (var k = 0; k < 50; k++)
                {
                    if (urlOffset >= urlsAndFilenames.Count)
                        break;

                    var urlAndFilename = urlsAndFilenames[urlOffset++];
                    Logger.AddMessage($"Downloaded {urlOffset} items from {urlsAndFilenames.Count}");
                    while (!File.Exists(urlAndFilename.Item2))
                    {
                        Download.DownloadPage(urlAndFilename.Item1, urlAndFilename.Item2);

                        if (!File.Exists(urlAndFilename.Item2))
                            Thread.Sleep(5000);
                    }
                    downloadedItems++;
                }
                
                if (urlOffset >= urlsAndFilenames.Count)
                    break;

                Logger.AddMessage($"Downloaded {urlOffset} items from {urlsAndFilenames.Count}. Delay 1 minute");
                Thread.Sleep(61000);
            }

            Logger.AddMessage($"!Finished. Downloaded {downloadedItems} from {urlsAndFilenames.Count}");
        }
    }
}
