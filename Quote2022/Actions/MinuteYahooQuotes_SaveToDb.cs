using System;
using System.Collections.Generic;
using Quote2022.Helpers;
using Quote2022.Models;

namespace Quote2022.Actions
{
    public static class MinuteYahooQuotes_SaveToDb
    {
        public static void Execute(string[] zipFiles, Action<string> showStatus)
        {
            var from = new TimeSpan(9, 45, 0);
            var to = new TimeSpan(15, 45, 0);

            var quotes = QuoteLoader.MinuteYahoo_GetQuotesFromZipFiles(showStatus, zipFiles, false, false);

            var iQuotes = new List<IntradayYahooQuote>();
            string lastSymbol = null;
            var lastDate = DateTime.MinValue;
            IntradayYahooQuote currentQuote = null;
            var hlVolatSum = 0.0;
            var ocVolatSum = 0.0;
            var preCount = 0;
            foreach (var q in quotes)
            {
                if (!string.Equals(q.Symbol, lastSymbol) || !Equals(lastDate, q.Timed.Date))
                {
                    if (currentQuote != null && currentQuote.Count > 0)
                    {
                        currentQuote.HL_AvgVolat = Convert.ToSingle(hlVolatSum / currentQuote.Count * 100.0);
                        currentQuote.OC_AvgVolat = Convert.ToSingle(ocVolatSum / currentQuote.Count * 100.0);
                        iQuotes.Add(currentQuote);
                    }

                    currentQuote = new IntradayYahooQuote { Symbol = q.Symbol, Date = q.Timed.Date };
                    hlVolatSum = 0.0;
                    ocVolatSum = 0.0;
                }

                var qTime = q.Timed.TimeOfDay;
                if (qTime < from)
                    currentQuote.PreCount++;
                else if (qTime >= from && qTime < to)
                {
                    currentQuote.Count++;
                    hlVolatSum += (q.High - q.Low) / (q.High + q.Low) / 2;
                    ocVolatSum += Math.Abs((q.Open - q.Close) / (q.Open + q.Close) / 2);

                    if (!currentQuote.Open.HasValue)
                    {
                        currentQuote.Open = q.Open;
                        currentQuote.OpenAt = qTime;
                    }

                    if ((currentQuote.High ?? float.MinValue) < q.High)
                    {
                        currentQuote.High = q.High;
                        currentQuote.HighAt = qTime;
                    }
                    if ((currentQuote.Low ?? float.MaxValue) > q.Low)
                    {
                        currentQuote.Low = q.Low;
                        currentQuote.LowAt = qTime;
                    }
                    currentQuote.Close = q.Close;
                    currentQuote.Volume += Convert.ToInt32(q.Volume);
                }
                else if (qTime >= to && !currentQuote.CloseAt.HasValue)
                {
                    currentQuote.CloseInNextFrame = q.Open;
                    currentQuote.CloseAt = qTime;
                }

                lastSymbol = q.Symbol;
                lastDate = q.Timed.Date;
            }

            if (currentQuote != null && currentQuote.Count > 0)
            {
                currentQuote.HL_AvgVolat = Convert.ToSingle(hlVolatSum / currentQuote.Count * 100.0);
                currentQuote.OC_AvgVolat = Convert.ToSingle(ocVolatSum / currentQuote.Count * 100.0);
                iQuotes.Add(currentQuote);
            }

            showStatus($"IntradayQuotes_SaveToDb. Save to database ...");
            SaveToDb.ClearAndSaveToDbTable(iQuotes, "Bfr_IntradayYahooQuotes", "Symbol", "Date", "Open", "High", "Low",
                "Close", "CloseInNextFrame", "Volume", "Count", "PreCount", "OpenAt", "HighAt", "LowAt", "CloseAt",
                "HL_AvgVolat", "OC_AvgVolat");
            SaveToDb.RunProcedure("pUpdateIntradayYahooQuotes");

            showStatus($"IntradayQuotes_SaveToDb. Finished!");
        }
    }
}
