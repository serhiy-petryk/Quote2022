using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.Polygon
{
    public static class PolygonCopyZipInfoToDb
    {
        private const string ZipFolder = @"E:\Quote\WebData\Minute\Polygon\Data";
        private const string ZipFileTemplate = ZipFolder + @"\MinutePolygon_{0}.zip";

        public static void Start()
        {
            Logger.AddMessage("Get new file list");

            var fileCount = 0;
            var itemCount = 0;
            var nonExistentDates = 0;

            var files = Directory.GetFiles(ZipFolder, "*.zip").ToDictionary(a => a.ToLower(), File.GetLastWriteTime);
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 150;
                cmd.CommandText = "SELECT [Date], max(timestamp) MaxTimeStamp FROM [dbQ2023].[dbo].[ZipLogMinutePolygon] group by date order by 1";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        var filename = string.Format(ZipFileTemplate, ((DateTime)rdr["Date"]).ToString("yyyyMMdd")).ToLower();
                        var timestamp = (DateTime)rdr["MaxTimeStamp"];

                        if (!files.ContainsKey(filename))
                            files.Add(filename, DateTime.MinValue);
                        else if (files.ContainsKey(filename) && files[filename] < timestamp)
                            files.Remove(filename);
                    }

                foreach (var kvp in files)
                {
                    fileCount++;
                    Logger.AddMessage($"Processed {fileCount} files from {files.Count}");

                    var fileDate = DateTime.ParseExact(Path.GetFileNameWithoutExtension(kvp.Key).Split('_')[1], "yyyyMMdd", CultureInfo.InvariantCulture);
                    cmd.CommandText = $"DELETE from [dbQ2023].[dbo].[ZipLogMinutePolygon] where date='{fileDate:yyyy-MM-dd}'";
                    cmd.ExecuteNonQuery();

                    if (File.Exists(kvp.Key))
                    {
                        var items = new List<LogEntry>();
                        using (var zipArchive = ZipFile.Open(kvp.Key, ZipArchiveMode.Read))
                            foreach (var entry in zipArchive.Entries.Where(a => a.Name.EndsWith(".csv")))
                            {
                                var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                                var symbol = ss[0];
                                var date = DateTime.ParseExact(ss[1], "yyyyMMdd", CultureInfo.InvariantCulture);
                                var lines = entry.GetLinesOfZipEntry().ToArray();
                                var logEntry = new LogEntry {Symbol = symbol, Date = date, Created = entry.LastWriteTime.DateTime};
                                for (var k = 1; k < lines.Length; k++)
                                {
                                    ss = lines[k].Split(',');
                                    var dateTime = DateTime.ParseExact(ss[0], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                                    var open = float.Parse(ss[1], CultureInfo.InvariantCulture);
                                    var high = float.Parse(ss[2], CultureInfo.InvariantCulture);
                                    var low = float.Parse(ss[3], CultureInfo.InvariantCulture);
                                    var close = float.Parse(ss[4], CultureInfo.InvariantCulture);
                                    var volume = long.Parse(ss[5], CultureInfo.InvariantCulture);
                                    var tradeCount = int.Parse(ss[7], CultureInfo.InvariantCulture);

                                    if (dateTime.Date != date)
                                        throw new Exception($"Bad date in zip {entry.Name}: {dateTime:yyyy-MM-dd HH:mm} ");

                                    if (k == 1)
                                        logEntry.MinTime = dateTime.TimeOfDay;

                                    logEntry.CountFull++;
                                    logEntry.VolumeFull += volume;
                                    logEntry.MaxTime = dateTime.TimeOfDay;
                                    logEntry.TradeCount += tradeCount;

                                    if (dateTime.TimeOfDay >= CsUtils.StartTrading && dateTime.TimeOfDay < CsUtils.EndTrading)
                                    {
                                        logEntry.Count++;
                                        logEntry.Volume += volume;
                                        if (logEntry.Count == 1)
                                        {
                                            logEntry.Open = open;
                                            logEntry.High = high;
                                            logEntry.Low = low;
                                            logEntry.Close = close;
                                        }
                                        else
                                        {
                                            logEntry.Close = close;
                                            if (high > logEntry.High) logEntry.High = high;
                                            if (low < logEntry.Low) logEntry.Low = low;
                                        }
                                    }
                                }

                                items.Add(logEntry);
                            }

                        DbUtils.SaveToDbTable(items, "dbQ2023..ZipLogMinutePolygon", "Symbol", "Date", "MinTime",
                            "MaxTime", "Count", "CountFull", "Open", "High", "Low", "Close", "Volume", "VolumeFull",
                            "TradeCount", "Created");
                        itemCount += items.Count;
                    }
                    else
                        nonExistentDates++;
                }
            }
            Logger.AddMessage($"!Finished. Processed {fileCount} zip files. Logged {itemCount} date&time items. Delete data for {nonExistentDates} non-existent dates.");
        }

        private class LogEntry
        {
            public string Symbol;
            public DateTime Date;
            public TimeSpan MinTime;
            public TimeSpan MaxTime;
            public short Count;
            public short CountFull;
            public float Open;
            public float High;
            public float Low;
            public float Close;
            public long Volume;
            public long VolumeFull;
            public int TradeCount;
            public DateTime Created;
        }
    }
}
