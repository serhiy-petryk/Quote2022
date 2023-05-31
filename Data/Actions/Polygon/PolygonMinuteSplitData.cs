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

        public static void Start(string zipFileName)
        {
            var skipIfExists = true;

            var folderId = Path.GetFileNameWithoutExtension(zipFileName);
            var log = new Log($"{folderId}.SplitLog.txt");
            var errorLog = new Log($"{folderId}.SplitErrors.txt");
            errorLog.Add($"File\tMessage\tContent");

            var statusCounts = new int[3];
            var items = new Dictionary<DateTime, List<FileItem>>();
            var cnt = 0;

            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0).OrderBy(a => a.Name.Split('_')[2]).ToArray())
                {
                    cnt++;
                    if (cnt % 10 == 0)
                        Logger.AddMessage($"Processed {cnt} from {zip.Entries.Count} files");

                    var oo = JsonConvert.DeserializeObject<PolygonCommon.cMinuteRoot>(entry.GetContentOfZipEntry());

                    if (oo.adjusted || !(oo.status == "OK" || oo.status == "DELAYED"))
                        throw new Exception("Check parser");

                    if (!string.IsNullOrEmpty(oo.next_url))
                    {
                        Debug.Print($"NEXT URL:\t{entry.Name}\t{oo.next_url}");
                        errorLog.Add($"{entry.Name}\tPartial downloading\tNext url: {oo.next_url}");
                        continue;
                    }

                    if (oo.count == 0 && (oo.results == null || oo.results.Length == 0))
                        continue;

                    var lastDate = DateTime.MinValue;
                    var linesToSave = new List<string>();
                    for (var k = 0; k < oo.results.Length; k++)
                    {
                        var item = oo.results[k];
                        if (item.Open > item.High || item.Open < item.Low || item.Close > item.High ||
                            item.Close < item.Low || item.Low < 0)
                            errorLog.Add($"{entry.Name}\tBad prices in {k} item\tItem date&time: {item.DateTime:yyyy-MM-dd HH:mm}");
                        if (item.Volume < 0)
                            errorLog.Add($"{entry.Name}\tBad volume in {k} item\tItem date&time: {item.DateTime:yyyy-MM-dd HH:mm}");
                        if (item.Volume == 0 && item.High != item.Low)
                            errorLog.Add($"{entry.Name}\tPrices are not equal when volume=0 in {k} line\tItem date&time: {item.DateTime:yyyy-MM-dd HH:mm}");

                        if (item.DateTime.Date != lastDate)
                        {
                            FileItem.CreateItem(items, oo.Symbol, lastDate, linesToSave, entry.LastWriteTime.DateTime);

                            linesToSave = new List<string>
                            {
                                $"# DateTime,Open,High,Low,Close,Volume,WeightedVolume,TradeCount. File {entry.Name}. Created at {entry.LastWriteTime.DateTime:yyyy-MM-dd HH:mm:ss}"
                            };

                            lastDate = item.DateTime.Date;
                            if (lastDate > entry.LastWriteTime.DateTime.AddHours(-9).AddDays(-1))
                                break; // Fresh quotes => all day data is not ready
                        }

                        var line =
                            $"{item.DateTime:yyyy-MM-dd HH:mm},{FloatToString(item.Open)},{FloatToString(item.High)},{FloatToString(item.Low)},{FloatToString(item.Close)},{item.Volume},{FloatToString(item.WeightedVolume)},{item.TradeCount}";
                        linesToSave.Add(line);
                    }

                    if (linesToSave.Count > 0 && string.IsNullOrEmpty(oo.next_url))
                        FileItem.CreateItem(items, oo.Symbol, lastDate, linesToSave, entry.LastWriteTime.DateTime);

                    if (cnt % 200 == 0)
                        ProcessFileItems(items, log, statusCounts, skipIfExists);

                }

            ProcessFileItems(items, log, statusCounts, skipIfExists);

            var errorCount = errorLog.Count - 1;
            errorLog.SaveAndClear();

            if (errorCount > 0)
                Logger.AddMessage($"!Finished for folder {folderId}. Found {errorCount} errors. New items: {statusCounts[0]:N0}. Overwritten items: {statusCounts[1]:N0}. Skipped items: {statusCounts[2]:N0}. Error filename: {errorLog.FileName}");
            else
                Logger.AddMessage($"!Finished for folder {folderId}. No errors. New items: {statusCounts[0]:N0}. Overwritten items: {statusCounts[1]:N0}. Skipped items: {statusCounts[2]:N0}.");
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
                        if (cnt % 100 == 0)
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

                        var zipEntry = zipArchive.CreateEntry(entryName);
                        zipEntry.LastWriteTime = fileItem.Created;
                        using (var writer = new StreamWriter(zipEntry.Open()))
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
