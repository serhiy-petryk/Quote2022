using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Quote2022.Helpers;

namespace Quote2022.Models
{
    public class QuoteGroup
    {
        public const double StartAmount = 1.0;
        public int Cnt;
        public float OpenToClose;
        public double D;
        public double H;
        public double L;
        public double R_N1;
        public double D_N1;
        public double Up_N1;
        public double Down_N1;
        public double H_N1;
        public double L_N1;

        public DateTime MinDate;
        public DateTime MaxDate;

        private int Cnt_N1;
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

        public double Limit30BuyAmt;
        public double Limit30BuyProfit;
        public int Limit30BuyStarted;
        public double Limit30BuyMinAmt;
        public double Limit30SellAmt;
        public double Limit30SellProfit;
        public int Limit30SellStarted;
        public double Limit30SellMinAmt;

        public QuoteGroup(IList<DayEoddataExtended> data, decimal stop = 0.01M, bool isStopPercent = false, bool printDetails = false)
        {
            Cnt = data.Count;
            OpenToClose = data.Average(a => a.OpenToClose);
            D = 100.0 * Math.Sqrt(data.Average(a => Math.Pow(a.OpenToClose - OpenToClose, 2)));
            H = 100.0 * data.Count(a => (a.Open - a.Low) < (Convert.ToDouble(isStopPercent ? Algorithm1.GetStopDeltaForPercent(stop, a.CL_P1) : stop) - 0.00005)) / Cnt;
            L = 100.0 * data.Count(a => (a.High - a.Open) < (Convert.ToDouble(isStopPercent ? Algorithm1.GetStopDeltaForPercent(stop, a.CL_P1) : stop) - 0.00005)) / Cnt;
            MinDate = data.Min(a => a.Date);
            MaxDate = data.Max(a => a.Date);

            var data_N1 = data.Where(a => a.IsValid1).ToList();
            Cnt_N1 = data_N1.Count;
            R_N1 = data_N1.Average(a => a.R1.Value);
            D_N1 = 100.0 * Math.Sqrt(data_N1.Average(a => Math.Pow(a.R1.Value - R_N1, 2)));
            var buyN1Cnt = data_N1.Count(a => a.Open_N1 < (a.CL_N1 - float.Epsilon));
            var sellN1Cnt = data_N1.Count(a => a.Open_N1 > (a.CL_N1 + float.Epsilon));
            Up_N1 = 100.0 * buyN1Cnt / data_N1.Count;
            Down_N1 = 100.0 * sellN1Cnt / data_N1.Count;
            H_N1 = 100.0 * data_N1.Count(a => (a.Open_N1 - a.Low_N1) < (Convert.ToDouble(isStopPercent ? Algorithm1.GetStopDeltaForPercent(stop, a.CL) : stop) - 0.00005)) / Cnt_N1;
            L_N1 = 100.0 * data_N1.Count(a => (a.High_N1 - a.Open_N1) < (Convert.ToDouble(isStopPercent ? Algorithm1.GetStopDeltaForPercent(stop, a.CL) : stop) - 0.00005)) / Cnt_N1;

            if (printDetails)
               Debug.Print($"BuyK\tBuyPrice\tSellK\tSellPrice\tSymbol\tDate\tPrevCL\tOpen\tHigh\tLow\tClose\tVolume");

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

            Limit30BuyAmt = StartAmount;
            Limit30BuyProfit = 0.0;
            Limit30BuyStarted = 0;
            Limit30BuyMinAmt = StartAmount;
            Limit30SellAmt = StartAmount;
            Limit30SellProfit = 0.0;
            Limit30SellStarted = 0;
            Limit30SellMinAmt = StartAmount;

            var cnt = 0;
            foreach (var o in data.Where(a => a.IsValid1).OrderBy(a => a.Date).ThenBy(a => a.Symbol))
            {
                cnt++;
                var stopAmt = isStopPercent ? Algorithm1.GetStopDeltaForPercent(stop, o.CL) : stop;
                var buyAbs = Algorithm1.BuyAbs(stopAmt, o.Open_N1, o.High_N1, o.Low_N1, o.CL_N1);
                var buyAbsPrice = Algorithm1.BuyAbsPrice(stopAmt, o.Open_N1, o.High_N1, o.Low_N1, o.CL_N1);
                var sellAbs = Algorithm1.SellAbs(stopAmt, o.Open_N1, o.High_N1, o.Low_N1, o.CL_N1);
                var sellAbsPrice = Algorithm1.SellAbsPrice(stopAmt, o.Open_N1, o.High_N1, o.Low_N1, o.CL_N1);

                BuyEndAmt = BuyEndAmt * Convert.ToDouble(buyAbs);
                SellEndAmt = SellEndAmt * Convert.ToDouble(sellAbs);

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

                if (buyAbsPrice > Convert.ToDecimal(o.Open_N1))
                {
                    BuyWins++;
                    currBuyMaxLossCnt=0;
                    StartBuyMaxLossKey = o.Symbol + "/" + o.Date.ToString("yyyy-MM-dd");
                }
                //  else if (buyAbsPrice < Convert.ToDecimal(o.Open_N1))
                else
                {
                    currBuyMaxLossCnt++;
                    if (currBuyMaxLossCnt > BuyMaxLossCnt)
                    {
                        BuyMaxLossCnt = currBuyMaxLossCnt;
                        BuyMaxLossKey = StartBuyMaxLossKey + " - " + o.Symbol + "/" + o.Date.ToString("yyyy-MM-dd");
                    }
                }

                if (sellAbsPrice < Convert.ToDecimal(o.Open_N1))
                {
                    SellWins++;
                    currSellMaxLossCnt = 0;
                    StartSellMaxLossKey = o.Symbol + "/" + o.Date.ToString("yyyy-MM-dd");
                }
                // else if (sellAbsPrice > Convert.ToDecimal(o.Open_N1))
                else
                {
                    currSellMaxLossCnt++;
                    if (currSellMaxLossCnt > SellMaxLossCnt)
                    {
                        SellMaxLossCnt = currSellMaxLossCnt;
                        SellMaxLossKey = StartSellMaxLossKey + " - " + o.Symbol + "/" + o.Date.ToString("yyyy-MM-dd");
                    }
                };

                // Limit x1
                Limit1BuyAmt = Limit1BuyAmt * Convert.ToDouble(buyAbs);
                if (Limit1BuyAmt > 10.0 * StartAmount)
                {
                    Limit1BuyProfit += Limit1BuyAmt - 10.0 * StartAmount;
                    Limit1BuyAmt = 10.0 * StartAmount;
                    if (Limit1BuyStarted == 0) Limit1BuyStarted = cnt;
                }
                else if (Limit1BuyProfit < double.Epsilon && Limit1BuyMinAmt > Limit1BuyAmt)
                    Limit1BuyMinAmt = Limit1BuyAmt;

                Limit1SellAmt = Limit1SellAmt * Convert.ToDouble(sellAbs);
                if (Limit1SellAmt > 10.0 * StartAmount)
                {
                    Limit1SellProfit += Limit1SellAmt - 10.0 * StartAmount;
                    Limit1SellAmt = 10.0 * StartAmount;
                    if (Limit1SellStarted == 0) Limit1SellStarted = cnt;
                }
                else if (Limit1SellProfit < double.Epsilon && Limit1SellMinAmt > Limit1SellAmt)
                    Limit1SellMinAmt = Limit1SellAmt;

                // Limit x3
                Limit3BuyAmt = Limit3BuyAmt * Convert.ToDouble(buyAbs);
                if (Limit3BuyAmt > 3.0 * StartAmount)
                {
                    Limit3BuyProfit += Limit3BuyAmt - 3.0 * StartAmount;
                    Limit3BuyAmt = 3.0 * StartAmount;
                    if (Limit3BuyStarted == 0) Limit3BuyStarted = cnt;
                }
                else if (Limit3BuyProfit < double.Epsilon && Limit3BuyMinAmt > Limit3BuyAmt)
                    Limit3BuyMinAmt = Limit3BuyAmt;

                Limit3SellAmt = Limit3SellAmt * Convert.ToDouble(sellAbs);
                if (Limit3SellAmt > 3.0 * StartAmount)
                {
                    Limit3SellProfit += Limit3SellAmt - 3.0 * StartAmount;
                    Limit3SellAmt = 3.0 * StartAmount;
                    if (Limit3SellStarted == 0) Limit3SellStarted = cnt;
                }
                else if (Limit3SellProfit < double.Epsilon && Limit3SellMinAmt > Limit3SellAmt)
                    Limit3SellMinAmt = Limit3SellAmt;

                // Limit x10
                Limit10BuyAmt = Limit10BuyAmt * Convert.ToDouble(buyAbs);
                if (Limit10BuyAmt > 10.0 * StartAmount)
                {
                    Limit10BuyProfit += Limit10BuyAmt - 10.0 * StartAmount;
                    Limit10BuyAmt = 10.0 * StartAmount;
                    if (Limit10BuyStarted == 0) Limit10BuyStarted = cnt;
                }
                else if (Limit10BuyProfit < double.Epsilon && Limit10BuyMinAmt > Limit10BuyAmt)
                    Limit10BuyMinAmt = Limit10BuyAmt;

                Limit10SellAmt = Limit10SellAmt * Convert.ToDouble(sellAbs);
                if (Limit10SellAmt > 10.0 * StartAmount)
                {
                    Limit10SellProfit += Limit10SellAmt - 10.0 * StartAmount;
                    Limit10SellAmt = 10.0 * StartAmount;
                    if (Limit10SellStarted == 0) Limit10SellStarted = cnt;
                }
                else if (Limit10SellProfit < double.Epsilon && Limit10SellMinAmt > Limit10SellAmt)
                    Limit10SellMinAmt = Limit10SellAmt;

                // Limit x30
                Limit30BuyAmt = Limit30BuyAmt * Convert.ToDouble(buyAbs);
                if (Limit30BuyAmt > 30.0 * StartAmount)
                {
                    Limit30BuyProfit += Limit30BuyAmt - 30.0 * StartAmount;
                    Limit30BuyAmt = 30.0 * StartAmount;
                    if (Limit30BuyStarted == 0) Limit30BuyStarted = cnt;
                }
                else if (Limit30BuyProfit < double.Epsilon && Limit30BuyMinAmt > Limit30BuyAmt)
                    Limit30BuyMinAmt = Limit30BuyAmt;

                Limit30SellAmt = Limit30SellAmt * Convert.ToDouble(sellAbs);
                if (Limit30SellAmt > 30.0 * StartAmount)
                {
                    Limit30SellProfit += Limit30SellAmt - 30.0 * StartAmount;
                    Limit30SellAmt = 30.0 * StartAmount;
                    if (Limit30SellStarted == 0) Limit30SellStarted = cnt;
                }
                else if (Limit30SellProfit < double.Epsilon && Limit30SellMinAmt > Limit30SellAmt)
                    Limit30SellMinAmt = Limit30SellAmt;

                if (printDetails)
                    Debug.Print(buyAbs + "\t" + buyAbsPrice + "\t" + sellAbs + "\t" + sellAbsPrice + "\t" + o.Symbol +
                                "\t" + o.Date.ToString("dd.MM.yyyy") + "\t" + o.CL + "\t" + o.Open_N1 + "\t" +
                                o.High_N1 + "\t" + o.Low_N1 + "\t" + o.CL_N1 + "\t" + o.VLM_N1.ToString("F0"));

                string GetString(double amt)
                {
                    return DoubleDoString(amt) + ", " + o.Symbol + "/" + o.Date.ToString("yyyy-MM-dd");
                }
            }

        }

        public static string GetHeader1() => "Cnt\tOpnToCL\tD\tH\tL\tMinDate\tMaxDate\t\t" +
                                             $"R_N1\tD_N1\tH_N1\tL_N1\tUp, %\tDown, %\tBuy EndAmt\tSell EndAmt\t" +
                                             $"Buy Wins,%\tSell Wins,%\tBuyMax LossCnt\tSellMax LossCnt\t" +
                                             $"BuyDraw Up,%\tBuyDraw Down,%\tSellDraw Up,%\tSellDraw Down,%\t" +
                                             $"Limit3 BuyCnt\tLimit3 SellCnt\t\t" +
                                             $"Limit1Buy\tLimit1Sell\tLimit3Buy\tLimit3Sell\tLimit10Buy\tLimit10Sell\t" +
                                             $"BuyDrawUpKey\tBuyDrawDownKey\tSellDrawUpKey\tSellDrawDownKey\t" +
                                             $"BuyMaxLossKey\tSellMaxLossKey";
        public static string GetHeader2() => "Cnt\tOpnToCL\tD\tH\tL\t\t" +
                                             $"R_N1\tD_N1\tH_N1\tL_N1\tUp %\tDown %\tBuy EndAmt\tSell EndAmt\t" +
                                             $"Buy Wins,%\tSell Wins,%\tBuyMax LossCnt\tSellMax LossCnt\t" +
                                             $"BuyDraw Up,%\tBuyDraw Down,%\tSellDraw Up,%\tSellDraw Down,%\t" +
                                             $"Limit3 BuyCnt\tLimit3 SellCnt\t\t" +
                                             $"Limit1Buy\tLimit1Sell\tLimit3Buy\tLimit3Sell\tLimit10Buy\tLimit10Sell\t" +
                                             $"BuyDrawUpKey\tBuyDrawDownKey\tSellDrawUpKey\tSellDrawDownKey\t" +
                                             $"BuyMaxLossKey\tSellMaxLossKey";
        public string GetContent1() =>
            $"{Cnt}\t{OpenToClose}\t{Math.Round(D, 1)}\t{Math.Round(H, 2)}\t{Math.Round(L, 2)}\t{MinDate:dd.MM.yyyy}\t{MaxDate:dd.MM.yyyy}\t\t" +
            $"{R_N1}\t{Math.Round(D_N1, 1)}\t{Math.Round(H_N1, 2)}\t{Math.Round(L_N1, 2)}\t" +
            $"{Math.Round(Up_N1, 1)}\t{Math.Round(Down_N1, 1)}\t{BuyEndAmt}\t{SellEndAmt}\t" +
            $"{Math.Round(100.0 * BuyWins / Cnt_N1, 2)}\t{Math.Round(100.0 * SellWins / Cnt_N1, 2)}\t" +
            $"{BuyMaxLossCnt}\t{SellMaxLossCnt}\t" +
            $"{BuyDrawUp * 100.0}\t{100.0 / BuyDrawDown}\t" + $"{SellDrawUp * 100.0}\t{100.0 / SellDrawDown}\t" +
            $"{Limit3BuyStarted}\t{Limit3SellStarted}\t\t" +
            $"{GetLimitString(Limit1BuyStarted, Limit1BuyProfit, Limit1BuyAmt, Limit1BuyMinAmt)}\t" +
            $"{GetLimitString(Limit1SellStarted, Limit1SellProfit, Limit1SellAmt, Limit1SellMinAmt)}\t" +
            $"{GetLimitString(Limit3BuyStarted, Limit3BuyProfit, Limit3BuyAmt, Limit3BuyMinAmt)}\t" +
            $"{GetLimitString(Limit3SellStarted, Limit3SellProfit, Limit3SellAmt, Limit3SellMinAmt)}\t" +
            $"{GetLimitString(Limit10BuyStarted, Limit10BuyProfit, Limit10BuyAmt, Limit10BuyMinAmt)}\t" +
            $"{GetLimitString(Limit10SellStarted, Limit10SellProfit, Limit10SellAmt, Limit10SellMinAmt)}\t" +
            $"{BuyDrawUpKey}\t{BuyDrawDownKey}\t{SellDrawUpKey}\t{SellDrawDownKey}\t" +
            $"{BuyMaxLossKey}\t{SellMaxLossKey}";
        public string GetContent2() =>
            $"{Cnt}\t{OpenToClose}\t{Math.Round(D, 1)}\t{Math.Round(H, 2)}\t{Math.Round(L, 2)}\t\t" +
            $"{R_N1}\t{Math.Round(D_N1, 1)}\t{Math.Round(H_N1, 2)}\t{Math.Round(L_N1, 2)}\t" +
            $"{Math.Round(Up_N1, 1)}\t{Math.Round(Down_N1, 1)}\t{BuyEndAmt}\t{SellEndAmt}\t" +
            $"{Math.Round(100.0 * BuyWins / Cnt_N1, 2)}\t{Math.Round(100.0 * SellWins / Cnt_N1, 2)}\t" +
            $"{BuyMaxLossCnt}\t{SellMaxLossCnt}\t" +
            $"{BuyDrawUp * 100.0}\t{100.0 / BuyDrawDown}\t" + $"{SellDrawUp * 100.0}\t{100.0 / SellDrawDown}\t" +
            $"{Limit3BuyStarted}\t{Limit3SellStarted}\t\t" +
            $"{GetLimitString(Limit1BuyStarted, Limit1BuyProfit, Limit1BuyAmt, Limit1BuyMinAmt)}\t" +
            $"{GetLimitString(Limit1SellStarted, Limit1SellProfit, Limit1SellAmt, Limit1SellMinAmt)}\t" +
            $"{GetLimitString(Limit3BuyStarted, Limit3BuyProfit, Limit3BuyAmt, Limit3BuyMinAmt)}\t" +
            $"{GetLimitString(Limit3SellStarted, Limit3SellProfit, Limit3SellAmt, Limit3SellMinAmt)}\t" +
            $"{GetLimitString(Limit10BuyStarted, Limit10BuyProfit, Limit10BuyAmt, Limit10BuyMinAmt)}\t" +
            $"{GetLimitString(Limit10SellStarted, Limit10SellProfit, Limit10SellAmt, Limit10SellMinAmt)}\t" +
            $"{BuyDrawUpKey}\t{BuyDrawDownKey}\t{SellDrawUpKey}\t{SellDrawDownKey}\t" +
            $"{BuyMaxLossKey}\t{SellMaxLossKey}";
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
