using System;
using System.IO;
using System.IO.Compression;

namespace Data.Helpers
{
    public static class CsUtils
    {
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
        /// <returns>Zip filename</returns>
        public static string ZipFile(string filename)
        {
            var zipFn = Path.ChangeExtension(filename, ".zip");
            if (File.Exists(zipFn))
                File.Delete(zipFn);

            using (var zip = System.IO.Compression.ZipFile.Open(zipFn, ZipArchiveMode.Create))
                zip.CreateEntryFromFile(filename, Path.GetFileName(filename), CompressionLevel.Optimal);

            return zipFn;
        }

        public static Tuple<DateTime, string> GetTimeStamp() => new Tuple<DateTime, string>(DateTime.Now.AddHours(-9),
            DateTime.Now.Date.AddHours(-9).ToString("yyyyMMdd"));
    }
}
