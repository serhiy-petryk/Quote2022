using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Quote2022.Helpers;
using Quote2022.Models;

namespace Quote2022.Actions.Wikipedia
{
    public static class Indices
    {
        public static void Download(Action<string> showStatusAction)
        {
            showStatusAction($"Wikipedia.Indices.Download. Started.");

            var urls = new string[]
            {
                "https://en.wikipedia.org/wiki/List_of_S%26P_500_companies",
                "https://en.wikipedia.org/wiki/List_of_S%26P_400_companies",
                "https://en.wikipedia.org/wiki/List_of_S%26P_600_companies",
                "https://en.wikipedia.org/wiki/Nasdaq-100"
                // old: https://en.wikipedia.org/wiki/List_of_NASDAQ-100_companies
            };

            var files = new string[]
            {
                Settings.IndicesWikipediaFolder + "IndexComponents_{0}\\Components_SP500_{0}.html",
                Settings.IndicesWikipediaFolder + "IndexComponents_{0}\\Components_SP400_{0}.html",
                Settings.IndicesWikipediaFolder + "IndexComponents_{0}\\Components_SP600_{0}.html",
                Settings.IndicesWikipediaFolder + "IndexComponents_{0}\\Components_Nasdaq100_{0}.html",
            };

            var timeStamp = Helpers.CsUtils.GetTimeStamp().Item2;
            for (var k = 0; k < urls.Length; k++)
            {
                showStatusAction($"Wikipedia.Indices.Download. Downloading from {urls[k]}");
                var filename = string.Format(files[k], timeStamp);
                Actions.Download.DownloadPage(urls[k], filename);
            }

            showStatusAction($"Wikipedia.Indices.Download finished");
        }

        // ===============================
        public static void Parse(string zipFile, Action<string> showStatusAction)
        {
            // var zipFile = @"E:\Quote\WebArchive\Indices\Wikipedia\WebArchive.Wikipedia.Indices.zip";
            showStatusAction($"Wikipedia.Indices.Parse. Started.");
            var items = new List<Models.IndexDbItem>();
            var changes = new List<Models.IndexDbChangeItem>();

            using (var zip = new ZipReader(zipFile))
                foreach (var file in zip.Where(a => a.Length > 0))
                {
                    showStatusAction($"Wikipedia.Indices.Parse. Processing '{file.FileNameWithoutExtension}'");
                    ParseFile(file);
                }

            showStatusAction($"Wikipedia.Indices.Parse finished");
        }

        #region ===========  Private section  =============
        private static void ParseFile(ZipReaderItem file)
        {
            var items = new Dictionary<string, Models.IndexDbItem>();
            var changes = new List<Models.IndexDbChangeItem>();

            var ss = file.FileNameWithoutExtension.Split('_');
            var indexName = ss[ss.Length - 2];
            var timeStamp = file.Created;

            var content = file.Content;
            var i1 = content.IndexOf("id=\"constituents\"", StringComparison.InvariantCultureIgnoreCase);
            if (i1 > 0)
            {
                var i2 = content.IndexOf("</table", i1 + 17, StringComparison.InvariantCultureIgnoreCase);
                var table = content.Substring(i1 + 17, i2 - i1 - 17);
                ParseTable(indexName, timeStamp, table, items);
                Debug.Print($"table: {items.Count}, {file.FileNameWithoutExtension}");
            }
            i1 = content.IndexOf(">stock symbol<", StringComparison.InvariantCultureIgnoreCase);
            if (items.Count == 0 && i1 > 0)
            {
                var i2 = content.IndexOf("</ul>", i1 + 17, StringComparison.InvariantCultureIgnoreCase);
                var list = content.Substring(i1 + 17, i2 - i1 - 17);
                ParseList(indexName, timeStamp, list, items);
                Debug.Print($"list: {items.Count}, {file.FileNameWithoutExtension}");
            }

            i1 = content.IndexOf("==Components==", StringComparison.InvariantCultureIgnoreCase);
            if (items.Count == 0 && i1 > 0)
            {
                var i2 = content.IndexOf("==External links==", i1, StringComparison.InvariantCultureIgnoreCase);
                var links = content.Substring(i1 + 14, i2 - i1 - 14);
                ParseLinks(indexName, timeStamp, links, items);
                Debug.Print($"==Components==: {file.FileNameWithoutExtension}");
            }

            i1 = content.IndexOf(">External links</a>", StringComparison.InvariantCultureIgnoreCase);
            if (items.Count == 0 && i1 > 0)
            {
                var i2 = content.IndexOf("</ol>", i1 + 17, StringComparison.InvariantCultureIgnoreCase);
                if (i2 == -1)
                    i2 = content.IndexOf("</ul>", i1 + 17, StringComparison.InvariantCultureIgnoreCase);
                var list = content.Substring(i1 + 17, i2 - i1 - 17);
                ParseList(indexName, timeStamp, list, items);
                Debug.Print($"External link: {items.Count}, {file.FileNameWithoutExtension}");
            }

            i1 = content.IndexOf(">Ticker Symbol</a></th>", StringComparison.InvariantCultureIgnoreCase);
            if (items.Count == 0 && i1 > 0)
            {
                items.Clear();
                var i2 = content.Substring(0, i1).LastIndexOf("<table", StringComparison.InvariantCultureIgnoreCase);
                var i3 = content.IndexOf("</table", i2, StringComparison.InvariantCultureIgnoreCase);
                var table = content.Substring(i2, i3 - i2);
                ParseTable(indexName, timeStamp, table, items);
                Debug.Print($"Ticker Symbol: {items.Count}, {file.FileNameWithoutExtension}");
            }

            if (items.Count == 0 && timeStamp < new DateTime(2010, 1, 1))
            {
                Debug.Print($"??????: {file.FileNameWithoutExtension}");
                return;
            }

            if (items.Count == 0)
                throw new Exception("Check parser");

            IndexDbItem.SaveToDb(indexName, timeStamp, items.Values);

            // Changes
            i1 = content.IndexOf("Recent and announced changes to the list of ", StringComparison.InvariantCultureIgnoreCase);
            if (i1 == -1)
                i1 = content.IndexOf("Recent changes to the list of ", StringComparison.InvariantCultureIgnoreCase);
            if (i1 > 0)
            {
                Debug.Print($"Changes: {file.FileNameWithoutExtension}");
                i1 = content.IndexOf(">Reason", StringComparison.InvariantCultureIgnoreCase);
                // i1 = content.IndexOf("<table ", i1, StringComparison.InvariantCultureIgnoreCase);
                var i2 = content.IndexOf("</table>", i1 + 7, StringComparison.InvariantCultureIgnoreCase);
                var table = content.Substring(i1, i2 - i1);

                var lastDate = DateTime.MinValue;
                var rows = table.Split(new[] { "</tr>" }, StringSplitOptions.None);
                for (var k = 0; k < rows.Length; k++)
                {
                    var cells = rows[k].Split(new[] { "</td>" }, StringSplitOptions.None);
                    var offset = cells.Length == 7 ? 0 : -1;
                    if (cells.Length > 4)
                    {
                        var date = lastDate;
                        if (offset == 0)
                        {
                            var sDate = GetCellValue(cells[0]).Replace("th, ", ", ");
                            if (sDate == "Apr/May 2011") sDate = "April 27, 2011";

                            date = DateTime.Parse(sDate, CultureInfo.InvariantCulture);
                            lastDate = date;
                        }

                        var addedSymbol = GetCellValue(cells[1 + offset]);
                        var addedName = GetCellValue(cells[2 + offset]);
                        var removedSymbol = GetCellValue(cells[3 + offset]);
                        var removedName = GetCellValue(cells[4 + offset]);
                        var item = new Models.IndexDbChangeItem
                        {
                            Index = indexName,
                            Date = date,
                            AddedSymbol = addedSymbol,
                            AddedName = addedName,
                            RemovedSymbol = removedSymbol,
                            RemovedName = removedName,
                            TimeStamp = timeStamp
                        };
                        changes.Add(item);
                    }
                }

                IndexDbChangeItem.SaveToDb(indexName, timeStamp, changes);
            }
        }

        private static void ParseTable(string indexName, DateTime timeStamp, string table, Dictionary<string, Models.IndexDbItem> items)
        {
            var rows = table.Replace("</tbody", "").Split(new[] { "</tr>" }, StringSplitOptions.None);

            // Header
            var symbolNo = -1;
            var nameNo = -1;
            var sectorNo = -1;
            var industryNo = -1;
            var cells = rows[0].Split(new[] { "</td>", "</th>" }, StringSplitOptions.None);
            for (var k = 0; k < cells.Length; k++)
            {
                var cell = GetCellValue(cells[k]);
                if (cell == null) continue;

                if (cell.IndexOf("symbol", StringComparison.InvariantCultureIgnoreCase) != -1)
                    symbolNo = k;
                else if (cell.IndexOf("ticker", StringComparison.InvariantCultureIgnoreCase) != -1)
                    symbolNo = k;
                else if (cell.IndexOf("security", StringComparison.InvariantCultureIgnoreCase) != -1)
                    nameNo = k;
                else if (cell.IndexOf("company", StringComparison.InvariantCultureIgnoreCase) != -1)
                    nameNo = k;
                if (cell.IndexOf("sector", StringComparison.InvariantCultureIgnoreCase) != -1)
                    sectorNo = k;
                if (cell.IndexOf("industry", StringComparison.InvariantCultureIgnoreCase) != -1)
                    industryNo = k;
            }

            for (var k = 1; k < rows.Length; k++)
            {
                cells = rows[k].Split(new[] { "</td>" }, StringSplitOptions.None);
                if (cells.Length > 1)
                {
                    var symbol = GetCellValue(cells[symbolNo]);
                    var name = GetCellValue(cells[nameNo]);
                    var sector = sectorNo == -1 ? null : GetCellValue(cells[sectorNo]);
                    var industry = industryNo == -1 ? null : GetCellValue(cells[industryNo]);
                    var item = new Models.IndexDbItem
                    { Index = indexName, Symbol = symbol, Name = name, Sector = sector, Industry = industry, TimeStamp = timeStamp };
                    if (!items.ContainsKey(symbol))
                        items.Add(symbol, item);
                }
            }
        }

        private static void ParseList(string indexName, DateTime timeStamp, string list, Dictionary<string, Models.IndexDbItem> items)
        {
            var rows = list.Split(new[] { "</li>" }, StringSplitOptions.None);
            for (var k = 0; k < rows.Length; k++)
            {
                var s = GetCellValue(rows[k])?.Trim();
                if (s == null) continue;

                if (s.EndsWith(")"))
                {
                    var i1 = s.LastIndexOf("(", StringComparison.InvariantCultureIgnoreCase);
                    var symbol = s.Substring(i1 + 1, s.Length - i1 - 2).Trim();
                    var name = s.Substring(0, i1).Trim();
                    var item = new Models.IndexDbItem { Index = indexName, Symbol = symbol, Name = name, TimeStamp = timeStamp };
                    if (!items.ContainsKey(symbol))
                        items.Add(symbol, item);
                }
                else
                    throw new Exception("Check parser");
            }
        }

        private static void ParseLinks(string indexName, DateTime timeStamp, string list, Dictionary<string, Models.IndexDbItem> items)
        {
//            var rows = list.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var rows = list.Split(new[] { "\n"}, StringSplitOptions.RemoveEmptyEntries);
            for (var k = 0; k < rows.Length; k++)
            {
                var s = GetCellValue(rows[k])?.Trim();
                if (s == null) continue;

                if (s.EndsWith(")"))
                {
                    var i1 = s.LastIndexOf("(", StringComparison.InvariantCultureIgnoreCase);
                    var symbol = s.Substring(i1 + 1, s.Length - i1 - 2).Trim();
                    var name = s.Substring(0, i1).Trim();
                    var item = new Models.IndexDbItem { Index = indexName, Symbol = symbol, Name = name, TimeStamp = timeStamp };
                    if (!items.ContainsKey(symbol))
                        items.Add(symbol, item);
                }
                else if (s== "External Links section.''") { }
                else
                    throw new Exception("Check parser");
            }
        }

        private static string GetCellValue(string cell)
        {
            var c1 = cell.Replace("</a>", "").Trim();
            if (c1.EndsWith("</sup>"))
            {
                var i2 = c1.LastIndexOf("<sup", StringComparison.InvariantCultureIgnoreCase);
                c1 = c1.Substring(0, i2);
            }
            var i1 = c1.LastIndexOf(">", StringComparison.InvariantCultureIgnoreCase);
            var ss = c1.Split('>');
            if (ss[ss.Length - 1].EndsWith(")") && ss[ss.Length - 1].IndexOf('(') == -1)
            {
                var i2 = ss[ss.Length - 2].IndexOf('(');
                var val = ss[ss.Length - 2].Substring(0, i2+1) + ss[ss.Length - 1];
                return val;
            }
            var value = c1.Substring(i1 + 1);
            if (string.IsNullOrWhiteSpace(value))
                value = null;
            return System.Net.WebUtility.HtmlDecode(value);
        }

        #endregion

    }
}
