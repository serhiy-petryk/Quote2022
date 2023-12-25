using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
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
            var to = DateTime.MinValue;
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 150;
                cmd.CommandText = "select count(*) from dbQ2023..DayPolygon a " +
                                  "left join dbQ2023..SymbolsPolygon b on a.Symbol = b.Symbol and a.Date between b.Date and isnull(b.[To], GetDate()) " +
                                  "inner join(select symbol, min(date) MinDate from dbQ2023..SymbolsPolygon group by symbol) c on a.Symbol = c.Symbol " +
                                  "where b.Symbol is null";
                var recs1 = (int)cmd.ExecuteScalar();

                cmd.CommandText = "select count(*) from dbQ2023..SymbolsPolygon a " +
                                  "inner join dbQ2023..SymbolsPolygon b on a.Symbol = b.Symbol and b.Date between a.Date and isnull(a.[To], GetDate()) " +
                                  "where a.Date<>b.Date";
                var recs2 = (int)cmd.ExecuteScalar();

                if (recs1 != 0 || recs2 != 0)
                {
                    MessageBox.Show(
                        $"There are errors in DayPolygon & DaySymbols tables. Check data using CheckPolygonSymbols.sql",
                        "", MessageBoxButtons.OK);
                    return;
                }


                cmd.CommandText = "SELECT Symbol, MIN(date) MinDate, MAX(date) MaxDate FROM dbQ2023..DayPolygon "+
                                  "WHERE Date >= DATEADD(day, -14, GetDate()) "+
                                  "GROUP BY Symbol ORDER BY 1";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        symbols.Add((string)rdr["Symbol"]);
                        var minDate = (DateTime)rdr["MinDate"];
                        if (minDate < from) from = minDate;
                        var maxDate = (DateTime)rdr["MaxDate"];
                        if (maxDate > to) to = maxDate;
                    }
            }

            Start(symbols, from, to);
        }

        public static void RunOthers_2023_12_14()
        {
            const int days = 7 * 10;
            // var lastDate = new DateTime(2023, 12, 8);
            var lastDate = new DateTime(2018, 12, 14);
            var startDate = new DateTime(2018, 3, 9);
            // var startDate = new DateTime(2021, 04, 02);
            var dates = new List<DateTime>();
            while (startDate<=lastDate)
            {
                dates.Add(startDate);
                startDate = startDate.AddDays(days);
            }

            foreach (var date in dates)
            {
                var from = date.AddDays(-days);
                var to = date;
                var symbols = new List<string>();
                using (var conn = new SqlConnection(Settings.DbConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = "select distinct a.symbol from dbPolygon2003..DayPolygon a " +
                                      "left join dbQ2023..FileLogMinutePolygon b on a.Symbol = b.Symbol and a.Date = b.Date " +
                                      $"where b.Symbol is null and a.date between '{from:yyyy-MM-dd}' and '{to:yyyy-MM-dd}' "+
                                      "order by 1";
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read()) symbols.Add((string)rdr["Symbol"]);
                    }
                Start(symbols, from,to);
            }
        }

        public static void Start(List<string> mySymbols, DateTime from, DateTime to)
        {
            Logger.AddMessage($"Started");

            var folder = string.Format(FolderTemplate, to.AddDays(1).ToString("yyyyMMdd"));
            if (MessageBox.Show(
                    $"You are going to download data from {from:yyyy-MM-dd} to {to:yyyy-MM-dd} for {mySymbols.Count} symbols in {folder} folder! Continue?", "",
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
                    zipEntries = zipArchive.Entries.ToDictionary(a => a.FullName.ToLower(), a => (object)null);
            }

            var virtualFileEntries = new List<VirtualFileEntry>();
            foreach (var mySymbol in mySymbols)
            {
                Logger.AddMessage($"Downloaded {cnt++} tickers from {mySymbols.Count}");

                var entryName = $@"{Path.GetFileNameWithoutExtension(zipFileName)}\pMin_{mySymbol}_{from:yyyyMMdd}.json";
                var urlTicker = PolygonCommon.GetPolygonTicker(mySymbol);
                if (PolygonCommon.IsTestTicker(urlTicker) || zipEntries.ContainsKey(entryName.ToLower()))
                    continue;

                // var url = $"https://api.polygon.io/v2/aggs/ticker/{urlTicker}/range/1/minute/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}?adjusted=false&sort=asc&limit=50000&apiKey={PolygonCommon.GetApiKey()}";
                var url = string.Format(UrlTemplate, urlTicker, from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"), PolygonCommon.GetApiKey());
                var o = Download.DownloadToString(url);
                if (o is Exception ex)
                    throw new Exception($"PolygonMinuteLoader: Error while download from {url}. Error message: {ex.Message}");

                ZipUtils.ZipVirtualFileEntries(zipFileName, new[] { new VirtualFileEntry(entryName, (string)o) });
            }

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
