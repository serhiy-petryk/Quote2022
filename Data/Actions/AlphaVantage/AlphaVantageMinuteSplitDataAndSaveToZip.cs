using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.Actions.AlphaVantage
{
    public static class AlphaVantageMinuteSplitDataAndSaveToZip
    {
        private const string DestinationDataFolder = @"E:\Quote\WebData\Minute\AlphaVantage\Data\";

        public static void Start(string folder)
        {
            var folderId = Path.GetFileName(folder) + @"\";
            var errorLog = new List<string>();
            var cnt = 0;

            var files = Directory.GetFiles(folder, "*.csv");
            foreach (var file in files)
            {
                cnt++;
                if (cnt % 10 == 0)
                    Logger.AddMessage($"Processed {cnt} files from {files.Length}");

                var fileId = folderId + Path.GetFileName(file);
                // var fileShortName = Path.GetFileNameWithoutExtension(file);
                var content = File.ReadAllText(file);
                var lines = content.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
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
                        SaveQuotesToFile(symbol, lastDate, linesToSave, fileCreated);
                        linesToSave.Clear();
                        linesToSave.Add(
                            $"# File {Path.GetFileNameWithoutExtension(file)}. Created at {File.GetLastWriteTime(file):yyyy-MM-dd HH:mm:ss}");
                        lastDate = date;
                    }

                    linesToSave.Add(line);
                }

                if (linesToSave.Count > 0)
                    SaveQuotesToFile(symbol, lastDate, linesToSave, fileCreated);
            }

            // Save errors to file
            var errorFileName = folder + $"\\Logs\\ErrorLogZip.txt";
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
        }

        private static void SaveQuotesToFile(string symbol, DateTime date, List<string> content, DateTime fileCreationTime)
        {
            if (content.Count < 2) return;

            var fileId = $"{symbol}_{date:yyyyMMdd}.csv";
            var fileId2 = $"{symbol}_{date:yyyyMMdd}";
            var zipFileName = $"{DestinationDataFolder}MAV_{date:yyyyMMdd}.zip";
            if (File.Exists(zipFileName))
            {
                /*using (var zip = new ZipReader(zipFileName))
                {
                    var entry = zip.FirstOrDefault(a => string.Equals(a.FileNameWithoutExtension, fileId2, StringComparison.InvariantCultureIgnoreCase));
                    if (entry!=null)
                    {
                        var oldLines = entry.AllLines.ToArray();
                        if (oldLines.Length != content.Count)
                            throw new Exception($"Different content. File: {zipFileName}. Id of new file: {content[0]}");
                        for (var k = 1; k < oldLines.Length; k++)
                        {
                            if (oldLines[k] != content[k])
                                throw new Exception($"Different content in {k} line of files. File: {zipFileName}. Id of new file: {content[0]}.{Environment.NewLine}{oldLines[k]}{Environment.NewLine}{content[k]}");
                        }
                        Debug.Print($"Exist: {date:yyyyMMdd}, {symbol}");
                        return;
                    }
                }*/
                using (var zipArchive = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                {
                    var entry = zipArchive.Entries.FirstOrDefault(a => string.Equals(a.Name, fileId, StringComparison.InvariantCultureIgnoreCase));
                    if (entry != null)
                    {
                        var oldLines = entry.GetLinesOfZipEntry().ToArray();
                        if (oldLines.Length != content.Count)
                            throw new Exception($"Different content. File: {zipFileName}. Id of new file: {content[0]}");
                        for (var k = 1; k < oldLines.Length; k++)
                        {
                            if (oldLines[k] != content[k])
                                throw new Exception($"Different content in {k} line of files. File: {zipFileName}. Id of new file: {content[0]}.{Environment.NewLine}{oldLines[k]}{Environment.NewLine}{content[k]}");
                        }
                        Debug.Print($"Exist: {date:yyyyMMdd}, {symbol}");
                        return;
                    }
                }

            }

            Debug.Print($"New: {date:yyyyMMdd}, {symbol}");
        }
    }
}
