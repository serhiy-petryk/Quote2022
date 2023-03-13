using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Quote2022.Helpers;

namespace Quote2022.Actions
{
    public static class MinuteYahoo_SaveLogToDb
    {
        public class LogEntry
        {
            public string File;
            public string Symbol;
            public DateTime Date;
            public TimeSpan MinTime;
            public TimeSpan MaxTime;
            public short Count;
            public float Open;
            public float High;
            public float Low;
            public float Close;
            public float Volume;
        }

        private static bool _isBusy = false;
        private static TimeSpan _startTrading = new TimeSpan(9, 30, 0);
        private static TimeSpan _endTrading = new TimeSpan(16, 0, 0);

        public static void Start(string[] zipFiles, Action<string> showStatusAction)
        {
            if (_isBusy)
            {
                MessageBox.Show("MinuteYahoo_SaveLogToDb is working now .. Can't run it again.");
                return;
            }

            _isBusy = true;

            var log = new List<LogEntry>();
            var cnt = 0;
            foreach (var zipFile in zipFiles)
            {
                showStatusAction($"MinuteYahoo_SaveLog is working for {Path.GetFileName(zipFile)}");

                // delete old log in database
                using (var conn = new SqlConnection(Settings.DbConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText =
                        $"DELETE FileLogIntradayYahoo WHERE [file]='{Path.GetFileNameWithoutExtension(zipFile)}'";
                    cmd.ExecuteNonQuery();
                }

                var dates = new Dictionary<DateTime, object[]>();
                using (var zip = new ZipReader(zipFile))
                    foreach (var item in zip)
                        if (item.Length > 0)
                        {
                            if (!item.FileNameWithoutExtension.ToUpper().StartsWith("YMIN-"))
                                throw new Exception(
                                    $"Bad file name in {zipFile}: {item.FileNameWithoutExtension}. Must starts with 'yMin-'");

                            cnt++;
                            if ((cnt % 100) == 0)
                                showStatusAction(
                                    $"MinuteYahoo_SaveLog is working for {Path.GetFileName(zipFile)}. Total file processed: {cnt:N0}");

                            var symbol = item.FileNameWithoutExtension.Substring(5);
                            var o = JsonConvert.DeserializeObject<Models.MinuteYahoo>(item.Content);
                            if (o.Chart.Error != null)
                            {
                                // errors.Add(symbol, o.Chart.Error.Description ?? o.Chart.Error.Code);
                                continue;
                            }

                            var minuteQuotes = o.GetQuotes(symbol);
                            var groups = minuteQuotes.GroupBy(a => a.Timed.Date);
                            foreach (var group in groups)
                            {
                                var entry = new LogEntry { File = Path.GetFileNameWithoutExtension(zipFile), Symbol = symbol };
                                log.Add(entry);
                                foreach (var q in group.OrderBy(a => a.Timed))
                                {
                                    entry.Count++;
                                    entry.Volume += q.Volume;
                                    entry.Close = q.Close;
                                    if (entry.Count == 1)
                                    {
                                        entry.Date = q.Timed.Date;
                                        entry.MinTime = q.Timed.TimeOfDay;
                                        entry.Open = q.Open;
                                        entry.High = q.High;
                                        entry.Low = q.Low;
                                    }
                                    else
                                    {
                                        entry.MaxTime = q.Timed.TimeOfDay;
                                        if (entry.High < q.High) entry.High = q.High;
                                        if (entry.Low > q.Low) entry.Low = q.Low;
                                    }
                                }
                            }
                        }

                showStatusAction($"MinuteYahoo_SaveLogToDb. Save data to database ...");
                // Save items to database table
                SaveToDb.SaveToDbTable(log, "FileLogIntradayYahoo", "File", "Symbol", "Date", "MinTime", "MaxTime",
                    "Count", "Open", "High", "Low", "Close", "Volume");

                _isBusy = false;
                showStatusAction($"MinuteYahoo_SaveLogToDb finished");
            }
        }
    }
}

