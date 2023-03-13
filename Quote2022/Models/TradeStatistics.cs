using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Quote2022.Models
{
    public class TradeStatistics
    {
        public const double StartAmount = 1.0;
        public double OpenToClose;
        public double OpenToCloseStDev;
        public double UpPercent;
        public double DownPercent;

        private int Cnt;
        public double BuyProfitPerc;
        public double SellProfitPerc;
        public double BuyEndAmt2;
        public double SellEndAmt2;
        public double BuyK;
        public double SellK;
        public double BuyEndAmt;
        public double SellEndAmt;
        public int BuyWins;
        public int SellWins;

        public double BuyDrawUp;
        public double BuyDrawDown;
        public double SellDrawUp;
        public double SellDrawDown;
        public string BuyDrawUpKey;
        public string BuyDrawDownKey;
        public string SellDrawUpKey;
        public string SellDrawDownKey;
        
        public int BuyMaxLossCnt;
        public int SellMaxLossCnt;
        public string BuyMaxLossKey;
        public string SellMaxLossKey;
        public string StartBuyMaxLossKey;
        public string StartSellMaxLossKey;

        public double Limit1BuyAmt;
        public double Limit1BuyProfit;
        public int Limit1BuyStarted;
        public double Limit1BuyMinAmt;
        public double Limit1SellAmt;
        public double Limit1SellProfit;
        public int Limit1SellStarted;
        public double Limit1SellMinAmt;

        public double Limit3BuyAmt;
        public double Limit3BuyProfit;
        public int Limit3BuyStarted;
        public double Limit3BuyMinAmt;
        public double Limit3SellAmt;
        public double Limit3SellProfit;
        public int Limit3SellStarted;
        public double Limit3SellMinAmt;

        public double Limit10BuyAmt;
        public double Limit10BuyProfit;
        public int Limit10BuyStarted;
        public double Limit10BuyMinAmt;
        public double Limit10SellAmt;
        public double Limit10SellProfit;
        public int Limit10SellStarted;
        public double Limit10SellMinAmt;

        public TradeStatistics(IEnumerable<Quote> data, IntradayParameters iParameters,  bool printDetails = false)
        {
            if (printDetails)
               Debug.Print($"BuyK\tBuyPrice\tSellK\tSellPrice\tSymbol\tDate\tOpen\tHigh\tLow\tClose\tVolume");

            BuyEndAmt = StartAmount;
            SellEndAmt = StartAmount;
            BuyWins = 0;
            SellWins = 0;
            BuyMaxLossCnt = 0;
            SellMaxLossCnt = 0;
            BuyMaxLossKey = null;
            SellMaxLossKey = null;
            var currBuyMaxLossCnt = 0;
            var currSellMaxLossCnt = 0;

            BuyDrawUp = 1.0;
            BuyDrawDown = 1.0;
            BuyDrawUpKey = null;
            BuyDrawDownKey = null;
            string sBuyMin = null;
            string sBuyMax = null;
            var buyMin = StartAmount;
            var buyMax = StartAmount;

            SellDrawUp = 1.0;
            SellDrawDown = 1.0;
            SellDrawUpKey = null;
            SellDrawDownKey = null;
            string sSellMin = null;
            string sSellMax = null;
            var sellMin = StartAmount;
            var sellMax = StartAmount;

            Limit1BuyAmt = StartAmount;
            Limit1BuyProfit = 0.0;
            Limit1BuyStarted = 0;
            Limit1BuyMinAmt = StartAmount;
            Limit1SellAmt = StartAmount;
            Limit1SellProfit = 0.0;
            Limit1SellStarted = 0;
            Limit1SellMinAmt = StartAmount;

            Limit3BuyAmt = StartAmount;
            Limit3BuyProfit = 0.0;
            Limit3BuyStarted = 0;
            Limit3BuyMinAmt = StartAmount;
            Limit3SellAmt = StartAmount;
            Limit3SellProfit = 0.0;
            Limit3SellStarted = 0;
            Limit3SellMinAmt = StartAmount;

            Limit10BuyAmt = StartAmount;
            Limit10BuyProfit = 0.0;
            Limit10BuyStarted = 0;
            Limit10BuyMinAmt = StartAmount;
            Limit10SellAmt = StartAmount;
            Limit10SellProfit = 0.0;
            Limit10SellStarted = 0;
            Limit10SellMinAmt = StartAmount;

            Cnt = 0;
            OpenToClose = 0.0;
            OpenToCloseStDev = 0.0;
            var buyN1Cnt = 0;
            var sellN1Cnt = 0;
            BuyK = 0.0;
            SellK = 0.0;
            var meanStDev = 0.0;
            var sumStDev = 0.0;
            foreach (var o in data)
            {
                Cnt++;
                var openToClose = 1.0 * o.Open / o.Close;
                OpenToClose += openToClose;
                
                var delta = openToClose - meanStDev;
                meanStDev += delta / Cnt;
                sumStDev += delta * (openToClose - meanStDev);

                buyN1Cnt += o.Open < (o.Close - float.Epsilon) ? 1 : 0;
                sellN1Cnt += o.Open > (o.Close + float.Epsilon) ? 1 : 0;

                var values = o.GetStatisticsValues(iParameters); // Item1 = Stop, Item2 = BuyProfitAmt, Item3 = SellProfitAmt
                var buyEndPrice = o.Open + values.Item2;
                var sellEndPrice = o.Open - values.Item3;
                var buyK = buyEndPrice / o.Open;
                var sellK = o.Open/sellEndPrice;

                BuyK += buyK;
                SellK += sellK;

                BuyEndAmt = BuyEndAmt * buyK;
                SellEndAmt = SellEndAmt * sellK;

                // Buy/Sell DrawUp/Down
                if (BuyEndAmt < buyMin)
                {
                    sBuyMin = GetString(BuyEndAmt);
                    buyMin = BuyEndAmt;
                }
                if (BuyEndAmt > buyMax)
                {
                    sBuyMax = GetString(BuyEndAmt);
                    buyMax = BuyEndAmt;
                }
                if (SellEndAmt < sellMin)
                {
                    sSellMin = GetString(SellEndAmt);
                    sellMin = SellEndAmt;
                }
                if (SellEndAmt > sellMax)
                {
                    sSellMax = GetString(SellEndAmt);
                    sellMax = SellEndAmt;
                }

                if (BuyEndAmt / buyMin > BuyDrawUp)
                {
                    BuyDrawUp = BuyEndAmt / buyMin;
                    BuyDrawUpKey = sBuyMin + "=>" + GetString(BuyEndAmt);
                }
                if (buyMax / BuyEndAmt > BuyDrawDown) {
                    BuyDrawDown = buyMax / BuyEndAmt;
                    BuyDrawDownKey = sBuyMax + "=>" + GetString(BuyEndAmt);
                }
                if (SellEndAmt / sellMin > SellDrawUp)
                {
                    SellDrawUp = SellEndAmt / sellMin;
                    SellDrawUpKey = sSellMin + "=>" + GetString(SellEndAmt);
                }
                if (sellMax / SellEndAmt > SellDrawDown)
                {
                    SellDrawDown = sellMax / SellEndAmt;
                    SellDrawDownKey = sSellMax + "=>" + GetString(SellEndAmt);
                    // SellDrawDownKey = DoubleDoString(100.0 / SellDrawDown) + "%: " + sSellMax + "=>" + GetString(SellAmt);
                }

                if (values.Item2 > 0) // Item2 = BuyProfitAmt
                {
                    BuyWins++;
                    currBuyMaxLossCnt=0;
                    StartBuyMaxLossKey = o.Symbol + "/" + o.TimeString;
                }
                //  else if (buyAbsPrice < Convert.ToDecimal(o.Open_N1))
                else
                {
                    currBuyMaxLossCnt++;
                    if (currBuyMaxLossCnt > BuyMaxLossCnt)
                    {
                        BuyMaxLossCnt = currBuyMaxLossCnt;
                        BuyMaxLossKey = StartBuyMaxLossKey + " - " + o.Symbol + "/" + o.TimeString;
                    }
                }

                if (values.Item3 > 0) // Item3 = SellProfitAmt
                {
                    SellWins++;
                    currSellMaxLossCnt = 0;
                    StartSellMaxLossKey = o.Symbol + "/" + o.TimeString;
                }
                // else if (sellAbsPrice > Convert.ToDecimal(o.Open_N1))
                else
                {
                    currSellMaxLossCnt++;
                    if (currSellMaxLossCnt > SellMaxLossCnt)
                    {
                        SellMaxLossCnt = currSellMaxLossCnt;
                        SellMaxLossKey = StartSellMaxLossKey + " - " + o.Symbol + "/" + o.TimeString;
                    }
                };

                // Limit x1
                Limit1BuyAmt = Limit1BuyAmt * Convert.ToDouble(buyK);
                if (Limit1BuyAmt > StartAmount)
                {
                    Limit1BuyProfit += Limit1BuyAmt - StartAmount;
                    Limit1BuyAmt = StartAmount;
                    if (Limit1BuyStarted == 0) Limit1BuyStarted = Cnt;
                }
                else if (Limit1BuyProfit < double.Epsilon && Limit1BuyMinAmt > Limit1BuyAmt)
                    Limit1BuyMinAmt = Limit1BuyAmt;

                Limit1SellAmt = Limit1SellAmt * Convert.ToDouble(sellK);
                if (Limit1SellAmt > StartAmount)
                {
                    Limit1SellProfit += Limit1SellAmt - StartAmount;
                    Limit1SellAmt = StartAmount;
                    if (Limit1SellStarted == 0) Limit1SellStarted = Cnt;
                }
                else if (Limit1SellProfit < double.Epsilon && Limit1SellMinAmt > Limit1SellAmt)
                    Limit1SellMinAmt = Limit1SellAmt;

                // Limit x3
                Limit3BuyAmt = Limit3BuyAmt * Convert.ToDouble(buyK);
                if (Limit3BuyAmt > 3.0 * StartAmount)
                {
                    Limit3BuyProfit += Limit3BuyAmt - 3.0 * StartAmount;
                    Limit3BuyAmt = 3.0 * StartAmount;
                    if (Limit3BuyStarted == 0) Limit3BuyStarted = Cnt;
                }
                else if (Limit3BuyProfit < double.Epsilon && Limit3BuyMinAmt > Limit3BuyAmt)
                    Limit3BuyMinAmt = Limit3BuyAmt;

                Limit3SellAmt = Limit3SellAmt * Convert.ToDouble(sellK);
                if (Limit3SellAmt > 3.0 * StartAmount)
                {
                    Limit3SellProfit += Limit3SellAmt - 3.0 * StartAmount;
                    Limit3SellAmt = 3.0 * StartAmount;
                    if (Limit3SellStarted == 0) Limit3SellStarted = Cnt;
                }
                else if (Limit3SellProfit < double.Epsilon && Limit3SellMinAmt > Limit3SellAmt)
                    Limit3SellMinAmt = Limit3SellAmt;

                // Limit x10
                Limit10BuyAmt = Limit10BuyAmt * Convert.ToDouble(buyK);
                if (Limit10BuyAmt > 10.0 * StartAmount)
                {
                    Limit10BuyProfit += Limit10BuyAmt - 10.0 * StartAmount;
                    Limit10BuyAmt = 10.0 * StartAmount;
                    if (Limit10BuyStarted == 0) Limit10BuyStarted = Cnt;
                }
                else if (Limit10BuyProfit < double.Epsilon && Limit10BuyMinAmt > Limit10BuyAmt)
                    Limit10BuyMinAmt = Limit10BuyAmt;

                Limit10SellAmt = Limit10SellAmt * Convert.ToDouble(sellK);
                if (Limit10SellAmt > 10.0 * StartAmount)
                {
                    Limit10SellProfit += Limit10SellAmt - 10.0 * StartAmount;
                    Limit10SellAmt = 10.0 * StartAmount;
                    if (Limit10SellStarted == 0) Limit10SellStarted = Cnt;
                }
                else if (Limit10SellProfit < double.Epsilon && Limit10SellMinAmt > Limit10SellAmt)
                    Limit10SellMinAmt = Limit10SellAmt;

                if (printDetails)
                    Debug.Print(buyK + "\t" + buyEndPrice + "\t" + sellK + "\t" + sellEndPrice + "\t" + o.Symbol +
                                "\t" + o.TimeString + "\t" + o.Open + "\t" +
                                o.High + "\t" + o.Low + "\t" + o.Close + "\t" + o.Volume.ToString("F0"));

                string GetString(double amt)
                {
                    return DoubleDoString(amt) + ", " + o.Symbol + "/" + o.TimeString;
                }
            }

            if (Cnt > 0)
            {
                OpenToClose /= Cnt;
                UpPercent = 100.0 * buyN1Cnt / Cnt;
                DownPercent = 100.0 * sellN1Cnt / Cnt;
                BuyK /= Cnt;
                SellK /= Cnt;
            }

            if (Cnt > 1)
                OpenToCloseStDev = 100.0 * Math.Sqrt(sumStDev / (Cnt - 1));
        }

        public static string[] GetHeaders(string[] startHeaders) => startHeaders.Concat(new[]
        {
            "Cnt", "(1-Open/CL),%", "StDev", "Up, %", "Down, %", "BuyK", "SellK", "(BuyK+ SellK)/2", "Buy EndAmt",
            "Sell EndAmt", "Buy Wins,%", "Sell Wins,%", "BuyMax LossCnt", "SellMax LossCnt", "BuyDraw Up,%",
            "BuyDraw Down,%", "SellDraw Up,%", "SellDraw Down,%", "Limit3 BuyCnt", "Limit3 SellCnt", "", "Limit1Buy",
            "Limit1Sell", "Limit3Buy", "Limit3Sell", "BuyDrawUpKey", "BuyDrawDownKey", "SellDrawUpKey",
            "SellDrawDownKey", "BuyMaxLossKey", "SellMaxLossKey"
        }).ToArray();

        public object[] GetValues(object[] startValues) => startValues.Concat(new object[]
        {
            Cnt, (1.0 - OpenToClose) * 100.0, Math.Round(OpenToCloseStDev, 2), Math.Round(UpPercent, 1),
            Math.Round(DownPercent, 1), (BuyK - 1.0) * 100.0, (SellK - 1.0) * 100.0,
            (BuyK - 1.0) * 50.0 + (SellK - 1.0) * 50.0, BuyEndAmt, SellEndAmt, Math.Round(100.0 * BuyWins / Cnt, 2),
            Math.Round(100.0 * SellWins / Cnt, 2), BuyMaxLossCnt, SellMaxLossCnt, BuyDrawUp * 100.0,
            100.0 / BuyDrawDown, SellDrawUp * 100.0, 100.0 / SellDrawDown, Limit3BuyStarted, Limit3SellStarted, null,
            GetLimitString(Limit1BuyStarted, Limit1BuyProfit, Limit1BuyAmt, Limit1BuyMinAmt),
            GetLimitString(Limit1SellStarted, Limit1SellProfit, Limit1SellAmt, Limit1SellMinAmt),
            GetLimitString(Limit3BuyStarted, Limit3BuyProfit, Limit3BuyAmt, Limit3BuyMinAmt),
            GetLimitString(Limit3SellStarted, Limit3SellProfit, Limit3SellAmt, Limit3SellMinAmt), BuyDrawUpKey,
            BuyDrawDownKey, SellDrawUpKey, SellDrawDownKey, BuyMaxLossKey, SellMaxLossKey
        }).ToArray();

        private static string DoubleDoString(double amt)
        {
            if (Math.Abs(amt) < double.Epsilon) return "0";
            if (Math.Abs(amt) < 1) return amt.ToString("F4", CultureInfo.InvariantCulture);
            if (Math.Abs(amt) < 10) return amt.ToString("F2", CultureInfo.InvariantCulture);
            if (Math.Abs(amt) < 1000) return amt.ToString("F1", CultureInfo.InvariantCulture);
            if (Math.Abs(amt) < 1000000) return amt.ToString("F0", CultureInfo.InvariantCulture);
            return amt.ToString("E2", CultureInfo.InvariantCulture);
        }

        private static string GetLimitString(int limitStarted, double limitProfit, double limitAmt, double limitMinAmt) =>
            $"{limitStarted}/{DoubleDoString(limitProfit)}/{DoubleDoString(limitAmt)}/{DoubleDoString(limitMinAmt)}";
    }
}
