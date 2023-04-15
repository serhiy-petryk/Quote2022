using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Data.Helpers;
using Newtonsoft.Json;

namespace Data.Actions.Polygon
{
    public static class PolygonMinuteSaveLogToDb
    {
        public static void Start(string zipFileNameOrFolderName)
        {
            var folderId = FolderReader.GetFolderId(zipFileNameOrFolderName);

            Logger.AddMessage($"Started. Delete old log in database.");
            DbUtils.ExecuteSql($"DELETE dbQ2023..FileLogMinutePolygon WHERE folder='{folderId}'");
            DbUtils.ExecuteSql($"DELETE dbQ2023..FileLogMinutePolygon_BlankFiles WHERE folder='{folderId}'");

            var errorLog = new List<string>();
            ProcessEntry(zipFileNameOrFolderName, errorLog);

            // Save errors to file
            var errorFileName = folderId + @".SaveLogToDbError.txt";
            if (File.Exists(errorFileName))
                File.Delete(errorFileName);
            File.AppendAllText(errorFileName, $"File\tMessage\tContent{Environment.NewLine}");
            File.AppendAllLines(errorFileName, errorLog);

            if (errorLog.Count > 0)
                Logger.AddMessage($"!Finished. Found {errorLog.Count} errors. Error filename: {errorFileName}");
            else
                Logger.AddMessage($"!Finished. No errors.");
        }

        private static void ProcessEntry(string zipFileNameOrFolderName, List<string> errorLog)
        {
            var log = new List<LogEntry>();
            var blankFiles = new List<BlankFile>();

            var cnt = 0;
            using (var reader = new FolderReader(zipFileNameOrFolderName, ".json"))
            {
                foreach (var entry in reader.Entries)
                {
                    cnt++;
                    if (cnt % 10 == 0)
                        Logger.AddMessage(
                            $"Processed {cnt} from {reader.Entries.Length} entries in {Path.GetFileName(zipFileNameOrFolderName)}");

                    var oo = JsonConvert.DeserializeObject<PolygonCommon.cMinuteRoot>(entry.AllText);
                    if (oo.adjusted || !(oo.status == "OK" || oo.status == "DELAYED"))
                        throw new Exception("Check parser");

                    if (!string.IsNullOrEmpty(oo.next_url))
                    {
                        Debug.Print($"NEXT URL:\t{entry.FileName}\t{oo.next_url}");
                        errorLog.Add($"{entry.FileName}\tPartial downloading\tNext url: {oo.next_url}");
                    }

                    if (oo.count == 0 && (oo.results == null || oo.results.Length == 0))
                    {
                        blankFiles.Add(new BlankFile
                        {
                            Folder = reader.FolderId,
                            FileName = entry.FileName,
                            FileCreated = entry.Created,
                            Symbol = oo.Symbol
                        });
                        continue;
                    }

                    LogEntry logEntry = null;
                    var lastDate = DateTime.MinValue;
                    for (var k = 0; k < oo.results.Length; k++)
                    {
                        var item = oo.results[k];
                        if (item.Open > item.High || item.Open < item.Low || item.Close > item.High ||
                            item.Close < item.Low || item.Low < 0)
                            errorLog.Add(
                                $"{entry.FileName}\tBad prices in {k} item\tItem date&time: {item.DateTime:yyyy-MM-dd HH:mm}");
                        if (item.Volume < 0)
                            errorLog.Add(
                                $"{entry.FileName}\tBad volume in {k} item\tItem date&time: {item.DateTime:yyyy-MM-dd HH:mm}");
                        if (item.Volume == 0 && item.High != item.Low)
                            errorLog.Add(
                                $"{entry.FileName}\tPrices are not equal when volume=0 in {k} line\tItem date&time: {item.DateTime:yyyy-MM-dd HH:mm}");

                        if (item.DateTime.Date != lastDate)
                        {
                            if (log.Count > 100000)
                                SaveToDb(log, blankFiles);

                            var position = (logEntry == null ? "First" : "Middle");
                            logEntry = new LogEntry
                            {
                                Folder = entry.FolderId,
                                FileName = entry.FileName,
                                Symbol = oo.Symbol,
                                Date = item.DateTime.Date,
                                Position = position,
                                Status = oo.status,
                                Created = entry.Created
                            };
                            log.Add(logEntry);
                            logEntry.MinTime = item.DateTime.TimeOfDay;

                            lastDate = item.DateTime.Date;
                        }

                        logEntry.CountFull++;
                        logEntry.VolumeFull += item.Volume;
                        logEntry.MaxTime = item.DateTime.TimeOfDay;
                        logEntry.TradeCount += item.TradeCount;

                        if (item.DateTime.TimeOfDay >= CsUtils.StartTrading && item.DateTime.TimeOfDay < CsUtils.EndTrading)
                        {
                            logEntry.Count++;
                            logEntry.Volume += item.Volume;
                            if (logEntry.Count == 1)
                            {
                                logEntry.Open = item.Open;
                                logEntry.High = item.High;
                                logEntry.Low = item.Low;
                                logEntry.Close = item.Close;
                            }
                            else
                            {
                                logEntry.Close = item.Close;
                                if (item.High > logEntry.High) logEntry.High = item.High;
                                if (item.Low < logEntry.Low) logEntry.Low = item.Low;
                            }
                        }
                    }

                    if (logEntry != null)
                        logEntry.Position = oo.next_url == null ? "Last" : "PARTIAL";
                }

            }

            if (log.Count > 0 || blankFiles.Count > 0)
                SaveToDb(log, blankFiles);
        }


        private static void SaveToDb(List<LogEntry> log, List<BlankFile> blankFiles)
        {
            Logger.AddMessage($"Save data to database ...");
            DbUtils.SaveToDbTable(log, "dbQ2023..FileLogMinutePolygon", "Folder", "FileName", "Symbol", "Date",
                "MinTime", "MaxTime", "Count", "CountFull", "Open", "High", "Low", "Close", "Volume", "VolumeFull",
                "TradeCount", "Status", "Position", "Created");

            DbUtils.SaveToDbTable(blankFiles, "dbQ2023..FileLogMinutePolygon_BlankFiles", "Folder", "FileName",
                "FileCreated", "Symbol");

            log.Clear();
            blankFiles.Clear();
        }

        #region ========  SubClasses  =========
        private class BlankFile
        {
            public string Folder;
            public string FileName;
            public DateTime FileCreated;
            public string Symbol;
        }

        private class LogEntry
        {
            public string Folder;
            public string FileName;
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
            public string Position;
            public string Status;
            public DateTime Created;
        }
        #endregion

    }
}
