using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Helpers;
using Data.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Data.Actions.FmpCloud
{
    public static class FmpCloudSymbolsTradableLoader
    {
        public static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries)
                    if (entry.Length > 0)
                    {
                        var items = JsonConvert.DeserializeObject<cItem[]>(entry.GetContentOfZipEntry()).ToArray();
                        itemCount += items.Length;

                        // Save data to buffer table of data server
                        Helpers.DbUtils.SaveToDbTable(items, "dbQuote2023..SymbolsTradableFmpCloud", "symbol", "name", "price",
                            "exchange", "exchangeShortName", "type");
                    }

            return itemCount;
        }

        #region ===========  Json SubClasses  ===========

        private class cItem
        {
            public string symbol;
            public string name;
            public float? price;
            public string exchange;
            public string exchangeShortName;
            public string type;
        }
        #endregion

    }
}
