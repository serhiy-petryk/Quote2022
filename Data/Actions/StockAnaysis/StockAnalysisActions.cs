using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Data.Helpers;
using Data.Models;
using Newtonsoft.Json;

namespace Data.Actions.StockAnaysis
{
    public class StockAnalysisActions
    {
        private const string URL = @"https://stockanalysis.com/actions/";
        private const string FOLDER = @"E:\Quote\WebData\Splits\StockAnalysis\Actions\";
        private const string POST_DATA_TEMPLATE = @"country%5B%5D=5&dateFrom={0}&dateTo={1}&currentTab=custom&limit_from=0";

        public static void Start(Action<string> logEvent)
        {
            logEvent($"StockAnalysisActions started");

            var timeStamp = CsUtils.GetTimeStamp();
            var htmlFileName = FOLDER + $@"StockAnalysisActions_{timeStamp.Item2}.html";

            // Download data to html file
            // Helpers.Download.DownloadPage(URL, htmlFileName);

            // Split and save to database
            // var items = new List<SplitModel>();
            ParseAndSaveToDb(htmlFileName);

            // Zip data
            var zipFileName = Helpers.CsUtils.ZipFile(htmlFileName);
            File.Delete(htmlFileName);

            // logEvent($"!StockAnalysisActions finished. Filename: {zipFileName} with {items.Count} items");*/
        }

        private static void ParseAndSaveToDb(string htmlFileName)
        {
            var timeStamp = File.GetLastWriteTime(htmlFileName);

            var content = File.ReadAllText(htmlFileName);
            var i1 = content.IndexOf("const data =", StringComparison.InvariantCulture);
            var i2 = content.IndexOf("}}]", i1 + 12, StringComparison.InvariantCulture);
            var s = content.Substring(i1 + 12, i2 - i1 - 12 + 3).Trim();
            var i12 = s.IndexOf("{\"type\":", StringComparison.InvariantCulture);
            i12 = s.IndexOf("{\"type\":", i12+8, StringComparison.InvariantCulture);
            var s2 = s.Substring(i12, s.Length - i12 - 1);

            var o = JsonConvert.DeserializeObject<cRoot>(s2);

            var f = 0;
            /*var rows = o.data.Trim().Split(new[] { "</tr>" }, StringSplitOptions.RemoveEmptyEntries);

            string lastDate = null;
            for (var k=0; k<rows.Length; k++) 
            {
                var row = rows[k];
                var cells = row.Trim().Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                var sDateOriginal = GetCellValue(cells[0]);
                var sDate = string.IsNullOrEmpty(sDateOriginal) ? lastDate : sDateOriginal;
                var date = DateTime.Parse(sDate, CultureInfo.InvariantCulture);

                cells[1] = System.Net.WebUtility.HtmlDecode(cells[1]).Trim();
                if (cells[1].EndsWith(")"))
                    cells[1] = cells[1].Substring(0, cells[1].Length - 1);
                else
                    throw new Exception("Check StockAnalysisActions parser");
                var symbol = GetCellValue(cells[1]);
                
                var i2 = cells[1].LastIndexOf("</span>", StringComparison.InvariantCulture);
                var i1 = cells[1].LastIndexOf(">", i2-7, StringComparison.InvariantCulture);
                var name = cells[1].Substring(i1 + 1, i2 - i1 - 1).Trim();

                var ratio = GetCellValue(cells[2]);

                var item = new SplitModel(symbol, date, name, ratio, null, timeStamp);
                items.Add(item);

                lastDate = sDate;
            }

            // Save data to database
            Helpers.DbUtils.ClearAndSaveToDbTable(items.Where(a => a.Date <= a.TimeStamp), "Bfr_SplitInvesting",
                "Symbol", "Date", "Name", "Ratio", "K", "TimeStamp");

            Helpers.DbUtils.ExecuteSql("INSERT INTO SplitInvesting (Symbol,[Date],Name,Ratio,K,[TimeStamp]) " +
                                       "SELECT a.Symbol, a.[Date], a.Name, a.Ratio, a.K, a.[TimeStamp] FROM Bfr_SplitInvesting a " +
                                       "LEFT JOIN SplitInvesting b ON a.Symbol = b.Symbol AND a.Date = b.Date " +
                                       "WHERE b.Symbol IS NULL");

            string GetCellValue(string cell)
            {
                var s = cell.Replace("</a>", "").Trim();
                var i1 = s.LastIndexOf('>');
                s = System.Net.WebUtility.HtmlDecode(s.Substring(i1 + 1)).Trim();
                return s;
            }*/
        }

        #region =======  Json subclasses  =============
        public class cRoot
        {
            public string type;
            public cData data;
        }
        public class cData
        {
            public string action;
            public string type;
            public object props;
            public cItem[] data;
            public int fullCount;
        }
        public class cItem
        {
            public string date;
            public string type;
            public string symbol;
            public string name;
            public string other;
            public string text;
        }
        #endregion
    }
}
