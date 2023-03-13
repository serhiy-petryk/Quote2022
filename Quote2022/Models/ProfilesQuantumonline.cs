using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Quote2022.Models
{
    public class ProfilesQuantumonline
    {
        private static string[] otherRowTypes = new[]
        {
            "Company's Online Profile", "Link to IPO Prospectus", "Link to Preliminary IPO Prospectus",
            "Link to Free Writing IPO Prospectus", "Click for current", "Go to Parent Company's Record",
            "Find All Related Securities for"
        };

        public string SymbolKey;
        public string Symbol = null;
        public string CUSIP = null;
        public string Exchange = null; // may be null
        public string PrevCUSIP = null;
        public string Name;

        public string NewSymbol = null;
        public DateTime? NewSymbolChanged = null;
        public string NewName = null;
        public string PrevSymbol = null; // https://www.quantumonline.com/search.cfm?tickersymbol=TMPO&sopt=symbol
        public DateTime? PrevSymbolChanged = null;
        public string PrevName = null; // see AA: Alcoa
        public DateTime? PrevNameChanged = null;
        DateTime? Changed = null; // ????

        public DateTime? IpoDate = null;
        public float? IpoSize = null;
        public string IpoPrice = null;

        public string Type = null;
        public string Subtype = null;
        public string CapStockType = null;
        public float? MarketCap = null;

        public bool IsDead => SymbolKey.EndsWith("*");
        public bool IsInvalid = false;
        public DateTime TimeStamp;

        public ProfilesQuantumonline(string htmlFileContent, string fileNameWithoutExtension, DateTime timeStamp)
        {
            TimeStamp = timeStamp;

            if (htmlFileContent.IndexOf("Not Found!", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                IsInvalid = true;
                Debug.Print($"Check 'not found symbol' for '{fileNameWithoutExtension}' file");
                return;
            }

            var i1 = htmlFileContent.IndexOf("<table bgcolor=\"#DCFDD7\"", StringComparison.InvariantCultureIgnoreCase);
            if (i1 < 0) throw new Exception("Check ProfileQuantumonline_Parse procedure");
            var i2 = htmlFileContent.IndexOf(@"Company's Online Information Links", i1, StringComparison.InvariantCultureIgnoreCase);
            if (i2 < 0) throw new Exception("Check ProfileQuantumonline_Parse procedure");
            var i3 = htmlFileContent.Substring(0, i2).LastIndexOf("<p>", StringComparison.InvariantCultureIgnoreCase);
            if (i3 < 0) throw new Exception("Check ProfileQuantumonline_Parse procedure");

            var s = htmlFileContent.Substring(i1 + 20, i3 - i1 - 20);

            // Remove tables
            i2 = s.IndexOf("</table>", StringComparison.InvariantCultureIgnoreCase);
            while (i2 != -1)
            {
                i1 = s.LastIndexOf("<table", i2, StringComparison.InvariantCultureIgnoreCase);
                s = s.Substring(0, i1) + s.Substring(i2 + 8);
                i2 = s.IndexOf("</table>", StringComparison.InvariantCultureIgnoreCase);
            }

            var ss1 = s.Split(new string[] { "</font>" }, StringSplitOptions.RemoveEmptyEntries);
            var rows = new List<string>();
            var rowAttributes = new List<string>();

            foreach (var s1 in ss1)
            {
                i1 = s1.LastIndexOf("<font", StringComparison.InvariantCultureIgnoreCase);
                i2 = s1.IndexOf(">", i1 + 5, StringComparison.InvariantCultureIgnoreCase);
                var s2 = RemoveTags(s1.Substring(i2 + 1)).Trim();
                if (!string.IsNullOrEmpty(s2))
                {
                    rows.Add(s2);
                    var attribute = s1.Substring(i1 + 5, i2 - i1 - 5).Trim();
                    rowAttributes.Add(attribute);
                }
            }

            if (rows.Count < 2)
                throw new Exception("Check ProfileQuantumonline_Parse procedure");
            if (rowAttributes[0].IndexOf("+1", StringComparison.InvariantCulture) == -1)
                throw new Exception("Check ProfileQuantumonline_Parse procedure");
            if (rows[1].IndexOf("Ticker Symbol:", StringComparison.InvariantCultureIgnoreCase) == -1)
                throw new Exception("Check ProfileQuantumonline_Parse procedure");

            // Set main properties
            Name = rows[0];
            var ss2 = rows[1].Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s2 in ss2)
            {
                var k = s2.IndexOf(':');
                var key = s2.Substring(0, k).Trim();
                var value = s2.Substring(k + 1).Trim();
                if (key == "Ticker Symbol")
                    SymbolKey = value;
                else if (key == "Exchange")
                {
                    i1 = value.IndexOf('*');
                    if (i1 != -1)
                        value = value.Substring(0, i1).Trim();
                    i1 = value.IndexOf('\r');
                    if (i1 != -1)
                        value = value.Substring(0, i1).Trim();
                    Exchange = value;
                }
                else if (key == "CUSIP")
                    CUSIP = value;
                else if (key == "Previous CUSIP")
                    PrevCUSIP = value;
                else
                    throw new Exception("Check 'Ticker Symbol' in ProfileQuantumonline_Parse procedure");
            }

            Exchange = SetNullableString(Exchange);
            Symbol = SymbolKey;
            while (Symbol.EndsWith("*"))
                Symbol = Symbol.Substring(0, Symbol.Length - 1);

            if (string.IsNullOrEmpty(Symbol) || string.IsNullOrEmpty(CUSIP))
                throw new Exception("Check 'Ticker Symbol' in ProfileQuantumonline_Parse procedure");

            // Set other properties
            for (var k1 = 2; k1 < rows.Count; k1++)
            {
                var row = rows[k1];
                if (row.StartsWith("IPO -", StringComparison.CurrentCultureIgnoreCase))
                {
                    SetIpoValues(row);
                    /*// IPO - 2/2/2021 - 87.00 Million Units @ $10.00/unit.
                    // IPO - 12/6/1999 - 4.50 Million Shares @ $8.00-10.00/share.
                    i1 = row.IndexOf("-", 5, StringComparison.InvariantCultureIgnoreCase);
                    ipoDate = DateTime.ParseExact(row.Substring(5, i1 - 5).Trim(), "M/d/yyyy", CultureInfo.InvariantCulture);

                    var sizeAndPrice = row.Substring(i1 + 1).Trim();
                    if (!string.IsNullOrEmpty(sizeAndPrice))
                    {
                        var t = GetSizeAndPriceOfIpo(sizeAndPrice);
                        ipoSize = t.Item1;
                        ipoPrice = t.Item2;
                    }*/
                }
                else if (row.StartsWith("Preliminary Prospectus Filed -", StringComparison.CurrentCultureIgnoreCase))
                {
                    SetIpoValues(row);
                }
                else if (row.StartsWith("Free Writing Prospectus Filed -", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Free Writing Prospectus Filed - 10/8/2020 - 16.00 Million Notes @ $25/note. (BAMH)
                    SetIpoValues(row);
                }
                else if (row.EndsWith("/share.")) //6.67 Million Shares @ $15.00/share. (ABDC*)
                {
                    SetSizeAndPriceOfIpo(row.Trim());
                }
                else if (row.EndsWith("/ADR.")) //11.43 Million ADRs @ $17.50/ADR. (ABLX*)
                {
                    SetSizeAndPriceOfIpo(row.Trim());
                }
                else if (row.EndsWith("/unit.")) //7.50 Million Units @ $10.00/unit. (ACAXU)
                {
                    SetSizeAndPriceOfIpo(row.Trim());
                }
                else if (row.EndsWith("/note.")) // 2.60 Million Notes @ $25/note.
                {
                    SetSizeAndPriceOfIpo(row.Trim());
                }
                else if (row.IndexOf("Market Value", StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    // Large Cap Stock -&nbsp;&nbsp; \n Market Value $22.5 Billion
                    var ss = row.Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s1 in ss)
                    {
                        var s2 = s1.Trim();
                        if (s2.StartsWith("Market Value"))
                            MarketCap = GetNumberFromString(s2.Substring(12).Trim());
                        else if (s2.EndsWith(" Cap Stock -"))
                            CapStockType = s2.Substring(0, s2.Length - 12).Trim();
                        else if (string.Equals(s2, "Cap Stock -"))
                        {
                        }
                        else
                            throw new Exception("Check 'Market Value' in ProfileQuantumonline_Parse procedure");
                    }
                    // Debug.Print($"CapStock: {capStockType}. Market cap: {sMarketCap}");
                }

                else if (row.StartsWith("Security Type:"))
                {
                    // Security Type:&nbsp;&nbsp; /n <a href="listwipo.cfm?type=SPACs&RequestTimeout=60">Special Purpose Acquisition Company (SPAC)</a>
                    var ss = row.Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
                    for (var k = 0; k < ss.Length; k++)
                        ss[k] = ss[k].Trim();
                    if (ss.Length >= 2 && ss[0] == "Security Type:" && ss[1].EndsWith("</a>"))
                    {
                        ss[1] = ss[1].Substring(0, ss[1].Length - 4);
                        i1 = ss[1].LastIndexOf(">", StringComparison.InvariantCultureIgnoreCase);
                        Type = ss[1].Substring(i1 + 1);
                    }

                    if (ss.Length == 5 && ss[3].EndsWith("ETF Subtype:") && ss[4].EndsWith("</a>"))
                    {
                        ss[4] = ss[4].Substring(0, ss[4].Length - 4);
                        i1 = ss[4].LastIndexOf(">", StringComparison.InvariantCultureIgnoreCase);
                        Subtype = ss[4].Substring(i1 + 1);
                    }
                    else if (ss.Length >= 4)
                        throw new Exception("Check 'Security Type:' in ProfileQuantumonline_Parse procedure");

                    if (string.IsNullOrEmpty(Type))
                        throw new Exception("Check 'Security Type:' in ProfileQuantumonline_Parse procedure");

                    // Debug.Print($"SecType: {type}. Subtype: {subtype}");
                }

                else if (row.StartsWith("* Symbol changed!"))
                {
                    // * Symbol changed!! New symbol: <a href="search.cfm?tickersymbol=SCOK&sopt=symbol">SCOK</a> \nas of 2/03/2010
                    i2 = row.LastIndexOf("</a>", StringComparison.InvariantCultureIgnoreCase);
                    i1 = row.LastIndexOf(">", i2, StringComparison.InvariantCultureIgnoreCase);
                    NewSymbol = row.Substring(i1 + 1, i2 - i1 - 1);

                    i1 = row.LastIndexOf("as of ", StringComparison.InvariantCultureIgnoreCase);
                    if (i1 != -1)
                        NewSymbolChanged = DateTime.ParseExact(row.Substring(i1 + 6), "M/dd/yyyy", CultureInfo.InvariantCulture);
                }

                else if (row.StartsWith("New Company Name:"))
                {
                    // New Company Name: SinoCoking Coal and Coke Chemical Industries, Inc.
                    NewName = row.Substring(17).Trim();
                    // Debug.Print($"NewName: {newName}. Key: {SymbolKey}");
                }

                else if (row.StartsWith("Previous Ticker Symbol:"))
                {
                    // Previous Ticker Symbol: ABLC &nbsp;&nbsp;&nbsp;Changed: 6/29/2000
                    var ss = row.Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
                    PrevSymbol = ss[0].Substring(23).Trim();
                    if (ss.Length == 2 && ss[1].StartsWith("Changed:"))
                        PrevSymbolChanged = DateTime.ParseExact(ss[1].Substring(8).Trim(), "M/dd/yyyy", CultureInfo.InvariantCulture);
                    else if (ss.Length != 1)
                        throw new Exception("Check 'Previous Ticker Symbol:' in ProfileQuantumonline_Parse procedure");
                    // Debug.Print($"PrevSymbol: {prevSymbol} at {prevSymbolChanged:dd.MM.yyyy}");
                }

                else if (row.StartsWith("Previous Name:"))
                {
                    // Previous Name: FelCor Lodging Trust, $1.95 Series A Cumul Convertible Preferred Stock &nbsp;&nbsp;&nbsp;Changed: 9/01/2017 
                    var ss = row.Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
                    PrevName = ss[0].Substring(14).Trim();
                    if (ss.Length == 2 && ss[1].StartsWith("Changed:"))
                        PrevNameChanged = DateTime.ParseExact(ss[1].Substring(8).Trim(), "M/dd/yyyy", CultureInfo.InvariantCulture);
                    else if (ss.Length != 1)
                        throw new Exception("Check 'Previous Name:' in ProfileQuantumonline_Parse procedure");
                    // Debug.Print($"PrevName: {prevName} at {prevNameChanged:dd.MM.yyyy}");
                }

                else if (row.StartsWith("&nbsp;&nbsp;&nbsp;Changed:"))
                {
                    // &nbsp;&nbsp;&nbsp;Changed: 7/24/2000 (APW*)
                    var ss = row.Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
                    if (ss.Length == 1 && ss[0].StartsWith("Changed:"))
                        Changed = DateTime.ParseExact(ss[0].Substring(8).Trim(), "M/dd/yyyy", CultureInfo.InvariantCulture);
                    else
                        throw new Exception("Check 'Chenged:' in ProfileQuantumonline_Parse procedure");
                }
                else if (row.StartsWith("ADR with an ADR ratio")) { }
                else if (row.StartsWith("Security has been ") && row.IndexOf("Called for:", StringComparison.InvariantCultureIgnoreCase) != -1) { }//Security has been [Partially] Called for
                else if (row.StartsWith("* NOTE:")) { }
                else if (row.StartsWith("Preliminary Prospectus Filed")) { }
                else if (row.StartsWith("Security's Distribution is Suspended!")) { }
                else if (row.StartsWith("A tender offer has been")) { }
                else if (otherRowTypes.Count(a => row.IndexOf(a, StringComparison.CurrentCultureIgnoreCase) != -1) != 0) { }
                else
                    throw new Exception("Check ProfileQuantumonline_Parse procedure");
            }

            //if (changed.HasValue)
            //  Debug.Print($"Change for {file.FileNameWithoutExtension.Substring(1)}. NewSymbol {newSymbol} at {newSymbolChanged:dd.MM.yyyy}. NewName: {newName}. PrevSymbol {prevSymbol} at {prevSymbolChanged:dd.MM.yyyy}. PrevName: {prevName} at {prevNameChanged:dd.MM.yyyy}. Changed:{changed.Value:dd.MM.yyyy}");

            /*if (!string.Equals(marketCapRow, null))
            {
                var a2 = RemoveTags(marketCapRow).Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var aa1 in a2)
                {
                    var aa2 = aa1.Trim();
                    var key2 = aa2.StartsWith("Market Value") ? "Market Value" : aa2;
                    if (!keys2.ContainsKey(key2))
                        keys2.Add(key2, null);
                }
            }*/

            string RemoveTags(string o) => o.Replace("<p>", "").Replace("<br>", "").Replace("<b>", "")
                .Replace("</b>", "").Replace("<center>", "").Replace("</center>", "");

            string SetNullableString(string o) => string.IsNullOrEmpty(o) ? null : o;

            void SetIpoValues(string row)
            {
                if (IpoDate.HasValue || IpoSize.HasValue || !string.IsNullOrEmpty(IpoPrice))
                    throw new Exception($"Check 'SetIpoValues' for {fileNameWithoutExtension} file");
                // IPO - 2/2/2021 - 87.00 Million Units @ $10.00/unit.
                // IPO - 12/6/1999 - 4.50 Million Shares @ $8.00-10.00/share.
                var i10 = row.IndexOf('-') + 1;
                var i11 = row.IndexOf("-", i10, StringComparison.InvariantCultureIgnoreCase);
                IpoDate = DateTime.ParseExact(row.Substring(i10, i11 - i10).Trim(), "M/d/yyyy", CultureInfo.InvariantCulture);

                var sizeAndPrice = row.Substring(i11 + 1).Trim();
                if (!string.IsNullOrEmpty(sizeAndPrice))
                {
                    SetSizeAndPriceOfIpo(sizeAndPrice);
                    /*var t = GetSizeAndPriceOfIpo(sizeAndPrice);
                    IpoSize = t.Item1;
                    IpoPrice = t.Item2;*/
                }
            }

            void SetSizeAndPriceOfIpo(string o)
            {
                // 4.69 Million ADRs @ $16.00/ADR.
                if (IpoSize.HasValue || !string.IsNullOrEmpty(IpoPrice))
                    throw new Exception($"Check 'SetSizeAndPriceOfIpo' for {fileNameWithoutExtension} file");
                var i10 = o.IndexOf('@');
                var s4 = i10 == -1 ? o : o.Substring(0, i10 - 1).Trim();
                var i11 = s4.LastIndexOf(' ');
                var s2 = s4.Substring(0, i11);
                IpoSize = GetNumberFromString(s2);

                if (i10 != -1)
                {
                    IpoPrice = o.Substring(i10 + 1).Trim();
                    if (IpoPrice.EndsWith("."))
                        IpoPrice = IpoPrice.Substring(0, IpoPrice.Length - 1);
                    /*var s3 = o.Substring(i10 + 1).Trim();
                    i10 = s3.LastIndexOf('/');
                    IpoPrice = s3.Substring(0, i10).Trim();*/
                }
            }
        }

        public override string ToString() => $"{SymbolKey}^{Exchange}^{Name}";

        private static float? GetNumberFromString(string o)
        {
            // 4.69 Million
            if (o.StartsWith("$"))
                o = o.Substring(1);
            if (o.IndexOf(' ') == -1)
                return float.Parse(o, CultureInfo.InvariantCulture)/1000000F;
            if (o.EndsWith("Million"))
            {
                var s = o.Substring(0, o.Length - 7).Trim();
                return string.IsNullOrEmpty(s)? (float?)null : float.Parse(s, CultureInfo.InvariantCulture);
            }

            if (o.EndsWith("Billion"))
            {
                var s = o.Substring(0, o.Length - 7).Trim();
                return string.IsNullOrEmpty(s) ? (float?)null : float.Parse(s, CultureInfo.InvariantCulture) * 1000F;
            }
            throw new Exception($"Check GetNumberFromString for '{o}'");
        }

    }
}
