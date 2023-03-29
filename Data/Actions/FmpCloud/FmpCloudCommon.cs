using System.Linq;
using Data.Helpers;

namespace Data.Actions.FmpCloud
{
    public static class FmpCloudCommon
    {
        public static string GetApiKey() => CsUtils.GetApiKeys("fmpcloud.io")[3];
        public static bool IsValidSymbol(string symbol)
        {
            if (symbol.Any(c => char.IsLower(c) || char.IsDigit(c) || c == '=' || c == '&'))
                return false;

            var i = symbol.IndexOf('.');
            if (i > 0)
            {
                if (i < symbol.Length - 2) // like XX.TO, 2 or more chars after dot
                    return false;
                else
                    return !(symbol.EndsWith(".L") || symbol.EndsWith(".F") || symbol.EndsWith(".V"));
            }
            return !symbol.EndsWith("-USD");
        }
    }
}
