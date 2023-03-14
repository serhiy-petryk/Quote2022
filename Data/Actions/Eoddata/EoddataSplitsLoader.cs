using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using Data.Helpers;
using Data.Models;

namespace Data.Actions.Eoddata
{
    public class EoddataSplitsLoader
    {
        private const string URL = @"https://eoddata.com/splits.aspx";

        public static void Start(Action<string> logEvent)
        {
            logEvent($"EoddataSplitsLoader started");

            var timeStamp = CsUtils.GetTimeStamp();
            var htmlFileName = $@"E:\Quote\WebData\Splits\Eoddata\EoddataSplits_{timeStamp.Item2}.html";

            // Download data to html file
            Helpers.Download.DownloadPage(URL, htmlFileName);

            // Split and save to dabase
            var zipFileName= ParseAndSaveToDb(htmlFileName);

            logEvent($"EoddataSplitsLoader finished. Filename: {zipFileName}");
        }

        private static string ParseAndSaveToDb(string htmlFileName)
        {
            var timeStamp = File.GetLastWriteTime(htmlFileName);
            var items = new List<SplitModel>();
            var fileLines = new List<string>{ "Exchange\tSymbol\tDate\tRatio" };

            var content = File.ReadAllText(htmlFileName);
            var i1 = content.IndexOf("<th>Ratio</th>", StringComparison.InvariantCulture);
            var i2 = content.IndexOf("</table>", i1+14, StringComparison.InvariantCulture);
            var rows = content.Substring(i1 + 14,i2 - i1 - 14).Trim().Split(new [] {"</tr>"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var row in rows)
            {
                var cells = row.Trim().Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                var exchange = GetCellValue(cells[0]);
                var symbol = GetCellValue(cells[1]);
                var sDate = GetCellValue(cells[2]);
                var date = DateTime.ParseExact(sDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                var ratio = GetCellValue(cells[3]);
                fileLines.Add($"{exchange}\t{symbol}\t{sDate}\t{ratio}");
                ratio = ratio.Replace('-', ':');
                var item = new SplitModel(exchange, symbol, date, ratio, timeStamp);
                items.Add(item);
            }

            // Save data to text file
            var txtFileName = Path.ChangeExtension(htmlFileName, ".txt");
            if (File.Exists(txtFileName))
                File.Delete(txtFileName);

            File.WriteAllLines(txtFileName, fileLines);

            // Zip data
            var zipFileName = Helpers.CsUtils.ZipFile(txtFileName);

            // Remove unnecessary files
            File.Delete(htmlFileName);
            File.Delete(txtFileName);

            // Save data to database
            Helpers.DbUtils.ClearAndSaveToDbTable(items.Where(a => a.Date <= a.TimeStamp), "Bfr_SplitEoddata",
                "Exchange", "Symbol", "Date", "Ratio", "K", "TimeStamp");
            Helpers.DbUtils.ExecuteSql("INSERT INTO SplitEoddata (Exchange,Symbol,[Date],Ratio,K,[TimeStamp]) " +
                                       "SELECT a.Exchange, a.Symbol, a.[Date], a.Ratio, a.K, a.[TimeStamp] FROM Bfr_SplitEoddata a " +
                                       "LEFT JOIN SplitEoddata b ON a.Exchange=b.Exchange AND a.Symbol = b.Symbol AND a.Date = b.Date " +
                                       "WHERE b.Symbol IS NULL");
            return zipFileName;

            string GetCellValue(string cell)
            {
                var s = cell.Replace("</a>", "").Trim();
                i1 = s.LastIndexOf('>');
                s = System.Net.WebUtility.HtmlDecode(s.Substring(i1 + 1)).Trim();
                return s;

            }
        }
    }
}
