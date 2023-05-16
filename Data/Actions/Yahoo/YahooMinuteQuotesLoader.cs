using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using Data.Helpers;

namespace Data.Actions.Yahoo
{
    public static class YahooMinuteQuotesLoader
    {
        private const string UrlTemplate = @"https://query2.finance.yahoo.com/v8/finance/chart/{0}?period1={1}&period2={2}&interval=1m&events=history";

        public static void Start()
        {
            /*var previousFriday = CsUtils.GetPreviousWeekday(DateTime.Now, DayOfWeek.Friday);
            var previousMonday = previousFriday.AddDays(-4);
            Start(previousMonday, 5, GetDefaultYahooSymbolList());*/
            var previousThursday = CsUtils.GetPreviousWeekday(DateTime.Now, DayOfWeek.Thursday);
            var previousMonday = previousThursday.AddDays(-3);
            Start(previousMonday, 5, GetDefaultYahooSymbolList());
        }

        public static void Start(DateTime from, int days, ICollection<string> yahooSymbols)
        {
            Logger.AddMessage($"Started");

            var timeStamp = CsUtils.GetTimeStamp();
            var folder = $@"D:\Quote\WebData\Minute\Yahoo\Data\YahooMinute_{timeStamp.Item2}\";

            var fromInSeconds = GetYahooDate(from);
            var toInSeconds = GetYahooDate(from.AddDays(days));

            var cnt = 0;
            var downloadErrors = new List<string>();
            foreach (var symbol in yahooSymbols)
            {
                cnt++;
                if (cnt%10 == 0)
                    Logger.AddMessage($@"Downloaded {cnt:N0} files from {yahooSymbols.Count:N0}");

                var url = string.Format(UrlTemplate, symbol, fromInSeconds, toInSeconds);
                var filename = folder + $"yMin-{symbol}.txt";
                var error = Download.DownloadToFile(url, filename);
                if (error != null)
                    downloadErrors.Add($"{symbol}\t{error}");
                Thread.Sleep(300);
            }

            if (downloadErrors.Count > 0)
            {
                var errorFileName = $@"{Directory.GetParent(folder.TrimEnd(Path.DirectorySeparatorChar))}\DownloadErrors_{timeStamp.Item2}.txt";
                File.WriteAllLines(errorFileName, downloadErrors);
            }

            // Zip data
            Logger.AddMessage($@"Zip data. {Directory.GetFiles(folder, "*.txt").Length:N0} files");
            var zipFileName = Helpers.ZipUtils.ZipFolder(folder);

            // Remove json files
            Directory.Delete(folder, true);

            if (downloadErrors.Count > 0)
                Logger.AddMessage($"!Finished. Found {downloadErrors.Count} ERRORS. Items: {yahooSymbols.Count:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
            else
                Logger.AddMessage($"!Finished. No errors. Items: {yahooSymbols.Count:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");

            // =========================
            int GetYahooDate(DateTime dt)
            {
                var offsetDate = new DateTime(1970, 1, 1);
                // var tzi = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

                // var seconds = (dt - offsetDate).TotalSeconds + (tzi.GetUtcOffset(dt)).TotalSeconds;
                var seconds = (dt - offsetDate).TotalSeconds;
                return Convert.ToInt32(seconds);
            }

        }

        private static List<string> GetDefaultYahooSymbolList()
        {
            // var symbols = new List<string> { "YS", "FORLU", "BLACR", "CANB", "BLACW", "YSBPW", "BLAC", "CLCO" };
            var symbols = new List<string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT '^DJI' Symbol UNION SELECT '^GSPC' UNION "+
                                  "SELECT b.YahooSymbol FROM dbQuote2022..DayEoddata a "+
                                  "INNER JOIN dbQuote2022..SymbolsEoddata b on a.Exchange = b.Exchange and a.Symbol = b.Symbol "+
                                  "WHERE b.YahooSymbol is not null AND a.volume* a.[close]>= 5000000 and a.date >= DATEADD(day, -30, GetDate()) UNION "+
                                  "SELECT b.Symbol from dbQuote2022..SymbolsEoddata a "+
                                  "RIGHT JOIN(SELECT * from dbQuote2022..ScreenerNasdaqStock "+
                                  "WHERE Deleted is null or Deleted > DATEADD(day, -30, GetDate())) b "+
                                  "ON a.NasdaqSymbol = b.Symbol WHERE a.Symbol is null AND b.MaxTradeValue > 5 ORDER BY 1";

                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        symbols.Add((string)rdr["Symbol"]);
            }

            return symbols;

        }
    }
}
