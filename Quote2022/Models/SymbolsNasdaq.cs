using System;

namespace Quote2022.Models
{
    public class SymbolsNasdaq
    {

        public class SymbolsNasdaqFile
        {
            public SymbolsNasdaqItem[] data;
            public string Message;
            public RequestStatus Status;
        }

        public class SymbolsNasdaqItem
        {
            public string Symbol;
            public string Name;
            public string MrktCategory;
            public string Exchange;
            public string SubCategory;
            public string Nasdaq100; // "Y"
            public string Region;
            public string Asset;
            public string Industry;
            public string Count; // always is empty
            public string Flag; // "SPAC" (Special Purpose Acquisition Company)
        }

        public class SymbolsNasdaqDbItem
        {
            public string Key => Exchange + "/" + Symbol;
            public string Exchange;
            public string Symbol;
            public string Name;
            public string MrktCategory;
            public string SubCategory;
            public string Nasdaq100; // "Y"
            public string Region;
            public string Asset;
            public string Industry;
            public string Flag; // "SPAN"
            public DateTime Created;

            public SymbolsNasdaqDbItem(SymbolsNasdaqItem o, DateTime created)
            {
                Exchange = string.IsNullOrEmpty(o.Exchange) ? "" : o.Exchange;
                Symbol = o.Symbol;
                Name = string.IsNullOrEmpty(o.Name) ? null : o.Name;
                MrktCategory = string.IsNullOrEmpty(o.MrktCategory) ? null : o.MrktCategory;
                SubCategory = string.IsNullOrEmpty(o.SubCategory) ? null : o.SubCategory;
                Nasdaq100 = string.IsNullOrEmpty(o.Nasdaq100) ? null : o.Nasdaq100;
                Region = string.IsNullOrEmpty(o.Region) ? null : o.Region;
                Asset = string.IsNullOrEmpty(o.Asset) ? null : o.Asset;
                Industry = string.IsNullOrEmpty(o.Industry) ? null : o.Industry;
                Flag = string.IsNullOrEmpty(o.Flag) ? null : o.Flag;
                Created = created;
            }

            public bool IsEqual(SymbolsNasdaqDbItem item)
            {
                return string.Equals(Exchange, item.Exchange) && string.Equals(Symbol, item.Symbol) &&
                       string.Equals(Name, item.Name) && string.Equals(MrktCategory, item.MrktCategory) &&
                       string.Equals(SubCategory, item.SubCategory) && string.Equals(Industry, item.Industry);
            }
        }

        public class RequestStatus
        {
            public int rCode;
            public string bCodeMessage;
            public string developerMessage;
        }
    }

}
