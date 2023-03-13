using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Quote2022.Actions.MinuteAlphaVantage
{
    public static class MAV_Tests
    {
        public static void CheckMissingDays(Action<string> showStatus)
        {
            showStatus($"CheckMissingDays started. Load data from database ...");

            var tradingDays = new List<DateTime>();
            var symbolList = new Dictionary<string, DateTime>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT date from TradingDays WHERE date>'2021-01-01' and date<'2023-02-01'";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        tradingDays.Add((DateTime)rdr["Date"]);

                cmd.CommandText = "SELECT symbol, min(Date) MinDate, max(Date) MaxDate from dbQuote2023..FileLogMinuteAlphaVantage group by symbol";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        symbolList.Add((string)rdr["Symbol"], (DateTime)rdr["MinDate"]);
            }

            var cnt = 0;
            foreach (var kvp in symbolList)
            {
                cnt++;
                if (cnt % 10 == 0)
                    showStatus($"CheckMissingDays. Processed {cnt} symbols from {symbolList.Count}");

                var dates = tradingDays.Where(d => d >= kvp.Value).ToArray();

                var actualDates = new List<DateTime>();
                using (var conn = new SqlConnection(Settings.DbConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"select distinct date from dbQuote2023..FileLogMinuteAlphaVantage where symbol='{kvp.Key}'";
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                            actualDates.Add((DateTime) rdr["Date"]);
                }

                var missingDates = dates.Where(d => actualDates.All(d2 => d2 != d)).ToList();

                if (missingDates.Count != 0)
                {
                    using (var conn = new SqlConnection(Settings.DbConnectionString))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = $"select [File], FileCreated from dbQuote2023..FileLogMinuteAlphaVantage_BlankFiles where Symbol='{kvp.Key}'";
                        using (var rdr = cmd.ExecuteReader())
                            while (rdr.Read())
                            {
                                var file = (string) rdr["File"];
                                var ss1 = file.Split(new[] {@"\", "_"}, StringSplitOptions.None);
                                var ss2 = ss1[1].Split(new[] { @"Y", "M" }, StringSplitOptions.None);
                                var periods = (int.Parse(ss2[1])-1)*12 + int.Parse(ss2[2]);
                                var date = (DateTime) rdr["FileCreated"] - new TimeSpan(5,20,0);
                                var startDate = date.AddDays(-periods * 30).Date;
                                var endDate = startDate.AddDays(29).Date;
                                var ddates = missingDates.ToArray();
                                foreach (var d in ddates)
                                {
                                    if (d >= startDate && d <= endDate)
                                    {
                                        missingDates.Remove(d);
                                        Debug.Print($"Blank file\t{kvp.Key}\t{d:yyyy-MM-dd}");
                                    }
                                }
                            }
                    }

                    if (missingDates.Count > 0)
                    {
                        var quotes = new Dictionary<DateTime, Tuple<float, float>>();
                        var missingDates2021 = missingDates.Where(a => a.Year < 2022).OrderBy(a => a).ToArray();
                        if (missingDates2021.Length > 0)
                        {
                            using (var conn = new SqlConnection(Settings.DbConnectionString))
                            using (var cmd = conn.CreateCommand())
                            {
                                conn.Open();
                                cmd.CommandText =
                                    $"select a.Date, a.Volume, a.Volume*a.[Close]/1000000 TradeValue from DayYahoo a " +
                                    $"inner join SymbolsEoddata b on a.Symbol=b.YahooSymbol " +
                                    $"where a.Date>'2021-02-01' AND b.AlphaVantageSymbol='{kvp.Key}'";
                                using (var rdr = cmd.ExecuteReader())
                                    while (rdr.Read())
                                    {
                                        if (!quotes.ContainsKey((DateTime) rdr["Date"]))
                                            quotes.Add((DateTime) rdr["Date"],
                                                new Tuple<float, float>((float) rdr["Volume"],
                                                    (float) rdr["TradeValue"]));
                                    }
                            }

                            foreach (var d in missingDates2021)
                            {
                                if (quotes.ContainsKey(d))
                                {
                                    if (quotes[d].Item1 != 0F)
                                        Debug.Print(
                                            $"Missing (there is volume)\t{kvp.Key}\t{d:yyyy-MM-dd}\t{quotes[d].Item1}\t{quotes[d].Item2}");
                                }
                                else
                                    Debug.Print($"Missing (no in db)\t{kvp.Key}\t{d:yyyy-MM-dd}");
                            }
                        }

                        quotes.Clear();
                        var missingDates2022 = missingDates.Where(a => a.Year >= 2022).OrderBy(a => a).ToArray();
                        if (missingDates2022.Length > 0)
                        {
                            using (var conn = new SqlConnection(Settings.DbConnectionString))
                            using (var cmd = conn.CreateCommand())
                            {
                                conn.Open();
                                cmd.CommandText =
                                    $"select a.Date, a.Volume, a.Volume*a.[Close]/1000000 TradeValue from DayEoddata a " +
                                    $"inner join SymbolsEoddata b on a.Symbol=b.Symbol and a.Exchange=b.Exchange " +
                                    $"where b.AlphaVantageSymbol='{kvp.Key}'";
                                using (var rdr = cmd.ExecuteReader())
                                    while (rdr.Read())
                                        quotes.Add((DateTime) rdr["Date"],
                                            new Tuple<float, float>((float) rdr["Volume"], (float) rdr["TradeValue"]));
                            }

                            foreach (var d in missingDates2022)
                            {
                                if (quotes.ContainsKey(d))
                                {
                                    if (quotes[d].Item1 != 0F)
                                        Debug.Print(
                                            $"Missing (there is volume)\t{kvp.Key}\t{d:yyyy-MM-dd}\t{quotes[d].Item1}\t{quotes[d].Item2}");
                                }
                                else
                                    Debug.Print($"Missing (no in db)\t{kvp.Key}\t{d:yyyy-MM-dd}");
                            }
                        }
                    }
                }
            }

            showStatus($"CheckMissingDays finished");
        }

        public static void RenameFiles(Action<string> showStatus)
        {
            var rr = new Dictionary<string, string>();
            for (var k = 1; k <= 12; k++)
            {
                rr.Add("year2month" + k, "Y2M" + k);
                rr.Add("year1month" + k, "Y1M" + k);
            }

            var folder = @"E:\Quote\WebData\Minute\AlphaVantage\DataBuffer.Y2M12\MissingSymbols\";
            var files = Directory.GetFiles(folder, "*.csv", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var fn = Path.GetFileNameWithoutExtension(file);
                if (fn.IndexOf("year", StringComparison.InvariantCulture) == -1 ||
                    fn.IndexOf("month", StringComparison.InvariantCulture) == -1)
                {
                }
                else
                {
                    var newFn = file;
                    foreach (var kvp in rr)
                    {
                        if (newFn.IndexOf("AV" + kvp.Key + "_", StringComparison.InvariantCulture) != -1)
                        {
                            newFn = file.Replace("AV" + kvp.Key + "_", "av" + kvp.Value + "_");
                            break;
                        }
                    }

                    if (string.Equals(newFn, file, StringComparison.InvariantCultureIgnoreCase))
                        throw new Exception("Check");

                    File.Move(file, newFn);
                }
            }

            showStatus($"RenameFiles finished");
        }
    }
}
