﻿using System;
using System.Collections.Generic;
using System.Linq;
using Data.Helpers;

namespace Data.Models
{
    public class IndexDbItem
    {
        public static void SaveToDb(ICollection<IndexDbItem> items)
        {
            if (items.Count > 0)
            {
                DbUtils.ClearAndSaveToDbTable(items, "dbQuote2023..Bfr_Indices", "Index", "Symbol", "Name", "Sector",
                    "Industry", "TimeStamp");
                DbUtils.RunProcedure("dbQuote2023..pUpdateIndices");
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
        public static void SaveToDb(ICollection<IndexDbChangeItem> items)
        {
            if (items.Count > 0)
            {
                DbUtils.ClearAndSaveToDbTable(items, "dbQuote2023..Bfr_IndexChanges", "Index", "Date", "Symbols",
                    "AddedSymbol", "AddedName", "RemovedSymbol", "RemovedName", "TimeStamp");
                DbUtils.RunProcedure("dbQuote2023..pUpdateIndexChanges");
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
