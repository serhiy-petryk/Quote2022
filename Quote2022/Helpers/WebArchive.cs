using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Quote2022.Actions.TradingView;

namespace Quote2022.Helpers
{
    public class WebArchive
    {
        public static void Download(string url, string filenameTemplate, Action<string> showStatusAction)
        {
            showStatusAction($"TradingView.WebArchive_Profile.DownloadData started. Url: {url}");

            var sourceUrl = System.Net.WebUtility.UrlEncode(url);
            var requestUrl = @"https://web.archive.org/__wb/sparkline?output=json&url=" + sourceUrl + @"&collection=web";

            showStatusAction($"TradingView.WebArchive_Profile.DownloadData. Prepare year list. Url: {url}");
            string yearsJson = null;
            while (yearsJson == null)
            {
                yearsJson = Actions.Download.GetResponse(requestUrl);
                if (yearsJson == null)
                {
                    showStatusAction($"TradingView.WebArchive_Profile.DownloadData. My be 'error: (429) Too Many Requests' Wait ...  Url: {url}");
                    Thread.Sleep(30000);
                }
            }

            var ooYearList = JsonConvert.DeserializeObject<WebArchive_Profile.cYearList>(yearsJson);

            var years = ooYearList.status.Keys.OrderBy(a => a).ToArray();
            foreach (var year in years)
            {
                showStatusAction($"TradingView.WebArchive_Profile.DownloadData. Prepare days list for {year} year.  Url: {url}");
                requestUrl = $"https://web.archive.org/__wb/calendarcaptures/2?url={sourceUrl}&date={year}&groupby=day";
                var daysJson = Actions.Download.GetResponse(requestUrl);
                var ooDayList = JsonConvert.DeserializeObject<WebArchive_Profile.cDayList>(daysJson.Replace("\"-\"", "200"));

                foreach (var day in ooDayList.items)
                {
                    if (day[1] == 200)
                    {
                        var timeStamp = $"{year}{day[0]:0000}";
                        requestUrl = $"https://web.archive.org/__wb/calendarcaptures/2?url={sourceUrl}&date={timeStamp}";

                        string dayItemsJson = null;
                        while (dayItemsJson == null)
                        {
                            dayItemsJson = Actions.Download.GetResponse(requestUrl);
                            if (dayItemsJson == null)
                            {
                                showStatusAction($"TradingView.WebArchive_Profile.DownloadData. My be 'error: (429) Too Many Requests' Wait ...  Url: {url}");
                                Thread.Sleep(30000);
                            }
                        }

                        var dayItems = JsonConvert.DeserializeObject<WebArchive_Profile.cDayList>(dayItemsJson.Replace("\"-\"", "200"));

                        foreach (var dayItem in dayItems.items)
                        {
                            if (dayItem[1] == 200)
                            {
                                var id = $"{timeStamp}{dayItem[0]:000000}";
                                var filename = string.Format(filenameTemplate, id);
                                if (!File.Exists(filename))
                                {
                                    requestUrl = $"https://web.archive.org/web/{id}/{sourceUrl}";

                                    showStatusAction($"TradingView.WebArchive_Profile.DownloadData. Download to {Path.GetFileName(filename)}");
                                    Actions.Download.DownloadPage(requestUrl, filename);
                                    if (File.Exists(filename))
                                    {
                                        var fileTime = DateTime.ParseExact($"{id}", "yyyyMMddHHmmss",
                                            CultureInfo.InvariantCulture);
                                        File.SetCreationTime(filename, fileTime);
                                        File.SetLastWriteTime(filename, fileTime);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            showStatusAction($"TradingView.WebArchive_Profile.DownloadData finished. Url: {url}");

        }
    }
}
