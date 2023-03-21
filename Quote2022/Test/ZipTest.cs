using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quote2022.Test
{
    class ZipTest
    {
        public static void Start()
        {
            GetAllEntries(@"E:\Quote\WebData\Minute\AlphaVantage\Data\MAV_20210217.zip");

            var folder = @"E:\Temp\temp2\ZipTest1";
            var zipFileName = @"E:\Temp\temp2\ZipTest1.zip";
            //var a1 = ZipFolder(folder);
            foreach (var file in Directory.GetFiles(folder))
            {
                AddFilesToZip(zipFileName, new [] {file}, "ZipTest1");
            }

        }

        public static string ZipFolder(string folderName)
        {
            var zipFn = (folderName.EndsWith("\\") || folderName.EndsWith("/")
                            ? folderName.Substring(0, folderName.Length - 1)
                            : folderName) + ".zip";
            // var zipFn = Path.GetDirectoryName(folderName) + @"\" + Path.GetFileNameWithoutExtension(folderName) + ".zip";
            // if (File.Exists(zipFn))
               //  File.Delete(zipFn);

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

        public static void AddFilesToZip(string zipPath, string[] files)
        {
            if (files == null || files.Length == 0)
            {
                return;
            }

            using (var zipArchive = System.IO.Compression.ZipFile.Open(zipPath, ZipArchiveMode.Update))
            {
                foreach (var file in files)
                {
                    // var fileInfo = new FileInfo(file);
                    var entryName = Path.GetFileName(Path.GetDirectoryName(file)) + Path.AltDirectorySeparatorChar + Path.GetFileName(file);
                    var oldEntries = zipArchive.Entries.Where(a => a.FullName == entryName).ToArray();
                    foreach (var o in oldEntries)
                    {
                        o.Delete();
                    }
                    // var oldEntry = zipArchive.Entries.FirstOrDefault(a => a.FullName == entryName);
                    // var oldEntry = zipArchive.GetEntry(entryName);
                    // oldEntries?.Delete();

                    zipArchive.CreateEntryFromFile(file, entryName);
                }
            }
        }

        public static void AddFilesToZip(string zipPath, string[] files, string entryPrefix)
        {
            if (files == null || files.Length == 0)
                return;

            using (var zipArchive = System.IO.Compression.ZipFile.Open(zipPath, ZipArchiveMode.Update))
            {
                foreach (var file in files)
                {
                    var entryName = (string.IsNullOrEmpty(entryPrefix) ? null : entryPrefix + Path.DirectorySeparatorChar) + Path.GetFileName(file);
                    
                    var oldEntries = zipArchive.Entries.Where(a => string.Equals(a.FullName, entryName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                    foreach (var o in oldEntries) o.Delete();

                    if (!string.IsNullOrEmpty(entryPrefix))
                    {
                        entryName = entryPrefix + Path.AltDirectorySeparatorChar + Path.GetFileName(file);
                        oldEntries = zipArchive.Entries.Where(a => string.Equals(a.FullName, entryName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                        foreach (var o in oldEntries) o.Delete();
                    }

                    zipArchive.CreateEntryFromFile(file, entryName);
                }
            }
        }

        public static void GetAllEntries(string zipFileName)
        {
            using (ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(zipFileName))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    Console.WriteLine(entry.FullName);
                }
            }
        }
        //E:\Quote\WebData\Minute\AlphaVantage\Data

    }
}
