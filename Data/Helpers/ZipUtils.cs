﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static T DeserializeEntry<T>(ZipArchiveEntry entry)
        {
            // PolygonCommon.cMinuteRoot oo;
            using (var stream = entry.Open())
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return SpanJson.JsonSerializer.Generic.Utf8.Deserialize<T>(memoryStream.ToArray());
            }
        }

        public static void ReZipFiles(string folderWithZipFiles, bool doesZipFileContainsFolder, Action<string> showStatusAction)
        {
            var zipFiles = Directory.GetFiles(folderWithZipFiles, "*.zip");
            foreach (var zipFile in zipFiles)
            {
                showStatusAction?.Invoke($"Process {Path.GetFileName(zipFile)} file");

                var folder = Path.Combine(Path.GetDirectoryName(zipFile), Path.GetFileNameWithoutExtension(zipFile));
                if (doesZipFileContainsFolder)
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, Path.GetDirectoryName(zipFile));
                else
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, folder);

                File.Move(zipFile, zipFile+".old");

                CreateZip(folder, zipFile);

                Directory.Delete(folder, true);
            }

            showStatusAction?.Invoke($"End of ReZipFiles procedure");
        }

        public static void CreateZip(string sourceName, string targetZipFileName)
        {
            ProcessStartInfo p = new ProcessStartInfo();
            p.FileName = "7za.exe";
            p.Arguments = $"a \"{targetZipFileName}\" \"{sourceName}\"";
            p.WindowStyle = ProcessWindowStyle.Hidden;
            p.CreateNoWindow = true;
            Process x = Process.Start(p);
            x.WaitForExit();
        }

        public static void ZipVirtualFileEntries(string zipFileName, IEnumerable<VirtualFileEntry> entries)
        {
            using (var zipArchive = System.IO.Compression.ZipFile.Open(zipFileName, ZipArchiveMode.Update))
                foreach (var entry in entries)
                {
                    var oldEntries = zipArchive.Entries.Where(a =>
                        string.Equals(a.FullName, entry.Name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                    foreach (var o in oldEntries)
                        o.Delete();

                    var zipEntry = zipArchive.CreateEntry(entry.Name);
                    using (var writer = new StreamWriter(zipEntry.Open()))
                        writer.Write(entry.Content);
                    zipEntry.LastWriteTime = new DateTimeOffset(entry.Timestamp);
                }
        }

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

        public static void AddLinesToZip(string zipPath, IEnumerable<string> contentLines, string entryName)
        {
            using (var zipArchive = System.IO.Compression.ZipFile.Open(zipPath, ZipArchiveMode.Update))
            {
                var oldEntries = zipArchive.Entries.Where(a => string.Equals(a.FullName, entryName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                foreach (var o in oldEntries)
                    o.Delete();

                var readmeEntry = zipArchive.CreateEntry(entryName);
                using (var writer = new StreamWriter(readmeEntry.Open()))
                    foreach (var line in contentLines)
                        writer.WriteLine(line);
            }
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

        // From https://stackoverflow.com/questions/69482441/copy-files-from-one-zip-file-to-another
        private static void CopyZipEntries(string sourceZipFile, string targetZipFile)
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
