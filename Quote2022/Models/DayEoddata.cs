using System;
using System.Data.Common;
using System.Globalization;

namespace Quote2022.Models
{
    public class DayEoddata
    {
        public string Symbol;
        public string Exchange;
        public DateTime Date;
        public float Open;
        public float High;
        public float Low;
        public float Close;
        public float Volume;

        public DayEoddata(string exchange, string[] ss)
        {
            Symbol = ss[0];
            Exchange = exchange.Trim().ToUpper();
            Date = DateTime.ParseExact(ss[1], "yyyyMMdd", CultureInfo.InvariantCulture);
            Open = float.Parse(ss[2].Trim(), CultureInfo.InvariantCulture);
            High = float.Parse(ss[3].Trim(), CultureInfo.InvariantCulture);
            Low = float.Parse(ss[4].Trim(), CultureInfo.InvariantCulture);
            Close = float.Parse(ss[5].Trim(), CultureInfo.InvariantCulture);
            Volume = float.Parse(ss[6].Trim(), CultureInfo.InvariantCulture);
        }

        public DayEoddata(DbDataReader rdr)
        {
            Symbol = (string)rdr["Symbol"];
            Exchange = (string)rdr["Exchange"];
            Date = (DateTime)rdr["Date"];
            Open = (float)rdr["Open"]; ;
            High = (float)rdr["High"]; ;
            Low = (float)rdr["Low"]; ;
            Close = (float)rdr["Close"]; ;
            Volume = (float)rdr["Volume"]; ;
        }
    }
}
