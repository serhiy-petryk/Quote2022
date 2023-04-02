using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Actions.AlphaVantage;
using Data.Helpers;
using Newtonsoft.Json;

namespace Data.Actions.Polygon
{
    public static class PolygonSplitData
    {
        private const string DestinationDataFolder = @"E:\Quote\WebData\Minute\Polygon\Data\";

        public static void Start(string zipFileName)
        {
            var zipFileId = Path.GetFileNameWithoutExtension(zipFileName) + @"\";
            var errorLog = new List<string>();
            var cnt = 0;
            var logFileName = Path.ChangeExtension(zipFileName, ".SplitLog.txt");
            var statusCounts = new int[3];
            var folderId = Path.GetFileNameWithoutExtension(zipFileName);
            var items = new Dictionary<DateTime, List<FileItem>>();

            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries.Where(a => a.Length > 0).OrderBy(a => a.Name.Split('_')[2]))
                {
                    cnt++;
                    if (cnt % 10 == 0)
                        Logger.AddMessage(
                            $"Processed {cnt} from {zip.Entries.Count} entries in {Path.GetFileName(zipFileName)}");

                    var fileId = $@"{folderId}\{Path.GetFileName(entry.Name)}";
                    var oo = JsonConvert.DeserializeObject<PolygonCommon.cMinuteRoot>(entry.GetContentOfZipEntry());
                    if (oo.adjusted || !(oo.status == "OK" || oo.status == "DELAYED"))
                        throw new Exception("Check parser");

                    if (!string.IsNullOrEmpty(oo.next_url))
                    {
                        Debug.Print($"NEXT URL:\t{entry.FullName}\t{oo.next_url}");
                        errorLog.Add($"{fileId}\tPartial downloading\tNext url: {oo.next_url}");
                    }

                    if (oo.count == 0 && (oo.results == null || oo.results.Length == 0))
                        continue;

                    var symbol = PolygonCommon.GetMyTicker(oo.Symbol);
                    var lastDate = DateTime.MinValue;
                    var linesToSave = new List<string>();
                    for (var k = 0; k < oo.results.Length; k++)
                    {
                        var item = oo.results[k];
                        if (item.Open > item.High || item.Open < item.Low || item.Close > item.High ||
                            item.Close < item.Low || item.Low < 0)
                            errorLog.Add($"{fileId}\tBad prices in {k} item\tItem date&time: {item.DateTime:yyyy-MM-dd HH:mm}");
                        if (item.Volume < 0)
                            errorLog.Add($"{fileId}\tBad volume in {k} item\tItem date&time: {item.DateTime:yyyy-MM-dd HH:mm}");
                        if (item.Volume == 0 && item.High != item.Low)
                            errorLog.Add($"{fileId}\tPrices are not equal when volume=0 in {k} line\tItem date&time: {item.DateTime:yyyy-MM-dd HH:mm}");

                        if (item.DateTime.Date != lastDate)
                        {
                            FileItem.CreateItem(items, symbol, lastDate, linesToSave, entry.LastWriteTime.DateTime);

                            linesToSave = new List<string>
                            {
                                $"# DateTime,Open,High,Low,Close,Volume,WeightedVolume,TradeCount. File {entry.Name}. Created at {entry.LastWriteTime:yyyy-MM-dd HH:mm:ss}"
                            };
                            lastDate = item.DateTime.Date;
                        }

                        var line =
                            $"{item.DateTime:yyyy-MM-dd HH:mm},{FloatToString(item.Open)},{FloatToString(item.High)},{FloatToString(item.Low)},{FloatToString(item.Close)},{item.Volume},{FloatToString(item.WeightedVolume)},{item.TradeCount}";
                        linesToSave.Add(line);
                    }

                    if (linesToSave.Count > 0 && string.IsNullOrEmpty(oo.next_url))
                        FileItem.CreateItem(items, symbol, lastDate, linesToSave, entry.LastWriteTime.DateTime);

                    if (cnt % 500 == 0)
                    {
                        ProcessFileItems(items, errorLog, folderId);
                        SaveFileItemLog(items, logFileName, statusCounts, folderId);
                    }
                }
            }

            ProcessFileItems(items, errorLog, folderId);
            SaveFileItemLog(items, logFileName, statusCounts, folderId);

            // Save errors to file
            var errorFileName = Path.ChangeExtension(zipFileName, ".SplitError.txt");
            if (File.Exists(errorFileName))
                File.Delete(errorFileName);

            File.AppendAllLines(errorFileName, new[] { $"File\tMessage\tContent" });
            File.AppendAllLines(errorFileName, errorLog);

            if (errorLog.Count > 0)
                Logger.AddMessage($"!Finished for folder {folderId}. Found {errorLog.Count} errors. New items: {statusCounts[2]:N0}. Old items: {statusCounts[1]:N0}. Items with error: {statusCounts[0]:N0}. Error filename: {errorFileName}");
            else
                Logger.AddMessage($"!Finished for folder {folderId}. New items: {statusCounts[2]:N0}. Old items: {statusCounts[1]:N0}. No errors");
        }

        private static string FloatToString(float f) => f.ToString(CultureInfo.InvariantCulture);

        public enum FileItemStatus { Error, Old, New };
        public class FileItem
        {

            public static void CreateItem(Dictionary<DateTime, List<FileItem>> items, string symbol, DateTime date, List<string> content, DateTime created)
            {
                if (content.Count < 2) return;

                if (!items.ContainsKey(date))
                    items.Add(date, new List<FileItem>());
                items[date].Add(new FileItem { Symbol = symbol, Date = date, ContentLines = content, Created = created });
            }

            public string Symbol;
            public DateTime Date;
            public List<string> ContentLines;
            public DateTime Created;
            public string FileId => $"{Symbol}_{Date:yyyyMMdd}.csv";
            public FileItemStatus Status;
        }













        public static void StartXX(string zipFileName)
        {
            /*var sw =new Stopwatch();
            sw.Start();

            // var folderId = Path.GetFileName(folder) + @"\";
            var log = new List<string>();
            var errorLog = new List<string>();
            var items = new Dictionary<DateTime, List<FileItem>>();
            var cnt = 0;
            var statusCounts = new int[3];

            // Create log file
            var logFileName = Path.ChangeExtension(zipFileName, ".log.txt");
            if (File.Exists(logFileName))
                File.Delete(logFileName);
            File.AppendAllLines(logFileName, new [] {$"Status\tFileId"});

            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0 && a.FullName.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
                {
                }


            var files = Directory.GetFiles(folder, $"*.json");
            foreach (var file in files)
            {
                cnt++;
                if (cnt % 10 == 0)
                    Logger.AddMessage($"Processed {cnt} files from {files.Length}. Folder: {folderId}");

                var fileId = folderId + Path.GetFileName(file);

                var lines = File.ReadAllLines(file);
                if (lines.Length == 0)
                    throw new Exception($"MinuteAlphaVantage_SplitData. Empty file: {fileId}");

                if (lines[0] != "time,open,high,low,close,volume" && lines[0] != "timestamp,open,high,low,close,volume")
                {
                    if (lines.Length > 1 && lines[1].Contains("Invalid API call"))
                        errorLog.Add($"{fileId}\tInvalid API call");
                    else if (lines.Length > 1 && lines[1].Contains("Thank you for using Alpha"))
                        errorLog.Add($"{fileId}\tThank you for using");
                    else
                        errorLog.Add($"{fileId}\tBad header");
                    continue;
                }

                if (lines.Length == 1)
                    continue;

                var fileCreated = File.GetLastWriteTime(file);
                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var symbol = char.IsDigit(ss[ss.Length - 1][0]) ? ss[ss.Length - 2] : ss[ss.Length - 1];
                var i = symbol.IndexOf('.');
                if (i != -1)
                {
                    throw new Exception("Need to check");
                    symbol = symbol.Substring(0, i - 1);
                }

                var lastDate = DateTime.MinValue;
                var linesToSave = new List<string>();
                for (var k = lines.Length - 1; k >= 1; k--)
                {
                    var line = lines[k];
                    var date = DateTime.ParseExact(line.Substring(0, line.IndexOf(',')),
                        "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Date;
                    if (date != lastDate)
                    {
                        FileItem.CreateItem(items, symbol, lastDate, linesToSave, fileCreated);

                        linesToSave = new List<string>
                        {
                            $"# File {Path.GetFileNameWithoutExtension(file)}. Created at {File.GetLastWriteTime(file):yyyy-MM-dd HH:mm:ss}"
                        };
                        lastDate = date;
                    }

                    linesToSave.Add(line);
                }

                if (linesToSave.Count > 0)
                    FileItem.CreateItem(items, symbol, lastDate, linesToSave, fileCreated);

                if (cnt % 500 == 0)
                {
                    ProcessFileItems(items, errorLog, onlyLog, folderId);
                    SaveFileItemLog(items, logFileName, statusCounts, folderId);
                }
            }

            ProcessFileItems(items, errorLog, onlyLog, folderId);
            SaveFileItemLog(items, logFileName, statusCounts, folderId);

            // Save errors to file
            var errorFileName = folder + $"\\Logs\\ErrorLogSaveToZip.txt";
            if (!Directory.Exists(Path.GetDirectoryName(errorFileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(errorFileName));
            if (File.Exists(errorFileName))
                File.Delete(errorFileName);

            File.AppendAllLines(errorFileName, new[] {$"File\tMessage\tContent"});
            File.AppendAllLines(errorFileName, errorLog);

            sw.Stop();
            Debug.Print($"AlphaVantageMinuteSplitDataAndSaveToZip duration: {sw.ElapsedMilliseconds/1000:N0}");

            if (errorLog.Count > 0)
                Logger.AddMessage($"!Finished for folder {folderId}. Found {errorLog.Count} errors. New items: {statusCounts[2]:N0}. Old items: {statusCounts[1]:N0}. Items with error: {statusCounts[0]:N0}. Error filename: {errorFileName}");
            else
                Logger.AddMessage($"!Finished for folder {folderId}. New items: {statusCounts[2]:N0}. Old items: {statusCounts[1]:N0}. No errors");*/
        }

        private static void SaveFileItemLog(Dictionary<DateTime, List<FileItem>> items, string logFileName, int[] statusCount, string folderId)
        {
            var allItems = items.SelectMany(a => a.Value).ToArray();
            items.Clear();

            Logger.AddMessage($"Save log. Folder: {folderId}. Item count: {allItems.Length}");

            statusCount[0] += allItems.Count(a => a.Status == FileItemStatus.Error);
            statusCount[1] += allItems.Count(a => a.Status == FileItemStatus.Old);
            statusCount[2] += allItems.Count(a => a.Status == FileItemStatus.New);

            var fileLines = allItems.Select(a => $"{a.Status}\t{a.FileId}");
            File.AppendAllLines(logFileName, fileLines);
        }

        private static void ProcessFileItems(Dictionary<DateTime, List<FileItem>> items, List<string> errorLog, string folderId)
        {
            Logger.AddMessage($"Process file items for {items.Count:N0} dates. Folder: {folderId}");

            foreach (var kvp in items)
            {
                var zipFileName = $"{DestinationDataFolder}MinutePolygon_{kvp.Key:yyyyMMdd}.zip";
                if (File.Exists(zipFileName))
                {
                    using (var zipArchive = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                        foreach (var item in kvp.Value)
                        {
                            var entry = zipArchive.Entries.FirstOrDefault(a =>
                                string.Equals(a.Name, item.FileId, StringComparison.InvariantCultureIgnoreCase));
                            if (entry != null)
                            {
                                var oldLines = entry.GetLinesOfZipEntry().ToArray();
                                if (oldLines.Length != item.ContentLines.Count)
                                {
                                    errorLog.Add(
                                        $"{item.FileId}\tDifferent content line numbers\t{oldLines.Length} lines in zip file, {item.ContentLines.Count} lines in new file");
                                    item.Status = FileItemStatus.Error;
                                    continue;
                                }

                                for (var k = 1; k < oldLines.Length; k++)
                                {
                                    if (oldLines[k] != item.ContentLines[k])
                                    {
                                        item.Status = FileItemStatus.Error;
                                        errorLog.Add(
                                            $"{item.FileId}\tDifferent content in {k} line\tContent line in zip:{oldLines[k]}");
                                        continue;
                                    }
                                }

                                item.Status = FileItemStatus.Old;
                            }
                            else // New item
                                item.Status = FileItemStatus.New;
                        }

                }
                else
                {
                    // New item
                    foreach (var item in kvp.Value)
                        item.Status = FileItemStatus.New;
                }
            }

            // Create new items
            foreach (var kvp in items.Where(a => a.Value.Any(a1 => a1.Status == FileItemStatus.New)))
            {
                var zipFileName = $"{DestinationDataFolder}MinutePolygon_{kvp.Key:yyyyMMdd}.zip";
                using (var zipArchive = System.IO.Compression.ZipFile.Open(zipFileName, ZipArchiveMode.Update))
                {
                    foreach (var fileItem in kvp.Value.Where(a => a.Status == FileItemStatus.New))
                    {
                        var entryName = $"MP_{kvp.Key:yyyyMMdd}/{fileItem.FileId}";
                        var oldEntries = zipArchive.Entries.Where(a => string.Equals(a.FullName, entryName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                        foreach (var o in oldEntries)
                            o.Delete();

                        var readmeEntry = zipArchive.CreateEntry(entryName);
                        using (var writer = new StreamWriter(readmeEntry.Open()))
                            foreach (var line in fileItem.ContentLines)
                                writer.WriteLine(line);
                    }
                }
            }
        }

    }
}
