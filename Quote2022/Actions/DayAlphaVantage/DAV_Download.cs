using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quote2022.Actions.DayAlphaVantage
{
    public static class DAV_Download
    {
        public class ApiKey
        {
            public string Key;
            public DateTime LastUsed = DateTime.MinValue;
            public string proxy;
        }

        private static ApiKey[] _apiKeys = new ApiKey[0];

        private const string timeStamp = "20230225";
        private static string DataFolder = Settings.DayAlphaVantageDataFolder + $"DayAlphaVantage_{timeStamp}\\";
        const string SymbolListFileName = Settings.DayAlphaVantageFolder + @"SymbolsToDownload.txt";
        const string ProxyListFileName = @"E:\Quote\WebData\ProxyList.txt";
        const string ApiKeysFileName = @"E:\Quote\WebData\Minute\AlphaVantage\ApiKeys.txt";

        private static readonly TimeSpan _delay = new TimeSpan(0, 0, 63);

        private static bool _isBusy = false;
        private static object _lockObject = new object();

        private static string _lastIp;
        private static DateTime _lastIpUsed = DateTime.MinValue;
        private static System.Timers.Timer _timer;

        private static List<Tuple<string, string>> _urlsAndFilenames;
        private static Action<string> _showStatusAction;

        private static int _totalItems;
        private static int _downloadedItems;
        private static bool _stopFlag;

        public static void Stop() => _stopFlag = true;

        public static void RefreshProxyList()
        {
            if (_showStatusAction != null)
                _showStatusAction($"DayAlphaVantage_Download. Before proxy list refresh ...");
            var uniqueProxy = new Dictionary<string, object>();
            foreach (var item in File.ReadAllLines(ProxyListFileName)
                .Where(a => !string.IsNullOrEmpty(a) && !a.StartsWith("#")))
            {
                var proxy = item.Split('\t')[0];
                if (!uniqueProxy.ContainsKey(proxy))
                    uniqueProxy.Add(proxy, null);
            }

            lock (_lockObject)
            {
                var proxies = uniqueProxy.Keys.ToList();
                var apiKeys = _apiKeys.ToList();
                for (var k = 0; k < proxies.Count; k++)
                {
                    var key = apiKeys.FirstOrDefault(a => a.proxy == proxies[k]);
                    if (key != null)
                    {
                        proxies.Remove(proxies[k--]);
                        apiKeys.Remove(key);
                    }
                }

                foreach (var proxy in proxies)
                {
                    if (apiKeys.Count == 0) break;
                    apiKeys[0].proxy = proxy;
                    apiKeys.RemoveAt(0);
                }

                foreach (var key in apiKeys)
                    key.proxy = null;
            }

            if (_showStatusAction != null)
                _showStatusAction($"DayAlphaVantage_Download. Proxy list refreshed. {uniqueProxy.Count} proxy items. {_apiKeys.Length} api keys.");
        }

        public static void Start(Action<string> showStatusAction)
        {
            if (_isBusy)
            {
                MessageBox.Show("DayAlphaVantage_Download is working now .. Can't run it again.");
                return;
            }

            _isBusy = true;
            _showStatusAction = showStatusAction;

            _apiKeys = File.ReadAllLines(ApiKeysFileName).Where(a => !string.IsNullOrEmpty(a) && !a.StartsWith("#"))
                .Select(a => new ApiKey() {Key = a}).ToArray();

            SetUrlsAndFilenames();

            RefreshProxyList();
            _totalItems = _urlsAndFilenames.Count;
            _downloadedItems = 0;
            _stopFlag = false;

            var tasks = _apiKeys.Select(a => Task.Factory.StartNew(() => Download(a)));
            Task.WaitAll(tasks.ToArray());

            _isBusy = false;
            _showStatusAction($"MinuteAlphaVantage_Download. Finished.");
        }

        private static void Download(ApiKey _apiKey)
        {
            while (true)
            {
                var ok = 0;
                var bad = 0;
                for (var k = 0; k < 5; k++)
                {
                    while (_apiKey.proxy == null)
                    {
                        if (_stopFlag)
                            return;
                        lock(_lockObject)
                            if (_urlsAndFilenames.Count==0) return;

                        Thread.Sleep(10000);
                    }

                    Tuple<string, string> item = null;
                    lock (_lockObject)
                    {
                        if (_urlsAndFilenames.Count > 0)
                        {
                            item = _urlsAndFilenames[0];
                            _urlsAndFilenames.RemoveAt(0);
                        }
                    }

                    if (item == null)
                        return;

                    var url = string.Format(item.Item1, _apiKey.Key);
                    Debug.Print($"{DateTime.Now}. Api: {_apiKey.Key}. Proxy: {_apiKey.proxy}. BeforeDownload: {url}, {item.Item2}");
                    if (!Actions.Download.DownloadPageProxy(url, item.Item2, _apiKey.proxy))
                    {
                        lock (_lockObject)
                        {
                            _urlsAndFilenames.Insert(0, item);
                        }
                        bad++;
                    }
                    else
                        ok++;
                }

                using (var conn = new SqlConnection(Settings.DbConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"INSERT into ProxyLog (Date, Proxy, Bad, OK) VALUES(GetDate(), '{_apiKey.proxy}', {bad}, {ok})";
                    cmd.ExecuteNonQuery();
                }

                if (_stopFlag)
                    return;

                Thread.Sleep(61000);
            }
        }

        private static void SetUrlsAndFilenames()
        {
            _showStatusAction($"IntradayAlphaVantage_Download. Define urls and filenames to download.");

            var urls = new Dictionary<string, string[]>();
            var ss = File.ReadAllLines(SymbolListFileName).Where(a => !a.StartsWith("#"));
            foreach (var s in ss)
            {
                if (string.IsNullOrEmpty(s)) continue;
                var ss1 = s.Split('\t');
                // https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol=IBM&outputsize=full&datatype=csv&apikey=BWHS2I3GBGN0BJK9

                var key = @"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol=" +
                          ss1[0].Trim() + $"&outputsize=full&datatype=csv&apikey={{0}}";
                if (!urls.ContainsKey(key))
                    urls.Add(key, ss1);
            }

            _urlsAndFilenames = new List<Tuple<string, string>>();
            foreach (var kvp in urls)
            {
                var filename = DataFolder + $"{kvp.Value[0].Trim()}_{timeStamp}.csv";
                if (!File.Exists(filename))
                {
                    _urlsAndFilenames.Add(new Tuple<string, string>(kvp.Key, filename));
                }
            }
        }


    }
}
