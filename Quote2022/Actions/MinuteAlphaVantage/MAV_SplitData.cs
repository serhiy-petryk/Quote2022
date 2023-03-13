using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Quote2022.Actions.MinuteAlphaVantage
{
    public class MAV_SplitData
    {
        private static bool _isBusy = false;

        public static void Start(string folder, Action<string> showStatusAction)
        {
            if (_isBusy)
            {
                MessageBox.Show("MinuteAlphaVantage_SplitData is working now .. Can't run it again.");
                return;
            }

            _isBusy = true;

            var sw = new Stopwatch();
            sw.Start();

            var destinationFolder = Directory.GetParent(folder) + @"\" + @"_DataByDate\";
            var files = Directory.GetFiles(folder, "*.csv");
            var errorLog = new List<string>();
            var cnt = 0;
            foreach (var file in files)
            {
                cnt++;
                if (cnt % 10 == 0)
                    showStatusAction($"MinuteAlphaVantage_SplitData is working. Processed {cnt} files from {files.Length}");

                var fileShortName = Path.GetFileNameWithoutExtension(file);
                var content = File.ReadAllText(file);
                var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                    throw new Exception($"MinuteAlphaVantage_SplitData. Empty file: {fileShortName}");

                if (lines[0] != "time,open,high,low,close,volume" && lines[0] != "timestamp,open,high,low,close,volume")
                {
                    if (lines.Length > 1 && lines[1].Contains("Invalid API call"))
                    {
                        errorLog.Add($"{fileShortName}\tInvalid API call");
                    }
                    else if (lines.Length > 1 && lines[1].Contains("Thank you for using Alpha"))
                        throw new Exception(
                            $"MinuteAlphaVantage_SplitData. 'Thank you for using' error in file: {fileShortName}");
                    else
                        throw new Exception(
                            $"MinuteAlphaVantage_SplitData. Bad header in file: {fileShortName}. Header: {lines[0]}");

                    continue;
                }

                if (lines.Length == 1)
                    continue;

                var fileCreated = File.GetCreationTime(file);
                var symbol = fileShortName.Split('_')[0];
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
                        linesToSave.Add($"# File {fileShortName}. Created at {File.GetCreationTime(file):yyyy-MM-dd HH:mm:ss}");
                        lastDate = date;
                    }
                    linesToSave.Add(line);
                }

                if (linesToSave.Count > 0)
                    SaveQuotesToFile(symbol, lastDate, linesToSave, fileCreated);
            }

            sw.Stop();
            Debug.Print($"MinuteAlphaVantage_SplitData. {cnt} items. {sw.ElapsedMilliseconds} millisecs");

            _isBusy = false;
            showStatusAction($"MinuteAlphaVantage_SplitData finished. Found {errorLog.Count} errors. See Debug.Output window. Folder: {Path.GetFileName(folder)}");

            Debug.Print($"******  MinuteAlphaVantage_SplitData errors  *******");
            foreach (var line in errorLog)
                Debug.Print(line);

            //==============================
            void SaveQuotesToFile(string symbol, DateTime date, List<string> content, DateTime fileCreationTime)
            {
                if (content.Count < 2) return;
                var dataFolder = destinationFolder + "MAV_" + date.ToString("yyyyMMdd") + @"\";
                if (!Directory.Exists(dataFolder))
                    Directory.CreateDirectory(dataFolder);

                var fn = dataFolder + symbol + "_" + date.ToString("yyyyMMdd") + ".csv";
                if (File.Exists(fn))
                {
                    // Check content of old and new files
                    var oldLines = File.ReadAllLines(fn);
                    if (oldLines.Length != content.Count)
                        throw new Exception($"Different content. File: {fn}. Id of new file: {content[0]}");
                    for (var k = 1; k < oldLines.Length; k++)
                    {
                        if (oldLines[k] != content[k])
                            throw new Exception($"Different content in {k} line of files. File: {fn}. Id of new file: {content[0]}.{Environment.NewLine}{oldLines[k]}{Environment.NewLine}{content[k]}");
                    }
                }
                else
                {
                    File.WriteAllLines(fn, content);
                    File.SetCreationTime(fn, fileCreationTime);
                    File.SetLastWriteTime(fn, fileCreationTime);
                }
            }
        }
    }
}
