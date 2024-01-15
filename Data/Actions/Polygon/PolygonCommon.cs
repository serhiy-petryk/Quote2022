using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.Polygon
{
    public static class PolygonCommon
    {
        private static readonly Dictionary<string, object> TestSymbols = new Dictionary<string, object>
        {
            {"BVNSC", null}, {"CBO", null}, {"CBO^A", null}, {"CBOpA", null}, {"CBX", null}, {"CGVIC", null},
            {"CIVEC", null}, {"CRUSC", null}, {"EVFTC", null}, {"FOANC", null}, {"GRBIC", null}, {"HFGIC", null},
            {"IBO", null}, {"IGZ", null}, {"MOGLC", null}, {"OKDCC", null}, {"PETZC", null}, {"RPIBC", null},
            {"ZAZZT", null}, {"ZBZX", null}, {"ZBZZT", null}, {"ZCZZT", null}, {"ZEXIT", null}, {"ZIEXT", null},
            {"ZJZZT", null}, {"ZTST", null}, {"ZVV", null}, {"ZVZZC", null}, {"ZVZZT", null}, {"ZWZZT", null},
            {"ZXIET", null}, {"ZXYZ.A", null}, {"ZXZZT", null}
        };

        public static string GetApiKey() => CsUtils.GetApiKeys("polygon.io")[1];
        public static string GetApiKey2003() => CsUtils.GetApiKeys("polygon.io.2003")[0];

        public static bool IsTestTicker(string ticker)
        {
            if (TestSymbols.ContainsKey(ticker)) return true;

            var test = ticker.Substring(1);
            return (test == "TEST" || test.StartsWith("TEST.") || test.StartsWith("TEST^") ||
                    test.StartsWith("TESTr") || test.StartsWith("TESTw") || test.StartsWith("TESTp"));
        }

        public static string GetMyTicker(string polygonTicker)
        {
            if (polygonTicker.EndsWith("pw"))
                polygonTicker = polygonTicker.Replace("pw", "^^W");
            else if (polygonTicker.EndsWith("pAw"))
                polygonTicker = polygonTicker.Replace("pAw", "^^AW");
            else if (polygonTicker.EndsWith("pEw"))
                polygonTicker = polygonTicker.Replace("pEw", "^^EW");
            else if (polygonTicker.Contains("p"))
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
            if (myTicker.EndsWith("^^W"))
                myTicker = myTicker.Replace("^^W", "pw");
            else if (myTicker.EndsWith("^^AW"))
                myTicker = myTicker.Replace("^^AW", "pAw");
            else if (myTicker.EndsWith("^^EW"))
                myTicker = myTicker.Replace("^^EW", "pEw");
            else if (myTicker.Contains("^"))
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

            public override string ToString()
            {
                return $"{DateTime:yyyy-MM-dd HH:mm},{ToString(Open)},{ToString(High)},{ToString(Low)},{ToString(Close)},{Volume},{ToString(WeightedVolume)},{TradeCount}";

                string ToString(float f) => f.ToString(CultureInfo.InvariantCulture);
            }
        }
        #endregion
    }
}
