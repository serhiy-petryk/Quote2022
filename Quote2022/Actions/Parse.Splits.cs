using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Quote2022.Helpers;
using Quote2022.Models;

namespace Quote2022.Actions
{
    public static partial class Parse
    {
        #region ========  InvestingHistory split parse + save to db  ========
        public static void SplitInvestingHistory_Parse(string file, Dictionary<string, SplitModel> data, Action<string> showStatusAction)
        {
            showStatusAction($"SplitInvestingHistory file parsing started.");

            var ss1 = Path.GetFileNameWithoutExtension(file).Split('_');
            var timeStamp = DateTime.ParseExact(ss1[ss1.Length - 1], "yyyyMMdd", CultureInfo.InvariantCulture);

            var rows = File.ReadAllText(file).Split(new string[] { @"<\/tr" }, StringSplitOptions.RemoveEmptyEntries);
            var lastDate = DateTime.MinValue;

            for (var k = 0; k < rows.Length - 1; k++)
            {
                var row = rows[k];
                var cells = row.Split(new string[] { @"<\/td" }, StringSplitOptions.RemoveEmptyEntries);
                if (cells.Length == 1 && k == rows.Length - 1)
                    break;

                var i1 = cells[0].LastIndexOf('>');
                var sDate = cells[0].Substring(i1 + 1).Trim();
                var date = string.IsNullOrEmpty(sDate) ? lastDate : DateTime.ParseExact(sDate, "MMM dd, yyyy", CultureInfo.InvariantCulture);

                var s = cells[1].Replace(@"<\/a>)", "").Replace(@"\n", "").Replace(@"\t", "").Trim();
                i1 = s.LastIndexOf('>');
                var symbol = s.Substring(i1 + 1).Trim();
                var i2 = s.LastIndexOf(@"<\/span>", StringComparison.InvariantCulture);
                i1 = s.LastIndexOf('>', i2 - 7);
                var name = s.Substring(i1 + 1, i2 - i1 - 1);
                i1 = s.IndexOf(@" title=\", StringComparison.InvariantCulture);
                i2 = s.IndexOf('>', i1 + 8);
                var title = s.Substring(i1 + 8, i2 - i1 - 8).Trim();
                if (title.StartsWith(@"""") && title.EndsWith(@""""))
                    title = title.Substring(1, title.Length - 2).Trim();
                if (title.EndsWith(@"\"))
                    title = title.Substring(0, title.Length - 1).Trim();

                i1 = cells[2].LastIndexOf('>');
                var ratio = cells[2].Substring(i1 + 1).Trim();

                var item = new SplitModel(symbol, date, name, ratio, title, timeStamp);
                if (!data.ContainsKey(item.Key))
                    data.Add(item.Key, item);

                lastDate = date;
            }

            showStatusAction($"SplitInvesting file parsing finished and saved to DB!!! {Path.GetFileName(file)}");
        }
        #endregion

        #region ========  StockSplitHistory parse + save to db  ========
        public static void StockSplitHistory_Parse(string zipFile, Action<string> showStatusAction)
        {
            showStatusAction($"StockSplitHistory parse + save to db STARTED!");

            var timeStamp = DateTime.ParseExact(Path.GetFileNameWithoutExtension(zipFile).Split('_')[1].Trim(), "yyyyMMdd",
                CultureInfo.InvariantCulture);
            var data = new List<SplitModel>();
            using (var zip = new ZipReader(zipFile))
                foreach (var item in zip)
                {
                    if (item.Length > 0 && item.FileNameWithoutExtension.ToUpper().StartsWith("SSH"))
                    {
                        var content = item.Content;
                        var i1 = content.IndexOf(@" Split History</b> Table</TD></TR>", StringComparison.InvariantCulture);
                        if (i1 > 0)
                        {
                            var i2 = content.LastIndexOf(">", i1, StringComparison.InvariantCulture);
                            var symbol = content.Substring(i2 + 1, i1 - i2 - 1);
                            var symbol1 = item.FileNameWithoutExtension.Substring(3);
                            if (!string.Equals(symbol, symbol1))
                                throw new Exception($"Check symbols in StockSplit file: {Path.GetFileName(item.FullName)}. Symbols are {symbol} and {symbol1}");

                            i2 = content.IndexOf(@"</table", i1 + 20, StringComparison.InvariantCulture);
                            var ss = content.Substring(i1, i2 - i1).Split(new string[] { "</tr>", "</TR>" }, StringSplitOptions.RemoveEmptyEntries);
                            if (ss.Length > 2 && ss[1].IndexOf(">Date<", StringComparison.InvariantCulture) > 0 && ss[1].IndexOf(">Ratio<", StringComparison.InvariantCulture) > 0)
                            {
                                for (var k = 2; k < ss.Length; k++)
                                {
                                    var ss1 = ss[k].Split(new string[] { "</TD", "</td" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (ss1.Length == 3)
                                    {
                                        i1 = ss1[0].LastIndexOf(">", StringComparison.InvariantCulture);
                                        var sDate = ss1[0].Substring(i1 + 1).Trim();
                                        var date = DateTime.ParseExact(sDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);

                                        i1 = ss1[1].LastIndexOf(">", StringComparison.InvariantCulture);
                                        var sRatio = ss1[1].Substring(i1 + 1).Trim();
                                        if (!sRatio.Contains(" for "))
                                        {
                                            // Debug.Print($"Bad ratio ({sRatio}, {symbol}, {date:yyyy-MM-dd})");
                                            if (string.Equals(symbol, "GECC") && DateTime.Equals(date, new DateTime(1998, 9, 16)))
                                                continue;
                                            throw new Exception($"Bad ratio ({sRatio}) in StockSplitHistory file: {Path.GetFileName(item.FullName)}");
                                        }

                                        var ratio = sRatio.Replace(" for ", ":");
                                        data.Add(new SplitModel(symbol, date, ratio, timeStamp));
                                    }
                                    else if (ss1.Length == 1 && string.IsNullOrEmpty(ss1[0].Trim())) { }
                                    else
                                        throw new Exception($"Check StockSplit file structure: {Path.GetFileName(item.FullName)}");
                                }
                            }
                            else
                                throw new Exception($"Check StockSplit file structure: {Path.GetFileName(item.FullName)}");
                        }
                        else
                            throw new Exception($"Check StockSplit file structure: {Path.GetFileName(item.FullName)}");
                    }
                }

            showStatusAction($"StockSplitHistory is saving to database..");
            SaveToDb.ClearAndSaveToDbTable(data.Where(a => a.Date <= a.TimeStamp), "Bfr_StockSplitHistory", "Symbol",
                "Date", "Ratio", "K", "TimeStamp");
            SaveToDb.RunProcedure("pUpdateStockSplitHistory");

            showStatusAction($"StockSplitHistory parse + save to db FINISHED!");
        }
        #endregion

        #region ========  Eoddata split parse + save to db  ========
        public static void SplitEoddata_Parse(string file, Action<string> showStatusAction)
        {
            showStatusAction($"SplitEoddata file parsing started.");
            var splits = new Dictionary<string, SplitModel>();

            var timeStamp = DateTime.ParseExact(Path.GetFileNameWithoutExtension(file).Split('_')[1].Trim(), "yyyyMMdd",
                CultureInfo.InvariantCulture);
            var lines = File.ReadAllLines(file, Encoding.Default); // or Encoding.UTF7
            if (lines.Length == 0 || !Equals(lines[0], "Exchange\tSymbol\tDate\tRatio"))
                throw new Exception($"Invalid Eoddata split file structure! {file}");

            for (var k = 1; k < lines.Length; k++)
            {
                var ss = lines[k].Split('\t');
                if (ss.Length != 4)
                    throw new Exception($"Invalid line of Eoddata split file! Line: {lines[k]}, file: {file}");
                var date = DateTime.Parse(ss[2].Trim(), CultureInfo.InvariantCulture);

                var item = new SplitModel(ss[0].Trim(), ss[1].Trim(), date, ss[3].Trim().Replace('-', ':'), timeStamp);
                if (!splits.ContainsKey(item.EoddataKey) && item.Key != "ADTX20220912")
                    splits.Add(item.EoddataKey, item);
            }

            if (splits.Count > 0)
                SaveToDb.SplitEoddata_SaveToDb(splits.Values);

            showStatusAction($"SplitEoddata file parsing finished and saved to DB!!! {Path.GetFileName(file)}");
        }
        #endregion

        #region ========  Investing.com split parse + save to db  ========
        // Url: https://www.investing.com/stock-split-calendar/ => copy data to Excel => to text file
        public static void SplitInvesting_Parse(string file, Action<string> showStatusAction)
        {
            showStatusAction($"SplitInvesting file parsing started.");
            var timeStamp = DateTime.ParseExact(Path.GetFileNameWithoutExtension(file).Split('_')[1].Trim(), "yyyyMMdd",
                CultureInfo.InvariantCulture);
            var splits = new Dictionary<string, SplitModel>();

            var lines = File.ReadAllLines(file, Encoding.Default); // or Encoding.UTF7
            if (lines.Length == 0 || !Equals(lines[0], "Split date\tCompany\tSplit ratio"))
                throw new Exception($"Invalid Investing.com split file structure! {file}");

            DateTime? lastDate = null;
            for (var k = 1; k < lines.Length; k++)
            {
                var ss = lines[k].Split('\t');
                if (ss.Length != 3)
                    throw new Exception($"Invalid line of Investing.com split file! Line: {lines[k]}, file: {file}");
                if (string.IsNullOrEmpty(ss[0]) && !lastDate.HasValue)
                    throw new Exception($"Error: Empty first date in Investing.com split file! Line: {lines[k]}, file: {file}");
                var date = string.IsNullOrEmpty(ss[0]) ? lastDate.Value : DateTime.ParseExact(ss[0], "MMM dd, yyyy", CultureInfo.InvariantCulture);
                var s = ss[1].Trim();
                if (!s.EndsWith(")"))
                    throw new Exception($"Error: The line must be ended with ')'! Line: {lines[k]}, file: {file}");
                var i = s.LastIndexOf("(", StringComparison.InvariantCultureIgnoreCase);
                var symbol = s.Substring(i + 1, s.Length - i - 2).Trim().ToUpper();
                var name = s.Substring(0, i).Trim();

                if (String.Equals(symbol, "US90274X5529=UBSS") && string.Equals(name, "Whiting Petroleum"))
                    symbol = "WLL";

                var item = new SplitModel(symbol, date, name, ss[2].Trim(), null, timeStamp);
                if (!splits.ContainsKey(item.Key))
                    splits.Add(item.Key, item);
                lastDate = date;
            }

            if (splits.Count > 0)
                SaveToDb.SplitInvesting_SaveToDb(splits.Values);

            showStatusAction($"SplitInvesting file parsing finished and saved to DB!!! {Path.GetFileName(file)}");
        }
        #endregion

        #region ========  Yahoo split parse + save to db  (zip) ========
        public static void SplitYahoo_Parse(string file, Action<string> showStatusAction)
        {
            showStatusAction($"SplitYahoo file parsing started.");
            var timeStamp = DateTime.ParseExact(Path.GetFileNameWithoutExtension(file).Split('_')[1].Trim(), "yyyyMMdd",
                CultureInfo.InvariantCulture);

            var splits = new Dictionary<string, SplitModel>();

            using (var zip = new ZipReader(file))
                foreach (var item in zip)
                {
                    if (item.Length > 0 && item.FileNameWithoutExtension.ToUpper().StartsWith("Y"))
                    {
                        var symbol = item.FileNameWithoutExtension.Substring(1).ToUpper();
                        var symbolSplits = new Dictionary<DateTime, string>();
                        var lines = item.Content.Split('\n');
                        if (lines[0].Equals("Date,Stock Splits"))
                        {
                            for (var k = 1; k < lines.Length; k++)
                            {
                                var ss = lines[k].Split(',');
                                var date = DateTime.ParseExact(ss[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                var ratio = ss[1].Trim();
                                var o = new SplitModel(symbol, date, ratio, timeStamp);
                                if (!splits.ContainsKey(o.Key))
                                    splits.Add(o.Key, o);
                            }
                        }
                        else
                            throw new Exception("");
                    }

                }

            if (splits.Count > 0)
                SaveToDb.SplitYahoo_SaveToDb(splits.Values);

            showStatusAction($"SplitYahoo file parsing finished and saved to DB!!! {Path.GetFileName(file)}");
        }
        #endregion

    }
}
