using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quote2022.Actions.MinuteAlphaVantage
{
    public static class MAV_SaveLogToDb
    {
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

        private static bool _isBusy = false;
        private static Action<string> _showStatusAction;
        private static TimeSpan _startTrading = new TimeSpan(9, 30, 0);
        private static TimeSpan _endTrading = new TimeSpan(16, 0, 0);

        public static void Start(string folder, Action<string> showStatusAction)
        {
            if (_isBusy)
            {
                MessageBox.Show("MinuteAlphaVantage_SaveLogToDb is working now .. Can't run it again.");
                return;
            }

            var sw = new Stopwatch();
            sw.Start();

            _isBusy = true;
            _showStatusAction = showStatusAction;

            var folderId = Path.GetFileName(folder) + @"\";
            // delete old log in database
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"DELETE dbQuote2023..FileLogMinuteAlphaVantage WHERE [file] like '{folderId}%'";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"DELETE dbQuote2023..FileLogMinuteAlphaVantage_BlankFiles WHERE [file] like '{folderId}%'";
                cmd.ExecuteNonQuery();
            }

            var errorCount = 0;
            var errorLog = new List<string>();
            var log = new ConcurrentBag<LogEntry>();
            var blankFiles = new ConcurrentBag<BlankFile>();
            var files = Directory.GetFiles(folder, $"*.csv");
            var cnt = 0;
            // Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 4 }, Check);
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 8 }, Check);

            _showStatusAction($"MinuteAlphaVantage_SaveLogToDb. Save data to database ...");
            // Save items to database table
            SaveToDb.SaveToDbTable(log, "dbQuote2023..FileLogMinuteAlphaVantage", "File", "Symbol", "Date", "MinTime", "MaxTime",
                "Count", "CountFull", "Open", "High", "Low", "Close", "Volume", "VolumeFull", "Position", "Created");

            SaveToDb.SaveToDbTable(blankFiles, "dbQuote2023..FileLogMinuteAlphaVantage_BlankFiles", "File", "FileCreated", "Symbol");

            // var errorFileName = Directory.GetParent(folder) + $"\\ErrorLog_{Path.GetFileName(folder)}_Y{year}M{month}.txt";
            var errorFileName = folder + $"\\Logs\\ErrorLog.txt";
            if (!Directory.Exists(Path.GetDirectoryName(errorFileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(errorFileName));
            if (File.Exists(errorFileName))
                File.Delete(errorFileName);
            File.AppendAllText(errorFileName, $"File\tMessage\tContent{Environment.NewLine}");
            File.AppendAllLines(errorFileName, errorLog);
            errorCount += errorLog.Count;


            _isBusy = false;
            _showStatusAction($"MinuteAlphaVantage_SaveLogToDb finished. Found {errorCount} errors. Error filename: {folder + $"\\ErrorLog_YxMx.txt"}");

            sw.Stop();
            Debug.Print($"MinuteAlphaVantageCheck: {sw.ElapsedMilliseconds} millisecs");

            // ==============================
            void Check(string file)
            {
                var fileId = folderId + Path.GetFileName(file);
                var fileCreated = File.GetCreationTime(file);

                cnt++;
                if (cnt % 100 == 0)
                    _showStatusAction($"MinuteAlphaVantage_SaveLogToDb is working. Processed {cnt} from {files.Length} files.");

                var context = File.ReadAllLines(file);
                if (context.Length == 0)
                {
                    errorLog.Add($"{fileId}\tEmpty file");
                    return;
                }
                if (context[0] != "time,open,high,low,close,volume" && context[0] != "timestamp,open,high,low,close,volume")
                {
                    if (context.Length > 1 && context[1].Contains("Invalid API call"))
                        return;
                    else if (context.Length > 1 && context[1].Contains("Thank you for using Alpha"))
                        errorLog.Add($"{fileId}\tThank you for using");
                    else
                        errorLog.Add($"{fileId}\tBad header");
                    return;
                }

                var symbol = Path.GetFileNameWithoutExtension(file).Split('_')[0];
                var i = symbol.IndexOf('.');
                if (i != -1)
                {
                    throw new Exception("Need to check");
                    symbol = symbol.Substring(0, i - 1);
                }

                if (context.Length == 1)
                {
                    blankFiles.Add(new BlankFile { File = fileId, FileCreated = File.GetCreationTime(file), Symbol = symbol });
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
            //====================================
        }

        public static void StartForYearMonth(string folder, Action<string> showStatusAction)
        {
            if (_isBusy)
            {
                MessageBox.Show("MinuteAlphaVantage_SaveLogToDb is working now .. Can't run it again.");
                return;
            }

            var sw = new Stopwatch();
            sw.Start();

            _isBusy = true;
            _showStatusAction = showStatusAction;

            var folderId = Path.GetFileName(folder) + @"\";
            // delete old log in database
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"DELETE dbQuote2023..FileLogMinuteAlphaVantage WHERE [file] like '{folderId}%'";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"DELETE dbQuote2023..FileLogMinuteAlphaVantage_BlankFiles WHERE [file] like '{folderId}%'";
                cmd.ExecuteNonQuery();
            }

            var errorCount = 0;
            for (var year = 2; year >= 1; year--)
                for (var month = 12; month >= 1; month--)
                {
                    var errorLog = new List<string>();
                    var log = new ConcurrentBag<LogEntry>();
                    var blankFiles = new ConcurrentBag<BlankFile>();
                    var files = Directory.GetFiles(folder, $"*Y{year}M{month}_*.csv");
                    var cnt = 0;
                    // Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 4 }, Check);
                    Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 8 }, Check);

                    _showStatusAction($"MinuteAlphaVantage_SaveLogToDb. Save data to database ...");
                    // Save items to database table
                    SaveToDb.SaveToDbTable(log, "dbQuote2023..FileLogMinuteAlphaVantage", "File", "Symbol", "Date", "MinTime", "MaxTime",
                        "Count", "CountFull", "Open", "High", "Low", "Close", "Volume", "VolumeFull", "Position", "Created");

                    SaveToDb.SaveToDbTable(blankFiles, "dbQuote2023..FileLogMinuteAlphaVantage_BlankFiles", "File", "FileCreated", "Symbol");

                    // var errorFileName = Directory.GetParent(folder) + $"\\ErrorLog_{Path.GetFileName(folder)}_Y{year}M{month}.txt";
                    var errorFileName = folder + $"\\Logs\\ErrorLog_Y{year}M{month}.txt";
                    if (!Directory.Exists(Path.GetDirectoryName(errorFileName)))
                        Directory.CreateDirectory(Path.GetDirectoryName(errorFileName));
                    if (File.Exists(errorFileName))
                        File.Delete(errorFileName);
                    File.AppendAllText(errorFileName, $"File\tMessage\tContent{Environment.NewLine}");
                    File.AppendAllLines(errorFileName, errorLog);
                    errorCount += errorLog.Count;

                    // ==============================
                    void Check(string file)
                    {
                        var fileId = folderId + Path.GetFileName(file);
                        var fileCreated = File.GetCreationTime(file);

                        cnt++;
                        if (cnt % 100 == 0)
                            _showStatusAction($"MinuteAlphaVantage_SaveLogToDb is working. Processed {cnt} from {files.Length} files. Y{year}M{month}");

                        var context = File.ReadAllLines(file);
                        if (context.Length == 0)
                        {
                            errorLog.Add($"{fileId}\tEmpty file");
                            return;
                        }
                        if (context[0] != "time,open,high,low,close,volume" && context[0] != "timestamp,open,high,low,close,volume")
                        {
                            if (context.Length > 1 && context[1].Contains("Invalid API call"))
                                return;
                            else if (context.Length > 1 && context[1].Contains("Thank you for using Alpha"))
                                errorLog.Add($"{fileId}\tThank you for using");
                            else
                                errorLog.Add($"{fileId}\tBad header");
                            return;
                        }

                        var symbol = Path.GetFileNameWithoutExtension(file).Split('_')[1];
                        var i = symbol.IndexOf('.');
                        if (i != -1)
                        {
                            throw new Exception("Need to check");
                            symbol = symbol.Substring(0, i - 1);
                        }

                        if (context.Length == 1)
                        {
                            blankFiles.Add(new BlankFile { File = fileId, FileCreated = File.GetCreationTime(file), Symbol = symbol });
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
                    //====================================

                }

            _isBusy = false;
            _showStatusAction($"MinuteAlphaVantage_SaveLogToDb finished. Found {errorCount} errors. Error filename: {folder + $"\\ErrorLog_YxMx.txt"}");

            sw.Stop();
            Debug.Print($"MinuteAlphaVantageCheck: {sw.ElapsedMilliseconds} millisecs");

        }
    }
}
