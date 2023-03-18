using System.Diagnostics;
using Data.Helpers;

namespace Data.Actions.Wikipedia
{
    public static class WikipediaIndexLoader
    {
        private static string[] urls = new string[]
        {
            "https://en.wikipedia.org/wiki/List_of_S%26P_500_companies",
            "https://en.wikipedia.org/wiki/List_of_S%26P_400_companies",
            "https://en.wikipedia.org/wiki/List_of_S%26P_600_companies",
            "https://en.wikipedia.org/wiki/Nasdaq-100"
            // old: https://en.wikipedia.org/wiki/List_of_NASDAQ-100_companies
        };

        private const string Folder = @"E:\Quote\WebData\Indices\Wikipedia\IndexComponents\";

        private static string[] files = new string[]
        {
            Folder + "IndexComponents_{0}\\Components_SP500_{0}.html",
            Folder + "IndexComponents_{0}\\Components_SP400_{0}.html",
            Folder + "IndexComponents_{0}\\Components_SP600_{0}.html",
            Folder + "IndexComponents_{0}\\Components_Nasdaq100_{0}.html",
        };

        public static void Start()
        {
            Logger.AddMessage($"Started");
            var a1 = System.Reflection.MethodBase.GetCurrentMethod();

            StackFrame sf = new StackFrame(1);
            var currentMethodName = sf.GetMethod();
            var k = 0;

            /*var timeStamp = CsUtils.GetTimeStamp();
            var jsonFileName = Folder + $"WikipediaIndexLoader_{timeStamp.Item2}.json";
            var zipFileName = Folder + $"WikipediaIndexLoader_{timeStamp.Item2}.zip";

            // Download data to html file
            Helpers.Download.DownloadPage(Url, jsonFileName);

            // Zip data
            Helpers.CsUtils.ZipFile(jsonFileName, zipFileName);

            // Parse and save to database
            var itemCount = ParseAndSaveToDb(zipFileName);

            File.Delete(jsonFileName);*/

            // logEvent($"!WikipediaIndexLoader finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
            Logger.AddMessage("!Finished");
        }


    }
}
