using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Quote2022.Helpers;
using Quote2022.Models;

namespace Quote2022.Actions
{
    public static partial class Parse
    {
      #region ==================  SymbolsStockanalysis_ParseAndSaveToDb  ==========================
        public static void SymbolsStockanalysis_ParseAndSaveToDb(Action<string> showStatusAction)
        {
            var filename = @"E:\Quote\WebData\Symbols\Stockanalysis\Stockanalysis.StockExchanges_20221216.txt";
            var oo = JsonConvert.DeserializeObject<string[,]>(File.ReadAllText(filename));
            var items = new List<SymbolsStockanalysisExchanges>();
            for (var i = 0; i < oo.GetLength(0); i++)
                items.Add(new SymbolsStockanalysisExchanges(oo[i, 0], oo[i, 1], File.GetCreationTime(filename)));

            SaveToDb.ClearAndSaveToDbTable(items, "Bfr_SymbolsStockanalysisStockExchanges", "Symbol", "Exchange",
                "Created");

            filename = @"E:\Quote\WebData\Symbols\Stockanalysis\Stockanalysis.EtfExchanges_20221216.txt";
            oo = JsonConvert.DeserializeObject<string[,]>(File.ReadAllText(filename));
            items = new List<SymbolsStockanalysisExchanges>();
            for (var i = 0; i < oo.GetLength(0); i++)
                items.Add(new SymbolsStockanalysisExchanges(oo[i, 0], oo[i, 1], File.GetCreationTime(filename)));

            SaveToDb.ClearAndSaveToDbTable(items, "Bfr_SymbolsStockanalysisEtfExchanges", "Symbol", "Exchange",
                "Created");
        }
        #endregion

        #region ==================  ProfileQuantumonline_Parse  ==========================
        public static void ProfileQuantumonline_ParseAndSaveToDb(string zipFile, Action<string> showStatusAction)
        {
            var profiles = new Dictionary<string, List<ProfilesQuantumonline>>();
            using (var zip = new ZipReader(zipFile))
                foreach (var file in zip.Where(a => a.FullName.EndsWith(".html", true, CultureInfo.InvariantCulture)))
                {
                    var item = new ProfilesQuantumonline(file.Content, file.FileNameWithoutExtension, file.Created);
                    if (!item.IsInvalid)
                    {
                        if (!profiles.ContainsKey(item.SymbolKey))
                            profiles.Add(item.SymbolKey, new List<ProfilesQuantumonline>());
                        profiles[item.SymbolKey].Add(item);

                        //if (profiles.Count > 1000)
                        //    break;
                    }
                }

            var data = profiles.Values.Select(a => a[0]).ToArray();
            SaveToDb.ClearAndSaveToDbTable(data, "ProfilesQuantumonline", "SymbolKey",
                "Exchange", "Symbol", "CUSIP", "Name", "PrevCUSIP", "NewSymbol", "NewSymbolChanged", "NewName",
                "PrevSymbol", "PrevSymbolChanged", "PrevName", "PrevNameChanged", "IpoDate", "IpoSize", "IpoPrice",
                "Type", "Subtype", "CapStockType", "MarketCap", "IsDead", "TimeStamp");

        }
        #endregion

        #region ==================  SymbolsQuantumonline_Parse  ==========================
        public static void SymbolsQuantumonlineZip_Parse(Action<string> showStatusAction)
        {
            var items = new Dictionary<string, List<SymbolsQuantumonline>>();
            var file1 = @"E:\Quote\WebData\Symbols\Quantumonline\QuantumonlineListNew.zip";
            SymbolsQuantumonline_GetUrls(file1, items);
            Debug.Print($"Items 1: {items.Count}");

            file1 = @"E:\Quote\WebData\Symbols\Quantumonline\QuantumonlineList.zip";
            SymbolsQuantumonline_GetUrls(file1, items, true);
            Debug.Print($"Items 2: {items.Count}");

            file1 = @"E:\Quote\WebData\Symbols\Quantumonline\QuantumonlineListOld.zip";
            SymbolsQuantumonline_GetUrls(file1, items, true);
            Debug.Print($"Items 3: {items.Count}");

            SaveToDb.ClearAndSaveToDbTable(items.Select(a => a.Value[0]), "SymbolsQuantumonline", "SymbolKey",
                "Exchange", "Symbol", "Name", "Url", "IsDead", "TimeStamp");
        }

        public static void SymbolsQuantumonline_GetUrls(string zipFile, Dictionary<string, List<SymbolsQuantumonline>> urls, bool trace = false)
        {
            var cnt = 0;
            // var urls = new Dictionary<string, string>();
            using (var zip = new ZipReader(zipFile))
            {
                foreach (var file in zip.Where(a => a.FullName.EndsWith(".html", true, CultureInfo.InvariantCulture)))
                {
                    if (file.FileNameWithoutExtension.Replace("%", "[%]").Substring(1).Length > 20)
                    {
                        // var item = SymbolsQuantumonlineContent_Parse(file.Content, file.FileNameWithoutExtension);
                        // Debug.Print(item == null ? "NULL" : item.ToString());
                    }
                    else
                    {
                        var newItems = SymbolsQuantumonlineContent_Parse(file.Content, file.FileNameWithoutExtension, File.GetCreationTime(zipFile));
                        cnt += newItems.Count;
                        foreach (var item in newItems)
                        {
                            if (!urls.ContainsKey(item.Url))
                                urls.Add(item.Url, new List<SymbolsQuantumonline>());
                            urls[item.Url].Add(item);
                        }

                        //if (cnt > 100)
                          // break;
                    }
                }
            }

            Debug.Print($"Item count: {cnt}. File: {Path.GetFileName(zipFile)}");
        }
        #endregion

        #region ========  SymbolsQuantumonline_Parse  ========
        public static List<SymbolsQuantumonline> SymbolsQuantumonlineContent_Parse(string content, string fileName, DateTime timeStamp)
        {
            var items = new List<SymbolsQuantumonline>();
            if (content.Contains("Matching Security Names for"))
            {
                var i1 = content.IndexOf("Matching Security Names for", StringComparison.InvariantCulture);
                var i2 = content.IndexOf("</table", i1 + 10, StringComparison.InvariantCulture);
                var rows = content.Substring(i1, i2 - i1).Split(new string[] { "</tr>" }, StringSplitOptions.None);
                for (var k = 1; k < rows.Length - 1; k++)
                {
                    var row = rows[k].Trim();
                    if (string.IsNullOrEmpty(row)) continue;
                    var cells = row.Split(new string[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries);

                    var symbol = cells[0].Replace("</font>", "").Replace("</b>", "").Replace("</a>", "").Trim();
                    i1 = symbol.LastIndexOf('>');
                    symbol = symbol.Substring(i1 + 1).Trim();

                    var name = cells[1].Replace("</font>", "").Trim();
                    i1 = name.LastIndexOf('>');
                    name = name.Substring(i1 + 1).TrimStart();
                    /*var ss = name.Split(new[] {"More results"}, StringSplitOptions.RemoveEmptyEntries);
                    if (ss.Length != 1)
                    {

                    }
                    name = ss[ss.Length - 1].Trim();*/
                    //i1 = name.IndexOfAny(new char[] { '\r', '\n' });
                    //if (i1 != -1)
                    //  name = name.Substring(0, i1);

                    var exchange = cells[2].Replace("</font>", "").Replace("</b>", "").Trim();
                    i1 = exchange.LastIndexOf('>');
                    exchange = exchange.Substring(i1 + 1).Trim();

                    i1 = cells[0].IndexOf("href=\"", StringComparison.InvariantCulture);
                    i2 = cells[0].IndexOf("\"", i1 + 6, StringComparison.InvariantCulture);
                    var url = cells[0].Substring(i1 + 6, i2 - i1 - 6);

                    items.Add(new SymbolsQuantumonline(symbol, exchange, name, url, timeStamp));
                }
            }
            else if (content.Contains("Sorry, but there were no matches for")) { }
            else if (content.Contains("An error occurred while executing the application")) { }
            else
                // return null;
                throw new Exception($"Check Parse action for QuantumonlineSymbols in {fileName} file");

            return items;
        }
        #endregion

        #region ========  Check Eoddata daily database  ========
        public static void DayEoddata_Check(Action<string> showStatusAction)
        {
            var badLines = new List<string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 150;

                    var files = Directory.GetFiles(Settings.DayEoddataFolder);
                    foreach (var file in files)
                    {
                        showStatusAction($"DayEoddata file is checking: {Path.GetFileName(file)}");

                        var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                        var exchange = ss[0];
                        var date = DateTime.ParseExact(ss[1], "yyyyMMdd", CultureInfo.InvariantCulture);
                        cmd.CommandText = $"SELECT * from DayEoddata WHERE Exchange='{exchange}' and date='{date:yyyy-MM-dd}' order by Symbol";
                        var dbLines = new List<string>();
                        using (var rdr = cmd.ExecuteReader())
                            while (rdr.Read())
                            {
                                var o = new DayEoddata(rdr);
                                dbLines.Add(
                                    $"{o.Symbol},{o.Date:yyyyMMdd},{o.Open.ToString(CultureInfo.InvariantCulture)},{o.High.ToString(CultureInfo.InvariantCulture)},{o.Low.ToString(CultureInfo.InvariantCulture)},{o.Close.ToString(CultureInfo.InvariantCulture)},{o.Volume.ToString("F0", CultureInfo.InvariantCulture)}");
                            }

                        string content = null;
                        using (var _zip = new ZipReader(file))
                        {
                            var fileContents = _zip.Select(a => a.Content).ToArray();
                            if (fileContents.Length == 1)
                                content = fileContents[0];
                            else
                                throw new Exception($"Error in zip file structure: {file}");
                        }
                        var fileLines = new List<string>();
                        var lines = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        string prevLine = null; // To ignore dublicates
                        foreach (var line in lines)
                        {
                            if (!string.Equals(line, prevLine))
                                fileLines.Add(line);
                            prevLine = line;
                        }

                        if (dbLines.Count != fileLines.Count)
                            throw new Exception($"Number of lines for {file} file don't match. Db lines:{dbLines.Count}, File lines:{fileLines.Count} ");
                        dbLines.Sort();
                        fileLines.Sort();
                        for (var k = 0; k < dbLines.Count; k++)
                        {
                            if (!string.Equals(dbLines[k], fileLines[k]))
                            {
                                Debug.Print(Path.GetFileNameWithoutExtension(file) + "\t" + dbLines[k]);
                                Debug.Print(Path.GetFileNameWithoutExtension(file) + "\t" + fileLines[k]+"\n");
                                badLines.Add(Path.GetFileNameWithoutExtension(file) + "\t" + dbLines[k]);
                                badLines.Add(Path.GetFileNameWithoutExtension(file) + "\t" + fileLines[k]);
                            }
                        }
                        /*var dbContent = string.Join(Environment.NewLine, dbLines);
                        var fileContent = string.Join(Environment.NewLine, fileLines);
                        if (dbContent!=fileContent)
                            throw new Exception($"Data of {file} file don't match");*/
                    }
                }

            }

            if (badLines.Count>0)
                showStatusAction($"Found {badLines.Count/2} don't matched lines. See Debug Output in Visual Studio for details");
            else
                showStatusAction($"DayEoddata file check finished!!!");
        }
        #endregion

        #region ========  Eoddata daily parse + save to db  ========
        public static IEnumerable<DayEoddata> DayEoddata_Data(Action<string> showStatusAction)
        {
            var saved = new Dictionary<Tuple<string, string>, string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            {
                showStatusAction($"DayEoddata. Before parsing procedure");
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 150;
                    cmd.CommandText = "SELECT distinct Exchange, date from DayEoddata";
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                            saved.Add(new Tuple<string, string>((string)rdr["Exchange"],
                                    ((DateTime)rdr["Date"]).ToString("yyyyMMdd", CultureInfo.InvariantCulture)), null);
                }
            }

            var files = Directory.GetFiles(Settings.DayEoddataFolder, "*.zip");
            foreach (var file in files)
            {
                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var key = new Tuple<string, string>(ss[0].ToUpper(), ss[1]);
                if (!saved.ContainsKey(key))
                {
                    showStatusAction($"DayEoddata file is parsing: {Path.GetFileName(file)}");
                    string content = null;
                    using (var _zip = new ZipReader(file))
                    {
                        var fileContents = _zip.Select(a => a.Content).ToArray();
                        if (fileContents.Length == 1)
                            content = fileContents[0];
                        else
                            throw new Exception($"Error in zip file structure: {file}");
                    }

                    var lines = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    string prevLine = null; // To ignore dublicates
                    foreach (var line in lines)
                    {
                        if (!string.Equals(line, prevLine))
                        {
                            prevLine = line;
                            yield return new DayEoddata(ss[0], line.Split(','));
                        }
                    }
                }

            }
            showStatusAction($"Data is saving to database ... Wait, please.");
        }
        #endregion

        #region ========  Yahoo indexes parse + save to db  ========

        public static void IndexesYahoo(Action<string> showStatusAction)
        {
            var data = new List<DayYahoo>();
            var cnt = 0;
            var files = Directory.GetFiles(Settings.DayYahooIndexesFolder);
            foreach (var file in files)
            {
                showStatusAction($"Yahoo index file is parsing: {Path.GetFileName(file)}");
                var lines = File.ReadAllLines(file);
                if (lines.Length == 0)
                    throw new Exception($"Invalid Day Yahoo quote file (no text lines): {file}");
                if (lines[0] != "Date,Open,High,Low,Close,Adj Close,Volume")
                    throw new Exception($"Invalid YahooIndex quote file (check file header): {file}");
                var symbol = Path.GetFileNameWithoutExtension(file).Split('_')[0];

                for (var k = 1; k < lines.Length; k++)
                {
                    if (!String.IsNullOrEmpty(lines[k].Trim()))
                    {
                        if (lines[k].Contains("null"))
                            Debug.Print($"{symbol}, {lines[k]}");
                        else
                        {
                            var ss = lines[k].Split(',');
                            var item = new DayYahoo(symbol, ss);
                            data.Add(item);
                            cnt++;
                        }
                    }
                }
            }

            if (data.Count > 0)
            {
                SaveToDb.IndexesYahoo_SaveToDb(data);
                data.Clear();
            }

            showStatusAction($"Yahoo index file parsing FINISHED!");
        }
        #endregion

        #region ========  Yahoo daily parse + save to db  ========
        public static void DayYahoo_Parse(string zipfile, Action<string> showStatusAction)
        {
            var data = new List<DayYahoo>();
            var clearTable = true;
            var cnt = 0;
            using (var zip = new ZipReader(zipfile))
                foreach (var file in zip)
                {
                    if (file.Length > 0 && file.FileNameWithoutExtension.ToUpper().StartsWith("Y"))
                    {
                        showStatusAction($"DayYahoo file is parsing: {file.FullName}");
                        var lines = file.Content.Split('\n');
                        if (lines.Length == 0)
                            throw new Exception($"Invalid Day Yahoo quote file (no text lines): {file}");
                        if (lines[0] != "Date,Open,High,Low,Close,Adj Close,Volume")
                            throw new Exception($"Invalid Day Yahoo quote file (check file header): {file}");
                        var symbol = file.FileNameWithoutExtension.Substring(1);
                        // if (symbol == "TOPS") continue; // TOPS & GMGI have big prices

                        for (var k = 1; k < lines.Length; k++)
                        {
                            if (!String.IsNullOrEmpty(lines[k].Trim()))
                            {
                                if (lines[k].Contains("null"))
                                    Debug.Print($"{symbol}, {lines[k]}");
                                else
                                {
                                    var ss = lines[k].Split(',');
                                    var item = new DayYahoo(symbol, ss);
                                    data.Add(item);
                                    cnt++;
                                }
                            }
                        }

                        if (data.Count > 100000)
                        {
                            showStatusAction($"DayYahoo file is saving to DB: {data.Count:N0} ({cnt:N0}) records");
                            SaveToDb.DayYahoo_SaveToDb(data, clearTable);
                            clearTable = false;
                            data.Clear();
                        }
                    }
                }

            if (data.Count > 0)
            {
                SaveToDb.DayYahoo_SaveToDb(data, clearTable);
                data.Clear();
            }

            showStatusAction($"DayYahoo file parsing finished!!!");
        }
        #endregion
    }
}
