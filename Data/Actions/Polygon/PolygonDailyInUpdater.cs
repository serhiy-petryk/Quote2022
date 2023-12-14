using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Polygon
{
    public class PolygonDailyInUpdater
    {
        private const string ZipFolder = @"E:\Quote\WebData\Minute\Polygon\Data";
        private const string ZipFileTemplate = ZipFolder + @"\MinutePolygon_{0}.zip";
        private static readonly TimeSpan StartTime = new TimeSpan(9, 54, 0);
        private static readonly TimeSpan EndTime = new TimeSpan(15, 45, 0);

        public static void Run()
        {
            Logger.AddMessage("Get symbol/date list");

            var items = new List<DbEntry>();
            var itemCount = 0;
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT * from dbQ2023..DayPolygon a "+
                                  "left join dbQ2023..DayPolygonIn b on a.Symbol = b.Symbol and a.Date = b.Date "+
                                  "left join dbQ2023Others..TradingDaysSpecific c on a.Date = c.Date "+
                                  "where c.Date is null and b.Symbol is null "+
                                  "order by a.Date, a.Symbol";
                cmd.CommandTimeout = 60 * 5;

                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        if (itemCount % 100 == 0)
                            Logger.AddMessage($"Generated {itemCount:N0} items");
                        var item = new DbEntry() { Symbol = (string)rdr["Symbol"], Date = (DateTime)rdr["Date"] };
                        FillDbEntry(item);
                        items.Add(item);
                        itemCount++;

                        if (items.Count > 1000)
                        {
                            Logger.AddMessage($"Saving items to database");
                            DbUtils.SaveToDbTable(items, "dbQ2023..DayPolygonIn", "Symbol", "Date", "Open", "High",
                                "Low", "Close", "Final", "Volume", "TradeCount");
                            items.Clear();
                        }
                    }

                if (items.Count > 0)
                {
                    Logger.AddMessage($"Saving items to database");
                    DbUtils.SaveToDbTable(items, "dbQ2023..DayPolygonIn", "Symbol", "Date", "Open", "High",
                        "Low", "Close", "Final", "Volume", "TradeCount");
                    items.Clear();
                }

                Logger.AddMessage($"!Finished. Processed {itemCount} items.");
            }
        }

        private static void FillDbEntry(DbEntry dbEntry)
        {
            var zipFilename = string.Format(ZipFileTemplate, dbEntry.Date.ToString("yyyyMMdd"));
            if (File.Exists(zipFilename))
            {
                var entryToFind = $"{dbEntry.Symbol}_{dbEntry.Date:yyyyMMdd}.csv";
                using (var zipArchive = ZipFile.Open(zipFilename, ZipArchiveMode.Read))
                    foreach (var entry in zipArchive.Entries.Where(a =>
                        string.Equals(a.Name, entryToFind, StringComparison.OrdinalIgnoreCase)))
                    {
                        var lines = entry.GetLinesOfZipEntry().ToArray();
                        // var dbEntry = new DbEntry() { Symbol = symbol, Date = date };
                        for (var k = 1; k < lines.Length; k++)
                        {
                            var ss = lines[k].Split(',');
                            var dateTime = DateTime.ParseExact(ss[0], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                            var open = float.Parse(ss[1], CultureInfo.InvariantCulture);
                            var high = float.Parse(ss[2], CultureInfo.InvariantCulture);
                            var low = float.Parse(ss[3], CultureInfo.InvariantCulture);
                            var close = float.Parse(ss[4], CultureInfo.InvariantCulture);
                            var volume = long.Parse(ss[5], CultureInfo.InvariantCulture);
                            var tradeCount = int.Parse(ss[7], CultureInfo.InvariantCulture);

                            if (dateTime.Date != dbEntry.Date)
                                throw new Exception($"Bad date in zip {entry.Name}: {dateTime:yyyy-MM-dd HH:mm} ");

                            if (dateTime.TimeOfDay >= StartTime && dateTime.TimeOfDay < EndTime)
                            {
                                dbEntry.Count++;
                                dbEntry.Volume += volume;
                                dbEntry.TradeCount += tradeCount;
                                if (dbEntry.Count == 1)
                                {
                                    dbEntry.Open = open;
                                    dbEntry.High = high;
                                    dbEntry.Low = low;
                                    dbEntry.Close = close;
                                }
                                else
                                {
                                    dbEntry.Close = close;
                                    if (high > dbEntry.High) dbEntry.High = high;
                                    if (low < dbEntry.Low) dbEntry.Low = low;
                                }
                            }
                            else if (dateTime.TimeOfDay >= EndTime && dateTime.TimeOfDay < CsUtils.EndTrading)
                            {
                                dbEntry.Final = open;
                                break;
                            }
                        }
                    }
            }
        }

        private class DbEntry
        {
            public string Symbol;
            public DateTime Date;
            public float Open;
            public float High;
            public float Low;
            public float Close;
            public float? Final;
            public long Volume;
            public int TradeCount;
            public int Count;
        }
    }
}
