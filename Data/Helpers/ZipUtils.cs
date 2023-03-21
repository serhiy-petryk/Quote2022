using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Data.Helpers
{
    public static class ZipUtils
    {
        /*  Open zip archive
         *  using (var zipArchive = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
         */

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

        public static void AddFileToZip(string zipPath, string file, string entryPrefix)
        {
            using (var zipArchive = System.IO.Compression.ZipFile.Open(zipPath, ZipArchiveMode.Update))
            {
                var entryName = (string.IsNullOrEmpty(entryPrefix) ? null : entryPrefix + Path.DirectorySeparatorChar) + Path.GetFileName(file);
                var oldEntries = zipArchive.Entries.Where(a => string.Equals(a.FullName, entryName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                foreach (var o in oldEntries)
                    o.Delete();

                if (!string.IsNullOrEmpty(entryPrefix))
                { // remove entries with AltDirectorySeparatorChar
                    // new entry have to have Path.AltDirectorySeparatorChar
                    entryName = entryPrefix + Path.AltDirectorySeparatorChar + Path.GetFileName(file);
                    oldEntries = zipArchive.Entries.Where(a => string.Equals(a.FullName, entryName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                    foreach (var o in oldEntries)
                        o.Delete();
                }

                zipArchive.CreateEntryFromFile(file, entryName);
            }
        }

        // From https://stackoverflow.com/questions/69482441/copy-files-from-one-zip-file-to-another
        static void CopyZipEntries(string sourceZipFile, string targetZipFile)
        {
            if (!File.Exists(targetZipFile))
            {
                using (var zipArchive = System.IO.Compression.ZipFile.Open(targetZipFile, ZipArchiveMode.Create)) { }
            }

            using (FileStream sourceFS = new FileStream(sourceZipFile, FileMode.Open))
            using (FileStream targetFS = new FileStream(targetZipFile, FileMode.Open))
            using (ZipArchive sourceZIP = new ZipArchive(sourceFS, ZipArchiveMode.Read, false))
            using (ZipArchive targetZIP = new ZipArchive(targetFS, ZipArchiveMode.Update, false))
                foreach (ZipArchiveEntry sourceEntry in sourceZIP.Entries)
                {
                    if (targetZIP.GetEntry(sourceEntry.FullName) is ZipArchiveEntry existingTargetEntry)
                        existingTargetEntry.Delete();

                    using (Stream target = targetZIP.CreateEntry(sourceEntry.FullName).Open())
                        sourceEntry.Open().CopyTo(target);
                }
        }

        #region =========  Extensions for ZipArchiveEntry  ===========
        public static IEnumerable<string> GetLinesOfZipEntry(this ZipArchiveEntry entry)
        {
            using (var entryStream = entry.Open())
            using (var reader = new StreamReader(entryStream, System.Text.Encoding.UTF8, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    yield return line;
            }
        }
        public static string GetContentOfZipEntry(this ZipArchiveEntry entry)
        {
            using (var entryStream = entry.Open())
            using (var reader = new StreamReader(entryStream, System.Text.Encoding.UTF8, true))
                return reader.ReadToEnd();
        }
        #endregion
    }
}
