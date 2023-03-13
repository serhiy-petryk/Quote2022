using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quote2022.Helpers;

namespace Quote2022.Actions.DayAlphaVantage
{
    public static class DAV_Parse
    {
        public static void Start(string zipFileName, Action<string> showStatus)
        {
            showStatus($"DayAlphaVantage_Parse started.");

            var quotes = new List<Models.DayAlphaVantage>();
            // Clear table
            Actions.SaveToDb.ClearAndSaveToDbTable(quotes, "dbQuote2023..Bfr_DayAlphaVantage", "Symbol", "Date",
                "Open", "High", "Low", "Close", "Volume", "Dividend", "Split");

            var errorLog = new List<string>{$"ErrorType\tSymbol"};
            var cnt = 0;
            using (var zip = new ZipReader(zipFileName))
                foreach (var item in zip)
                    if (item.Length > 0 && item.FullName.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
                    {
                        cnt++;
                        if (cnt % 100 == 0)
                            showStatus($"DayAlphaVantage_Parse processed {cnt:N0} files");

                        var lines = item.AllLines.ToArray();
                        if (lines.Length == 1 && lines[0] == "{}") continue;

                        var symbol = item.FileNameWithoutExtension.Split('_')[0];
                        if (lines.Length > 1 && lines[1].Contains("Invalid API call"))
                        {
                            errorLog.Add($"Invalid API call\t{symbol}");
                            continue;
                        }

                        if (!string.Equals(lines[0], "timestamp,open,high,low,close,adjusted_close,volume,dividend_amount,split_coefficient"))
                            throw new Exception($"DAV_Parse. Invalid file header in {item.FullName}");

                        for (var k = lines.Length - 1; k >= 1; k--)
                            quotes.Add(new Models.DayAlphaVantage(symbol, lines[k]));

                        if (quotes.Count > 100000)
                        {
                            Actions.SaveToDb.SaveToDbTable(quotes, "dbQuote2023..Bfr_DayAlphaVantage", "Symbol", "Date",
                                "Open", "High", "Low", "Close", "Volume", "Dividend", "Split");
                            quotes.Clear();
                        }
                    }

            if (quotes.Count > 0)
            {
                Actions.SaveToDb.SaveToDbTable(quotes, "dbQuote2023..Bfr_DayAlphaVantage", "Symbol", "Date", "Open", "High",
                    "Low", "Close", "Volume", "Dividend", "Split");
                quotes.Clear();
            }

            if (errorLog.Count > 1)
            {
                var errorFileName = Path.GetDirectoryName(zipFileName) + @"\errorLog.txt";
                if (File.Exists(errorFileName))
                    File.Delete(errorFileName);
                File.WriteAllLines(errorFileName, errorLog);
                showStatus($"DayAlphaVantage_Parse FINISHED! {errorLog.Count-1} errors. See {errorFileName}");
            }
            else
                showStatus($"DayAlphaVantage_Parse FINISHED! No errors.");
        }
    }
}
