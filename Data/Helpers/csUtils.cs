using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Helpers
{
    public static class csUtils
    {
        public static string ZipFolder(string folderName)
        {
            var zipFn = Path.GetDirectoryName(folderName) + @"\" + Path.GetFileNameWithoutExtension(folderName) + ".zip";
            if (File.Exists(zipFn))
                File.Delete(zipFn);

            System.IO.Compression.ZipFile.CreateFromDirectory(folderName, zipFn, CompressionLevel.Optimal, true);
            return zipFn;
        }

        public static string ZipFiles(string[] filenames)
        {
            if (filenames.Length == 0)
                return null;

            var zipFn = Path.GetDirectoryName(filenames[0]) + @"\" + Path.GetFileNameWithoutExtension(filenames[0]) + ".zip";
            if (File.Exists(zipFn))
                File.Delete(zipFn);

            using (var zip = System.IO.Compression.ZipFile.Open(zipFn, ZipArchiveMode.Create))
                foreach (var filename in filenames)
                    zip.CreateEntryFromFile(filename, Path.GetFileName(filename), CompressionLevel.Optimal);

            return zipFn;
        }

        public static string ZipFile(string filename)
        {
            var zipFn = Path.GetDirectoryName(filename) + @"\" + Path.GetFileNameWithoutExtension(filename) + ".zip";
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
