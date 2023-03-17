﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Data.Helpers;
using Data.Models;
using Newtonsoft.Json;

namespace Data.Actions.StockAnaysis
{
    public class StockAnalysisIPOs
    {
        private const string Url = @"https://stockanalysis.com/api/screener/s/f?m=ipoDate&s=desc&c=ipoDate,s,n,ipoPrice,ippc,exchange,sector,industry,employees&cn=5000&i=histip";
        private const string Folder = @"E:\Quote\WebData\Splits\StockAnalysis\IPOs\";

        public static void Start(Action<string> logEvent)
        {
            logEvent($"StockAnalysisIPOs started");

            var timeStamp = CsUtils.GetTimeStamp();
            var jsonFileName = Folder + $"StockAnalysisIPOs_{timeStamp.Item2}.json";
            var zipFileName = Folder + $"StockAnalysisIPOs_{timeStamp.Item2}.zip";

            // Download data to html file
            Helpers.Download.DownloadPage(Url, jsonFileName);

            // Zip data
           Helpers.CsUtils.ZipFile(jsonFileName, zipFileName);

            // Parse and save to database
            var itemCount = ParseAndSaveToDb(zipFileName);

            File.Delete(jsonFileName);

            logEvent($"!StockAnalysisIPOs finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = new ZipReader(zipFileName))
                foreach (var zipItem in zip)
                    if (zipItem.Length > 0)
                    {
                        var oo = JsonConvert.DeserializeObject<cRoot>(zipItem.Content);
                        foreach (var item in oo.data.data)
                            item.TimeStamp = zipItem.Created;
                        itemCount += oo.data.data.Length;

                        // Save data to database
                        if (oo.data.data.Length > 0)
                        {
                            DbUtils.ClearAndSaveToDbTable(oo.data.data, "dbQuote2023..Bfr_IpoStockAnalysis", "Symbol", "Date", "Exchange",
                                "Name", "IpoPrice", "CurrentPrice", "Sector", "Industry", "employees", "TimeStamp");

                            DbUtils.RunProcedure("dbQuote2023..pUpdateIpoStockAnalysis");
                        }

                    }

            return itemCount;
        }

        #region =======  Json subclasses  =============
        public class cRoot
        {
            public int status;
            public cData data;
        }

        public class cData
        {
            public int resultsCount;
            public cItem[] data;
        }
        public class cItem
        {
            public string s;
            public DateTime ipoDate;
            public string exchange;
            public string n;
            public float ipoPrice;
            public float ippc;
            public string sector;
            public string industry;
            public int? employees;

            public string Symbol => s;
            public DateTime Date => ipoDate;
            public string Exchange => exchange;
            public string Name => n;
            public float IpoPrice => ipoPrice;
            public float CurrentPrice => ippc;
            public string Sector => string.IsNullOrEmpty(sector) ? null : sector;
            public string Industry => string.IsNullOrEmpty(industry) ? null : industry;
            public DateTime TimeStamp;
        }
        #endregion
    }
}