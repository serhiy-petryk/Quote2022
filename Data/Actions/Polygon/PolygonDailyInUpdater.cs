using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Polygon
{
    public class PolygonDailyInUpdater
    {
        public static void Run()
        {
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT a.Symbol, a.Date from DayPolygon a "+
                                  "left join DayPolygonIn b on a.Symbol = b.Symbol and a.Date = b.Date "+
                                  "where a.TradeCount >= 5000 and a.Volume* a.[Close]/ 1000000 > 10 and b.Symbol is null "+
                                  "order by a.Date, a.Symbol";

                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        Update((string)rdr["Symbol"], (DateTime)rdr["Date"]);
            }
        }

        private static void Update(string symbol, DateTime date)
        {

        }

    }
}
