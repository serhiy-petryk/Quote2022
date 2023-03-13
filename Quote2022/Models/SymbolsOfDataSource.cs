using System;
using System.Collections.Generic;
using System.Data;

namespace Quote2022.Models
{
    public class SymbolsOfDataSource
    {
        // public string ExchangeAndSymbol => Symbol + @"/" + Exchange;
        public string Exchange;
        public string Symbol;
        public string Asset;
        public string Sector;
        public string Industry;
        public List<string> Kinds = new List<string>();
        public double TradeValue;
        public int TradeValueId = int.MinValue;
        public string TvType;
        public string TvSubtype;
        public string TvSector;
        public string TvIndustry;
        public string TvSectorAndIndustry => $"{TvSector}/{TvIndustry}";
        public float? TvRecommend;
        public int TvRecommendId = int.MinValue;

        public string TvFullType => $"{TvType}/{TvSubtype}";

        public SymbolsOfDataSource(IDataReader rdr)
        {
            Exchange = (string)rdr["Exchange"];
            Symbol = (string)rdr["Symbol"];
            Asset = GetDbString(rdr["Asset"]);
            Sector = GetDbString(rdr["Sector"]) ?? Asset;
            Industry = GetDbString(rdr["Industry"]) ?? Asset;
            TradeValue = (double)rdr["TradeValue"];
            TvType = GetDbString(rdr["TvType"]);
            TvSubtype = GetDbString(rdr["TvSubtype"]);
            TvSector = GetDbString(rdr["TvSector"]);
            TvIndustry = GetDbString(rdr["TvIndustry"]);
            if (rdr["TvRecommend"] is float o)
                TvRecommend = o;
        }

        private static string GetDbString(object rdrValue) => ReferenceEquals(rdrValue, DBNull.Value) ? null : (string) rdrValue;
    }

}
