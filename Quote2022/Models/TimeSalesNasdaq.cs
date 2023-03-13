using System;
using System.Globalization;
using System.Linq;

namespace Quote2022.Models
{
    public class TimeSalesNasdaq
    {
        public static string[] UrlTimes = new string[]
        {
            "09:30", "10:00", "10:30", "11:00", "11:30", "12:00", "12:30", "13:00", "13:30", "14:00", "14:30",
            "15:00", "15:30"
        };

        public static bool? IsEqual(Trade[] oo1, Trade[] oo2)
        {
            if (oo1 == null || oo2 == null) return null;
            if (oo1.Length != oo2.Length) return false;
            for (var k = 0; k < oo1.Length; k++)
            {
                if (!oo1[k].IsEqual(oo2[k]))
                    return false;
            }

            return true;
        }

        //========================================
        public Data data;
        public Status status;

        public bool ShouldBeReload
        {
            get
            {
                if (status.bCodeMessage != null)
                    foreach (var eroor in status.bCodeMessage.Where(a => a.code != 1001))
                        return true;
                return false;
            }
        }

        public TopTableRow DaySummaryInfo => data?.topTable.rows[0];
        //            public override string ToString() => $"{nlsVolume}^{previousClose}^{todayHighLow}^{fiftyTwoWeekHighLow}";
        public string GetSummarryDifference(TopTableRow o)
        {
            if (data == null) return null;

            if (!string.Equals(data.topTable.rows[0].todayHighLow, o.todayHighLow))
                return "DifferentTodayHighLow";
            if (!string.Equals(data.topTable.rows[0].previousClose, o.previousClose))
                return "DifferentPreviousClose";
            if (!string.Equals(data.topTable.rows[0].nlsVolume, o.nlsVolume))
                return "DifferentVolume";
            if (!string.Equals(data.topTable.rows[0].fiftyTwoWeekHighLow, o.fiftyTwoWeekHighLow))
                return "Different52WeekHighLow";
            return null;
        }


        public Trade[] GetTrades()
        {
            if (data == null) return null;

            var trades = new Trade[data.rows.Length];
            for (var k = 0; k < trades.Length; k++)
                trades[k]=new Trade(data.rows[k]);
            return trades;
        }

        public class Trade
        {
            public TimeSpan Time;
            public float Price;
            public int Volume;

            private static CultureInfo usCulture = new CultureInfo("en-US");
            public Trade(DataRow row)
            {
                Time = TimeSpan.Parse(row.nlsTime, CultureInfo.InvariantCulture);
                Price = float.Parse(row.nlsPrice, NumberStyles.Any, usCulture);
                Volume = int.Parse(row.nlsShareVolume, NumberStyles.Any, usCulture);
            }

            public bool IsEqual(Trade o)
            {
                return TimeSpan.Equals(Time, o.Time) && float.Equals(Price, o.Price) && int.Equals(Volume, o.Volume);
            }

            public override string ToString() => Time.ToString("hh\\:mm\\:ss") + " " + Price.ToString(usCulture) + " " + Volume.ToString();
        }

        public class Data
        {
            public string symbol;
            public int totalRecords;//	:	185
            public int offset;//	:	0
            public int limit;//	:	200000
            public DataRow headers;
            public DataRow[] rows;
            public TopTable topTable;
            public Description description;
            public string[] message;
        }

        public class DataRow
        {
            public string nlsTime;//	:	09:59:59
            public string nlsPrice;//	:	$ 148.66
            public string nlsShareVolume;//	:	100

        }
        public class TopTable
        {
            public TopTableRow headers;
            public TopTableRow[] rows;
        }

        public class TopTableRow
        {
            public string nlsVolume; //	:	0
            public string previousClose; //	:	$149.01
            // public decimal previousClose; //	:	$149.01
            public string todayHighLow; //	:	N/A
            public string fiftyTwoWeekHighLow; //	:
            public override string ToString() => $"{nlsVolume}^{previousClose}^{todayHighLow}^{fiftyTwoWeekHighLow}";
        }
        public class Description
        {
            public string message;
            public string url;
        }
        public class Status
        {
            public int rCode;
            public CodeMessage[] bCodeMessage;
            public string developerMessage;
        }

        public class CodeMessage
        {
            public int code;
            public string errorMessage;
        }
    }

}
