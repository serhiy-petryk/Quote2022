using System;
using System.Collections.Generic;
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
        #region ========  SymbolsYahooLookup Parse & SaveToDB  ========
        public static void SymbolsYahooLookup_ParseAndSaveToDb(string zipFile, Action<string> showStatusAction)
        {
            var data = new Dictionary<string, SymbolsYahooLookup.Document>();
            var ss = Path.GetFileNameWithoutExtension(zipFile).Split('_');
            var timeStamp = DateTime.ParseExact(ss[ss.Length - 1].Trim(), "yyyyMMdd", CultureInfo.InvariantCulture);

            using (var zip = new ZipReader(zipFile))
                foreach (var file in zip.Where(a => a.FullName.EndsWith(".json", true, CultureInfo.InvariantCulture)))
                {
                    var o = JsonConvert.DeserializeObject<SymbolsYahooLookup>(file.Content);
                    if (o.finance.error!=null)
                        throw new Exception($"There is an error in {file.FileNameWithoutExtension} file. Message: {o.finance.error}");
                    foreach (var item in o.finance.result[0].documents)
                    {
                        if (item.symbol.Contains('.')) // Not US symbols
                            continue;

                        item.Normalize(timeStamp);
                        if (item.symbol == "ACT" && item.quoteType == "etf")
                        {
                            Debug.Print($"File for ACT/etf: {file.FileNameWithoutExtension}");
                        }
                        if (!data.ContainsKey(item.Key))
                            data.Add(item.Key, item);
                        else if (string.IsNullOrEmpty(data[item.Key].shortName))
                            data[item.Key] = item;
                        else if (!string.Equals(item.shortName, data[item.Key].shortName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (string.Equals(item.Key, "BCI^PCX") || string.Equals(item.Key, "BCD^PCX"))
                            {
                                if (data[item.Key].shortName.StartsWith("abrdn ", StringComparison.InvariantCultureIgnoreCase))
                                    data[item.Key] = item;
                            }
                            else if (string.IsNullOrEmpty(item.shortName)) { }
                            else
                                throw new Exception($"Error! different names for {item.Key}. File: {file.FileNameWithoutExtension}");
                        }
                    }
                }

            if (data.Count > 0)
            {
                var ii = data.Select(a => a.Value.rank).Distinct().ToArray();

                SaveToDb.ClearAndSaveToDbTable(data.Values, "Bfr_SymbolsYahooLookup", "exchange", "symbol", "shortName",
                    "quoteType", "industryName", "rank", "TimeStamp");
                SaveToDb.RunProcedure("pUpdateSymbolsYahooLookup", new Dictionary<string, object> { { "@Date", timeStamp } });
            }

        }
        #endregion

        #region ========  Nasdaq Stock Screener Parse & SaveToDB  ========
        public static void ScreenerNasdaq_ParseAndSaveToDb(string zipFile, Action<string> showStatusAction)
        {
            using (var zip = new ZipReader(zipFile))
                foreach (var file in zip.Where(a => a.FullName.EndsWith(".csv", true, CultureInfo.InvariantCulture)))
                {
                    showStatusAction($"ScreenerNasdaq '{file.FileNameWithoutExtension}' file parsing & save to database started.");
                    var ss = file.FileNameWithoutExtension.Split('_');
                    var timeStamp = DateTime.ParseExact(ss[ss.Length - 1].Trim(), "yyyyMMdd", CultureInfo.InvariantCulture);
                    var items = new List<Models.ScreenerNasdaq>();
                    var lines = file.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length == 0 || !Equals(lines[0], "Symbol,Name,Last Sale,Net Change,% Change,Market Cap,Country,IPO Year,Volume,Sector,Industry"))
                        throw new Exception($"Invalid Nasdaq stock screener file structure! {file}");

                    string lastLine = null;
                    for (var k = 1; k < lines.Length; k++)
                    {
                        if (Equals(lastLine, lines[k])) continue;
                        items.Add(new Models.ScreenerNasdaq(timeStamp, lines[k]));
                    }

                    if (items.Count > 0)
                    {
                        SaveToDb.ClearAndSaveToDbTable(items, "Bfr_ScreenerNasdaq", "Symbol", "Name", "LastSale",
                            "NetChange", "Change", "MarketCap", "Country", "IPOYear", "Volume", "Sector", "Industry",
                            "TimeStamp");
                        SaveToDb.RunProcedure("pUpdateScreenerNasdaq", new Dictionary<string, object> { { "@Date", timeStamp } });
                    }
                    showStatusAction($"ScreenerNasdaq '{file.FileNameWithoutExtension}' file parsing & save to database FINISHED!");
                }
        }
        #endregion

        #region ========  Eoddata symbols parse + save to db ========
        public static void SymbolsEoddata_ParseAndSaveToDb(string zipFile, Action<string> showStatusAction)
        {
            showStatusAction($"SymbolsEoddata file is parsing: {Path.GetFileName(zipFile)}");
            SaveToDb.RunProcedure("pUpdateSymbolsEoddata_Before");

            using (var zip = new ZipReader(zipFile))
                foreach (var file in zip.Where(a => a.FullName.EndsWith(".txt", true, CultureInfo.InvariantCulture)))
                {
                    var items = new List<SymbolsEoddata>();
                    showStatusAction($"SymbolsEoddata file is parsing: {Path.GetFileName(file.FileNameWithoutExtension)}");
                    var ss = file.FileNameWithoutExtension.Split('_');
                    var exchange = ss[0].Trim().ToUpper();
                    var date = file.Created;
                    var lines = file.Content.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                    var firstLine = true;

                    // Add data to array (items)
                    foreach (var line in lines)
                    {
                        if (firstLine)
                        {
                            if (!string.Equals(line, "Symbol\tDescription"))
                                throw new Exception($"SymbolsEoddata_Parse error! Please, check the first line of {file} file");
                            firstLine = false;
                        }
                        else if (!string.IsNullOrEmpty(line))
                            items.Add(new SymbolsEoddata(exchange, date, line.Split('\t')));
                    }

                    // Save data to buffer table of data server
                    SaveToDb.ClearAndSaveToDbTable(items, "Bfr_SymbolsEoddata", "Symbol", "Exchange", "Name", "Created");
                    SaveToDb.RunProcedure("pUpdateSymbolsEoddata", new Dictionary<string, object> { { "@Exchange", exchange }, { "@Date", date } });
                    items.Clear();

                    showStatusAction($"SymbolsEoddata file finished: {Path.GetFileName(file.FileNameWithoutExtension)}");
                }

            SaveToDb.RunProcedure("pUpdateSymbolsXref");

            showStatusAction($"SymbolsEoddata file FINISHED! File name: {Path.GetFileName(zipFile)}");
        }
        #endregion

        #region ==================  SymbolsNasdaq_ParseAndSaveToDb  ==========================
        public static void SymbolsNasdaq_ParseAndSaveToDb(string zipFile, Action<string> showStatusAction)
        {
            var symbols = new Dictionary<string, List<SymbolsNasdaq.SymbolsNasdaqDbItem>>();
            using (var zip = new ZipReader(zipFile))
                foreach (var file in zip.Where(a => a.FullName.EndsWith(".txt", true, CultureInfo.InvariantCulture)))
                {
                    showStatusAction($"{file.FileNameWithoutExtension} file is parsing.");
                    var oo = JsonConvert.DeserializeObject<SymbolsNasdaq.SymbolsNasdaqFile>(file.Content);
                    if (oo.Status.rCode == 200)
                    {
                        foreach (var o in oo.data)
                        {
                            var dbItem = new SymbolsNasdaq.SymbolsNasdaqDbItem(o, file.Created);
                            if (string.Equals(dbItem.Asset, "ETF", StringComparison.InvariantCultureIgnoreCase) ||
                                string.Equals(dbItem.Asset, "STOCKS", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (!symbols.ContainsKey(dbItem.Symbol))
                                    symbols.Add(dbItem.Symbol, new List<SymbolsNasdaq.SymbolsNasdaqDbItem>());
                                symbols[dbItem.Symbol].Add(dbItem);
                            }
                        }
                    }
                    else
                        throw new Exception(
                            $"Invalid status rCode in {file.FileNameWithoutExtension} file. status rCode: {oo.Status.rCode}");

                    // break;
                }

            // Check
            foreach (var items in symbols.Values)
            {
                var firstItem = items[0];
                for (var k = 1; k < items.Count; k++)
                {
                    if (!firstItem.IsEqual(items[k]))
                        throw new Exception($"SymbolsNasdaq_ParseAndSaveToDb error! Different items for symbol {firstItem.Symbol}");
                }
            }

            showStatusAction("Saving data to database (SymbolsNasdaq_ParseAndSaveToDb method).");

            var data = symbols.Values.Select(a => a[0]).ToArray();
            SaveToDb.ClearAndSaveToDbTable(data, "Bfr_SymbolsNasdaq", "Exchange", "Symbol", "Name", "MrktCategory",
                "SubCategory", "Nasdaq100", "Region", "Asset", "Industry", "Flag", "Created");

            var ss = Path.GetFileNameWithoutExtension(zipFile).Split('_');
            var timeStamp = DateTime.ParseExact(ss[ss.Length - 1].Trim(), "yyyyMMdd", CultureInfo.InvariantCulture);
            SaveToDb.RunProcedure("pUpdateSymbolsNasdaq", new Dictionary<string, object> { { "@Date", timeStamp } });

            showStatusAction("FINISHED (SymbolsNasdaq_ParseAndSaveToDb method)!");
        }
        #endregion

        #region ==========  Symbols Nanex Parse =============
        public static void SymbolsNanex_Parse(string[] files, List<SymbolsNanex> data, Action<string> showStatusAction)
        {
            foreach (var file in files)
            {
                var exchange = Path.GetFileNameWithoutExtension(file).Split('_')[0];
                var s = File.ReadAllText(file, Settings.Encoding);
                var created = File.GetLastWriteTime(file);
                var i1 = s.IndexOf("<table", StringComparison.Ordinal);
                var i2 = s.IndexOf(@"</table>", StringComparison.Ordinal);
                if (i1 > 0 && i2 > i1)
                {
                    var ss = s.Substring(i1, i2 - i1).Split(new string[] { "<tr" }, StringSplitOptions.RemoveEmptyEntries);
                    var uniqueIDs = new List<string>();
                    for (var i = 0; i < ss.Length; i++)
                    {
                        string[] ss1 = ss[i].Split(new string[] { "<td" }, StringSplitOptions.RemoveEmptyEntries);
                        if (ss1.Length == 8)
                        {
                            string exchange1 = GetTDElement(ss1[1]);
                            string type = GetTDElement(ss1[2]);
                            if (type.ToLower() != "type")
                            {// not header string
                                string symbol = GetTDElement(ss1[3]);
                                string name = GetTDElement(ss1[4]);
                                string activity = GetTDElement(ss1[5]);
                                string lastQuoteDate = GetTDElement(ss1[6]);
                                string lastTradeDate = GetTDElement(ss1[7]);
                                var e = new SymbolsNanex(exchange, type, symbol, name, activity, lastQuoteDate, lastTradeDate, created);
                                if (!e.IsTest)
                                {
                                    var uniqueID = (symbol + "\t" + exchange).ToUpper();
                                    if (uniqueIDs.Contains(uniqueID))
                                    {// check on uniqueID
                                        int k = uniqueIDs.IndexOf(uniqueID);
                                        var e1 = data[k];
                                        if ((e1.LastQuoteDate ?? DateTime.MinValue) < (e.LastQuoteDate ?? DateTime.MinValue))
                                        {
                                            data.RemoveAt(k);
                                            uniqueIDs.RemoveAt(k);
                                            data.Add(e);
                                            uniqueIDs.Add(uniqueID);
                                        }
                                    }
                                    else
                                    {
                                        data.Add(e);
                                        uniqueIDs.Add(uniqueID);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                    throw new Exception("Invalid Nanex symbol file context. Filename: " + file);
            }

            string GetTDElement(string s)
            {
                var s1 = s.Trim();
                if (s1.ToLower().EndsWith("</tr>")) s1 = s1.Substring(0, s1.Length - 5);
                var i1 = s1.LastIndexOf("<", StringComparison.Ordinal);
                var i2 = s1.Substring(0, i1).LastIndexOf(">", StringComparison.Ordinal);
                if (i2 > i1)
                    throw new Exception("Invalid TD token for Nanex symbol file. Token value: " + s);
                else
                    return s1.Substring(i2 + 1, i1 - i2 - 1).Trim();
            }
        }
        #endregion

    }
}
