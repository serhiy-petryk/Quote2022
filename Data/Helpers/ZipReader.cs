using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Data.Helpers
{
    public class ZipReader: IEnumerable<ZipReaderItem>, IDisposable
    {
        private readonly ZipArchive _zip;
        public ZipReader(string filename) => _zip = ZipFile.Open(filename, ZipArchiveMode.Read);
        public void Dispose() => _zip.Dispose();

        public IEnumerator<ZipReaderItem> GetEnumerator()
        {
            foreach (var entry in _zip.Entries.OrderBy(a => a.LastWriteTime))
            {
                    var item = new ZipReaderItem()
                    {
                        // Reader = reader,
                        _entry = entry,
                        Created = entry.LastWriteTime.DateTime,
                        Length = entry.Length,
                        FullName = entry.FullName
                    };
                    yield return item;
            }
        }

        public IEnumerator<ZipReaderItem> GetEnumeratorX()
        {
            foreach (var entry in _zip.Entries.OrderBy(a => a.LastWriteTime))
            {
                using (var entryStream = entry.Open())
                using (var reader = new StreamReader(entryStream, System.Text.Encoding.UTF8, true))
                {
                    var item = new ZipReaderItem()
                    {
                       //  Reader = reader,
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
        // public StreamReader Reader;
        internal ZipArchiveEntry _entry;// { get; set; }
        public IEnumerable<string> AllLines
        {
            get
            {
                string line;
                using (var entryStream = _entry.Open())
                using (var reader = new StreamReader(entryStream, System.Text.Encoding.UTF8, true))
                {
                    while ((line = reader.ReadLine()) != null)
                        yield return line;
                }
            }
        }
        public string Content
        {
            get
            {
                using (var entryStream = _entry.Open())
                using (var reader = new StreamReader(entryStream, System.Text.Encoding.UTF8, true))
                    return reader.ReadToEnd();
            }
        }

        public string FullName;
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FullName);
        public long Length;
        public DateTime Created;
    }
}
