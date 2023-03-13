using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Quote2022.Models
{
    public class MinuteYahoo
    {
        private class QuoteCorrection
        {
            public float[] PriceValues;
            // public int? VolumeFactor;
            public double? Split;
            public bool Remove = false;
            public bool PriceChecked = false;
            public bool SplitChecked = false;
        }

        public cChart Chart { get; set; }

        private static Dictionary<string, Dictionary<DateTime, QuoteCorrection>> _allCorrections = null;

        public static bool IsQuotePriceChecked(Quote q) => _allCorrections.ContainsKey(q.Symbol) &&
                                                           _allCorrections[q.Symbol].ContainsKey(q.Timed) &&
                                                           _allCorrections[q.Symbol][q.Timed].PriceChecked;
        public static bool IsQuoteSplitChecked(Quote q) => _allCorrections.ContainsKey(q.Symbol) &&
                                                           _allCorrections[q.Symbol].ContainsKey(q.Timed) &&
                                                           _allCorrections[q.Symbol][q.Timed].SplitChecked;

        public static void ClearCorrections() => _allCorrections = null;
        private static Dictionary<DateTime, QuoteCorrection> GetCorrections(string symbol)
        {
            if (_allCorrections == null)
            {
                _allCorrections=new Dictionary<string, Dictionary<DateTime, QuoteCorrection>>();
                var lines = File.ReadAllLines(Settings.MinuteYahooCorrectionFiles)
                    .Where(a => !string.IsNullOrEmpty(a) && !a.Trim().StartsWith("#"));
                foreach (var line in lines)
                {
                    var ss = line.Split('\t');
                    var symbolKey = ss[0].Trim().ToUpper();
                    if (!_allCorrections.ContainsKey(symbolKey))
                        _allCorrections.Add(symbolKey, new Dictionary<DateTime, QuoteCorrection>());

                    var a1 = _allCorrections[symbolKey];
                    var corr = new QuoteCorrection();
                    DateTime dateKey;
                    switch (ss[2].Trim().ToUpper())
                    {
                        case "REMOVE":
                        case "DELETE":
                            dateKey = DateTime.ParseExact(ss[1], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                            corr.Remove = true;
                            break;
                        case "PRICE":
                            dateKey = DateTime.ParseExact(ss[1], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                            corr.PriceValues = new float[4];
                            for (var k = 0; k < 4; k++)
                                corr.PriceValues[k] = float.Parse(ss[k + 3].Trim(), CultureInfo.InvariantCulture);
                            break;
                        case "SPLIT":
                            dateKey = DateTime.ParseExact(ss[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var f1 = double.Parse(ss[3].Trim(), CultureInfo.InvariantCulture);
                            var f2 = double.Parse(ss[4].Trim(), CultureInfo.InvariantCulture);
                            corr.Split = f1 / f2;
                            break;
                        case "PRICECHECKED":
                            dateKey = DateTime.ParseExact(ss[1], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                            corr.PriceChecked = true;
                            break;
                        case "SPLITCHECKED":
                            dateKey = DateTime.ParseExact(ss[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            corr.SplitChecked = true;
                            break;
                        /*case "VOLUME":
                            corr.VolumeFactor = int.Parse(ss[3].Trim());
                            break;*/
                        default:
                            throw new Exception($"Check MinuteYahoo correction file: {Settings.MinuteYahooCorrectionFiles}. '{ss[2]}' is invalid action");
                    }

                    a1.Add(dateKey, corr);
                }
            }

            return _allCorrections.ContainsKey(symbol) ? _allCorrections[symbol] : null;
        }

        private static DateTime TimeStampToDateTime(long timeStamp, IEnumerable<cTradingPeriod> periods)
        {
            var aa = periods.Where(p => p.Start <= timeStamp && p.End >= timeStamp).ToArray();
            if (aa.Length == 1)
                return (new DateTime(1970, 1, 1)).AddSeconds(timeStamp).AddSeconds(aa[0].GmtOffset);
            throw new Exception("Check TimeStampToDateTime procedure in Quote2022.Models.MinuteYahoo");
        }

        private string _metaSymbol => Chart.Result[0].Meta.Symbol;
        private Dictionary<DateTime, QuoteCorrection> _corrections;
        public List<Quote> GetQuotes(string symbol)
        {
            if (_metaSymbol != symbol)
                throw new Exception($"MinuteYahoo error. Different symbol. Filename symbol is '{symbol}, file context symbol is '{_metaSymbol}'");

            var quotes = new List<Quote>();
            _corrections = GetCorrections(symbol);
            if (Chart.Result[0].TimeStamp == null)
            {
                if (Chart.Result[0].Indicators.Quote[0].Close != null)
                    throw new Exception("Check Normilize procedure in  in Quote2022.Models.MinuteYahoo");
                return quotes;
            }

            var periods = new List<MinuteYahoo.cTradingPeriod>();
            var a1 = Chart.Result[0].Meta.TradingPeriods;
            var len1 = a1.GetLength(1);
            for (var k1 = 0; k1 < a1.Length; k1++)
            for (var k2 = 0; k2 < len1; k2++)
                periods.Add(a1[k1, k2]);
            
            periods = periods.OrderBy(a => a.Start).ToList();

            for (var k = 0; k < Chart.Result[0].TimeStamp.Length; k++)
            {
                if (Chart.Result[0].Indicators.Quote[0].Open[k].HasValue &&
                    Chart.Result[0].Indicators.Quote[0].High[k].HasValue &&
                    Chart.Result[0].Indicators.Quote[0].Low[k].HasValue &&
                    Chart.Result[0].Indicators.Quote[0].Close[k].HasValue &&
                    Chart.Result[0].Indicators.Quote[0].Volume[k].HasValue)
                {
                    var q = GetQuote(TimeStampToDateTime(Chart.Result[0].TimeStamp[k], periods), Chart.Result[0].Indicators.Quote[0], k);
                    if (q != null)
                        quotes.Add(q);
                }
                else if (!Chart.Result[0].Indicators.Quote[0].Open[k].HasValue &&
                         !Chart.Result[0].Indicators.Quote[0].High[k].HasValue &&
                         !Chart.Result[0].Indicators.Quote[0].Low[k].HasValue &&
                         !Chart.Result[0].Indicators.Quote[0].Close[k].HasValue &&
                         !Chart.Result[0].Indicators.Quote[0].Volume[k].HasValue)
                {
                }
                else
                    throw new Exception($"Please, check quote data for {Chart.Result[0].TimeStamp[k]} timestamp (k={k})");
            }

            if (quotes.Count > 0 && quotes[quotes.Count - 1].Timed.TimeOfDay == new TimeSpan(16, 0, 0))
                quotes.RemoveAt(quotes.Count - 1);

            return quotes;
        }

        private Quote GetQuote(DateTime timed, cQuote fileQuote, int quoteNo)
        {
            var qCorr = _corrections != null && _corrections.ContainsKey(timed) ? _corrections[timed] : new QuoteCorrection();
            var split = _corrections != null && _corrections.ContainsKey(timed.Date) ? _corrections[timed.Date].Split : null;

            if (qCorr.Remove) return null;

            var q = new Quote() {Symbol = _metaSymbol, Timed = timed, Volume = fileQuote.Volume[quoteNo].Value};
            if (qCorr.PriceValues == null)
            {
                if (split.HasValue)
                {
                    q.Open = Convert.ToSingle(Math.Round(fileQuote.Open[quoteNo].Value * split.Value, 4));
                    q.High = Convert.ToSingle(Math.Round(fileQuote.High[quoteNo].Value * split.Value, 4));
                    q.Low = Convert.ToSingle(Math.Round(fileQuote.Low[quoteNo].Value * split.Value, 4));
                    q.Close = Convert.ToSingle(Math.Round(fileQuote.Close[quoteNo].Value * split.Value, 4));
                }
                else
                {
                    q.Open = Convert.ToSingle(fileQuote.Open[quoteNo].Value);
                    q.High = Convert.ToSingle(fileQuote.High[quoteNo].Value);
                    q.Low = Convert.ToSingle(fileQuote.Low[quoteNo].Value);
                    q.Close = Convert.ToSingle(fileQuote.Close[quoteNo].Value);
                }
            }
            else
            {
                if (split.HasValue)
                {
                    q.Open = Convert.ToSingle(qCorr.PriceValues[0] * split.Value);
                    q.High = Convert.ToSingle(qCorr.PriceValues[1] * split.Value);
                    q.Low = Convert.ToSingle(qCorr.PriceValues[2] * split.Value);
                    q.Close = Convert.ToSingle(qCorr.PriceValues[3] * split.Value);
                }
                else
                {
                    q.Open = Convert.ToSingle(qCorr.PriceValues[0]);
                    q.High = Convert.ToSingle(qCorr.PriceValues[1]);
                    q.Low = Convert.ToSingle(qCorr.PriceValues[2]);
                    q.Close = Convert.ToSingle(qCorr.PriceValues[3]);
                }
            }

            return q;
        }

        #region ===============  SubClasses  ==================
        public class cChart
        {
            public cResult[] Result { get; set; }
            public cError Error { get; set; }
        }

        public class cError
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }

        public class cResult
        {
            public cMeta Meta { get; set; }
            public long[] TimeStamp { get; set; }
            public cIndicators Indicators { get; set; }
        }

        public class cMeta
        {
            public string Currency { get; set; }
            public string Symbol { get; set; }
            public string ExchangeName { get; set; }
            public string InstrumentType { get; set; }
            public cTradingPeriod[,] TradingPeriods { get; set; }
        }

        public class cIndicators
        {
            public cQuote[] Quote { get; set; }
        }

        public class cQuote
        {
          public double?[] Open { get; set; }
          public double?[] High { get; set; }
          public double?[] Low { get; set; }
          public double?[] Close { get; set; }
          public long?[] Volume { get; set; }
        }

        public class cTradingPeriod
        {
          public string TimeZone { get; set; }
          public long Start { get; set; }
          public long End { get; set; }
          public long GmtOffset { get; set; }
        }
        #endregion
    }
}
