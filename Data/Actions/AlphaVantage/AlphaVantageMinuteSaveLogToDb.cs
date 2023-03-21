using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.Actions.AlphaVantage
{
    public static class AlphaVantageMinuteSaveLogToDb
    {
        private static readonly TimeSpan _startTrading = new TimeSpan(9, 30, 0);
        private static readonly TimeSpan _endTrading = new TimeSpan(16, 0, 0);

        public static void Start(string folder)
        {
            var folderId = Path.GetFileName(folder) + @"\";

            // delete old log in database
            Logger.AddMessage($"Started. Delete old log in database.");
            DbUtils.ExecuteSql($"DELETE dbQuote2023..FileLogMinuteAlphaVantage WHERE [file] like '{folderId}%'");
            DbUtils.ExecuteSql($"DELETE dbQuote2023..FileLogMinuteAlphaVantage_BlankFiles WHERE [file] like '{folderId}%'");

            var errorLog = new List<string>();
            var log = new ConcurrentBag<LogEntry>();
            var blankFiles = new ConcurrentBag<BlankFile>();
            var cnt = 0;

            var files = Directory.GetFiles(folder, $"*.csv");
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 8 }, Check);

            // Save items to database table
            Logger.AddMessage($"Save data to database ...");
            DbUtils.SaveToDbTable(log, "dbQuote2023..FileLogMinuteAlphaVantage", "File", "Symbol", "Date", "MinTime",
                "MaxTime", "Count", "CountFull", "Open", "High", "Low", "Close", "Volume", "VolumeFull", "Position",
                "Created");

            DbUtils.SaveToDbTable(blankFiles, "dbQuote2023..FileLogMinuteAlphaVantage_BlankFiles", "File",
                "FileCreated", "Symbol");

            // Save errors to file
            var errorFileName = folder + $"\\Logs\\ErrorLog.txt";
            if (!Directory.Exists(Path.GetDirectoryName(errorFileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(errorFileName));
            if (File.Exists(errorFileName))
                File.Delete(errorFileName);

            File.AppendAllText(errorFileName, $"File\tMessage\tContent{Environment.NewLine}");
            File.AppendAllLines(errorFileName, errorLog);

            if (errorLog.Count > 0)
                Logger.AddMessage($"!Finished. Found {errorLog.Count} errors. Error filename: {errorFileName}");
            else
                Logger.AddMessage($"!Finished. No errors.");

            // !!! THE END
            return;

            #region =========  Internal method (Check)  ============
            void Check(string file)
            {
                var fileId = folderId + Path.GetFileName(file);
                var fileCreated = File.GetLastWriteTime(file);

                cnt++;
                if (cnt % 10 == 0)
                    Logger.AddMessage($"Processed {cnt} from {files.Length} files.");

                var context = File.ReadAllLines(file);
                if (context.Length == 0)
                {
                    errorLog.Add($"{fileId}\tEmpty file");
                    return;
                }
                if (context[0] != "time,open,high,low,close,volume" && context[0] != "timestamp,open,high,low,close,volume")
                {
                    if (context.Length > 1 && context[1].Contains("Invalid API call"))
                        errorLog.Add($"{fileId}\tInvalid API call");
                    else if (context.Length > 1 && context[1].Contains("Thank you for using Alpha"))
                        errorLog.Add($"{fileId}\tThank you for using");
                    else
                        errorLog.Add($"{fileId}\tBad header");
                    return;
                }

                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var symbol = char.IsDigit(ss[ss.Length - 1][0]) ? ss[ss.Length - 2] : ss[ss.Length - 1];

                var i = symbol.IndexOf('.');
                if (i != -1)
                {
                    throw new Exception("Need to check");
                    symbol = symbol.Substring(0, i - 1);
                }

                if (context.Length == 1)
                {
                    blankFiles.Add(new BlankFile { File = fileId, FileCreated = File.GetLastWriteTime(file), Symbol = symbol });
                }

                LogEntry logEntry = null;
                var lastDate = DateTime.MinValue;
                for (var k = 1; k < context.Length; k++)
                {
                    var line = context[k];
                    var lines = line.Split(',');
                    if (lines.Length != 6)
                    {
                        errorLog.Add($"{fileId}\tBad {k} line length\t{line}");
                        continue;
                    }

                    var dateAndTime = DateTime.ParseExact(lines[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    var date = dateAndTime.Date;
                    var time = dateAndTime.TimeOfDay;
                    var open = float.Parse(lines[1], CultureInfo.InvariantCulture);
                    var high = float.Parse(lines[2], CultureInfo.InvariantCulture);
                    var low = float.Parse(lines[3], CultureInfo.InvariantCulture);
                    var close = float.Parse(lines[4], CultureInfo.InvariantCulture);
                    var volume = long.Parse(lines[5], CultureInfo.InvariantCulture);

                    if (open > high || open < low || close > high || close < low || low < 0)
                        errorLog.Add($"{fileId}\tBad prices in {k} line\t{line}");
                    if (volume < 0)
                        errorLog.Add($"{fileId}\tBad volume in {k} line\t{line}");
                    if (volume == 0 && high != low)
                        errorLog.Add($"{fileId}\tPrices are not equal when volume=0 in {k} line\t{line}");

                    if (date != lastDate)
                    {
                        var position = (logEntry == null ? "First" : "Middle");
                        logEntry = new LogEntry { File = fileId, Symbol = symbol, Date = date, Position = position, Created = fileCreated };
                        log.Add(logEntry);
                        logEntry.MaxTime = time;

                        lastDate = date;
                    }

                    logEntry.CountFull++;
                    logEntry.VolumeFull += volume;
                    logEntry.MinTime = time;
                    // if (logEntry.MinTime > time) logEntry.MinTime = time;
                    // if (logEntry.MaxTime < time) logEntry.MaxTime = time;

                    if (time > _startTrading && time <= _endTrading)
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
                            logEntry.Open = open;
                            if (high > logEntry.High) logEntry.High = high;
                            if (low < logEntry.Low) logEntry.Low = low;
                        }
                    }
                }

                if (logEntry != null)
                    logEntry.Position = "Last";
            }
            #endregion
        }

        #region ========  SubClasses  =========
        public class BlankFile
        {
            public string File;
            public DateTime FileCreated;
            public string Symbol;
        }

        public class LogEntry
        {
            public string File;
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
            public string Position;
            public DateTime Created;

            public override string ToString() => $"{File}\t{Symbol}\t{Date:yyyy-MM-dd}\t{Helpers.CsUtils.GetString(MinTime)}\t{Helpers.CsUtils.GetString(MaxTime)}\t{Count}\t{Open}\t{High}\t{Low}\t{Close}\t{Volume}";
        }
        #endregion
    }
}
