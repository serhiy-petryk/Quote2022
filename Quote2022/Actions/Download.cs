using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Quote2022.Helpers;
using Quote2022.Models;

namespace Quote2022.Actions
{
    public static class Download
    {

        #region ==================  MinuteAlphaVantage_Download  ==========================

        public static void MinuteAlphaVantage_Download(Action<string> showStatusAction)
        {
            const string dataFolder = @"E:\Quote\WebData\Minute\AlphaVantage\YearMonth\";
            const string symbolListFileName = @"E:\Quote\WebData\Minute\AlphaVantage\SymbolsToDownload.txt";
            // var apiKeys = new[] { "TK4Q66GMN8YDXDVZ", "TXQMV0KYX4WBX7VS", "QDYJLC03FUZX4VN2" };
            var apiKeys = new[] { "TXQMV0KYX4WBX7VS" };

            showStatusAction($"MinuteAlphaVantage_Download. Define urls and filenames to download.");
            var periodIds = new Dictionary<string, DateTime>();
            for (var k = 12; k >= 12; k--)
                periodIds.Add($"year2month{k}", DateTime.Today.AddYears(-1).AddMonths(-k));
            /*for (var k = 12; k >= 1; k--)
                periodIds.Add($"year2month{k}", DateTime.Today.AddYears(-1).AddMonths(-k));
            for (var k = 12; k >= 1; k--)
                periodIds.Add($"year1month{k}", DateTime.Today.AddMonths(-k));*/

            var folder = @"E:\Quote\WebData\Minute\AlphaVantage\Data\";
            var files = Directory.GetFiles(folder, "AVyear2month12_*.csv", SearchOption.AllDirectories);
            var fileKeys = new Dictionary<string, object>();
            foreach (var file in files)
            {
                var key = Path.GetFileNameWithoutExtension(file).Split('_')[1];
                if (!fileKeys.ContainsKey(key))
                    fileKeys.Add(key, null);
            }
            var fileKeys2 = fileKeys.Keys.OrderBy(a => a).ToArray(); ;

            var symbols = new Dictionary<string, DateTime[]>();
            var ss = File.ReadAllLines(symbolListFileName).Where(a => !a.StartsWith("#")).ToArray();
            var ss3 = ss.Select(a => a.Split('\t')[0]).OrderBy(a => a).ToArray();
            foreach (var s in ss)
            {
                if (string.IsNullOrEmpty(s)) continue;
                var ss1 = s.Split('\t');
                if (ss1.Length == 5 && !fileKeys.ContainsKey(ss1[0]))
                {
                    symbols.Add(ss1[0].Trim(), new DateTime[] { DateTime.Parse(ss1[3].Trim(), CultureInfo.InvariantCulture), DateTime.Parse(ss1[4].Trim(), CultureInfo.InvariantCulture) });
                }
                else if (fileKeys.ContainsKey(ss1[0]))
                {
                    fileKeys.Remove(ss1[0]);
                }

            }

            /*using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT symbol, sum(volume*[close])/1000000 TradingValue, count(*) recs, min(Date) MinDate , max(Date) MaxDate " +
                                  "FROM DayEoddata where volume>= 300000 and symbol not like '%.%' and symbol not like '%-%' "+
                                  "group by symbol order by 2 desc";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        var dates = new[] {(DateTime) rdr["MinDate"], (DateTime) rdr["MaxDate"]};
                        symbols.Add((string) rdr["Symbol"], dates);
                    }
            }*/

            var urls = new Dictionary<string, string>();
            var apiKeyCount = 0;
            foreach (var kvp1 in symbols)
                foreach (var kvp2 in periodIds)
                {
                    // var minDate = kvp1.Value[0] < new DateTime(2022, 6, 1) ? new DateTime(2000, 1, 1) : kvp1.Value[0];
                    var minDate = new DateTime(2000, 1, 1);
                    var maxDate = kvp1.Value[1];

                    if (kvp2.Value > maxDate || kvp2.Value.AddMonths(2) < minDate)
                        continue;

                    var filename = dataFolder + $"AV{kvp2.Key}_{kvp1.Key}.csv";
                    if (!File.Exists(filename))
                    {
                        var k1 = urls.Count % (5 * apiKeys.Length);
                        var k2 = k1 / 5;
                        urls.Add(
                            @"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY_EXTENDED&symbol=" + kvp1.Key +
                            $"&slice={kvp2.Key}&interval=1min&adjusted=false&datatype=csv&apikey={apiKeys[k2]}", filename);
                    }
                }

            apiKeyCount = 0;
            var dailyLimit = 5480;
            var urlCount = 0;
            foreach (var kvp in urls)
            {
                apiKeyCount++;
                if (apiKeyCount == 5 * apiKeys.Length + 1)
                {
                    dailyLimit--;
                    if (dailyLimit < 0)
                    {
                        throw new Exception($"Exceed daily limit in MinuteAlphaVantage_Download");
                    }

                    apiKeyCount = 1;
                    showStatusAction($"MinuteAlphaVantage_Download. Pause. Downloaded {urlCount} urls from {urls.Count}");
                    Thread.Sleep(70 * 1000);
                }
                DownloadPage(kvp.Key, kvp.Value);
                if (File.Exists(kvp.Value))
                {
                    var s = File.ReadAllText(kvp.Value);
                    if (s.Contains("Thank you for using"))
                        throw new Exception($"Thank you error in MinuteAlphaVantage_Download. Urls: {kvp.Key}");
                }
                urlCount++;
            }
            showStatusAction($"MinuteAlphaVantage_Download finished!");
        }
        #endregion

        #region ==================  SymbolsQuantumonline_Download  ==========================

        public static void DayYahoo_Download(Action<string> showStatusAction)
        {
            var symbols = new Dictionary<string, object>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "select a.symbol from SymbolsYahooLookup a left join DayYahoo b on a.Symbol = b.Symbol where b.Symbol is null and NotValid is null;";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        symbols.Add((string)rdr["Symbol"], null);
            }

            Debug.Print(string.Join(Environment.NewLine, symbols.Keys));
        }
        #endregion

        #region ==================  SymbolsQuantumonline_Download  ==========================
        public static void SymbolsYahooLookup_Download(string asset, Action<string> showStatusAction)
        {
            var urlChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
            var urlTemplate = @"https://query1.finance.yahoo.com/v1/finance/lookup?formatted=false&lang=en-US&region=US&query={1}*&type={0}&count=10000&start=0&corsDomain=finance.yahoo.com";
            var timeStamp = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");
            var fileTemplate = Settings.SymbolsYahooLookupFolder + @"SymbolsYahooLookup_" + timeStamp + @"\syl_{1}_{0}.json";
            var folder = Path.GetDirectoryName(fileTemplate);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var level = 0;
            var finishedUrlKeys = new List<string>();
            var keys = new List<string> { "" };
            while (keys.Count != 0 && level < 10)
            {
                var urlKeys = new Dictionary<string, string>();
                foreach (var key in keys)
                    foreach (var c in urlChars)
                    {
                        var newKey = key + c.ToString();
                        urlKeys.Add(newKey, GetFileName(newKey));
                    }

                var toDownloadKeys = urlKeys.Where(a => !File.Exists(a.Value)).ToArray();
                Debug.Print($"SymbolsYahooLookup_Download. Downloading {toDownloadKeys.Length} from {urlKeys.Count} items for level: {level}. FinishedUrlKeys: {finishedUrlKeys.Count}. {DateTime.Now}");
                var cnt = 0;
                foreach (var kvp in toDownloadKeys)
                {
                    showStatusAction($"{cnt} from {toDownloadKeys.Length} lookup keys were downloaded. Level: {level}. Asset: {asset}");
                    DownloadPage(GetUrl(kvp.Key), kvp.Value, true);
                    cnt++;
                }
                /*Parallel.ForEach(toDownloadKeys, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (kvp) =>
                {
                    showStatusAction($"{cnt} from {toDownloadKeys.Length} lookup keys were downloaded");
                    DownloadPage(GetUrl(kvp.Key), kvp.Value);
                    cnt++;
                });*/

                keys.Clear();
                foreach (var urlKey in urlKeys)
                {
                    var o = JsonConvert.DeserializeObject<SymbolsYahooLookup>(File.ReadAllText(urlKey.Value));
                    if (o.finance.error != null)
                        throw new Exception($"There is an error in {urlKey.Value}. Message: {o.finance.error}");

                    if (o.finance.result[0].total < o.finance.result[0].count)
                        finishedUrlKeys.Add(urlKey.Key);
                    else if (o.finance.result[0].count > 0)
                        keys.Add(urlKey.Key);
                }

                level++;
            }

            Debug.Print($"FINISHED!!! {DateTime.Now}");
            showStatusAction($"FINISHED!!!");

            string GetUrl(string urlKey) => string.Format(urlTemplate, asset, urlKey);
            string GetFileName(string urlKey) => string.Format(fileTemplate, asset, urlKey);
        }
        #endregion

        #region ==================  NasdaqTrades_Download  ==========================
        public static void TimeSalesNasdaq_Download(Action<string> showStatusAction)
        {
            showStatusAction($"TimeSalesNasdaq_Download started!");

            var symbols = new List<string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "select distinct symbol from DayEoddata WHERE " +
                                  "Volume>1000000 and date>=DATEADD(HOUR, 5, DATEADD(day, -15, GetDate())) AND " +
                                  "((symbol not like '%-%' and symbol not like '%.%') or symbol like '%.[A-B]')";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        symbols.Add((string)rdr["Symbol"]);
            }

            if (MessageBox.Show($"You are going to download data for {symbols.Count} symbols! Continue?", "", MessageBoxButtons.OKCancel) != DialogResult.OK)
                return;

            var timeStamp = DateTime.Now.AddHours(4).AddDays(-1).Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var urlsAndFileNames = new Dictionary<string, string>();
            foreach (var symbol in symbols)
                foreach (var time in TimeSalesNasdaq.UrlTimes)
                {
                    var url = string.Format(Settings.TimeSalesNasdaqUrlTemplate, time, symbol);
                    var filename = string.Format(Settings.TimeSalesNasdaqFileTemplate, time.Replace(":", ""), symbol, timeStamp);
                    urlsAndFileNames.Add(url, filename);
                }

            // Create folder
            foreach (var aa1 in urlsAndFileNames.Take(1))
            {
                var folder = Path.GetDirectoryName(aa1.Value);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }

            var cnt = 0;
            string badFile = null;
            Parallel.ForEach(urlsAndFileNames, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (kvp, state) =>
            {
                if (!File.Exists(kvp.Value))
                {
                    DownloadPage(kvp.Key, kvp.Value, true);
                    if ((new System.IO.FileInfo(kvp.Value).Length) < 10)
                    {
                        badFile = kvp.Value;
                        state.Stop();
                    }
                }

                cnt++;
                if (cnt % 20 == 0)
                {
                    showStatusAction($"{cnt} from {urlsAndFileNames.Count} symbols were downloaded");
                    Application.DoEvents();
                }
            });

            if (!string.IsNullOrEmpty(badFile))
            {
                showStatusAction($"TimeSalesNasdaq_Download ERROR! See '{Path.GetFileName(badFile)}' file");
                return;
            }

            showStatusAction($"{cnt} from {urlsAndFileNames.Count} symbols were downloaded");

            var badFiles = new List<string>();
            for (var k = 0; k < 3; k++)
            {
                badFiles.Clear();
                var newUrlsAndFileNames = new Dictionary<string, string>();
                foreach (var kvp in urlsAndFileNames)
                {
                    if (File.Exists(kvp.Value))
                    {
                        var oo = JsonConvert.DeserializeObject<TimeSalesNasdaq>(File.ReadAllText(kvp.Value));
                        if (oo == null || oo.ShouldBeReload)
                            newUrlsAndFileNames.Add(kvp.Key, kvp.Value);
                    }
                    else
                        throw new Exception($"Missing downloaded file {kvp.Value}. Please, check 'TimeSalesNasdaq_Download.' method");

                }

                urlsAndFileNames = newUrlsAndFileNames;
                if (urlsAndFileNames.Count == 0)
                    break;

                showStatusAction($"{urlsAndFileNames.Count} files need to reload");
                var cnt2 = 0;
                foreach (var kvp in urlsAndFileNames)
                {
                    DownloadPage(kvp.Key, kvp.Value, true);
                    cnt2++;
                    if (cnt2 % 10 == 0)
                    {
                        showStatusAction($"{cnt2} from {urlsAndFileNames.Count} symbols were reloaded");
                        Application.DoEvents();
                    }
                }

                /*Parallel.ForEach(urlsAndFileNames, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (kvp) =>
                {
                    DownloadPage(kvp.Key, kvp.Value, true);

                    cnt2++;
                    if (cnt2 % 10 == 0)
                    {
                        showStatusAction($"{cnt2} from {urlsAndFileNames.Count} symbols were downloaded");
                        Application.DoEvents();
                    }
                });*/

                Thread.Sleep(3000);
            }

            if (urlsAndFileNames.Count > 0)
            {
                Debug.Print("***** Need to reload (format - symbol/time): *****");
                foreach (var kvp in urlsAndFileNames)
                {
                    var aa1 = Path.GetFileNameWithoutExtension(kvp.Value).Split('_');
                    var time = aa1[aa1.Length - 1].Substring(0, 2) + ":" + aa1[aa1.Length - 1].Substring(2);
                    var symbol = aa1[aa1.Length - 3];
                    Debug.Print(symbol + "\t" + time);
                }
            }
            showStatusAction($"TimeSalesNasdaq_Download FINISHED! {urlsAndFileNames.Count} symbols need to reload. See log/Debug.Output");
        }

        #endregion

        #region ==================  SymbolsStockanalysis_Download  ==========================

        public static void SymbolsStockanalysis_Download(Action<string> showStatusAction)
        {
            showStatusAction($"SymbolsStockanalysisDownload started!");
            var urlsAndFilenames = new Dictionary<string, string>
            {
                { @"stocks/__data.json?x-sveltekit-invalidated=_1", @"Stockanalysis.StockList_{0}.txt"},
                {@"stocks/screener/__data.json?x-sveltekit-invalidated=_1", @"Stockanalysis.StockScreener_{0}.txt"},
                {@"etf/__data.json?x-sveltekit-invalidated=_1", @"Stockanalysis.EtfList_{0}.txt"},
                {@"etf/screener/__data.json?x-sveltekit-invalidated=_1", @"Stockanalysis.EtfScreener_{0}.txt"},
                {@"api/screener/s/d/exchange.json", @"Stockanalysis.StockExchanges_{0}.txt"},
                {@"api/screener/e/d/exchange.json", @"Stockanalysis.EtfExchanges_{0}.txt"}
            };

            var urlRoot = @"https://stockanalysis.com/";
            var fileRoot = @"E:\Quote\WebData\Symbols\Stockanalysis\";

            foreach (var kvp in urlsAndFilenames)
            {
                DownloadPage(urlRoot + kvp.Key,
                    string.Format(fileRoot + kvp.Value,
                        DateTime.Today.AddDays(-1).ToString("yyyyMMdd", CultureInfo.InvariantCulture)));
            }

            showStatusAction($"SymbolsStockanalysisDownload FINISHED!");

        }
        #endregion

        #region ==================  SymbolsQuantumonline_Download  ==========================
        public static void SymbolsQuantumonline_Rename(Action<string> showStatusAction)
        {
            var oldPath = @"E:\Temp\Quote\QuantumonlineList\";
            var newPath = @"E:\Temp\Quote\QuantumonlineListNew\";
            var levelPathes = new List<string>();

            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);

            for (var k = 0; k < 20; k++)
            {
                var levelPath = newPath + "Level" + k.ToString("D2") + @"\";
                levelPathes.Add(levelPath);
                if (!Directory.Exists(levelPath))
                    Directory.CreateDirectory(levelPath);
            }

            var files = Directory.GetFiles(oldPath, "*.html");
            for (var k = 0; k < files.Length; k++)
            {
                var fn = Path.GetFileNameWithoutExtension(files[k]);
                var newFn = levelPathes[fn.Length - 2] + Path.GetFileName(files[k]);
                File.Move(files[k], newFn);
            }
        }
        #endregion

        #region ==================  ProfilesQuantumonline_Download  ==========================

        public static void ProfilesQuantumonline_Download(Action<string> showStatusAction)
        {
            const string folderList = @"E:\Quote\WebData\Symbols\Quantumonline\";
            const string folderProfile = @"E:\Quote\WebData\Symbols\Quantumonline\Profiles3\";

            Debug.Print($"Point1: {DateTime.Now}");
            var urls = new Dictionary<string, List<SymbolsQuantumonline>>();
            var items = new Dictionary<string, SymbolsQuantumonline>();
            var zipFiles = Directory.GetFiles(folderList, "*.zip");
            foreach (var zipFile in zipFiles)
                Parse.SymbolsQuantumonline_GetUrls(zipFile, urls);

            Debug.Print($"Point2: {DateTime.Now}. Urls: {urls.Count}");
            var urlsAndFileNames = new Dictionary<string, string>();
            foreach (var url in urls.Keys.Where(a => a.IndexOf('+') == -1))
            {
                if (url.StartsWith("search.cfm?tickersymbol=") && url.EndsWith("&sopt=symbol"))
                {
                    var symbol = url.Substring(24, url.Length - 36).Replace("*", "#");
                    urlsAndFileNames.Add(@"https://www.quantumonline.com/" + url, folderProfile + @"p" + symbol + ".html");
                }
                else
                    throw new Exception("Check ");
            }

            Debug.Print($"ProfilesQuantumonline_Download. Downloading {urlsAndFileNames.Count} profiles");
            showStatusAction($"ProfilesQuantumonline_Download. Downloading {urlsAndFileNames.Count} profiles");

            Parallel.ForEach(urlsAndFileNames, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (kvp) =>
            {
                DownloadPage(kvp.Key, kvp.Value);
            });

            Debug.Print($"Point3: {DateTime.Now}. Urls: {urls.Count}");
            var symbolsAndFileNames = new Dictionary<string, string>();
            foreach (var url in urls.Keys.Where(a => a.IndexOf('+') != -1))
            {
                if (url.StartsWith("search.cfm?tickersymbol=") && url.EndsWith("&sopt=symbol"))
                {
                    var symbol = url.Substring(24, url.Length - 36);
                    symbolsAndFileNames.Add(symbol, folderProfile + @"p" + symbol.Replace("*", "#") + ".html");
                }
                else
                    throw new Exception("Check ");
            }

            Debug.Print($"ProfilesQuantumonline_Download. Downloading {symbolsAndFileNames.Count} profiles");
            showStatusAction($"ProfilesQuantumonline_Download. Downloading {symbolsAndFileNames.Count} profiles");

            Parallel.ForEach(symbolsAndFileNames, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (kvp) =>
            {
                var parameters = new NameValueCollection { { "tickersymbol", kvp.Key }, { "sopt", "symbol" }, { "1.0.1", "Body" } };
                DownloadPage_POST(@"https://www.quantumonline.com/search.cfm", kvp.Value, parameters);
            });

            Debug.Print($"Point4: {DateTime.Now}");
        }
        public static void xxProfilesQuantumonline_Download(Action<string> showStatusAction)
        {
            const string folderList = @"E:\Quote\WebData\Symbols\Quantumonline\";
            const string folderProfile = @"E:\Quote\WebData\Symbols\Quantumonline\Profiles\";

            var items = new Dictionary<string, SymbolsQuantumonline>();
            var zipFiles = Directory.GetFiles(folderList, "*.zip");
            foreach (var zipFile in zipFiles)
                using (var zip = new ZipReader(zipFile))
                    foreach (var file in zip.Where(a => a.FullName.EndsWith(".html", true, CultureInfo.InvariantCulture)))
                    {
                        if (file.FileNameWithoutExtension.Replace("%", "[%]").Substring(1).Length <= 20)
                        {
                            var newItems = Parse.SymbolsQuantumonlineContent_Parse(file.Content, file.FileNameWithoutExtension, File.GetCreationTime(zipFile));
                            foreach (var item in newItems)
                                if (!items.ContainsKey(item.ToString()))
                                    items.Add(item.ToString(), item);
                            //if (!urls.ContainsKey(item.Url))
                            //  urls.Add(item.Url, null);
                            // break;
                        }
                    }

            //var urls = items.Values.Select(a=>a.Url).ToList();
            //var aa1 = urls.Distinct().ToList();

            var urls = new Dictionary<string, string>();
            foreach (var item in items.Values)
            {
                if (item.Url.StartsWith("search.cfm?tickersymbol=") && item.Url.EndsWith("&sopt=symbol"))
                {
                    var symbol = item.Url.Substring(24, item.Url.Length - 36).Replace("*", "#");
                    var url = @"https://www.quantumonline.com/" + item.Url;
                    if (!urls.ContainsKey(url))
                        urls.Add(url, folderProfile + @"\s" + symbol + ".html");
                }
                else
                    throw new Exception("Check ");
            }

            Parallel.ForEach(urls, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (kvp) =>
            {
                DownloadPage(kvp.Key, kvp.Value);
            });

        }
        #endregion

       #region ==============  Nanex Symbols  =================
        public static string[] SymbolsNanex_Download(Action<string> showStatusAction)
        {
            var timeStamp = DateTime.Today.ToString("yyyyMMdd");
            var files = new List<string>();
            showStatusAction("Nanex Symbols Downloading ...");
            foreach (var exchange in Settings.NanexExchanges)
            {
                var file = string.Format(Settings.SymbolsNanexTemplateFile, exchange, timeStamp);
                if (File.Exists(file)) File.Delete(file);
                var url = string.Format(Settings.SymbolsNanexTemplateUrl, exchange, "e"); // e - Equities
                DownloadPage(url, file);
                files.Add(file);
            }
            showStatusAction("Nanex Symbols Downloaded");
            return files.ToArray();
        }
        #endregion

        public static string GetResponse(string url, bool isXMLHttpRequest = false)
        {
            string response = null;
            using (var wc = new WebClientEx())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                try
                {
                    // wc.Headers.Add("Cache-Control", "no-cache");
                    if (isXMLHttpRequest)
                        wc.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    var uri = new Uri(url);
                    
                    wc.Headers.Add(HttpRequestHeader.Referer, uri.Host);

                    var bb = wc.DownloadData(url);
                    response = Encoding.UTF8.GetString(bb);
                    return response;
                }
                catch (Exception ex)
                {
                    /*if (ex is WebException webEx && webEx.Response is HttpWebResponse webResponse)
                    {
                        if (webResponse.StatusCode == HttpStatusCode.NotFound)
                            response = "NotFound";
                        else if (webResponse.StatusCode == HttpStatusCode.Moved)
                            response = "Moved";
                    }
                    else if (ex is WebException)
                    {
                        Debug.Print($"{DateTime.Now}. Web Exception: {url}. Message: {ex.Message}");
                    }
                    else*/
                    if (ex is WebException)
                    {
                        Debug.Print($"{DateTime.Now}. Web Exception: {url}. Message: {ex.Message}");
                    }
                    else
                        throw ex;
                }
            }

            return response;
        }

        public static string DownloadPage(string url, string filename, bool isXMLHttpRequest = false)
        {
            string response = null;
            using (var wc = new WebClientEx())
            {
                /*if (ServicePointManager.DefaultConnectionLimit != int.MaxValue)
                {
                    ServicePointManager.DefaultConnectionLimit = int.MaxValue;
                    WebRequest.DefaultWebProxy = null;
                }
                wc.Proxy = null;*/
                wc.Encoding = System.Text.Encoding.UTF8;
                try
                {
                    // wc.Headers.Add("Cache-Control", "no-cache");
                    if (isXMLHttpRequest)
                        wc.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    var bb = wc.DownloadData(url);
                    response = Encoding.UTF8.GetString(bb);
                    if (!string.IsNullOrEmpty(filename))
                    {
                        if (File.Exists(filename))
                            File.Delete(filename);
                        var folder = Path.GetDirectoryName(filename);
                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        File.WriteAllText(filename, response, Encoding.UTF8);
                        //                File.WriteAllText(filename, response);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is WebException webEx && webEx.Response is HttpWebResponse webResponse)
                    {
                        if (webResponse.StatusCode == HttpStatusCode.NotFound)
                            response = "NotFound";
                        else if (webResponse.StatusCode == HttpStatusCode.Moved)
                            response = "Moved";
                    }
                    else if (ex is WebException)
                    {
                        Debug.Print($"{DateTime.Now}. Web Exception: {url}. Message: {ex.Message}");
                    }
                    else
                        throw ex;
                }
            }

            return response;
        }

        public class WebClientEx : WebClient
        {
            public int? TimeoutInMilliseconds;
            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = (HttpWebRequest)base.GetWebRequest(address);
                request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";
                request.AllowAutoRedirect = true;
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                if (TimeoutInMilliseconds.HasValue)
                    request.Timeout = TimeoutInMilliseconds.Value;
                return request;
            }
        }

        private static string DownloadPage_POST(string url, string filename, object parameters)
        {
            // see https://stackoverflow.com/questions/5401501/how-to-post-data-to-specific-url-using-webclient-in-c-sharp
            string response = null;
            using (var wc = new WebClient())
            {
                if (ServicePointManager.DefaultConnectionLimit != int.MaxValue)
                {
                    ServicePointManager.DefaultConnectionLimit = int.MaxValue;
                    WebRequest.DefaultWebProxy = null;
                }
                wc.Proxy = null;

                try
                {
                    if (parameters is NameValueCollection nvc)
                        response = Encoding.UTF8.GetString(wc.UploadValues(url, "POST", nvc));
                    else if (parameters is string json)
                        response = wc.UploadString(url, "POST", json);
                    else
                        throw new Exception("DownloadPage_POST. Invalid type of request parameters");
                }
                catch (Exception ex)
                {
                    if (ex is WebException webEx && webEx.Response is HttpWebResponse webResponse)
                    {
                        if (webResponse.StatusCode == HttpStatusCode.NotFound)
                            response = "NotFound";
                        else if (webResponse.StatusCode == HttpStatusCode.Moved)
                            response = "Moved";
                    }
                    else
                        throw ex;
                }
            }

            if (!string.IsNullOrEmpty(filename))
            {
                if (File.Exists(filename))
                    File.Delete(filename);
                File.WriteAllText(filename, response, Encoding.UTF8);
            }

            return response;
        }

        /*private static string DownloadPage_POST(string url, string filename, string queryString)
        {
            // see https://stackoverflow.com/questions/5401501/how-to-post-data-to-specific-url-using-webclient-in-c-sharp
            string response = null;
            using (var wc = new WebClient())
            {
                if (ServicePointManager.DefaultConnectionLimit != int.MaxValue)
                {
                    ServicePointManager.DefaultConnectionLimit = int.MaxValue;
                    WebRequest.DefaultWebProxy = null;
                }
                wc.Proxy = null;
                

                try
                {
                    response = Encoding.UTF8.GetString(wc.UploadString(url, "POST", queryString));
                }
                catch (Exception ex)
                {
                    if (ex is WebException webEx && webEx.Response is HttpWebResponse webResponse)
                    {
                        if (webResponse.StatusCode == HttpStatusCode.NotFound)
                            response = "NotFound";
                        else if (webResponse.StatusCode == HttpStatusCode.Moved)
                            response = "Moved";
                    }
                    else
                        throw ex;
                }
            }

            if (!string.IsNullOrEmpty(filename))
            {
                if (File.Exists(filename))
                    File.Delete(filename);
                File.WriteAllText(filename, response, Encoding.UTF8);
            }

            return response;
        }*/

        public static bool DownloadPageProxy(string url, string filename, string proxyIp, bool isXMLHttpRequest = false)
        {
            try
            {
                var proxy = new WebProxy(proxyIp);
                var wc = new WebClient {Proxy = proxy, Encoding = Encoding.UTF8};
                // var url = @"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY_EXTENDED&symbol=PCH&slice=year2month12&interval=1min&adjusted=false&datatype=csv&apikey=QDYJLC03FUZX4VN2";
                var result = wc.DownloadString(url);

                if (File.Exists(filename))
                    File.Delete(filename);
                
                var folder = Path.GetDirectoryName(filename);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                File.WriteAllText(filename, result, Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                Debug.Print($"Error. Proxy: {proxyIp}. {ex.Message}. Time: {DateTime.Now}");
                return false;
            }
        }
    }
}
