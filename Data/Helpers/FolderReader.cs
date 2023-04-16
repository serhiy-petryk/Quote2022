using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Data.Helpers
{
    public class FolderReader: IDisposable
    {
        public static string GetFolderId(string zipFileNameOrFolderName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var cc = zipFileNameOrFolderName.ToList();
            while (cc.Count > 0 && invalidChars.Contains(cc[cc.Count - 1]))
                cc.RemoveAt(cc.Count - 1);
            zipFileNameOrFolderName = new string(cc.ToArray());

            return (Directory.Exists(zipFileNameOrFolderName))
                ? Path.GetFileName(zipFileNameOrFolderName)
                : Path.GetFileNameWithoutExtension(zipFileNameOrFolderName);
        }

        public readonly string FolderId;
        // public readonly string Name;
        public readonly ReaderEntry[] Entries;

        private readonly ZipArchive Zip;

        public FolderReader(string zipFileNameOrFolderName, string fileNameEndsWith = null)
        {
            FolderId = GetFolderId(zipFileNameOrFolderName);
            // Name = Path.GetFileName(zipFileNameOrFolderName);

            if (File.Exists(zipFileNameOrFolderName) && zipFileNameOrFolderName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
            {
                FolderId = Path.GetFileNameWithoutExtension(zipFileNameOrFolderName);
                Zip = ZipFile.Open(zipFileNameOrFolderName, ZipArchiveMode.Read);
                Entries = (string.IsNullOrEmpty(fileNameEndsWith)
                        ? Zip.Entries.Where(a => a.Length > 0)
                        : Zip.Entries.Where(a => a.Length > 0 &&
                            a.Name.EndsWith(fileNameEndsWith, StringComparison.InvariantCultureIgnoreCase)))
                    .Select(a => new ReaderEntry(a, FolderId)).ToArray();
            }
            else if (Directory.Exists(zipFileNameOrFolderName))
            {
                FolderId = Path.GetFileName(zipFileNameOrFolderName);
                Entries = (string.IsNullOrEmpty(fileNameEndsWith)
                        ? Directory.GetFiles(zipFileNameOrFolderName)
                        : Directory.GetFiles(zipFileNameOrFolderName).Where(a =>
                            a.EndsWith(fileNameEndsWith, StringComparison.InvariantCultureIgnoreCase)).ToArray())
                    .Select(a => new ReaderEntry(a)).ToArray();
            }
            else
                throw new Exception($"{zipFileNameOrFolderName}: Invalid zip file name or folder name");
        }

        public void Dispose() => Zip?.Dispose();

        #region ==========  ReaderEntry class  ===========
        public class ReaderEntry
        {
            private readonly string File;
            private readonly ZipArchiveEntry Entry;

            public readonly string FileName;
            public readonly string FolderId;
            public readonly DateTime Created;
            public long Length => File != null ? new FileInfo(File).Length : Entry.Length;
            public string AllText => File != null ? System.IO.File.ReadAllText(File) : Entry.GetContentOfZipEntry();
            public string[] AllLines => File != null ? System.IO.File.ReadAllLines(File) : Entry.GetLinesOfZipEntry().ToArray();

            public ReaderEntry(string file)
            {
                FolderId = Path.GetFileName(Path.GetDirectoryName(file));
                File = file;
                FileName = Path.GetFileName(file);
                Created = System.IO.File.GetLastWriteTime(file);
            }

            public ReaderEntry(ZipArchiveEntry entry, string folderId)
            {
                FolderId = folderId;
                Entry = entry;
                FileName = Entry.Name;
                Created = Entry.LastWriteTime.DateTime;
            }

        }
        #endregion


    }
}
