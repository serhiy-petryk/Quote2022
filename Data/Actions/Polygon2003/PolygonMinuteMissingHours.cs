using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Helpers;
using Newtonsoft.Json;

namespace Data.Actions.Polygon2003
{
    public class PolygonMinuteMissingHours
    {
        private const string TextFile = @"E:\Quote\WebData\Minute\Polygon2003\DataBuffer\Corrections_20201127.txt";
        private const string DestinartionDataFolder = @"E:\Quote\WebData\Minute\Polygon2003\DataBuffer\";
        private const string SourceDataFolder = @"E:\Quote\WebData\Minute\Polygon\DataBuffer\";

        public static void Start()
        {
            Logger.AddMessage($"PolygonMinuteMissingHours Started");

            var from = new DateTime(2020, 11, 27, 13, 0, 0);
            var to = new DateTime(2020, 11, 27, 16, 0, 0);
            var result = new Dictionary<string, Polygon.PolygonCommon.cMinuteItem[]>();
            // var zipFileName = DestinartionDataFolder + "MP2003_20201205.zip";
            var zipFileName = SourceDataFolder + "Minute5Years_20230401.zip";
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
            {
                var symbols = zip.Entries.Where(a => !string.IsNullOrEmpty(a.Name)).Select(a => a.Name.Split('_')[1])
                    .Distinct().ToArray();

                var cnt = 0;
                foreach (var symbol in symbols)
                {
                    if (cnt % 10 == 0)
                        Logger.AddMessage($"PolygonMinuteMissingHours: processed {cnt:N0} of {symbols.Length:N0} items");

                    cnt++;

                    var entries = zip.Entries.Where(a => a.Name.Contains("_" + symbol + "_", StringComparison.InvariantCultureIgnoreCase)).OrderBy(a => a.Name).ToArray();
                    ZipArchiveEntry lastEntry = null;
                    foreach (var entry in entries)
                    {
                        var date = DateTime.ParseExact(Path.GetFileNameWithoutExtension(entry.Name.Split('_')[2]), "yyyyMMdd", CultureInfo.InvariantCulture);
                        if (date >= from.Date)
                        {
                            if (lastEntry != null)
                            {
                                var oo = JsonConvert.DeserializeObject<Polygon.PolygonCommon.cMinuteRoot>(lastEntry.GetContentOfZipEntry());
                                if (oo.resultsCount > 0)
                                {
                                    var items = oo.results.Where(a => a.DateTime >= from && a.DateTime < to).ToArray();
                                    if (items.Length > 0) result.Add(oo.Symbol, items);
                                }
                            }

                            break;
                        }
                        lastEntry = entry;
                    }
                }
            }

            // File.WriteAllText(TextFile, );
            var sb = new StringBuilder();
            sb.AppendLine($"Symbol,DateTime,Open,High,Low,Close,Volume,WeightedVolume,TradeCount");
            foreach (var kv in result)
            foreach (var item in kv.Value)
            {
                sb.AppendLine($"{kv.Key},{item}");
            }
            File.WriteAllText(TextFile, sb.ToString());
        }

        public static void StartDestination()
        {
            var from = new DateTime(2020, 11, 27, 13, 0, 0);
            var to = new DateTime(2020, 11, 27, 16, 0, 0);
            var result = new Dictionary<string, int>();
            var zipFileName = DestinartionDataFolder + "MP2003_20201205.zip";
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var oo = JsonConvert.DeserializeObject<Polygon.PolygonCommon.cMinuteRoot>(entry.GetContentOfZipEntry());
                    //var items1 = oo.results.Where(a => a.DateTime == from).ToArray();
                    //if (items1.Length == 1)
                    //  Debug.Print($"Symbol: {oo.Symbol}. TradeCount: {items1[0].TradeCount}");

                    if (oo.resultsCount > 0)
                    {
                        var items = oo.results.Where(a => a.DateTime > from && a.DateTime < to).ToArray();
                        result.Add(oo.Symbol, items.Length);
                    }

                }

        }
    }
}
