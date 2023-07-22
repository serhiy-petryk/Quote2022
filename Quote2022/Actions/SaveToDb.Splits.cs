using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Quote2022.Models;

namespace Quote2022.Actions
{
    public static partial class SaveToDb
    {
        #region ===============  Investing.com Splits  ==================
        public static void SplitInvestingHistory_SaveToDb(IEnumerable<SplitModel> items)
        {
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                ClearAndSaveToDbTable(conn, items.Where(a=>a.Date<=a.TimeStamp), "dbQ2023Others..Bfr_SplitInvestingHistory", "Symbol", "Date", "Name", "Title",
                    "Ratio", "K", "TimeStamp");

                cmd.CommandText = "INSERT INTO dbQ2023Others..SplitInvestingHistory " +
                                  "SELECT a.* FROM dbQ2023Others..Bfr_SplitInvestingHistory a " +
                                  "LEFT JOIN dbQ2023Others..SplitInvestingHistory b ON a.Symbol = b.Symbol AND a.Date = b.Date " +
                                  "WHERE b.Symbol IS NULL";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT count(*) as Recs FROM dbQ2023Others..Bfr_SplitInvestingHistory a " +
                                  "INNER JOIN dbQ2023Others..SplitInvestingHistory b ON a.Symbol = b.Symbol AND a.Date = b.Date " +
                                  "WHERE a.Ratio<>b.Ratio";
                //                    "WHERE a.Split<>b.Split and a.Symbol<>'HIHI'";
                var recs = cmd.ExecuteScalar();
                if (!Equals(recs, 0))
                    throw new Exception($"Invalid {recs} splits in SplitInvestingHistory table in DB");
            }
        }
        #endregion

        #region ===============  Eoddata Splits  ==================
        public static void SplitEoddata_SaveToDb(IEnumerable<SplitModel> items)
        {
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                ClearAndSaveToDbTable(conn, items.Where(a => a.Date <= a.TimeStamp), "dbQ2023Others..Bfr_SplitEoddata", "Exchange", "Symbol", "Date", "Ratio", "K", "TimeStamp");
                cmd.CommandText = "INSERT INTO dbQ2023Others..SplitEoddata (Exchange,Symbol,[Date],Ratio,K,[TimeStamp]) " +
                                  "SELECT a.Exchange, a.Symbol, a.[Date], a.Ratio, a.K, a.[TimeStamp] FROM dbQ2023Others..Bfr_SplitEoddata a " +
                                  "LEFT JOIN dbQ2023Others..SplitEoddata b ON a.Exchange=b.Exchange AND a.Symbol = b.Symbol AND a.Date = b.Date " +
                                  "WHERE b.Symbol IS NULL";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT count(*) as Recs FROM dbQ2023Others..Bfr_SplitEoddata a " +
                                  "INNER JOIN dbQ2023Others..SplitEoddata b ON a.Exchange=b.Exchange AND a.Symbol = b.Symbol AND a.Date = b.Date " +
                                  "WHERE a.Ratio<>b.Ratio";
                var recs = cmd.ExecuteScalar();
                if (!Equals(recs, 0))
                    throw new Exception($"Invalid {recs} splits in SplitEoddata table in DB");
            }
        }
        #endregion

        #region ===============  Investing.com Splits  ==================
        public static void SplitInvesting_SaveToDb(IEnumerable<SplitModel> items)
        {
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                ClearAndSaveToDbTable(conn, items.Where(a => a.Date <= a.TimeStamp), "dbQ2023Others..Bfr_SplitInvesting", "Symbol", "Date", "Name", "Ratio", "K",
                    "TimeStamp");
                cmd.CommandText = "INSERT INTO dbQ2023Others..SplitInvesting (Symbol,[Date],Name,Ratio,K,[TimeStamp]) " +
                                  "SELECT a.Symbol, a.[Date], a.Name, a.Ratio, a.K, a.[TimeStamp] FROM dbQ2023Others..Bfr_SplitInvesting a " +
                                  "LEFT JOIN dbQ2023Others..SplitInvesting b ON a.Symbol = b.Symbol AND a.Date = b.Date " +
                                  "WHERE b.Symbol IS NULL";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT count(*) as Recs FROM dbQ2023Others..Bfr_SplitInvesting a " +
                                  "INNER JOIN dbQ2023Others..SplitInvesting b ON a.Symbol = b.Symbol AND a.Date = b.Date " +
                                  "WHERE a.Ratio<>b.Ratio and a.Symbol<>'HIHI'";
                var recs = cmd.ExecuteScalar();
                if (!Equals(recs, 0))
                    throw new Exception($"Invalid {recs} splits in SplitInvesting table in DB");
            }
        }
        #endregion

        #region ===============  Yahoo Splits  ==================
        public static void SplitYahoo_SaveToDb(IEnumerable<SplitModel> items)
        {
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                ClearAndSaveToDbTable(conn, items.Where(a => a.Date <= a.TimeStamp), "dbQ2023Others..Bfr_SplitYahoo", "Symbol", "Date", "Ratio", "K", "TimeStamp");
                cmd.CommandText = "INSERT INTO dbQ2023Others..SplitYahoo (Symbol, Date, Ratio, K, Timestamp) " +
                                  "SELECT a.Symbol, a.Date, a.Ratio, a.K, a.Timestamp FROM dbQ2023Others..Bfr_SplitYahoo a " +
                                  "LEFT JOIN dbQ2023Others..SplitYahoo b ON a.Symbol = b.Symbol AND a.Date = b.Date " +
                                  "WHERE b.Symbol IS NULL";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT count(*) as Recs FROM dbQ2023Others..Bfr_SplitYahoo a " +
                                  "INNER JOIN dbQ2023Others..SplitYahoo b ON a.Symbol = b.Symbol AND a.Date = b.Date " +
                                  "WHERE a.Ratio<>b.Ratio " +
                                  "and NOT (a.Symbol='CLSN' and a.Date='2013-10-29') " +
                                  "and NOT(a.Symbol= 'CTXS' and a.Date= '2017-02-01') " +
                                  "and NOT(a.Symbol= 'ENPC' and a.Date= '2021-03-26') " +
                                  "and NOT(a.Symbol= 'USWS' and a.Date= '2021-10-01')";
                var recs = cmd.ExecuteScalar();
                if (!Equals(recs, 0))
                    throw new Exception($"Invalid {recs} splits in SplitYahoo table in DB");
            }
        }
        #endregion
    }
}
