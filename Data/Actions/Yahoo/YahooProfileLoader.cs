using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Actions.Yahoo
{
    public static class YahooProfileLoader
    {
        public static void Start(Action<string> logEvent)
        {
            logEvent($"YahooProfileLoader finished");
        }


    }
}
