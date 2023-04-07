using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using Data.Helpers;

namespace Data.Actions.Polygon
{
    public static class PolygonTemp
    {
        public static void MoveUnnecessaryFiles()
        {
            return;
            var sourceFolder = @"E:\Quote\WebData\Minute\Polygon\DataBuffer\MinutePolygon_20230402\";
            var destinationFolder = @"E:\Quote\WebData\Minute\Polygon\Temp\";

            var filenames = new List<string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 150;
                cmd.CommandText = "SELECT  * from "+
                                  "(select a.Filename, b.Filename Filename2, count(*) Recs from "+
                                  "(select * from dbQ2023..FileLogMinutePolygon where folder = 'MinutePolygon_20230402' and filename like '%_20180[3-6]%') a "+
                                  "inner join (select * from dbQ2023..FileLogMinutePolygon where folder = 'MinutePolygon_20230402' and filename like '%_20180[3-6]%') b " +
                                  "on a.Symbol = b.Symbol and a.Date = b.Date and a.Filename <> b.Filename group by a.Filename, b.Filename) x "+
                                  "inner join(select FileName, count(*) recs from dbQ2023..FileLogMinutePolygon " +
                                  "where folder = 'MinutePolygon_20230402' and filename like '%_20180[3-6]%' group by Filename) y "+
                                  "on x.FileName = y.Filename where x.recs = y.recs order by x.FileName ";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        filenames.Add((string)rdr["Filename"]);
            }

            foreach (var filename in filenames)
            {
                var oldFN = sourceFolder + Path.GetFileName(filename);
                var newFN = destinationFolder + Path.GetFileName(filename);
                File.Move(oldFN, newFN);
            }
        }

        public static void UnzipMissingFiles()
        {
            return;

            var sourceZip = @"E:\Quote\WebData\Minute\Polygon\DataBuffer\MinutePolygon_20230331.zip";
            var destinationFolder = @"E:\Quote\WebData\Minute\Polygon\Temp\";

            var filenames = new Dictionary<string, object>( StringComparer.InvariantCultureIgnoreCase);
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 150;
                cmd.CommandText = "select distinct a.Filename from "+
                                  "(select * from dbQ2023..FileLogMinutePolygon where folder = 'MinutePolygon_20230331' and date <= '2018-04-03') a "+
                                  "left join(select * from dbQ2023..FileLogMinutePolygon where folder = 'MinutePolygon_20230327' and date <= '2018-04-03') b " +
                                  "on a.Symbol = b.Symbol and a.Date = b.Date where b.Date is null ";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        filenames.Add((string)rdr["Filename"], null);
            }

            using (var zipArchive = ZipFile.Open(sourceZip, ZipArchiveMode.Read))
                foreach (var entry in zipArchive.Entries)
                    if (filenames.ContainsKey(entry.Name))
                    {
                        var content = entry.GetContentOfZipEntry();
                        var fn = destinationFolder + entry.Name;
                        if (File.Exists(fn))
                        {

                        }
                        File.WriteAllText(fn, content);
                        File.SetCreationTime(fn, entry.LastWriteTime.DateTime);
                        File.SetLastWriteTime(fn, entry.LastWriteTime.DateTime);
                    }

        }
    }
}
