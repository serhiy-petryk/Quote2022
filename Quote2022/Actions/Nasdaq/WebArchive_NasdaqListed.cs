using System;
using System.Collections.Generic;

namespace Quote2022.Actions.Nasdaq
{
    public static class WebArchive_NasdaqListed
    {
        #region =============  Download Html Data  ===============
        public static void DownloadHtmlData(Action<string> showStatusAction)
        {
            var url = @"http://www.nasdaqtrader.com/dynamic/symdir/nasdaqlisted.txt";
            showStatusAction($"WebArchive_NasdaqListed finished");
        }

        public class cYearList
        {
            public Dictionary<string, object> status;
            public string first_ts;
            public string last_ts;
        }
        public class cDayList
        {
            public int[][] items;
        }
        #endregion

    }
}
