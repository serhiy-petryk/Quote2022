using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Data.Helpers;
using Data.Models;
using Newtonsoft.Json;

namespace Data.Actions.Investing
{
    public class InvestingSplitsLoader
    {
        private const string URL = @"https://www.investing.com/stock-split-calendar/Service/getCalendarFilteredData";
        private const string POST_DATA_TEMPLATE = @"country%5B%5D=5&dateFrom={0}&dateTo={1}&currentTab=custom&limit_from=0";

        public static void Start(Action<string> logEvent)
        {
            logEvent($"InvestingSplitsLoader started");

            var timeStamp = CsUtils.GetTimeStamp();
            var postData = string.Format(POST_DATA_TEMPLATE,
                timeStamp.Item1.AddDays(-30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                timeStamp.Item1.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            var jsonFileName = $@"E:\Quote\WebData\Splits\Investing\InvestingSplits_{timeStamp.Item2}.json";

            // Download data to html file
            Helpers.Download.DownloadPage_POST(URL, jsonFileName, postData, true);

            // Split and save to dabase
            var items = new List<SplitModel>();
            ParseAndSaveToDb(jsonFileName, items);

            // Zip data
            var zipFileName = Helpers.CsUtils.ZipFile(jsonFileName);
            File.Delete(jsonFileName);

            logEvent($"InvestingSplitsLoader finished. Filename: {zipFileName} with {items.Count} items");
        }

        private static void ParseAndSaveToDb(string jsonFileName, List<SplitModel> items)
        {
            var timeStamp = File.GetLastWriteTime(jsonFileName);

            var o = JsonConvert.DeserializeObject<cRoot>(File.ReadAllText(jsonFileName));
            var rows = o.data.Trim().Split(new[] { "</tr>" }, StringSplitOptions.RemoveEmptyEntries);

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
                    throw new Exception("Check InvestingSplitsLoader parser");
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
            }
        }

        #region =======  Json subclasses  =============
        public class cRoot
        {
            public string data;
            public int rows_num;
            public long last_time_scope;
            public DateTime From => new DateTime(1970, 1, 1).AddSeconds(last_time_scope);
        }
        #endregion
    }
}
