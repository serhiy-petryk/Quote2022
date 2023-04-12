using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Newtonsoft.Json;

namespace Data.Actions.Polygon
{
    public static class PolygonMinuteSplitData
    {
        private const string DestinationDataFolder = @"E:\Quote\WebData\Minute\Polygon\Data\";

        public static void Start(string folder)
        {
            var skipIfExists = true;

            var folderName = Path.GetFileName(folder);

            Logger.AddMessage($"Get file list");
            var files = Directory.GetFiles(folder, "*.json").OrderBy(a=>a.Split('_')[3]).ToArray();

            var log = new Log($"{folder}.SplitLog.txt");
            var errorLog = new Log($"{folder}.SplitErrors.txt");
            errorLog.Add($"File\tMessage\tContent");

            var statusCounts = new int[3];
            var items = new Dictionary<DateTime, List<FileItem>>();
            var cnt = 0;
            foreach (var file in files)
            {
                cnt++;
                if (cnt % 10 == 0)
                    Logger.AddMessage($"Processed {cnt} from {files.Length} files");

                var filename = Path.GetFileName(file);
                var oo = JsonConvert.DeserializeObject<PolygonCommon.cMinuteRoot>(File.ReadAllText(file));

                if (oo.adjusted || !(oo.status == "OK" || oo.status == "DELAYED"))
                    throw new Exception("Check parser");

                if (!string.IsNullOrEmpty(oo.next_url))
                {
                    Debug.Print($"NEXT URL:\t{filename}\t{oo.next_url}");
                    errorLog.Add($"{filename}\tPartial downloading\tNext url: {oo.next_url}");
                    continue;
                }

                if (oo.count == 0 && (oo.results == null || oo.results.Length == 0))
                    continue;

                // if (PolygonCommon.IsTestTicker(oo.Symbol)) continue;

                var lastDate = DateTime.MinValue;
                var linesToSave = new List<string>();
                for (var k = 0; k < oo.results.Length; k++)
                {
                    var item = oo.results[k];
                    if (item.Open > item.High || item.Open < item.Low || item.Close > item.High ||
                        item.Close < item.Low || item.Low < 0)
                        errorLog.Add($"{filename}\tBad prices in {k} item\tItem date&time: {item.DateTime:yyyy-MM-dd HH:mm}");
                    if (item.Volume < 0)
                        errorLog.Add($"{filename}\tBad volume in {k} item\tItem date&time: {item.DateTime:yyyy-MM-dd HH:mm}");
                    if (item.Volume == 0 && item.High != item.Low)
                        errorLog.Add($"{filename}\tPrices are not equal when volume=0 in {k} line\tItem date&time: {item.DateTime:yyyy-MM-dd HH:mm}");

                    if (item.DateTime.Date != lastDate)
                    {
                        FileItem.CreateItem(items, oo.Symbol, lastDate, linesToSave, File.GetLastWriteTime(file));

                        linesToSave = new List<string>
                        {
                            $"# DateTime,Open,High,Low,Close,Volume,WeightedVolume,TradeCount. File {filename}. Created at {File.GetLastWriteTime(file):yyyy-MM-dd HH:mm:ss}"
                        };

                        lastDate = item.DateTime.Date;
                        if (lastDate > File.GetLastWriteTime(file).AddHours(-9).AddDays(-1))
                            break; // Fresh quotes => all day data is not ready
                    }

                    var line =
                        $"{item.DateTime:yyyy-MM-dd HH:mm},{FloatToString(item.Open)},{FloatToString(item.High)},{FloatToString(item.Low)},{FloatToString(item.Close)},{item.Volume},{FloatToString(item.WeightedVolume)},{item.TradeCount}";
                    linesToSave.Add(line);
                }

                if (linesToSave.Count > 0 && string.IsNullOrEmpty(oo.next_url))
                    FileItem.CreateItem(items, oo.Symbol, lastDate, linesToSave, File.GetLastWriteTime(file));

                if (cnt % 200 == 0)
                {
                    ProcessFileItems(items, log, statusCounts, skipIfExists);
                }
            }

            ProcessFileItems(items, log, statusCounts, skipIfExists);

            var errorCount = errorLog.Count - 1;
            errorLog.SaveAndClear();

            if (errorCount > 0)
                Logger.AddMessage($"!Finished for folder {folderName}. Found {errorCount} errors. New items: {statusCounts[0]:N0}. Overwritten items: {statusCounts[1]:N0}. Skipped items: {statusCounts[2]:N0}. Error filename: {errorLog.FileName}");
            else
                Logger.AddMessage($"!Finished for folder {folderName}. No errors. New items: {statusCounts[0]:N0}. Overwritten items: {statusCounts[1]:N0}. Skipped items: {statusCounts[2]:N0}.");
        }

        private static string FloatToString(float f) => f.ToString(CultureInfo.InvariantCulture);

        private static void ProcessFileItems(Dictionary<DateTime, List<FileItem>> items, Log log,  int[] statusCounts, bool skipIfExists)
        {
            var totalItemsCount = items.SelectMany(a => a.Value).Count();
            var cnt = 0;
            foreach (var kvp in items)
            {
                var zipFileName = $"{DestinationDataFolder}MinutePolygon_{kvp.Key:yyyyMMdd}.zip";
                using (var zipArchive = System.IO.Compression.ZipFile.Open(zipFileName, ZipArchiveMode.Update))
                    foreach (var fileItem in kvp.Value)
                    {
                        cnt++;
                        if (cnt % 1000 == 0)
                            Logger.AddMessage($"Process {cnt} file items from {totalItemsCount} of data chunk ");

                        var entryName = $"MP_{kvp.Key:yyyyMMdd}/{fileItem.FileId}";
                        var oldEntries = zipArchive.Entries.Where(a =>
                                string.Equals(a.FullName, entryName, StringComparison.InvariantCultureIgnoreCase))
                            .ToArray();
                        if (oldEntries.Length != 0 && skipIfExists)
                        {
                            log.Add($"{fileItem.FileId}\tSkipped");
                            statusCounts[2]++;
                            continue;
                        }

                        foreach (var o in oldEntries)
                        {
                            log.Add($"{fileItem.FileId}\tOverwritten");
                            statusCounts[1]++;
                            o.Delete();
                        }

                        if (oldEntries.Length == 0)
                        {
                            log.Add($"{fileItem.FileId}\tAdded");
                            statusCounts[0]++;
                        }

                        var readmeEntry = zipArchive.CreateEntry(entryName);
                        using (var writer = new StreamWriter(readmeEntry.Open()))
                            foreach (var line in fileItem.ContentLines)
                                writer.WriteLine(line);
                    }
            }

            items.Clear();
            log.SaveAndClear();

            Debug.Print($"Split MinutePolygon. Memory usage: {CsUtils.MemoryUsedInBytes / 1024:N0}KB");
        }

        #region ==========  SubClasses  ===========
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
        }

        public class Log : List<string>
        {
            public readonly string FileName;

            public Log(string filename)
            {
                FileName = filename;
                if (File.Exists(filename))
                    File.Delete(filename);
            }

            public void SaveAndClear()
            {
                File.AppendAllLines(FileName, this);
                Clear();
            }
        }
        #endregion
    }
}
