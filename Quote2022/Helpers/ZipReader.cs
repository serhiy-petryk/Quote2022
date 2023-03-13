using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Quote2022.Helpers
{
    public class ZipReader: IEnumerable<ZipReaderItem>, IDisposable
    {
        private readonly ZipArchive _zip;
        public ZipReader(string filename) => _zip = ZipFile.Open(filename, ZipArchiveMode.Read);
        public void Dispose() => _zip.Dispose();

        public IEnumerator<ZipReaderItem> GetEnumerator()
        {
            foreach (var entry in _zip.Entries)
            {
                using (var entryStream = entry.Open())
                using (var reader = new StreamReader(entryStream, System.Text.Encoding.UTF8, true))
                {
                    var item = new ZipReaderItem()
                    {
                        Reader = reader,
                        Created = entry.LastWriteTime.DateTime,
                        Length = entry.Length,
                        FullName = entry.FullName
                    };
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class ZipReaderItem
    {
        public StreamReader Reader;
        public IEnumerable<string> AllLines
        {
            get
            {
                string line;
                while ((line = Reader.ReadLine()) != null)
                    yield return line;
            }
        }
        public string Content => Reader.ReadToEnd();
        public string FullName;
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FullName);
        public long Length;
        public DateTime Created;
    }
}
