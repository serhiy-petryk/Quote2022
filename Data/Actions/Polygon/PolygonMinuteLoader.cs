using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using Data.Helpers;

namespace Data.Actions.Polygon
{
    public static class PolygonMinuteLoader
    {
        private const string UrlTemplate = "https://api.polygon.io/v2/aggs/ticker/{0}/range/1/minute/{1}/{2}?adjusted=false&sort=asc&limit=50000&apiKey={3}";
        private const string FolderTemplate = @"E:\Quote\WebData\Minute\Polygon\DataBuffer\MinutePolygon_{0}\";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            Logger.AddMessage($"Define symbols to download ...");
            var symbols = new List<string>();
            var from = DateTime.MaxValue;
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 150;
                cmd.CommandText = "SELECT Symbol, MIN(date) MinDate FROM dbQ2023..DayPolygon "+
                                  "WHERE Volume*[Close]>= 5000000 and Date >= DATEADD(day, -14, GetDate()) "+
                                  "GROUP BY Symbol ORDER BY 1";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        symbols.Add((string) rdr["Symbol"]);
                        var minDate = (DateTime) rdr["MinDate"];
                        if (minDate < from) from = minDate;
                    }
            }

            // var previousFriday = CsUtils.GetPreviousWeekday(DateTime.Now, DayOfWeek.Friday);
            //var from = previousFriday.AddDays(-11);
            var to = DateTime.Now.AddHours(-9).Date.AddDays(-1);

            Start(symbols, from, to);
        }

        public static void Start(List<string> mySymbols, DateTime from, DateTime to)
        {
            Logger.AddMessage($"Started");

            var folder = string.Format(FolderTemplate, to.AddDays(1).ToString("yyyyMMdd"));
            if (MessageBox.Show(
                    $"You are going to download data for {mySymbols.Count} symbols in {folder} folder! Continue?", "",
                    MessageBoxButtons.OKCancel) != DialogResult.OK)
            {
                Logger.AddMessage($"!Canceled.");
                return;
            }

            var cnt = 0;
            var zipFileName = $@"E:\Quote\WebData\Minute\Polygon\DataBuffer\MinutePolygon_{to.AddDays(1):yyyyMMdd}.zip";
            var zipEntries = new Dictionary<string, object>();
            if (File.Exists(zipFileName))
            {
                using (var zipArchive = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    zipEntries = zipArchive.Entries.ToDictionary(a => a.FullName.ToLower(), a => (object) null);
            }

            var virtualFileEntries = new List<VirtualFileEntry>();
            foreach (var mySymbol in mySymbols)
            {
                Logger.AddMessage($"Downloaded {cnt++} tickers from {mySymbols.Count}");

                var entryName =  $@"{Path.GetFileNameWithoutExtension(zipFileName)}\pMin_{mySymbol}_{from:yyyyMMdd}.json";
                var urlTicker = PolygonCommon.GetPolygonTicker(mySymbol);
                if (PolygonCommon.IsTestTicker(urlTicker) || zipEntries.ContainsKey(entryName.ToLower()))
                    continue;

                // var url = $"https://api.polygon.io/v2/aggs/ticker/{urlTicker}/range/1/minute/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}?adjusted=false&sort=asc&limit=50000&apiKey={PolygonCommon.GetApiKey()}";
                var url = string.Format(UrlTemplate, urlTicker, from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"), PolygonCommon.GetApiKey());
                var o = Download.DownloadToString(url);
                if (o is Exception ex)
                    throw new Exception($"PolygonMinuteLoader: Error while download from {url}. Error message: {ex.Message}");

                virtualFileEntries.Add(new VirtualFileEntry(entryName, (string)o));
            }

            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);

            Logger.AddMessage($"!Finished. No errors. {mySymbols.Count} symbols. Zip file name: {zipFileName}");
        }

        /*public static void StartWithDateRange()
        {
            Logger.AddMessage($"Started");

            var folder = $@"E:\Quote\WebData\Minute\Polygon\DataBuffer\Minute5Years_20230412.1\";

            var symbolAndDates = new List<Tuple<string, DateTime, DateTime>>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 500;
                //cmd.CommandText = "select symbol, min(date) MinDate, max(date) MaxDate from dbQ2023..DayPolygon " +
                  //                "where [close]*[volume]>=5000000 and date>= '2018-04-03' group by symbol";
                cmd.CommandText = "select a.Symbol, min(a.Date) MinDate, max(a.Date) MaxDate from dbQ2023..DayPolygon a "+
                                  "left join dbQ2023..ZipLogMinutePolygon b on a.Symbol = b.Symbol and a.Date = b.Date " +
                                  "where a.[Close]*a.Volume >= 5000000 and b.Date is null and a.Date >= '2018-04-10' "+
                                  "group by a.Symbol order by 1";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        symbolAndDates.Add(Tuple.Create((string)rdr["Symbol"], (DateTime)rdr["MinDate"], (DateTime)rdr["MaxDate"]));
            }

            var cnt = 0;
            var maxDate = new DateTime(2023, 4, 06);
            foreach (var item in symbolAndDates)
            {
                Logger.AddMessage($"Downloaded {cnt++} tickers from {symbolAndDates.Count}");

                var currentDate = item.Item2;
                while (currentDate <= item.Item3)
                {
                    var endDate = currentDate.AddMonths(2);
                    if (endDate > maxDate)
                        endDate = maxDate;
                    currentDate = currentDate.AddDays(-5); // overlay between day

                    var jsonFileName = $"{folder}pMin_{item.Item1}_{currentDate:yyyyMMdd}.json";
                    var urlTicker = PolygonCommon.GetPolygonTicker(item.Item1);
                    var url = $"https://api.polygon.io/v2/aggs/ticker/{urlTicker}/range/1/minute/{currentDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}?adjusted=false&sort=asc&limit=50000&apiKey={PolygonCommon.GetApiKey()}";
                    if (!File.Exists(jsonFileName))
                    {
                        Download.DownloadToFile(url, jsonFileName);
                        if (File.Exists(jsonFileName))
                        {
                        }
                        else
                        {
                            throw new Exception($"Error while downloading. Url: {url}. Filename: {Path.GetFileName(jsonFileName)}");
                            // ! error
                        }
                    }

                    if (endDate == maxDate)
                        break;

                    currentDate = endDate;
                }
            }

            Logger.AddMessage($"!Finished. Downloaded data for {cnt} tickers");
        }*/
    }
}
