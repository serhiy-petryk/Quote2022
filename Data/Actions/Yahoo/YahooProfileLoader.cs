using System;

namespace Data.Actions.Yahoo
{
    public static class YahooProfileLoader
    {
        public static void Start(Action<string> logEvent)
        {
            logEvent($"YahooProfileLoader started");

            logEvent($"YahooProfileLoader finished");
        }


    }
}
