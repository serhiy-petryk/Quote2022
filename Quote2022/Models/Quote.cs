using System;
using System.Collections.Generic;
using Quote2022.Helpers;

namespace Quote2022.Models
{
    public class Quote
    {
        public string Symbol;
        public DateTime Timed;
        public string TimeString => CsUtils.GetString(Timed);
        public float Open;
        public float High;
        public float Low;
        public float Close;
        public long Volume;
        public override string ToString() => Symbol + "\t" + CsUtils.GetString(Timed) + "\t" + Open + "\t" + High + "\t" + Low + "\t" + Close + "\t" + Volume;

        public string QuoteError =>
            (Open > High || Open < Low || Close > High || Close < Low || Low < 0)
                ? "BadPrices"
                : (Volume < 0 ? "BadVolume" : null);

        public Tuple<float, double, double> GetStatisticsValues(IntradayParameters intradayParameters)
        {  // return Item1 = Stop, Item2 = BuyProfitAmt, Item3 = SellProfitAmt
            double stop;
            if (intradayParameters.IsStopPercent)
            {
                stop = Math.Round(Open * Convert.ToDouble(intradayParameters.Stop) / 100.0, 2);
                if (stop < 0.01) stop = 0.01;
            }
            else
                stop = Convert.ToDouble(intradayParameters.Stop);

            var buyEndPrice = (Open - Low) < (stop - 0.00005) ? Close : Open - stop;
            var sellEndPrice = (High - Open) < (stop - 0.00005) ? Close : Open + stop;
            var fees = Convert.ToDouble(intradayParameters.Fees);
            var buyAmt = Math.Round(buyEndPrice - Open - fees, 4);
            var sellAmt = Math.Round(Open - sellEndPrice - fees, 4);
            return new Tuple<float, double, double>(Convert.ToSingle(stop), buyAmt, sellAmt);
        }
    }

    public class IntradayQuote : Quote
    {
        public int PriceId
        {
            get
            {
                if (Open < 1.0F) return 0;
                if (Open < 2.0F) return 1;
                if (Open < 5.0F) return 2;
                if (Open < 10.0F) return 3;
                if (Open < 20.0F) return 4;
                if (Open < 50.0F) return 5;
                if (Open < 100.0F) return 6;
                if (Open < 200.0F) return 7;
                return 8;
            }
        }

        public TimeSpan TimeFrameId;
    }

    public class IntradayYahooQuote 
    {
        public string Symbol;
        public DateTime Date;
        public float? Open;
        public float? High;
        public float? Low;
        public float? Close;
        public float? CloseInNextFrame;
        public int Volume;

        public short Count;
        public byte PreCount;
        public TimeSpan? OpenAt;
        public TimeSpan? HighAt;
        public TimeSpan? LowAt;
        public TimeSpan? CloseAt;

        public float? HL_AvgVolat;
        public float? OC_AvgVolat;
    }

    public class StatisticsQuote : IntradayQuote
    {
        public DateTime Date => Timed.Date;
        public TimeSpan Time => Timed.TimeOfDay;
        public float OpenToCL => Convert.ToSingle((1.0 - 1.0 * Open / Close) * 100.0);
        public double BuyPerc => BuyAmt / Open * 100.0;
        public double SellPerc => SellAmt / Open * 100.0;
        public double BuyAmt;
        public double SellAmt;
        public bool BuyWins => BuyAmt > 0.0;
        public bool SellWins => SellAmt > 0.0;
        public DateTime Week => CsUtils.GetFirstDayOfWeek(Timed);
        public byte DayOfWeek => Convert.ToByte((int)Timed.DayOfWeek);
        public float Stop;
        public bool IsStopPerc;
        public float Fees;

        public StatisticsQuote(IntradayQuote q, IntradayParameters intradayParameters)
        {
            Symbol = q.Symbol;
            TimeFrameId = q.TimeFrameId;
            Timed = q.Timed;
            Open = q.Open;
            High = q.High;
            Low = q.Low;
            Close = q.Close;
            Volume = q.Volume;

            IsStopPerc = intradayParameters.IsStopPercent;
            Fees = Convert.ToSingle(intradayParameters.Fees);

            var values = q.GetStatisticsValues(intradayParameters);
            Stop = values.Item1;
            BuyAmt = values.Item2;
            SellAmt = values.Item3;
        }
    }

    public class QuotesInfo
    {
        public int Count = 0;
        public DateTime From = DateTime.MaxValue;
        public DateTime To = DateTime.MinValue;
        public Dictionary<string, object> Symbols = new Dictionary<string, object>();

        public void Update(Quote quote)
        {
            Count++;
            if (quote.Timed < From) From = quote.Timed;
            if (quote.Timed > To) To = quote.Timed;
            if (!Symbols.ContainsKey(quote.Symbol)) Symbols.Add(quote.Symbol, null);
        }

        public string GetStatus() =>
            $"Data from {CsUtils.GetString(From)} to {CsUtils.GetString(To)}, {Symbols.Count} symbols, {Count:N0} quotes";
    }
}
