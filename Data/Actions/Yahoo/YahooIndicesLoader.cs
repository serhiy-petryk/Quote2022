using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Data.Helpers;
using Data.Models;

namespace Data.Actions.Yahoo
{
    public static class YahooIndicesLoader
    {
        private const string UrlTemplate = "https://query1.finance.yahoo.com/v7/finance/download/{2}?period1={0}&period2={1}&interval=1d&events=history&includeAdjustedClose=true";
        private static readonly string[] Symbols = new[] { "^DJI", "^GSPC" };

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var maxDate = new DateTime(2000, 1, 1);
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "select max([date]) MaxDate from dbQ2023Others..TradingDays";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        maxDate = (DateTime)rdr["MaxDate"];
                        break;
                    }
            }

            var from = GetYahooTime(maxDate.AddDays(-30));
            var to = GetYahooTime(DateTime.Now);

            var timeStamp = CsUtils.GetTimeStamp();
            var entries = new List<VirtualFileEntry>();
            var data = new List<DayYahoo>();

            // Download data
            foreach (var symbol in Symbols)
            {
                Logger.AddMessage($"Download data for {symbol}");
                var url = string.Format(UrlTemplate, from, to, symbol);
                var o = Download.DownloadToString(url);
                if (o is Exception ex)
                    throw new Exception($"YahooIndicesLoader: Error while download from {url}. Error message: {ex.Message}");

                var lines = ((string)o).Split('\n');
                if (lines.Length == 0)
                    throw new Exception($"Invalid Day Yahoo quote file (no text lines): {o}");
                if (lines[0] != "Date,Open,High,Low,Close,Adj Close,Volume")
                    throw new Exception($"Invalid YahooIndex quotes file (check file header): {lines[0]}");

                for (var k = 1; k < lines.Length; k++)
                {
                    if (!String.IsNullOrEmpty(lines[k].Trim()))
                    {
                        if (lines[k].Contains("null"))
                            Debug.Print($"{symbol}, {lines[k]}");
                        else
                        {
                            var ss = lines[k].Split(',');
                            var item = new DayYahoo(symbol, ss);
                            data.Add(item);
                        }
                    }
                }
            }

            if (data.Count > 0)
            {
                DbUtils.ClearAndSaveToDbTable(data, "dbQ2023Others..Bfr_DayYahooIndexes", "Symbol", "Date", "Open", "High", "Low",
                    "Close", "Volume", "AdjClose");
                DbUtils.ExecuteSql("INSERT into dbQ2023Others..DayYahooIndexes (Symbol, Date, [Open], High, Low, [Close], Volume, AdjClose) " +
                                   "SELECT a.Symbol, a.Date, a.[Open], a.High, a.Low, a.[Close], a.Volume, a.AdjClose " +
                                   "from dbQ2023Others..Bfr_DayYahooIndexes a " +
                                   "left join dbQ2023Others..DayYahooIndexes b on a.Symbol = b.Symbol and a.Date = b.Date " +
                                   "where b.Symbol is null");

                Logger.AddMessage($"Update trading days");
                DbUtils.RunProcedure("dbQ2023Others..pRefreshTradingDays");
            }

            Logger.AddMessage($"!Finished. Last trade date: {data.Max(a => a.Date):yyyy-MM-dd}");

            long GetYahooTime(DateTime dt) => Convert.ToInt64((dt - new DateTime(1970, 1, 1)).TotalSeconds + 18000);
        }
    }
}

