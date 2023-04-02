using System;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.Polygon
{
    public static class PolygonCommon
    {
        public static string GetApiKey() => CsUtils.GetApiKeys("polygon.io")[1];

        public static bool IsTestTicker(string ticker)
        {
            var test = ticker.Substring(1);
            return (test == "TEST" || test.StartsWith("TEST.") || test.StartsWith("TEST^") ||
                    test.StartsWith("TESTr") || test.StartsWith("TESTw") || test.StartsWith("TESTp"));
        }

        public static string GetMyTicker(string polygonTicker)
        {
            if (polygonTicker.Contains("p"))
                polygonTicker = polygonTicker.Replace("p", "^");
            else if (polygonTicker.Contains("rw"))
                polygonTicker = polygonTicker.Replace("rw", ".RTW");
            else if (polygonTicker.Contains("r"))
                polygonTicker = polygonTicker.Replace("r", ".RT");
            else if (polygonTicker.Contains("w"))
                polygonTicker = polygonTicker.Replace("w", ".WI");

            if (polygonTicker.Any(char.IsLower))
                throw new Exception($"Check PolygonCommon.GetMyTicker method for '{polygonTicker}' ticker");

            return polygonTicker;
        }

        public static string GetPolygonTicker(string myTicker)
        {
            if (myTicker.Contains("^"))
                myTicker = myTicker.Replace("^", "p");
            else if (myTicker.Contains(".RTW"))
                myTicker = myTicker.Replace(".RTW", "rw");
            else if (myTicker.Contains(".RT"))
                myTicker = myTicker.Replace(".RT", "r");
            else if (myTicker.Contains(".WI"))
                myTicker = myTicker.Replace(".WI", "w");

            return myTicker;
        }

        #region ========  Json classes  ===========
        public class cMinuteRoot
        {
            public string ticker;
            public int queryCount;
            public int resultsCount;
            public int count;
            public bool adjusted;
            public string status;
            public string next_url;
            public string request_id;
            public cMinuteItem[] results;
            public string Symbol => PolygonCommon.GetMyTicker(ticker);
        }
        public class cMinuteItem
        {
            public long t;
            public float o;
            public float h;
            public float l;
            public float c;
            public long v;
            public float vw;
            public int n;

            public DateTime DateTime => CsUtils.GetEstDateTimeFromUnixSeconds(t / 1000);
            public float Open => o;
            public float High => h;
            public float Low => l;
            public float Close => c;
            public long Volume => v;
            public float WeightedVolume => vw;
            public int TradeCount => n;
        }
        #endregion
    }
}
