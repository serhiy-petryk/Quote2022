using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Quote2022.Helpers;
using Quote2022.Models;

namespace Quote2022.Actions.TradingView
{
    public static class Recommend_Parse
    {
        public static void Parse(string zipFile, Action<string> showStatusAction)
        {
            showStatusAction($"Recommend_Parse. Started.");

            using (var zip = new ZipReader(zipFile))
                foreach (var file in zip.Where(a => a.Length > 0))
                {
                    showStatusAction($"Recommend_Parse '{file.FileNameWithoutExtension}' file parsing & save to database started.");
                    var ss = file.FileNameWithoutExtension.Split('_');
                    var timeStamp = DateTime.ParseExact(ss[ss.Length - 1].Trim(), "yyyyMMdd", CultureInfo.InvariantCulture);

                    var o = JsonConvert.DeserializeObject<Data.Models.ScreenerTradingView>(file.Content);
                    var items = o.data.Select(a => a.GetDbItem(timeStamp)).ToArray();

                    if (items.Length > 0)
                    {
                        SaveToDb.ExecuteSql($"DELETE from dbQuote2023..ScreenerTvRecommend WHERE [TimeStamp]='{timeStamp:yyyy-MM-dd}'");
                        SaveToDb.SaveToDbTable(items, "dbQuote2023..ScreenerTvRecommend", "Symbol", "Exchange",
                            "TimeStamp", "Recommend");
                    }
                }

            showStatusAction($"Recommend_Parse FINISHED");

        }
    }
}
