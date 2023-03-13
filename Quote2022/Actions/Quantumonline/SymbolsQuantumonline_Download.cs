using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Quote2022.Actions.Quantumonline
{
    class SymbolsQuantumonline_Download
    {
        public static void Start(Action<string> showStatusAction)
        {
            var urlChars = new char[]
            {
                (char) 32, (char) 33, (char) 34, (char) 36, (char) 37, (char) 38, /*(char) 39,*/ (char) 40, (char) 41,
                (char) 42, (char) 43, (char) 44, (char) 45, (char) 46, (char) 47, (char) 48, (char) 49, (char) 50,
                (char) 51, (char) 52, (char) 53, (char) 54, (char) 55, (char) 56, (char) 57, (char) 58, (char) 63,
                (char) 64, (char) 65, (char) 66, (char) 67, (char) 68, (char) 69, (char) 70, (char) 71, (char) 72,
                (char) 73, (char) 74, (char) 75, (char) 76, (char) 77, (char) 78, (char) 79, (char) 80, (char) 81,
                (char) 82, (char) 83, (char) 84, (char) 85, (char) 86, (char) 87, (char) 88, (char) 89, (char) 90,
                (char) 174, (char) 195, (char) 199, (char) 201, (char) 205, (char) 209, (char) 214, (char) 8211,
                (char) 8212, (char) 8217
            };

            var urlAndFileCharXref = new Dictionary<char, char>()
            {
                {(char) 34, (char) 96}, {(char) 42, (char) 35}, {(char) 47, (char) 61}, {(char) 58, (char) 59},
                {(char) 63, (char) 94}
            };

            /*var fileAndUrlCharXref = new Dictionary<char, char>();
            foreach(var kvp in urlAndFileCharXref)
                fileAndUrlCharXref.Add(kvp.Value, kvp.Key);

            var pathTemplate = @"E:\Temp\Quote\Test\s{0}.txt";
            foreach (var c in urlChars)
            {
                var c1 = urlAndFileCharXref.ContainsKey(c) ? urlAndFileCharXref[c] : c;
                File.Create(string.Format(pathTemplate, c1.ToString()));
            }*/

            var urlTemplate = @"https://www.quantumonline.com/search.cfm?tickersymbol={0}&sopt=sname&1.0.1=Search";
            var fileTemplate = @"E:\Temp\Quote\Test\s{0}.html";
            var toDownloadKeysTemplate = @"E:\Temp\Quote\Test\ToDownloadKeys-{0}.txt";
            var finishedKeysTemplate = @"E:\Temp\Quote\Test\FinishedKeys-{0}.txt";
            var level = 0;
            var finishedUrlKeys = new List<string>();
            var keys = new List<string> { "" };
            while (keys.Count != 0)
            {
                if (File.Exists(string.Format(toDownloadKeysTemplate, level.ToString())))
                {
                    var fileName = string.Format(toDownloadKeysTemplate, level.ToString());
                    keys.Clear();
                    foreach (var line in File.ReadAllLines(fileName))
                        keys.Add(line);

                    fileName = string.Format(finishedKeysTemplate, level.ToString());
                    finishedUrlKeys.Clear();
                    foreach (var line in File.ReadAllLines(fileName))
                        finishedUrlKeys.Add(line);

                    Debug.Print($"SymbolsQuantumonline_Download. Downloaded {keys.Count} items for level: {level}. FinishedUrlKeys: {finishedUrlKeys.Count}");
                }
                else
                {
                    var urlKeys = new Dictionary<string, string>();
                    foreach (var key in keys)
                        foreach (var c in urlChars)
                        {
                            var newKey = c.ToString() + key;
                            if (!urlKeys.ContainsKey(newKey) && finishedUrlKeys.FirstOrDefault(f => newKey.Contains(f)) == null && GetLenUrl(newKey) <= 20)
                                urlKeys.Add(newKey, GetFileName(newKey));

                            newKey = key + c.ToString();
                            if (!urlKeys.ContainsKey(newKey) && finishedUrlKeys.FirstOrDefault(f => newKey.Contains(f)) == null && GetLenUrl(newKey) <= 20)
                                urlKeys.Add(newKey, GetFileName(newKey));
                        }

                    if (urlKeys.ContainsKey(" "))
                        urlKeys.Remove(" ");
                    if (urlKeys.ContainsKey("  "))
                        urlKeys.Remove("  ");

                    showStatusAction($"QuantumonlineSymbols downloading... Level: {level}. Urls: {urlKeys.Count}");
                    var toDownloadKeys = urlKeys.Where(a => !File.Exists(a.Value)).ToArray();
                    Debug.Print($"SymbolsQuantumonline_Download. Downloading {toDownloadKeys.Length} from {urlKeys.Count} items for level: {level}. FinishedUrlKeys: {finishedUrlKeys.Count}. {DateTime.Now}");
                    Parallel.ForEach(toDownloadKeys, new ParallelOptions { MaxDegreeOfParallelism = 10 }, (kvp) =>
                    {
                        Download.DownloadPage(GetUrl(kvp.Key), kvp.Value);
                    });

                    keys.Clear();
                    foreach (var urlKey in urlKeys)
                    {
                        var items = Parse.SymbolsQuantumonlineContent_Parse(File.ReadAllText(urlKey.Value), urlKey.Value, File.GetCreationTime(urlKey.Value));
                        var checkedItems = items.Where(a => (a.HtmlName + " ").IndexOf(urlKey.Key, StringComparison.InvariantCultureIgnoreCase) == -1).ToArray();
                        if (checkedItems.Length != 0)
                        {
                            Debug.Print($"Not Checked. Key: {urlKey.Key}. Name: {checkedItems[0].HtmlName}");
                            throw new Exception($"Not Checked. Key: {urlKey.Key}. Name: {checkedItems[0].HtmlName}");
                        }

                        if (items.Count >= 200)
                            keys.Add(urlKey.Key);
                        else if (items.Count > 0)
                            finishedUrlKeys.Add(urlKey.Key);
                    }

                    var fileName = string.Format(toDownloadKeysTemplate, level.ToString());
                    System.IO.File.WriteAllLines(fileName, keys);
                    fileName = string.Format(finishedKeysTemplate, level.ToString());
                    System.IO.File.WriteAllLines(fileName, finishedUrlKeys);
                }

                level++;
            }

            Debug.Print($"FINISHED!!! {DateTime.Now}");
            showStatusAction($"FINISHED!!!");

            string GetUrl(string urlKey) => string.Format(urlTemplate, Uri.EscapeDataString(urlKey.Replace("%", "[%]")));
            int GetLenUrl(string urlKey) => urlKey.Replace("%", "[%]").Length;

            string GetFileName(string urlKey)
            {
                var cc = urlKey.ToCharArray();
                for (var k = 0; k < cc.Length; k++)
                {
                    if (urlAndFileCharXref.ContainsKey(cc[k]))
                        cc[k] = urlAndFileCharXref[cc[k]];
                }
                return string.Format(fileTemplate, new string(cc));
            }
        }
    }
}
