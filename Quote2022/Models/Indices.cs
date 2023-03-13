using System;
using System.Collections.Generic;
using System.Linq;

namespace Quote2022.Models
{
    public class IndexDbItem
    {
        public static void SaveToDb(string indexName, DateTime timeStamp, IEnumerable<IndexDbItem> items)
        {
            if (items.Count() > 0)
            {
                Actions.SaveToDb.ClearAndSaveToDbTable(items, "dbQuote2023..Bfr_Indices", "Index", "Symbol", "Name",
                    "Sector", "Industry", "TimeStamp");
                Actions.SaveToDb.RunProcedure("dbQuote2023..pUpdateIndices",
                    new Dictionary<string, object> {{"Index", indexName}, {"TimeStamp", timeStamp}});
            }
        }

        public string Index;
        public string Symbol;
        public string Name;
        public string Sector;
        public string Industry;
        public DateTime TimeStamp;
    }

    public class IndexDbChangeItem
    {
        public static void SaveToDb(string indexName, DateTime timeStamp, IEnumerable<IndexDbChangeItem> items)
        {
            if (items.Count() > 0)
            {
                Actions.SaveToDb.ClearAndSaveToDbTable(items, "dbQuote2023..Bfr_IndexChanges", "Index", "Date",
                    "Symbols", "AddedSymbol", "AddedName", "RemovedSymbol", "RemovedName", "TimeStamp");
                Actions.SaveToDb.RunProcedure("dbQuote2023..pUpdateIndexChanges",
                    new Dictionary<string, object> {{"Index", indexName}, {"TimeStamp", timeStamp}});
            }
        }

        public string Index;
        public DateTime Date;
        public string Symbols => $"{AddedSymbol}-{RemovedSymbol}";
        public string AddedSymbol;
        public string AddedName;
        public string RemovedSymbol;
        public string RemovedName;
        public DateTime TimeStamp;
    }


}
