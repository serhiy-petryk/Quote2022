using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Quote2022.Helpers;

namespace Quote2022.Actions.SymbolsYahoo
{
    public static class ProfileYahoo_Parse
    {
        public static void Start(string zipFileName, Action<string> showStatusAction)
        {
            showStatusAction($"ProfileYahoo_Parse. Started.");

            var items = new List<Models.ProfileYahoo>();
            var errorLog = new List<string> {"ErrorType\tSymbol"};
            var cnt = 0;
            using (var zip = new ZipReader(zipFileName))
            {
                // var totalItems = zip.Count();
                foreach (var entry in zip)
                {
                    cnt++;
                    if (cnt % 20 == 0)
                        showStatusAction($"ProfileYahoo_Parse processed {cnt:N0} files");

                    if (entry.Length > 0 && entry.FullName.EndsWith(".html", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var fileSymbol = entry.FileNameWithoutExtension.Split('_')[1];
                        var timeStamp = DateTime.ParseExact(entry.FileNameWithoutExtension.Split('_')[2], "yyyyMMdd",
                            CultureInfo.InvariantCulture);
                        var content = entry.Content;
                        var i1 = content.IndexOf("<h1 class=\"D(ib) Fz(18px)\">", StringComparison.InvariantCultureIgnoreCase);
                        if (i1 == -1)
                        {
                            i1 = content.IndexOf("Symbols similar to", StringComparison.InvariantCultureIgnoreCase);
                            if (i1 > 0)
                            {
                                errorLog.Add($"Symbol not found\t{fileSymbol}");
                                Debug.Print($"Symbol not found\t{fileSymbol}");
                                continue;
                            }
                            throw new Exception("Check");
                        }
                        content = content.Substring(i1 + 27);
                        var i2 = content.IndexOf("</h1", StringComparison.InvariantCultureIgnoreCase);
                        var s = content.Substring(0, i2);
                        if (!s.EndsWith(")"))
                            throw new Exception("Check");
                        i1 = s.LastIndexOf("(", StringComparison.InvariantCultureIgnoreCase);
                        var name = System.Net.WebUtility.HtmlDecode(s.Substring(0, i1).Trim());
                        if (string.IsNullOrEmpty(name))
                            name = null;
                        var symbol = System.Net.WebUtility.HtmlDecode(s.Substring(i1 + 1, s.Length - i1 - 2).Trim());
                        if (!string.Equals(symbol, fileSymbol))
                            throw new Exception("Check symbol");

                        content = content.Substring(i2 + 4);
                        i2 = content.IndexOf(@"</span", StringComparison.InvariantCultureIgnoreCase);
                        i1 = content.LastIndexOf(">", i2, StringComparison.InvariantCultureIgnoreCase);
                        s = content.Substring(i1 + 1, i2 - i1 - 1);
                        i1 = s.IndexOf("-", StringComparison.InvariantCultureIgnoreCase);
                        if (i1 == -1)
                        {
                            errorLog.Add($"Bad exchange\t{fileSymbol}");
                            Debug.Print($"Bad exchange\t{fileSymbol}");
                            continue;
                        }

                        var exchange = s.Substring(0, i1).Trim();

                        i1 = content.IndexOf(@">Sector(s)</span", StringComparison.InvariantCultureIgnoreCase);
                        if (i1 > 0)
                        {
                            content = content.Substring(i1 + 16);
                            i2 = content.IndexOf(@"</span", StringComparison.InvariantCultureIgnoreCase);
                            i1 = content.LastIndexOf(">", i2, StringComparison.InvariantCultureIgnoreCase);
                            var sector = System.Net.WebUtility.HtmlDecode(content.Substring(i1 + 1, i2 - i1 - 1).Trim());
                            if (string.IsNullOrEmpty(sector))
                                sector = null;

                            content = content.Substring(i2 + 5);
                            i1 = content.IndexOf(@">Industry</span", StringComparison.InvariantCultureIgnoreCase);
                            content = content.Substring(i1 + 15);
                            i2 = content.IndexOf(@"</span", StringComparison.InvariantCultureIgnoreCase);
                            i1 = content.LastIndexOf(">", i2, StringComparison.InvariantCultureIgnoreCase);
                            var industry = System.Net.WebUtility.HtmlDecode(content.Substring(i1 + 1, i2 - i1 - 1).Trim());
                            if (string.IsNullOrEmpty(industry))
                                industry = null;

                            content = content.Substring(i2 + 5);
                            i1 = content.IndexOf(@">Full Time Employees</span", StringComparison.InvariantCultureIgnoreCase);
                            content = content.Substring(i1 + 26);
                            i2 = content.IndexOf(@"</span", StringComparison.InvariantCultureIgnoreCase);
                            i1 = content.LastIndexOf(">", i2, StringComparison.InvariantCultureIgnoreCase);
                            s = System.Net.WebUtility.HtmlDecode(content.Substring(i1 + 1, i2 - i1 - 1).Trim());
                            var employees = string.IsNullOrEmpty(s)
                                ? (int?)null
                                : int.Parse(s.Replace(",", ""), CultureInfo.InvariantCulture);
                            var item = new Models.ProfileYahoo
                            {
                                Symbol = symbol,
                                Exchange = exchange,
                                Name = name,
                                Type = "stock",
                                Sector = sector,
                                Industry = industry,
                                Employees = employees,
                                TimeStamp = timeStamp
                            };
                            items.Add(item);
                        }
                        else
                        {
                            i1 = content.IndexOf(@">Fund Overview</span", StringComparison.InvariantCultureIgnoreCase);
                            if (i1 > 0)
                            {
                                content = content.Substring(i1 + 20);
                                i1 = content.IndexOf(@">Category</span", StringComparison.InvariantCultureIgnoreCase);
                                i2 = content.IndexOf(@"<span", i1 + 15, StringComparison.InvariantCultureIgnoreCase);
                                content = content.Substring(i2 + 4);
                                i2 = content.IndexOf(@"</span", StringComparison.InvariantCultureIgnoreCase);
                                i1 = content.LastIndexOf(">", i2, StringComparison.InvariantCultureIgnoreCase);
                                var category = System.Net.WebUtility.HtmlDecode(content.Substring(i1 + 1, i2 - i1 - 1).Trim());
                                if (category == "N/A")
                                    category = null;

                                content = content.Substring(i2 + 5);
                                i1 = content.IndexOf(@">Fund Family</span", StringComparison.InvariantCultureIgnoreCase);
                                i2 = content.IndexOf(@"<span", i1 + 18, StringComparison.InvariantCultureIgnoreCase);
                                content = content.Substring(i2 + 4);
                                i2 = content.IndexOf(@"</span", StringComparison.InvariantCultureIgnoreCase);
                                i1 = content.LastIndexOf(">", i2, StringComparison.InvariantCultureIgnoreCase);
                                var fondFamily =
                                    System.Net.WebUtility.HtmlDecode(content.Substring(i1 + 1, i2 - i1 - 1).Trim());

                                content = content.Substring(i2 + 5);
                                i1 = content.IndexOf(@">Net Assets</span", StringComparison.InvariantCultureIgnoreCase);
                                i2 = content.IndexOf(@"<span", i1 + 17, StringComparison.InvariantCultureIgnoreCase);
                                content = content.Substring(i2 + 4);
                                i2 = content.IndexOf(@"</span", StringComparison.InvariantCultureIgnoreCase);
                                i1 = content.LastIndexOf(">", i2, StringComparison.InvariantCultureIgnoreCase);
                                s = System.Net.WebUtility.HtmlDecode(content.Substring(i1 + 1, i2 - i1 - 1).Trim());
                                var netAssets = (float?)null;
                                if (s.EndsWith("M", StringComparison.InvariantCultureIgnoreCase))
                                    netAssets = float.Parse(s.Substring(0, s.Length - 1), CultureInfo.InvariantCulture);
                                else if (s.EndsWith("K", StringComparison.InvariantCultureIgnoreCase))
                                    netAssets = Convert.ToSingle(Math.Round(
                                        float.Parse(s.Substring(0, s.Length - 1), CultureInfo.InvariantCulture) / 1000.0F, 3));
                                else if (s.EndsWith("B", StringComparison.InvariantCultureIgnoreCase))
                                    netAssets = Convert.ToSingle(Math.Round(
                                        float.Parse(s.Substring(0, s.Length - 1), CultureInfo.InvariantCulture) * 1000.0F, 0));
                                else if (s.EndsWith("T", StringComparison.InvariantCultureIgnoreCase))
                                    netAssets = Convert.ToSingle(Math.Round(
                                        float.Parse(s.Substring(0, s.Length - 1), CultureInfo.InvariantCulture) * 1000000.0F, 0));
                                else if (s == "N/A") { }
                                else
                                    throw new Exception("Check netAssets");

                                content = content.Substring(i2 + 5);
                                i1 = content.IndexOf(@">YTD Daily Total Return</span",
                                    StringComparison.InvariantCultureIgnoreCase);
                                i2 = content.IndexOf(@"<span", i1 + 29, StringComparison.InvariantCultureIgnoreCase);
                                content = content.Substring(i2 + 4);
                                i2 = content.IndexOf(@"</span", StringComparison.InvariantCultureIgnoreCase);
                                i1 = content.LastIndexOf(">", i2, StringComparison.InvariantCultureIgnoreCase);
                                s = System.Net.WebUtility.HtmlDecode(content.Substring(i1 + 1, i2 - i1 - 1).Trim());
                                var ytdDailyTotalReturn = s == "N/A" ? (float?)null : float.Parse(s.Substring(0, s.Length - 1), CultureInfo.InvariantCulture);

                                content = content.Substring(i2 + 5);
                                i1 = content.IndexOf(@">Yield</span", StringComparison.InvariantCultureIgnoreCase);
                                i2 = content.IndexOf(@"<span", i1 + 12, StringComparison.InvariantCultureIgnoreCase);
                                content = content.Substring(i2 + 4);
                                i2 = content.IndexOf(@"</span", StringComparison.InvariantCultureIgnoreCase);
                                i1 = content.LastIndexOf(">", i2, StringComparison.InvariantCultureIgnoreCase);
                                s = System.Net.WebUtility.HtmlDecode(content.Substring(i1 + 1, i2 - i1 - 1).Trim());
                                var yield = s == "N/A" ? (float?)null : float.Parse(s.Substring(0, s.Length - 1), CultureInfo.InvariantCulture);

                                content = content.Substring(i2 + 5);
                                i1 = content.IndexOf(@">Legal Type</span", StringComparison.InvariantCultureIgnoreCase);
                                i2 = content.IndexOf(@"<span", i1 + 17, StringComparison.InvariantCultureIgnoreCase);
                                content = content.Substring(i2 + 4);
                                i2 = content.IndexOf(@"</span", StringComparison.InvariantCultureIgnoreCase);
                                i1 = content.LastIndexOf(">", i2, StringComparison.InvariantCultureIgnoreCase);
                                var legalType = System.Net.WebUtility.HtmlDecode(content.Substring(i1 + 1, i2 - i1 - 1).Trim());
                                var item = new Models.ProfileYahoo
                                {
                                    Symbol = symbol,
                                    Exchange = exchange,
                                    Name = name,
                                    Type = "etf",
                                    Category = category,
                                    Family = fondFamily,
                                    NetAssets = netAssets,
                                    YtdDailyTotalReturn = ytdDailyTotalReturn,
                                    Yield = yield,
                                    LegalType = legalType,
                                    TimeStamp = timeStamp
                                };
                                items.Add(item);
                            }
                            else
                            {
                                var item = new Models.ProfileYahoo
                                { Symbol = symbol, Exchange = exchange, Name = name, Type = "?", TimeStamp = timeStamp };
                                items.Add(item);
                            }
                        }

                    }
                }
            }
            if (items.Count > 0)
            {
                SaveToDb.ClearAndSaveToDbTable(items, "dbQuote2023..Bfr_ProfileYahoo", "Symbol", "Exchange", "Name", "Type",
                    "Sector", "Industry", "Employees", "Category", "Family", "NetAssets", "YtdDailyTotalReturn",
                    "Yield", "LegalType", "TimeStamp");
                items.Clear();
            }

            SaveToDb.RunProcedure("dbQuote2023..pUpdateProfileYahoo");

            var errorLogFileName = Path.GetDirectoryName(zipFileName) +
                                   $"\\errorLog_{Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(zipFileName))}.txt";

            if (File.Exists(errorLogFileName))
                File.Delete(errorLogFileName);
            File.WriteAllLines(errorLogFileName, errorLog);
            showStatusAction($"ProfileYahoo_Parse. Finished. Found {errorLog.Count-1} errors. See {errorLogFileName}");
        }

    }
}
