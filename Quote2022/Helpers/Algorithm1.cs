using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Quote2022.Models;

namespace Quote2022.Helpers
{
    public class Algorithm1
    {
        private static Random random = new Random();

        private static Func<DayEoddataExtended, float>[] fGroups1 = {a => a.VlmToWAvg};

        private static Func<DayEoddataExtended, float>[] fGroups =
        {
            a => a.DJI_ToWAvg, a => a.GSPC_ToWAvg, a => a.VlmToWAvg, a => a.MaxPVlmToWAvg, a => a.CloseToWAvg,
            a => a.OpenToClose, a => a.CL, a => a.WAvgVLM, a => a.WAvgVolatility,
            a => (a.High - a.Low) / a.CL * 100.0F / a.WAvgVolatility, a => a.Changed, a => a.DayChanged, a => a.TopUp,
            a => a.TopDown, a => a.BreakUp, a => a.BreakDown, a => a.WAvgVolatility2
        };

        private static string[] sGroups =
        {
            "DJI_ToWAvg", "GSPC_ToWAvg", "VlmToWAvg", "MaxPVlmToWAvg", "CloseToWAvg", "OpenToClose", "Close", "WAvgVLM",
            "WAvgVolat", "(a.High-a.Low)/a.CL*100.0/a.WAvgVolat", "Changed", "DayChanged", "TopUp", "TopDown",
            "BreakUp", "BreakDown", "WAvgVolat2"
        };

        private static decimal[] stops =
        {
            10000M, 0.01M, 0.02M, 0.05M, 0.1M, 0.2M, 0.5M, 1M
        };

        public static void Execute(IEnumerable<string> dataSets, Action<string> showStatusAction)
        {
            // MaxLossCountExecute();
            // return;

            var data = GetData(dataSets, showStatusAction);

            PrintGeneral(data);
            // PrintStops(data);
            // PrintLevel1(data, 24);

            // var mondayDetails = new QuoteGroup(data.Where(o=>o.Date.DayOfWeek == DayOfWeek.Monday).ToList(), 0.01M, false, true);
        }

        private static void MaxLossCountExecute()
        {
            RunTest(500000);
            RunTest(200000);
            RunTest(100000);
            RunTest(50000);
            RunTest(20000);
            RunTest(10000);
            RunTest(5000);
            RunTest(2000);
            RunTest(1000);
            RunTest(5000);
            RunTest(2000);
            RunTest(1000);
            RunTest(500);
            RunTest(200);
            RunTest(100);

            void RunTest(int numberCount)
            {
                var start = 0.0;
                Debug.Print($"\n");
                for (var k = 0; k < 10; k++)
                {
                    TestMaxLossCount(numberCount, start, start + 0.04);
                    start += 0.04;
                }
            }

            void TestMaxLossCount(int numberCount, double minRange, double maxRange)
            {
                var numbers = GetRandoms(numberCount);
                var success = 0;
                var currCount = 0;
                var maxLossCount = 0;
                var kMax = 0;
                for (var k = 0; k < numbers.Length; k++)
                {
                    // Debug.Print($"{k}\t{numbers[k]}");
                    if (numbers[k] >= minRange && numbers[k] < maxRange)
                    {
                        success++;
                        if (maxLossCount < currCount)
                        {
                            maxLossCount = currCount;
                            kMax = k;
                        }
                        currCount = 0;
                    }
                    else
                    {
                        currCount++;
                    }
                }

                // Debug.Print($"{kMax}");
                Debug.Print($"{numberCount}\t{maxLossCount}\t{Math.Round(1.0 * success / numberCount * 100, 2)}");
            }
        }

        private static void PrintLevel1(List<DayEoddataExtended> data, int chunkSize)
            {
            Debug.Print($"GroupName\tGn\tMin\tMax\t{QuoteGroup.GetHeader2()}");
            for (var k = 0; k < fGroups.Length; k++)
            {
                var gData = Chunk(data.OrderBy(fGroups[k]), (data.Count + chunkSize - 1) / chunkSize);
                // Debug.Print($"\n{sGroups[k]}");
                // Debug.Print($"G{k}\tMin\tMax\t{QuoteGroup.GetHeader2()}");
                var cnt = 0;
                foreach (var d in gData)
                {
                    var min = d.Min(fGroups[k]);
                    var max = d.Max(fGroups[k]);
                    var qg = new QuoteGroup(d.ToList());
                    // Debug.Print($"{cnt}\t{min}\t{max}\t{qg.GetContent2()}");
                    Debug.Print($"{sGroups[k]}\t{cnt}\t{min}\t{max}\t{qg.GetContent2()}");
                    cnt++;
                }
            }
        }

        private static void PrintGeneral(List<DayEoddataExtended> data)
        {
            // General total
            var total = new QuoteGroup(data);
            // Debug.Print($"\n**All records of data set**\n\t{QuoteGroup.GetHeader1()}");
            Debug.Print($"\nALL records\t{QuoteGroup.GetHeader1()}");
            Debug.Print("\t" + total.GetContent1());

            // Group by day of week
            var aa1 = data.GroupBy(a => a.Date.DayOfWeek).OrderBy(a => (int)a.Key);
            var aa2 = aa1.ToDictionary(a => a.Key, a => new QuoteGroup(a.ToList()));
            Debug.Print($"\n**Group by Day of Week**\nDay\t{QuoteGroup.GetHeader1()}");
            foreach (var kvp in aa2)
                Debug.Print($"{kvp.Key}\t{kvp.Value.GetContent1()}");

            // Group by sectors
            var aa11 = data.GroupBy(a => a.Sector ?? "").OrderBy(a => a.Key);
            var aa21 = aa11.ToDictionary(a => a.Key, a => new QuoteGroup(a.ToList()));
            Debug.Print($"\n**Group by Sectors**\nSector\t{QuoteGroup.GetHeader1()}");
            foreach (var kvp in aa21)
                Debug.Print($"{kvp.Key}\t{kvp.Value.GetContent1()}");

            /*// Group by industries
            aa11 = data.GroupBy(a => a.Industry ?? "").OrderBy(a => a.Key);
            aa21 = aa11.ToDictionary(a => a.Key, a => new QuoteGroup(a.ToList()));
            Debug.Print($"\n**Group by Industries**\nIndustry\t{QuoteGroup.GetHeader1()}");
            foreach (var kvp in aa21)
                Debug.Print($"{kvp.Key}\t{kvp.Value.GetContent1()}");*/

            // Group by sector&industries
            aa11 = data.GroupBy(a => (a.Sector ?? "") + "/" + (a.Industry ?? "")).OrderBy(a => a.Key);
            aa21 = aa11.ToDictionary(a => a.Key, a => new QuoteGroup(a.ToList()));
            Debug.Print($"\n**Group by Sector&Industries**\nSector/Industry\t{QuoteGroup.GetHeader1()}");
            foreach (var kvp in aa21)
                Debug.Print($"{kvp.Key}\t{kvp.Value.GetContent1()}");
        }

        private static double[] GetRandoms(int count)
        {
            var numbers = new double[count];
            for (var k = 0; k < count; k++)
                numbers[k] = random.NextDouble();
            return numbers;
        }

        private static void PrintStops(List<DayEoddataExtended> data)
            {
            Debug.Print($"\n**All records of data set**\n\tStop\t{QuoteGroup.GetHeader1()}");
            foreach (var stop in stops)
            {
                var dataStop = new QuoteGroup(data, stop, false);
                Debug.Print($"\t{stop}$\t{dataStop.GetContent1()}");
            }
            foreach (var stop in stops)
            {
                var dataStop = new QuoteGroup(data, stop * 10, true);
                Debug.Print($"\t{stop * 10}%\t{dataStop.GetContent1()}");
            }
            // Group by day of week
            Debug.Print($"\n**Group by Day of Week**\nStop\tDay\t{QuoteGroup.GetHeader1()}");
            var aa1 = data.GroupBy(a => a.Date.DayOfWeek).OrderBy(a => (int)a.Key);
            foreach (var dataByDay in aa1)
            {
                var day = dataByDay.First().Date.DayOfWeek;
                foreach (var stop in stops)
                {
                    var dataStop = new QuoteGroup(dataByDay.ToList(), stop, false, false);
                    Debug.Print($"{stop}$\t{day}\t{dataStop.GetContent1()}");
                }
                foreach (var stop in stops)
                {
                    var dataStop = new QuoteGroup(dataByDay.ToList(), stop * 10, true, false);
                    Debug.Print($"{stop * 10}%\t{day}\t{dataStop.GetContent1()}");
                }
            }
        }

        private static List<DayEoddataExtended> GetData(IEnumerable<string> dataSets, Action<string> showStatusAction)
        {
            var sectors = new Dictionary<string, string>();
            var industries = new Dictionary<string, string>();
            var data = new Dictionary<string, DayEoddataExtended>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 150;
                    cmd.CommandText = "SELECT * from vScreenerNasdaq_Sector";
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                            sectors.Add((string)rdr["Symbol"], (string)rdr["Sector"]);

                    cmd.CommandText = "SELECT * from vScreenerNasdaq_Industry";
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                            industries.Add((string)rdr["Symbol"], (string)rdr["Industry"]);

                    foreach (var dataSet in dataSets)
                    {
                        switch (dataSet)
                        {
                            case "2013":
                                cmd.CommandText = "SELECT * from vDayEoddata2013Extended";
                                break;
                            case "2022":
                                cmd.CommandText = "SELECT * from vDayEoddataExtended";
                                break;
                            default:
                                throw new Exception($"{dataSet} is not valid data set");
                        }

                        if (!string.IsNullOrEmpty(cmd.CommandText))
                        {
                            showStatusAction($"Reading data from {dataSet} data set ...");
                            using (var rdr = cmd.ExecuteReader())
                                while (rdr.Read())
                                {
                                    var o = new DayEoddataExtended(rdr);
                                    if (sectors.ContainsKey(o.Symbol))
                                        o.Sector = sectors[o.Symbol];
                                    if (industries.ContainsKey(o.Symbol))
                                        o.Industry = industries[o.Symbol];

                                    if (!data.ContainsKey(o.Key))
                                        data.Add(o.Key, o);
                                }
                        }
                    }
                }
            }

            var list = data.Values.OrderBy(a=>a.Date).ThenBy(a=>a.Symbol).ToList();
            var date = DateTime.MinValue;
            var cnt = 0;
            foreach (var o in list.OrderBy(o => o.Date).ThenByDescending(o => o.Changed))
            {
                if (date != o.Date)
                {
                    date = o.Date;
                    cnt = 0;
                }
                o.TopUp = cnt++;
            }

            date = DateTime.MinValue;
            foreach (var o in list.OrderBy(o => o.Date).ThenBy(o => o.Changed))
            {
                if (date != o.Date)
                {
                    date = o.Date;
                    cnt = 0;
                }
                o.TopDown = cnt++;
            }

            showStatusAction($"End reading data. Records: {data.Count}");
            return list;
        }

        public static Decimal BuyAbsPrice(decimal stopDelta, float open, float high, float low, float close)
        {
            var dOpen = Convert.ToDecimal(open);
            return (dOpen - Convert.ToDecimal(low)) >= stopDelta ? dOpen - stopDelta : Convert.ToDecimal(close);
        }

        public static decimal BuyPercPrice(decimal stopPercent, float lastClose, float open, float high, float low, float close)
        {
            var dOpen = Convert.ToDecimal(open);
            var stopDelta = GetStopDeltaForPercent(stopPercent, lastClose);
            return (dOpen - Convert.ToDecimal(low)) >= stopDelta ? dOpen - stopDelta : Convert.ToDecimal(close);
        }

        public static Decimal SellAbsPrice(decimal stopDelta, float open, float high, float low, float close)
        {
            var dOpen = Convert.ToDecimal(open);
            return (Convert.ToDecimal(high) - dOpen) >= stopDelta ? dOpen + stopDelta : Convert.ToDecimal(close);
        }

        public static decimal SellPercPrice(decimal stopPercent, float lastClose, float open, float high, float low, float close)
        {
            var dOpen = Convert.ToDecimal(open);
            var stopDelta = GetStopDeltaForPercent(stopPercent, lastClose);
            return (Convert.ToDecimal(high) - dOpen) >= stopDelta ? dOpen + stopDelta : Convert.ToDecimal(close);
        }

        //==============================
        public static decimal BuyAbs(decimal stopDelta, float open, float high, float low, float close)
        {
            var dOpen = Convert.ToDecimal(open);
            return ((dOpen - Convert.ToDecimal(low)) >= stopDelta ? dOpen - stopDelta : Convert.ToDecimal(close)) / dOpen;
        }

        public static decimal BuyPerc(decimal stopPercent, float lastClose, float open, float high, float low, float close)
        {
            var dOpen = Convert.ToDecimal(open);
            var stopDelta = GetStopDeltaForPercent(stopPercent, lastClose);
            return ((dOpen - Convert.ToDecimal(low)) >= stopDelta ? dOpen - stopDelta : Convert.ToDecimal(close)) / dOpen;
        }

        public static decimal SellAbs(decimal stopDelta, float open, float high, float low, float close)
        {
            var dOpen = Convert.ToDecimal(open);
            return dOpen / ((Convert.ToDecimal(high) - dOpen) >= stopDelta ? dOpen + stopDelta : Convert.ToDecimal(close));
        }
        public static decimal SellPerc(decimal stopPercent, float lastClose, float open, float high, float low, float close)
        {
            var dOpen = Convert.ToDecimal(open);
            var stopDelta = GetStopDeltaForPercent(stopPercent, lastClose);
            return dOpen/((Convert.ToDecimal(high) - dOpen) >= stopDelta ? dOpen + stopDelta : Convert.ToDecimal(close));
        }

        public static decimal GetStopDeltaForPercent(decimal stopPercent, float lastClose)
        {
            var dLastClose = Convert.ToDecimal(lastClose);
            var stopDelta = Math.Round(dLastClose * stopPercent / 100.0M, 2);
            if (stopDelta < 0.01M)
                stopDelta = 0.01M;
            return stopDelta;
        }

        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize).ToList();
                source = source.Skip(chunksize);
            }
        }

        public static int GetChunkSize(int dataCount, int chunkCount) => (dataCount + chunkCount - 1) / chunkCount;
    }
}
