using System;
using System.IO;
using System.IO.Compression;

namespace Data.Helpers
{
    public static class CsUtils
    {
        public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            // From https://stackoverflow.com/questions/6346119/compute-the-datetime-of-an-upcoming-weekday
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            var daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd).Date;
        }

        public static DateTime GetPreviousWeekday(DateTime start, DayOfWeek day) => GetNextWeekday(start, day).AddDays(-7);

        public static int GetFileSizeInKB(string filename) => Convert.ToInt32(new FileInfo(filename).Length / 1024.0);

        /// <summary>
        /// Zip folder
        /// </summary>
        /// <param name="folderName">Folder to zip</param>
        /// <returns>Zip filename</returns>
        public static string ZipFolder(string folderName)
        {
            var zipFn = (folderName.EndsWith("\\") || folderName.EndsWith("/")
                ? folderName.Substring(0, folderName.Length - 1)
                : folderName) + ".zip";
            // var zipFn = Path.GetDirectoryName(folderName) + @"\" + Path.GetFileNameWithoutExtension(folderName) + ".zip";
            if (File.Exists(zipFn))
                File.Delete(zipFn);

            System.IO.Compression.ZipFile.CreateFromDirectory(folderName, zipFn, CompressionLevel.Optimal, true);
            return zipFn;
        }

        /// <summary>
        /// Zip file
        /// </summary>
        /// <param name="filename">File to zip</param>
        /// <param name="zipFileName"></param>
        /// <returns>Zip filename</returns>
        public static string ZipFile(string filename, string zipFileName = null)
        {
            if (zipFileName == null)
            {
                zipFileName = Path.ChangeExtension(filename, ".zip");
                // if (File.Exists(zipFileName))
                   // File.Delete(zipFileName);
            }

            using (var zip = System.IO.Compression.ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                zip.CreateEntryFromFile(filename, Path.GetFileName(filename), CompressionLevel.Optimal);

            return zipFileName;
        }

        public static Tuple<DateTime, string> GetTimeStamp(int hourOffset = -9) => new Tuple<DateTime, string>(DateTime.Now.AddHours(hourOffset),
            DateTime.Now.AddHours(hourOffset).ToString("yyyyMMdd"));
    }
}
