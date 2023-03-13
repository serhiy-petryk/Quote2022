using System;
using System.Data.Common;
using System.Globalization;

namespace Quote2022.Models
{
    public class DayEoddataExtended
    {
        public string Key => Symbol + Date.ToString("yyyyMMdd", CultureInfo.InstalledUICulture);

        public float? R1;
        public float? R2;
        public float? R3;
        public float? R4;
        public float? R5;
        
        public float VlmToWAvg => VLM / WAvgVLM;
        public float MaxPVlmToWAvg => MaxPVLM / WAvgVLM;
        public float CloseToWAvg => CL / WAvgCL;
        public float OpenToClose => Open / CL;

        public string Exchange;
        public string Symbol;
        public DateTime Date;
        public float Open;
        public float High;
        public float Low;
        public float CL;
        public float VLM;
        public float DayChanged; // (Close-Open)/Close
        public float Changed; // (Close-PrevClose)/Close
        public float DJI_ToAvg;
        public float DJI_ToWAvg;
        public float GSPC_ToAvg;
        public float GSPC_ToWAvg;
        public float AvgCL;
        public float WAvgCL;
        public float AvgVLM;
        public float WAvgVLM;
        public float AvgVolatility;
        public float WAvgVolatility;
        public float AvgVolatility2;
        public float WAvgVolatility2;
        public float MaxPVLM;
        public float MinPVLM;
        public float MaxPHigh;
        public float MinPLow;
        public float Open_P1;
        public float High_P1;
        public float Low_P1;
        public float CL_P1;
        public float VLM_P1;
        public float Open_N1;
        public float High_N1;
        public float Low_N1;
        public float CL_N1;
        public float VLM_N1;
        public float? Open_N2;
        public float? High_N2;
        public float? Low_N2;
        public float? CL_N2;
        public float? VLM_N2;
        public float? Open_N3;
        public float? High_N3;
        public float? Low_N3;
        public float? CL_N3;
        public float? VLM_N3;
        public float? Open_N4;
        public float? High_N4;
        public float? Low_N4;
        public float? CL_N4;
        public float? VLM_N4;
        public float? Open_N5;
        public float? High_N5;
        public float? Low_N5;
        public float? CL_N5;
        public float? VLM_N5;
        public string Split_N1;
        public string Split_N2;
        public string Split_N3;
        public string Split_N4;
        public string Split_N5;

        public bool IsValid1;
        public bool IsValid2;
        public bool IsValid3;
        public bool IsValid4;
        public bool IsValid5;

        public int TopUp;
        public int TopDown;
        public float BreakUp;
        public float BreakDown;

        public string Sector;
        public string Industry;

        public DayEoddataExtended(DbDataReader dr)
        {
            Exchange = (string)dr["Exchange"];
            Symbol = (string)dr["Symbol"];
            Date = (DateTime)dr["Date"];
            Open = (float)dr["Open"];
            High = (float)dr["High"];
            Low = (float)dr["Low"];
            CL = (float)dr["CL"];
            VLM = (float)dr["VLM"];
            DJI_ToAvg = (float)dr["DJI_ToAvg"];
            DJI_ToWAvg = (float)dr["DJI_ToWAvg"];
            GSPC_ToAvg = (float)dr["GSPC_ToAvg"];
            GSPC_ToWAvg = (float)dr["GSPC_ToWAvg"];
            AvgCL = (float)dr["AvgCL"];
            WAvgCL = (float)dr["WAvgCL"];
            AvgVLM = (float)dr["AvgVLM"];
            WAvgVLM = (float)dr["WAvgVLM"];
            AvgVolatility = (float)dr["AvgVolatility"];
            WAvgVolatility = (float)dr["WAvgVolatility"];
            AvgVolatility2 = (float)dr["AvgVolatility2"];
            WAvgVolatility2 = (float)dr["WAvgVolatility2"];
            MaxPVLM = (float)dr["MaxPVLM"];
            MinPVLM = (float)dr["MinPVLM"];
            MaxPHigh = (float)dr["MaxPHigh"];
            MinPLow = (float)dr["MinPLow"];
            Open_P1 = (float)dr["Open_P1"];
            High_P1 = (float)dr["High_P1"];
            Low_P1 = (float)dr["Low_P1"];
            CL_P1 = (float)dr["CL_P1"];
            VLM_P1 = (float)dr["VLM_P1"];
            Open_N1 = (float)dr["Open_N1"];
            High_N1 = (float)dr["High_N1"];
            Low_N1 = (float)dr["Low_N1"];
            CL_N1 = (float)dr["CL_N1"];
            VLM_N1 = (float)dr["VLM_N1"];
            Open_N2 = (float?)GetValue(dr["Open_N2"]);
            High_N2 = (float?)GetValue(dr["High_N2"]);
            Low_N2 = (float?)GetValue(dr["Low_N2"]);
            CL_N2 = (float?)GetValue(dr["CL_N2"]);
            VLM_N2 = (float?)GetValue(dr["VLM_N2"]);
            Open_N3 = (float?)GetValue(dr["Open_N3"]);
            High_N3 = (float?)GetValue(dr["High_N3"]);
            Low_N3 = (float?)GetValue(dr["Low_N3"]);
            CL_N3 = (float?)GetValue(dr["CL_N3"]);
            VLM_N3 = (float?)GetValue(dr["VLM_N3"]);
            Open_N4 = (float?)GetValue(dr["Open_N4"]);
            High_N4 = (float?)GetValue(dr["High_N4"]);
            Low_N4 = (float?)GetValue(dr["Low_N4"]);
            CL_N4 = (float?)GetValue(dr["CL_N4"]);
            VLM_N4 = (float?)GetValue(dr["VLM_N4"]);
            Open_N5 = (float?)GetValue(dr["Open_N5"]);
            High_N5 = (float?)GetValue(dr["High_N5"]);
            Low_N5 = (float?)GetValue(dr["Low_N5"]);
            CL_N5 = (float?)GetValue(dr["CL_N5"]);
            VLM_N5 = (float?)GetValue(dr["VLM_N5"]);
            Split_N1 = Equals(dr["Split_N1"], DBNull.Value) ? null : (string)dr["Split_N1"];
            Split_N2 = Equals(dr["Split_N2"], DBNull.Value) ? null : (string)dr["Split_N2"];
            Split_N3 = Equals(dr["Split_N3"], DBNull.Value) ? null : (string)dr["Split_N3"];
            Split_N4 = Equals(dr["Split_N4"], DBNull.Value) ? null : (string)dr["Split_N4"];
            Split_N5 = Equals(dr["Split_N5"], DBNull.Value) ? null : (string)dr["Split_N5"];

            IsValid1 = string.IsNullOrEmpty(Split_N1);
            IsValid2 = IsValid1 && string.IsNullOrEmpty(Split_N2) && Open_N2.HasValue;
            IsValid3 = IsValid2 && string.IsNullOrEmpty(Split_N3) && Open_N3.HasValue;
            IsValid4 = IsValid3 && string.IsNullOrEmpty(Split_N4) && Open_N4.HasValue;
            IsValid5 = IsValid4 && string.IsNullOrEmpty(Split_N5) && Open_N5.HasValue;

            R1 = IsValid1 ? Open_N1 / CL_N1 : (float?)null;
            R2 = IsValid2 ? Open_N2 / CL_N2 : (float?)null;
            R3 = IsValid3 ? Open_N3 / CL_N3 : (float?)null;
            R4 = IsValid4 ? Open_N4 / CL_N4 : (float?)null;
            R5 = IsValid5 ? Open_N5 / CL_N5 : (float?)null;

            DayChanged = (CL - Open) / CL * 100.0F;
            Changed = (CL - CL_P1) / CL * 100.0F;
            BreakUp = (Low - High_P1) / High_P1 * 100.0F;
            BreakDown = (Low_P1 - High) / Low_P1 * 100.0F;
        }

        private object GetValue(object o) => Equals(o, DBNull.Value) ? null : o;
    }
}
