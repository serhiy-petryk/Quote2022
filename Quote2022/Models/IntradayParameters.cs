using System;
using System.Collections.Generic;
using System.Text;
using Quote2022.Helpers;

namespace Quote2022.Models
{
    public class IntradayParameters
    {
        public List<TimeSpan> TimeFrames;
        public bool CloseInNextFrame;
        public decimal Stop;
        public bool IsStopPercent;
        public decimal Fees;

        public string GetTimeFramesInfo()
        {
            var sbParameters = new StringBuilder();
            if (TimeFrames.Count > 1)
                sbParameters.Append($"Time frames: {CsUtils.GetString(TimeFrames[0])}-{CsUtils.GetString(TimeFrames[TimeFrames.Count - 1])}, interval: {CsUtils.GetString(TimeFrames[1] - TimeFrames[0])}");
            else if (TimeFrames.Count == 1)
                sbParameters.Append($"Time frames: {CsUtils.GetString(TimeFrames[0])}");

            if (CloseInNextFrame)
                sbParameters.Append(", closeInNextFrame");

            return sbParameters.ToString();
        }
        public string GetFeesInfo()
        {
            var sStopPercent = IsStopPercent ? "%" : "$";
            return $"Stop: {Stop}{sStopPercent}, fees: {Fees}$ per share";
        }

        public string GetFileNameSuffix()
        {
            var sStopPercent = IsStopPercent ? "P" : "";
            return $"{Convert.ToInt32(Stop * 100)}{sStopPercent}-{Convert.ToInt32(Fees * 1000)}";
        }
    }
}
