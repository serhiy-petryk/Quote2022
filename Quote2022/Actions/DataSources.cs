using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Quote2022.Helpers;
using Quote2022.Models;

namespace Quote2022.Actions
{
    public static class DataSources
    {
        private static Dictionary<Tuple<int, DateTime, DateTime>, Dictionary<string, SymbolsOfDataSource>>
            _activeSymbols = new Dictionary<Tuple<int, DateTime, DateTime>, Dictionary<string, SymbolsOfDataSource>>();

        public static Dictionary<string, SymbolsOfDataSource> GetActiveSymbols(int numberOfSymbols = 100000, DateTime? from = null, DateTime? to = null)
        {
            from = from ?? new DateTime(2022, 11, 1);
            to = to ?? new DateTime(2022, 12, 31);

            var key = new Tuple<int, DateTime, DateTime>(numberOfSymbols, from.Value, to.Value);
            if (!_activeSymbols.ContainsKey(key))
            {
                var symbols = new Dictionary<string, SymbolsOfDataSource>();
                using (var conn = new SqlConnection(Settings.DbConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText =
                        $"select a.Exchange, a.Symbol, ISNULL(ISNULL(b.[Index], a.Asset),'STOCKS') Kind, c.TradeValue, " +
                        "a.Asset, a.Sector, a.Industry, a.TvType, a.TvSubtype, a.TvSector, a.TvIndustry, a.TvRecommend " +
                        "FROM SymbolsEoddata a " +
                        "left join [Indexes] b on a.Symbol = b.Symbol " +
                        $"inner join (select TOP {numberOfSymbols} exchange, symbol, SUM([Close] * Volume) TradeValue, " +
                        "MIN([Close]) MinClose, MAX([Close]) MaxClose, Min(Volume) MinVolume FROM DayEoddata " +
                        $"WHERE date between '{from:yyyy-MM-dd}' and '{to:yyyy-MM-dd}' " +
                        "GROUP BY Exchange, Symbol " +
                        "HAVING Min([Close])>=1.0 AND MAX([Close])<=300 AND Min(Volume)>=300000 " +
                        "ORDER BY TradeValue DESC) c on a.Exchange = c.Exchange and a.Symbol = c.Symbol " +
                        "ORDER BY c.TradeValue desc";

                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                        {
                            var o = new SymbolsOfDataSource(rdr);
                            if (!symbols.ContainsKey(o.Symbol))
                                symbols.Add(o.Symbol, o);
                            symbols[o.Symbol].Kinds.Add((string) rdr["Kind"]);
                        }
                }


                //var gData = Chunk(data.OrderBy(fGroups[k]), (data.Count + chunkSize - 1) / chunkSize);

                var chunkCount = 10;
                var aa = Algorithm1.Chunk(symbols.Values.OrderByDescending(a => a.TradeValue),
                    Algorithm1.GetChunkSize(symbols.Count, chunkCount)).ToList();

                for (var k = 0; k < aa.Count; k++)
                    foreach (var a1 in aa[k])
                        a1.TradeValueId = k;

                foreach (var a in symbols.Values.Where(a => !a.TvRecommend.HasValue))
                    a.TvRecommendId = 0;

                var aa1 = symbols.Values.Where(a => a.TvRecommend.HasValue).OrderBy(a => a.TvRecommend.Value).ToList();
                aa = Algorithm1.Chunk(aa1, Algorithm1.GetChunkSize(aa1.Count, chunkCount)).ToList();

                for (var k = 0; k < aa.Count; k++)
                    foreach (var a1 in aa[k])
                        a1.TvRecommendId = k+1;

                _activeSymbols[key] = symbols;
            }

            return _activeSymbols[key];
        }
    }
}
