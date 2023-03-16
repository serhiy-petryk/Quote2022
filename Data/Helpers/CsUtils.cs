using System;
using System.IO;
using System.IO.Compression;

namespace Data.Helpers
{
    public static class CsUtils
    {
        public static int GetFileSizeInKB(string filename) => Convert.ToInt32(new FileInfo(filename).Length / 1024.0);

        /// <summary>
        /// Zip folder
        /// </summary>
        /// <param name="folderName">Folder to zip</param>
        /// <returns>Zip filename</returns>
        public static string ZipFolder(string folderName)
        {
            var zipFn = Path.GetDirectoryName(folderName) + @"\" + Path.GetFileNameWithoutExtension(folderName) + ".zip";
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
            DateTime.Now.Date.AddHours(hourOffset).ToString("yyyyMMdd"));
    }
}
