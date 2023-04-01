using System;
using System.Linq;

namespace Data.Actions.Polygon
{
    public static class PolygonCommon
    {
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
    }
}
