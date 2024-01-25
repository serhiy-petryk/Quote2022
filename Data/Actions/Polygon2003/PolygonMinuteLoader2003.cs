using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Data.Actions.Polygon;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Polygon2003
{
    public static class PolygonMinuteLoader2003
    {
        private const string UrlTemplate = "https://api.polygon.io/v2/aggs/ticker/{0}/range/1/minute/{1}/{2}?adjusted=false&sort=asc&limit=50000&apiKey={3}";
        private const string ZipFileNameTemplate = @"E:\Quote\WebData\Minute\Polygon2003\DataBuffer\MP2003_{0}.zip";
        private const string FileNameTemplate = @"E:\Quote\WebData\Minute\Polygon2003\DataBuffer\MP2003_{0}\MP2003_{2}_{1}.json";

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
                cmd.CommandTimeout = 300;
                cmd.CommandText = "select a.Symbol, Min(a.Date) MinDate, MAX(a.Date) MaxDate from dbPolygon2003..DayPolygon a "+
                                  "inner join(select MinDate, iif(DATEADD(Day,70,MinDate)< GetDate(), DATEADD(Day, 70, MinDate), GetDate()) MaxDate "+
                                  "from (select DATEADD(day, -5, max(date)) MinDate from dbPolygon2003MinuteLog..MinutePolygonLog) x) b "+
                                  "on a.Date between b.MinDate and b.MaxDate GROUP BY a.Symbol order by 1";
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

            Logger.AddMessage($"Finished!");
        }

        public static void StartAll()
        {
            Logger.AddMessage($"Started");

            var startDate=new DateTime(2003,9,6);
            var endDate = DateTime.Today.AddDays(-70);
            var dates = new List<DateTime>();

            while (startDate <= endDate)
            {
                dates.Add(startDate);
                startDate = startDate.AddDays(+70);
            }

            foreach (var from in dates)
            {
                var to = from.AddDays(70 - 1);
                var zipFileName = string.Format(ZipFileNameTemplate, to.AddDays(1).ToString("yyyyMMdd"));
                if (File.Exists(zipFileName)) continue;

                var mySymbols = new List<string>();

                Logger.AddMessage($"Define symbols to download from {from:yyyy-MM-dd} to {to:yyyy-MM-dd} ...");

                using (var conn = new SqlConnection(Settings.DbConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandTimeout = 150;
                    cmd.CommandText = $"SELECT DISTINCT Symbol FROM dbPolygon2003..DayPolygon WHERE Date between '{from:yyyy-MM-dd}' AND '{to:yyyy-MM-dd}' ORDER BY 1";
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                            mySymbols.Add((string) rdr["Symbol"]);
                }

                Start(mySymbols, from, to);
            }

            Logger.AddMessage($"Finished!");
        }

        public static void Start(List<string> mySymbols, DateTime from, DateTime to)
        {
            Logger.AddMessage($"Started");

            var cnt = 0;
            var zipFileName = string.Format(ZipFileNameTemplate, to.AddDays(1).ToString("yyyyMMdd"));
            if (File.Exists(zipFileName))
                return;

            var folder = Path.GetDirectoryName(string.Format(FileNameTemplate, to.AddDays(1).ToString("yyyyMMdd"), "", ""));
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            Task task = null;
            foreach (var mySymbol in mySymbols)
            {
                Logger.AddMessage($"Downloaded {cnt++} tickers from {mySymbols.Count}. Dates from {from:yyyy-MM-dd} to {to:yyyy-MM-dd}.");

                var symbol = PolygonCommon.GetPolygonTicker(mySymbol);
                var fileName = string.Format(FileNameTemplate, to.AddDays(1).ToString("yyyyMMdd"), from.ToString("yyyyMMdd"), mySymbol);
                if (File.Exists(fileName))
                    continue;

                // var url = $"https://api.polygon.io/v2/aggs/ticker/{urlTicker}/range/1/minute/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}?adjusted=false&sort=asc&limit=50000&apiKey={PolygonCommon.GetApiKey()}";
                var url = string.Format(UrlTemplate, symbol, from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"), PolygonCommon.GetApiKey2003());
                var o = Download.DownloadToString(url);
                if (o is Exception ex)
                    throw new Exception($"PolygonMinuteLoader: Error while download from {url}. Error message: {ex.Message}");

                task?.Wait();
                task = File.WriteAllTextAsync(fileName, (string)o);
            }

            task?.Wait();

            Logger.AddMessage($"Create zip file: {Path.GetFileName(zipFileName)}");
            ZipUtils.CreateZip(folder, zipFileName);
            Directory.Delete(folder, true);

            Logger.AddMessage($"!Finished. No errors. {mySymbols.Count} symbols. Folder: {folder}");
        }

        public static void StartZip(List<string> mySymbols, DateTime from, DateTime to)
        {
            Logger.AddMessage($"Started");

            var cnt = 0;
            var zipFileName = string.Format(ZipFileNameTemplate, to.AddDays(1).ToString("yyyyMMdd"));
            var zipEntries = new Dictionary<string, object>();
            if (File.Exists(zipFileName))
            {
                using (var zipArchive = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    zipEntries = zipArchive.Entries.ToDictionary(a => a.FullName.ToLower(), a => (object)null);
            }

            var virtualFileEntries = new List<VirtualFileEntry>();
            foreach (var mySymbol in mySymbols)
            {
                Logger.AddMessage($"Downloaded {cnt++} tickers from {mySymbols.Count}. Dates from {from:yyyy-MM-dd} to {to:yyyy-MM-dd}.");

                var entryName = $@"{Path.GetFileNameWithoutExtension(zipFileName)}\pMin_{mySymbol}_{from:yyyyMMdd}.json";
                var urlTicker = PolygonCommon.GetPolygonTicker(mySymbol);
                if (zipEntries.ContainsKey(entryName.ToLower()))
                    continue;

                // var url = $"https://api.polygon.io/v2/aggs/ticker/{urlTicker}/range/1/minute/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}?adjusted=false&sort=asc&limit=50000&apiKey={PolygonCommon.GetApiKey()}";
                var url = string.Format(UrlTemplate, urlTicker, from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"), PolygonCommon.GetApiKey2003());
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
