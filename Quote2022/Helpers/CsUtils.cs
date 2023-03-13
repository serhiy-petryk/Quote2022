using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Quote2022.Helpers
{
    public static class CsUtils
    {
        public static string OpenZipFileDialog(string folder) => OpenFileDialogGeneric(folder, @"zip files (*.zip)|*.zip");
        public static string OpenTxtFileDialog(string folder) => OpenFileDialogGeneric(folder, @"Text|*.txt");
        public static string OpenCsvFileDialog(string folder) => OpenFileDialogGeneric(folder, @"CSV Files (*.csv)|*.csv");
        public static string OpenFileDialogGeneric(string folder, string filter)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = folder;
                ofd.RestoreDirectory = true;
                ofd.Multiselect = false;
                ofd.Filter = filter;
                if (ofd.ShowDialog() == DialogResult.OK)
                    return ofd.FileName;
                return null;
            }
        }

        public static string[] OpenFileDialogMultiselect(string folder, string filter, string title = null)
        {
            using (var ofd = new OpenFileDialog())
            {
                if (!string.IsNullOrEmpty(title))
                    ofd.Title = title;
                ofd.InitialDirectory = folder;
                ofd.RestoreDirectory = true;
                ofd.Multiselect = true;
                ofd.Filter = filter;
                if (ofd.ShowDialog() == DialogResult.OK)
                    return ofd.FileNames;
                return null;
            }
        }

        public static int GetWeekOfDate(DateTime dt) => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Saturday);
        public static DateTime GetFirstDayOfWeek(DateTime dt) => dt.AddDays(-(int)dt.DayOfWeek + (int)DayOfWeek.Monday);

        public static string GetString(object o)
        {
            if (o is DateTime dt)
                return dt.ToString(dt.TimeOfDay == TimeSpan.Zero ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm");
            else if (o is TimeSpan ts)
                return ts.ToString("hh\\:mm");
            else if (Equals(o, null)) return null;
            return o.ToString();
        }

        public static List<TimeSpan> GetTimeFrames(TimeSpan from, TimeSpan to, TimeSpan interval)
        {
            var data = new List<TimeSpan>();
            var ts = from;
            while (ts <= to)
            {
                data.Add(ts);
                ts = ts.Add(interval);
            }
            return data;
        }

        public static DateTime RoundDown(this DateTime dt, TimeSpan d)
        { // see answer of redent84 in https://stackoverflow.com/questions/7029353/how-can-i-round-up-the-time-to-the-nearest-x-minutes
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }

        public static long MemoryUsedInBytes
        {
            get
            {
                // clear memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                return GC.GetTotalMemory(true);
            }
        }

        public static Tuple<DateTime, string> GetTimeStamp() => new Tuple<DateTime, string>(DateTime.Now.AddHours(-9),
            DateTime.Now.Date.AddHours(-9).ToString("yyyyMMdd"));
    }
}
