using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.OffScreen;
using Microsoft.WindowsAPICodePack.Dialogs;
using Quote2022.Actions;
using Quote2022.Actions.MinuteAlphaVantage;
using Quote2022.Actions.Nasdaq;
using Quote2022.Helpers;
using Quote2022.Models;

namespace Quote2022
{
    public partial class Form1 : Form
    {
        private object _lock = new object();
        private ChromiumWebBrowser browser;// = new ChromiumWebBrowser("www.eoddata.com");
        private System.Net.CookieContainer eoddataCookies = new System.Net.CookieContainer();

        public Form1()
        {
            InitializeComponent();

            dataGridView1.Paint += new PaintEventHandler(dataGridView1_Paint);
            dataGridView1.DataSource = Data.Models.LoaderItem.DataGridLoaderItems;

            //=========================
            StatusLabel.Text = "";
            clbIntradayDataList.Items.AddRange(IntradayResults.ActionList.Select(a => a.Key).ToArray());
            cbIntradayStopInPercent_CheckedChanged(null, null);
            for (var item = 0; item < clbIntradayDataList.Items.Count; item++)
            {
                clbIntradayDataList.SetItemChecked(item, true);
            }

            StartImageAnimation();

            // Logger.MessageAdded += (sender, args) => StatusLabel.Text = args.FullMessage;
            Data.Helpers.Logger.MessageAdded += (sender, args) => this.BeginInvoke((Action)(() => StatusLabel.Text = args.FullMessage));

            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                browser = new ChromiumWebBrowser("www.eoddata.com");
                browser.FrameLoadEnd += Browser_FrameLoadEnd;
            }
        }

        private async void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                var isLogged = await IsLogged();
                if (isLogged)
                {
                    // MessageBox.Show("OK");
                    var cookieManager = Cef.GetGlobalCookieManager();
                    var visitor = new CookieCollector();

                    cookieManager.VisitUrlCookies(browser.Address, true, visitor);

                    var cookies = await visitor.Task; // AWAIT !!!!!!!!!
                    eoddataCookies = new System.Net.CookieContainer();
                    foreach (var cookie in cookies)
                    {
                        eoddataCookies.Add(new System.Net.Cookie()
                        {
                            Name = cookie.Name,
                            Value = cookie.Value,
                            Domain = cookie.Domain
                        });
                    }

                    Data.Actions.Eoddata.EoddataCommon.FnGetEoddataCookies = () => { return eoddataCookies; };
                    Invoke(new Action(() =>
                    {
                        lblEoddataLogged.Text = "Logged in eoddata.com";
                        lblEoddataLogged.BackColor = System.Drawing.Color.Green;
                    }));

                    // var cookieHeader = CookieCollector.GetCookieHeader(cookies);
                    return;
                }

                var userAndPassword = Data.Helpers.CsUtils.GetApiKeys("eoddata.com")[0].Split('^');
                var script = $"document.getElementById('ctl00_cph1_lg1_txtEmail').value='{userAndPassword[0]}';" +
                    $"document.getElementById('ctl00_cph1_lg1_txtPassword').value='{userAndPassword[1]}';" +
                    "document.getElementById('ctl00_cph1_lg1_chkRemember').checked = true;" +
                    "document.getElementById('ctl00_cph1_lg1_btnLogin').click();";
                browser.ExecuteScriptAsync(script);
            }
        }

        private async Task<bool> IsLogged()
        {
            const string script = @"(function()
{
  return document.getElementById('ctl00_cph1_lg1_lblName') == undefined ? null : document.getElementById('ctl00_cph1_lg1_lblName').innerHTML;
  })();";

            var response = await browser.EvaluateScriptAsync(script);
            var logged = response.Success && Equals(response.Result, "Sergei Petrik");
            Invoke(new Action(() =>
            {
                if (logged)
                {
                    lblEoddataLogged.Text = "Logged in eoddata.com";
                    lblEoddataLogged.BackColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblEoddataLogged.Text = "Not logged in eoddata.com";
                    lblEoddataLogged.BackColor = System.Drawing.Color.Red;
                }
            }));

            return logged;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            browser?.Dispose();
            CefSharp.Cef.Shutdown();
        }

        #region ===========  Image Animation  ============
        // taken from https://social.msdn.microsoft.com/Forums/windows/en-US/0d9e790e-6816-40e7-96fe-bbf333a4abc0/show-animated-gif-in-datagridview?forum=winformsdatacontrols
        void dataGridView1_Paint(object sender, PaintEventArgs e)
        {
            //Update the frames. The cell would paint the next frame of the image late on.
            ImageAnimator.UpdateFrames();
        }
        private void StartImageAnimation()
        {
            var image = Data.Models.LoaderItem.GetAnimatedImage();
            ImageAnimator.Animate(image, new EventHandler(this.OnFrameChanged));
        }
        private void OnFrameChanged(object o, EventArgs e)
        {
            if (dataGridView1.Columns.Count > 4)
            {
                //Force a call to the Paint event handler.
                dataGridView1.InvalidateColumn(1);
                dataGridView1.InvalidateColumn(4);
            }
        }
        #endregion

        #region ==========  EventHandlers of controls  ===========
        private void dataGridView1_SelectionChanged(object sender, EventArgs e) => dataGridView1.ClearSelection();
        #endregion

        private void cbIntradayStopInPercent_CheckedChanged(object sender, EventArgs e)
        {
            if (cbIntradayStopInPercent.Checked)
            {
                nudIntradayStop.DecimalPlaces = 1;
                nudIntradayStop.Minimum = 0.1M;
                nudIntradayStop.Increment = 0.1M;
                nudIntradayStop.Value = 0.5M;
            }
            else
            {
                nudIntradayStop.DecimalPlaces = 2;
                nudIntradayStop.Minimum = 0.01M;
                nudIntradayStop.Increment = 0.01M;
                nudIntradayStop.Value = 0.03M;
            }
        }

        private void ShowStatus(string message)
        {
            if (statusStrip1.InvokeRequired)
                Invoke(new MethodInvoker(delegate { ShowStatus(message); }));
            else
                StatusLabel.Text = message;

           Application.DoEvents();
        }

        private void btnDayYahooParse_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenZipFileDialog(Settings.DayYahooFolder) is string fn && !string.IsNullOrEmpty(fn))
                Actions.Parse.DayYahoo_Parse(fn, ShowStatus);
        }

        private void btnDayYahooIndicesParse_Click(object sender, EventArgs e) => Actions.Parse.IndicesYahoo(ShowStatus);

        private void btnSymbolsNanex_Click(object sender, EventArgs e)
        {
            var files = Actions.Download.SymbolsNanex_Download(ShowStatus);
            var data = new List<SymbolsNanex>();
            Actions.Parse.SymbolsNanex_Parse(files, data, ShowStatus);
            ShowStatus($"Save Nanex Symbols");
            SaveToDb.SymbolsNanex_SaveToDb(data);
            ShowStatus($"Nanex Symbols: FINISHED!");
        }

        private void btnDayEoddataParse_Click(object sender, EventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            SaveToDb.DayEoddata_SaveToDb(Actions.Parse.DayEoddata_Data(ShowStatus), ShowStatus);
            ShowStatus($"DayEoddata file parsing finished!!!");

            // SaveToDb.ClearDbTable("xDayEoddata");
            // Parse.DayEoddata_Parse(ShowStatus);
            sw.Stop();
            Debug.Print("Time: " + sw.ElapsedMilliseconds);
        }

        private void btnSplitYahooParse_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenZipFileDialog(Settings.SplitYahooFolder) is string fn && !string.IsNullOrEmpty(fn))
                Actions.Parse.SplitYahoo_Parse(fn, ShowStatus);
        }

        private void btnSplitInvestingParse_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenTxtFileDialog(Settings.SplitInvestingFolder) is string fn && !string.IsNullOrEmpty(fn))
                Actions.Parse.SplitInvesting_Parse(fn, ShowStatus);
        }

        private void btnSplitEoddataParse_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenTxtFileDialog(Settings.SplitEoddataFolder) is string fn && !string.IsNullOrEmpty(fn))
                Actions.Parse.SplitEoddata_Parse(fn, ShowStatus);
        }

        private void btnAlgorithm1_Click(object sender, EventArgs e)
        {
            var dataSet = new List<string>();
            if (cb2013.Checked) dataSet.Add("2013");
            if (cb2022.Checked) dataSet.Add("2022");
            Helpers.Algorithm1.Execute(dataSet, ShowStatus);
        }

        private void btnDailyEoddataCheck_Click(object sender, EventArgs e) => Actions.Parse.DayEoddata_Check(ShowStatus);

        private void btnParseScreenerNasdaqParse_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenZipFileDialog(Settings.ScreenerNasdaqFolder) is string fn && !string.IsNullOrEmpty(fn))
                Actions.Parse.ScreenerNasdaq_ParseAndSaveToDb(fn, ShowStatus);
        }

        private void btnStockSplitHistoryParse_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenZipFileDialog(Settings.StockSplitHistoryFolder) is string fn && !string.IsNullOrEmpty(fn))
                Actions.Parse.StockSplitHistory_Parse(fn, ShowStatus);
        }

        private void btnSplitInvestingHistoryParse_Click(object sender, EventArgs e)
        {
            var files = Directory.GetFiles(Settings.SplitInvestingHistoryFolder, "*.txt");
            var data = new Dictionary<string, SplitModel>();
            foreach (var file in files)
                Actions.Parse.SplitInvestingHistory_Parse(file, data, ShowStatus);

            ShowStatus($"SplitInvestingHistory is saving to database");
            SaveToDb.SplitInvestingHistory_SaveToDb(data.Values);
            ShowStatus($"SplitInvestingHistory parse & save to database FINISHED!");
        }

        private void btnQuantumonlineProfilesParse_Click(object sender, EventArgs e)
        {
            Actions.Parse.ProfileQuantumonline_ParseAndSaveToDb(@"E:\Quote\WebData\Symbols\Quantumonline\Profiles\Profiles.zip", ShowStatus);
            return;
            if (CsUtils.OpenZipFileDialog(Settings.ProfileQuantumonlineFolder) is string fn && !string.IsNullOrEmpty(fn))
                Actions.Parse.ProfileQuantumonline_ParseAndSaveToDb(fn, ShowStatus);
        }

        private void btnSymbolsStockanalysisDownload_Click(object sender, EventArgs e)
        {
            Actions.Download.SymbolsStockanalysis_Download(ShowStatus);
        }

        private void btnSymbolsStockanalysisParse_Click(object sender, EventArgs e) => Actions.Parse.SymbolsStockanalysis_ParseAndSaveToDb(ShowStatus);

        private void btnSymbolsNasdaqParse_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenZipFileDialog(Settings.SymbolsNasdaqFolder) is string fn && !string.IsNullOrEmpty(fn))
                Actions.Parse.SymbolsNasdaq_ParseAndSaveToDb(fn, ShowStatus);
        }

        private void btnRefreshSymbolsData_Click(object sender, EventArgs e)
        {
            ShowStatus($"RefreshSymbolsData is starting");
            SaveToDb.RunProcedure("pRefreshSymbols");
            ShowStatus($"RefreshSymbolsData FINISHED!");
        }

        private void btnSymbolsEoddataParse_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenZipFileDialog(Settings.SymbolsEoddataFolder) is string fn && !string.IsNullOrEmpty(fn))
                Actions.Parse.SymbolsEoddata_ParseAndSaveToDb(fn, ShowStatus);
        }

        private void btnTimeSalesNasdaqDownload_Click(object sender, EventArgs e)
        {
            Actions.Download.TimeSalesNasdaq_Download(ShowStatus);
        }

        private async void btnSymbolsQuantumonlineDownload_Click(object sender, EventArgs e)
        {
            btnSymbolsQuantumonlineDownload.Enabled = false;
            await Task.Factory.StartNew(() => Actions.Quantumonline.SymbolsQuantumonline_Download.Start(ShowStatus));
            btnSymbolsQuantumonlineDownload.Enabled = true;
        }

        private void btnSymbolsQuantumonlineParse_Click(object sender, EventArgs e) => Actions.Parse.SymbolsQuantumonlineZip_Parse(ShowStatus);

        private void btnRefreshSpitsData_Click(object sender, EventArgs e)
        {
            ShowStatus($"RefreshSpitsData is starting");
            SaveToDb.RunProcedure("pRefreshSplits");
            ShowStatus($"RefreshSplitsData FINISHED!");
        }

        private void btnTimeSalesNasdaqSaveLog_Click(object sender, EventArgs e)
        {
            ShowStatus($"Started");
            var folders = Directory.GetDirectories(@"E:\Quote\WebData\Minute\Nasdaq", "TS_2022*", SearchOption.TopDirectoryOnly);
            foreach (var folder in folders)
            {
                Check.TimeSalesNasdaq_SaveLog(folder, true, ShowStatus);
            }
            ShowStatus($"FINISHED!");
        }

        private void btnTimeSalesNasdaqSaveSummary_Click(object sender, EventArgs e)
        {
            ShowStatus($"Started");
            var folders = Directory.GetDirectories(@"E:\Quote\WebData\Minute\Nasdaq", "TS_2022*", SearchOption.TopDirectoryOnly);
            foreach (var folder in folders)
            {
                Check.TimeSalesNasdaq_SaveSummary(folder, true, ShowStatus);
            }
            ShowStatus($"FINISHED!");
        }

        private void btnUpdateTradingDays_Click(object sender, EventArgs e)
        {
            ShowStatus($"UpdateTradingDays is starting");
            SaveToDb.RunProcedure("dbQ2023Others..pRefreshTradingDays");
            ShowStatus($"UpdateTradingDays FINISHED!");
        }

        //==================
        //  Temp section
        //=================
        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void btnSymbolsYahooLookupDownload_Click(object sender, EventArgs e)
        {
            ShowStatus($"SymbolsYahooLookupDownload Started (equity)");
            Actions.Download.SymbolsYahooLookup_Download("equity", ShowStatus);
            ShowStatus($"SymbolsYahooLookupDownload Started (etf)");
            Actions.Download.SymbolsYahooLookup_Download("etf", ShowStatus);
            ShowStatus($"SymbolsYahooLookupDownload FINISHED!");
        }

        private void btnSymbolsYahooLookupParse_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenFileDialogGeneric(Settings.SymbolsYahooLookupFolder, @"*_202?????.zip files (*.zip)|*_202?????.zip") is string fn && !string.IsNullOrEmpty(fn))
                Actions.Parse.SymbolsYahooLookup_ParseAndSaveToDb(fn, ShowStatus);
        }

        private void btnDayYahooDownload_Click(object sender, EventArgs e)
        {
            Actions.Download.DayYahoo_Download(ShowStatus);
        }

        private void btnMinuteYahooLog_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenZipFileDialog(Settings.MinuteYahooDataFolder) is string fn && !string.IsNullOrEmpty(fn))
                Check.MinuteYahoo_SaveLog(new [] {fn}, ShowStatus);
        }

        #region ============  Intradaya Statistics  ==============
        private void btnCheckYahooMinuteData_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenFileDialogMultiselect(Settings.MinuteYahooDataFolder, @"YahooMinute_202?????.zip file (*.zip)|YahooMinute_202?????.zip") is string[] files && files.Length > 0)
                Check.MinuteYahoo_CheckData(ShowStatus, files);
        }

        private void btnPrepareYahooMinuteZipCache_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenFileDialogMultiselect(Settings.MinuteYahooDataFolder, @"YahooMinute_202?????.zip file (*.zip)|YahooMinute_202?????.zip") is string[] files && files.Length > 0)
                QuoteLoader.MinuteYahoo_PrepareTextCache(ShowStatus, files);
        }

        private void btnIntradayGenerateReport_Click(object sender, EventArgs e)
        {
            if (clbIntradayDataList.CheckedItems.Count == 0)
            {
                MessageBox.Show(@"Виберіть хоча б один тип даних");
                return;
            }

            var iParameters = IntradayGetParameters();
            if (iParameters.TimeFrames == null) return;

            var zipFile = CsUtils.OpenFileDialogGeneric(Settings.MinuteYahooCacheFolder, @"Cache*.zip file (*.zip)|Cache*.zip");
            if (string.IsNullOrEmpty(zipFile)) return;

            var sw = new Stopwatch();
            sw.Start();
            ShowStatus($"Data generation for report");

            // Define minute quotes
            var quotesInfoMinute = new QuotesInfo();
            var minuteQuotes = QuoteLoader.MinuteYahoo_GetQuotesFromZipCache(ShowStatus, zipFile, true, quotesInfoMinute);

            // Prepare quote list
            var quotesInfo = new QuotesInfo();
            var quotes = QuoteLoader.GetIntradayQuotes(null, minuteQuotes, iParameters, quotesInfo).ToArray();
            Debug.Print($"*** After GetIntradayQuotes. StopWatch: {sw.ElapsedMilliseconds:N0}. Used memory: {CsUtils.MemoryUsedInBytes:N0}");

            var data = new Dictionary<string, ExcelHelper.StatisticsData>();
            foreach (var o in clbIntradayDataList.CheckedItems)
            {
                var key = IntradayResults.ActionList[(string)o].Method.Name;
                Debug.Print($"*** Before prepare {key}. StopWatch: {sw.ElapsedMilliseconds:N0}. Used memory: {CsUtils.MemoryUsedInBytes:N0}");
                ShowStatus($"Generation '{key}' report");

                var reportLines = IntradayResults.ActionList[(string)o](quotes, iParameters);
                var sd = new ExcelHelper.StatisticsData
                {
                    Title = o.ToString(), Header1 = quotesInfo.GetStatus(), Header2 = iParameters.GetTimeFramesInfo(),
                    Header3 = iParameters.GetFeesInfo(), Table = reportLines
                };
                data.Add(key, sd);
                IntradayPrintReportLines(reportLines);

                Debug.Print($"*** After prepare {key}. StopWatch: {sw.ElapsedMilliseconds:N0}. Used memory: {CsUtils.MemoryUsedInBytes:N0}");
            }

            var excelFileName = IntradayGetExcelFileName(zipFile, "Intraday", iParameters);
            Helpers.ExcelHelper.SaveStatisticsToExcel(data, excelFileName, quotesInfoMinute.GetStatus());

            sw.Stop();
            Debug.Print($"*** btnIntradayGenerateReport_Click finished. StopWatch: {sw.ElapsedMilliseconds:N0}. Used memory: {CsUtils.MemoryUsedInBytes:N0}");
            ShowStatus($"Report is ready! Filename: {excelFileName}");
        }

        private void btnIntradayByTimeReports_Click(object sender, EventArgs e) =>
            IntradayByTimeReport(true, "M{0}M", "IntradayByTime");

        private void btnIntradayByTimeReportsClosedInNextFrame_Click(object sender, EventArgs e) =>
            IntradayByTimeReport(false, "N{0}M", "IntradayByTimeNext");

        private void IntradayByTimeReport(bool fullDay, string sheetNameTemplate, string fileNamePrefix)
        {
            var zipFile = CsUtils.OpenFileDialogGeneric(Settings.MinuteYahooCacheFolder, @"Cache*.zip file (*.zip)|Cache*.zip");
            if (string.IsNullOrEmpty(zipFile)) return;

            var sw = new Stopwatch();
            sw.Start();

            var iParameters = IntradayGetParameters();
            iParameters.CloseInNextFrame = !fullDay;
            var startTime = fullDay ? new TimeSpan(9, 30, 0) : new TimeSpan(9, 45, 0);
            var endTime = fullDay ? new TimeSpan(16, 00, 0) : new TimeSpan(15, 45, 0);
            var durationInMinutes = Convert.ToInt32((endTime - startTime).TotalMinutes);

            // Get minute quotes
            var quotesInfoMinute = new QuotesInfo();
            var minuteQuotes = QuoteLoader.MinuteYahoo_GetQuotesFromZipCache(ShowStatus, zipFile, true, quotesInfoMinute).ToArray();
            Debug.Print($"*** After load StopWatch: {sw.ElapsedMilliseconds:N0}. Used memory: {CsUtils.MemoryUsedInBytes:N0}");

            var data = new Dictionary<string, ExcelHelper.StatisticsData>();
            for (var m = 1; m <= durationInMinutes; m++)
            {
                if ((durationInMinutes % m) == 0)
                {
                    Debug.Print($"*** Before process {m}min StopWatch: {sw.ElapsedMilliseconds:N0}. Used memory: {CsUtils.MemoryUsedInBytes:N0}");
                    ShowStatus($" Data generation for {m} minute frames");

                    var quotesInfo = new QuotesInfo();
                    iParameters.TimeFrames = CsUtils.GetTimeFrames(startTime, endTime, new TimeSpan(0, m, 0));
                    var quotes = QuoteLoader.GetIntradayQuotes(ShowStatus, minuteQuotes, iParameters, quotesInfo);
                    var reportLines = IntradayResults.ByTime(quotes, iParameters);
                    var sd = new ExcelHelper.StatisticsData
                    {
                        Title = $"By Time ({m}min)", Header1 = quotesInfo.GetStatus(),
                        Header2 = iParameters.GetTimeFramesInfo(), Header3 = iParameters.GetFeesInfo(),
                        Table = reportLines
                    };
                    data.Add(string.Format(sheetNameTemplate, m.ToString()), sd);
                    IntradayPrintReportLines(reportLines);

                    Debug.Print($"*** After process {m}min StopWatch: {sw.ElapsedMilliseconds:N0}. Used memory: {CsUtils.MemoryUsedInBytes:N0}");
                }
            }

            ShowStatus("Saving to excel");
            var excelFileName = IntradayGetExcelFileName(zipFile, fileNamePrefix, iParameters);
            Helpers.ExcelHelper.SaveStatisticsToExcel(data, excelFileName, quotesInfoMinute.GetStatus());
            ShowStatus($"Finished! Filename: {excelFileName}");

            sw.Stop();
            Debug.Print($"*** Finished StopWatch: {sw.ElapsedMilliseconds:N0}. Used memory: {CsUtils.MemoryUsedInBytes:N0}");
        }

        private void btnIntradayStaisticsSaveToDB_Click(object sender, EventArgs e)
        {
            var iParameters = IntradayGetParameters();
            if (iParameters.TimeFrames == null) return;

            var zipFile = CsUtils.OpenFileDialogGeneric(Settings.MinuteYahooCacheFolder, @"Cache*.zip file (*.zip)|Cache*.zip");
            if (string.IsNullOrEmpty(zipFile)) return;

            var sw = new Stopwatch();
            sw.Start();
            ShowStatus($"Data generation for report");

            // Define minute quotes
            var quotesInfoMinute = new QuotesInfo();
            var minuteQuotes = QuoteLoader.MinuteYahoo_GetQuotesFromZipCache(ShowStatus, zipFile, true, quotesInfoMinute);

            // Prepare quote list
            var closeInNextFrame = cbCloseInNextFrame.Checked;
            var quotesInfo = new QuotesInfo();
            var quotes = QuoteLoader.GetIntradayQuotes(null, minuteQuotes, iParameters, quotesInfo)
                .Select(a => new StatisticsQuote(a, iParameters)).ToArray();

            ShowStatus($"Saving data to to 'Bfr_StatQuote' table of database...");

            SaveToDb.ClearAndSaveToDbTable(quotes, "Bfr_StatQuote", "Symbol", "Date", "TimeFrameId", "Time", "Open",
                "High", "Low", "Close", "Volume", "BuyPerc", "SellPerc", "BuyAmt", "SellAmt", "BuyWins", "SellWins",
                "Week", "DayOfWeek", "Stop", "IsStopPerc", "Fees", "PriceId");

            Debug.Print($"*** After GetIntradayQuotes. StopWatch: {sw.ElapsedMilliseconds:N0}. Used memory: {CsUtils.MemoryUsedInBytes:N0}");

            sw.Stop();
            ShowStatus($"Finished! Data saved to 'Bfr_StatQuote' table of database. Duration: {sw.ElapsedMilliseconds:N0} milliseconds");
        }


        private void btnIntradayPrintDetails_Click(object sender, EventArgs e)
        {
            // var quotes = GetIntradayQuotes().Where(a => a.Symbol == "PULS").OrderBy(a => a.Timed).ToList();
        }

        private IntradayParameters IntradayGetParameters()
        {
            var p = new IntradayParameters
            {
                TimeFrames = IntradayGetTimeFrames(), CloseInNextFrame = cbCloseInNextFrame.Checked,
                Fees = nudIntradayFees.Value, Stop = nudIntradayStop.Value,
                IsStopPercent = cbIntradayStopInPercent.Checked
            };
            return p;
        }

        private string IntradayGetExcelFileName(string dataFileName, string fileNamePrefix, IntradayParameters iParameters)
        {
            var aa = Path.GetFileNameWithoutExtension(dataFileName).Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            var excelFilename = Settings.MinuteYahooReportFolder + fileNamePrefix + "_" + aa[aa.Length - 1] + "-" +
                                iParameters.GetFileNameSuffix() + ".xlsx";
            return excelFilename;
        }

        private string xIntradayGetTimeFramesInfo(IList<TimeSpan> timeFrames, bool closeInNextFrame)
        {
            var sbParameters = new StringBuilder();
            if (timeFrames.Count > 1)
                sbParameters.Append($"Time frames: {CsUtils.GetString(timeFrames[0])}-{CsUtils.GetString(timeFrames[timeFrames.Count - 1])}, interval: {CsUtils.GetString(timeFrames[1] - timeFrames[0])}");
            else if (timeFrames.Count == 1)
                sbParameters.Append($"Time frames: {CsUtils.GetString(timeFrames[0])}");

            if (closeInNextFrame)
                sbParameters.Append(", closeInNextFrame");

            return sbParameters.ToString();
        }

        private List<TimeSpan> IntradayGetTimeFrames()
        {
            var interval = new TimeSpan(0, Convert.ToInt32(nudInterval.Value), 0);
            var from = new TimeSpan(Convert.ToInt32(nudFromHour.Value), Convert.ToInt32(nudFromMinute.Value), 0);
            var to = new TimeSpan(Convert.ToInt32(nudToHour.Value), Convert.ToInt32(nudToMinute.Value), 0);
            string error = null;
            if (from > to)
                error = "Time frame error: 'From' value must be less than 'To' value";
            else if (interval.TotalMinutes < 1)
                error = "Time frame error: 'Interval' value must be greater than or equal to 1";
            else if (interval.TotalMinutes > (to-from).TotalMinutes)
                error = "Time frame error: 'Interval' value must be less than or equal to difference between 'To' and 'From'";
            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            var timeFrames = CsUtils.GetTimeFrames(from, to, interval);
            return timeFrames;
        }

        private void IntradayPrintReportLines(List<object[]> reportLines)
        {
            Debug.Print("===================================================================");
            foreach (var value in reportLines)
            {
                var sb = new StringBuilder();
                foreach (var a in value)
                    sb.Append((sb.Length == 0 ? "" : "\t") + CsUtils.GetString(a));

                Debug.Print(sb.ToString());
            }
        }
        #endregion

        private void ExcelTest()
        {
            var iParameters = IntradayGetParameters();
            if (iParameters.TimeFrames == null) return;

            var zipFile = CsUtils.OpenFileDialogGeneric(Settings.MinuteYahooCacheFolder, @"Cache*.zip file (*.zip)|Cache*.zip");
            if (string.IsNullOrEmpty(zipFile)) return;

            var quotesInfoMinute = new QuotesInfo();
            var minuteQuotes = QuoteLoader.MinuteYahoo_GetQuotesFromZipCache(ShowStatus, zipFile, true, quotesInfoMinute);

            var quotesInfo = new QuotesInfo();
            var quotes = QuoteLoader.GetIntradayQuotes(null, minuteQuotes, iParameters, quotesInfo).ToArray();

            var reportLines = IntradayResults.ByTime(quotes, iParameters);
            IntradayPrintReportLines(reportLines);

            var data = new Dictionary<string, ExcelHelper.StatisticsData>();
            var statisticsData = new ExcelHelper.StatisticsData()
            {
                Title = "ByTimeX", Header1 = quotesInfo.GetStatus(), Header2 = iParameters.GetTimeFramesInfo(),
                Header3 = iParameters.GetFeesInfo(), Table = reportLines
            };
            data.Add("ByTimeX", statisticsData);
            var excelFileName = IntradayGetExcelFileName(zipFile, "Test", iParameters);
            Helpers.ExcelHelper.SaveStatisticsToExcel(data, excelFileName);

            ShowStatus($"Finished! Filename: {excelFileName}");
        }

        private void btnCompareMinuteYahooZips_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenFileDialogMultiselect(Settings.MinuteYahooDataFolder,
                    @"YahooMinute_202*.zip file (*.zip)|YahooMinute_202*.zip", "Select two YahooMinute files") is string[] files &&
                files.Length > 0)
            {
                if (files.Length != 2)
                    MessageBox.Show("You should to select 2 YahooMinute files");
                else
                {
                    Check.MinuteYahoo_CompareZipFiles(ShowStatus, files[0], files[1]);
                }
            }

        }

        private void btnMinuteYahooErrorCheck_Click(object sender, EventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            if (CsUtils.OpenFileDialogMultiselect(Settings.MinuteYahooDataFolder, @"zip files (*.zip)|*.zip") is string[] files && files.Length > 0)
                Check.MinuteYahoo_ErrorCheck(files, ShowStatus);
            sw.Stop();
            Debug.Print($"btnMinuteYahooErrorCheck_Click: {sw.ElapsedMilliseconds:N0} millisecs");
        }

        private void btnIntradayYahooQuotesSaveToDB_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenFileDialogMultiselect(Settings.MinuteYahooDataFolder, @"YahooMinute_202?????.zip file (*.zip)|YahooMinute_202?????.zip") is string[] files && files.Length > 0)
                Actions.MinuteYahooQuotes_SaveToDb.Execute(files, ShowStatus);
        }

        private void btnMinuteAlphaVantageDownload_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenTxtFileDialog(Settings.MinuteAlphaVantageFolder) is string symbolListFile)
            {
                var symbols = File.ReadAllLines(symbolListFile).Where(a => !a.StartsWith("#") && !string.IsNullOrEmpty(a.Trim())).ToArray();
                Task.Factory.StartNew(() => MAV_Download.Start(symbols, ShowStatus));
            }

            //Download.MinuteAlphaVantage_Download(ShowStatus);
        }

        private void xxbtnMinuteAlphaVantageSaveLogToDb_Click(object sender, EventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = @"E:\Quote\WebData\Minute\AlphaVantage\DataBuffer", IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Task.Factory.StartNew(() => MAV_SaveLogToDb.Start(dialog.FileName, ShowStatus));
            }
        }

        private void btnMinuteYahooSaveLogToDb_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenFileDialogMultiselect(Settings.MinuteYahooDataFolder, @"zip files (*.zip)|*.zip") is string[] files && files.Length > 0)
                Actions.MinuteYahoo_SaveLogToDb.Start(files, ShowStatus);
        }

        private void btnMinuteAlphaVantageDownloadStop_Click(object sender, EventArgs e)
        {
            MAV_Download.Stop();
            Actions.DayAlphaVantage.DAV_Download.Stop();
        }

        private void btnIntradayAlphaVantageRefreshProxyList_Click(object sender, EventArgs e)
        {
            MAV_Download.RefreshProxyList();
            Actions.DayAlphaVantage.DAV_Download.RefreshProxyList();
        }

        private async void btnMinuteAlphaVantageSplitData_Click(object sender, EventArgs e)
        {
            btnMinuteAlphaVantageSplitData.Enabled = false;
            
            var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = @"E:\Quote\WebData\Minute\AlphaVantage\DataBuffer",
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                await Task.Factory.StartNew(() => MAV_SplitData.Start(dialog.FileName, ShowStatus));
            }

            btnMinuteAlphaVantageSplitData.Enabled = true;
        }

        private void btnDayAlphaVantageDownload_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => Actions.DayAlphaVantage.DAV_Download.Start(ShowStatus));
        }

        private void btnDayAlphaVantageParse_Click(object sender, EventArgs e)
        {
            if (CsUtils.OpenFileDialogGeneric(Settings.DayAlphaVantageDataFolder, @"DayAlphaVantage_202*.zip file (*.zip)|DayAlphaVantage_202*.zip") is string file)
                Actions.DayAlphaVantage.DAV_Parse.Start(file, ShowStatus);
        }

        private async void btnProfileYahooParse_Click(object sender, EventArgs e)
        {
            btnProfileYahooParse.Enabled = false;

            if (CsUtils.OpenFileDialogGeneric(Settings.ProfileYahooFolder, "Zip Files|*.zip") is string fn && !string.IsNullOrEmpty(fn))
                await Task.Factory.StartNew(() => Actions.SymbolsYahoo.ProfileYahoo_Parse.Start(fn, ShowStatus));

            btnProfileYahooParse.Enabled = true;
        }

        private async void btnScreenerNasdaqDownload_Click(object sender, EventArgs e)
        {
            btnScreenerNasdaqDownload.Enabled = false;
            await Task.Factory.StartNew(() => ScreenerNasdaq_Download.Start(ShowStatus));
            btnScreenerNasdaqDownload.Enabled = true;
        }

        private async void btnNasdaqScreenerParse_Click(object sender, EventArgs e)
        {
            btnScreenerNasdaqParse.Enabled = false;
            if (CsUtils.OpenFileDialogGeneric(Settings.ScreenerNasdaqFolder, "Zip Files|*.zip") is string fn && !string.IsNullOrEmpty(fn))
                await Task.Factory.StartNew(() => ScreenerNasdaq_Parse.Start(fn, ShowStatus));

            btnScreenerNasdaqParse.Enabled = true;
        }

        private async void btnWA_DownloadEoddataSymbols_Click(object sender, EventArgs e)
        {
            btnWA_DownloadEoddataSymbols.Enabled = false;

            // var exchanges = new[] { "AMEX", "NASDAQ", "NYSE" };
            // var exchanges = new[] { "NASDAQ", "NYSE" };
            var exchanges = new[] { "OTCBB" };
            var letters = Enumerable.Range('A', 'Z' - 'A' + 1).Select(c => (char)c).ToArray();
            foreach (var exchange in exchanges)
            foreach (var letter in letters)
            {
                await Task.Factory.StartNew(() =>
                    Actions.Eoddata.WebArchive_Symbols.DownloadData($"https://www.eoddata.com/stocklist/{exchange}/{letter}.htm",
                        $"E:\\Quote\\WebArchive\\Symbols\\Eoddata\\{exchange}\\{exchange}_{letter}_{{0}}.htm",
                        ShowStatus));
            }
            btnWA_DownloadEoddataSymbols.Enabled = true;
        }

        private async void btnWA_ParseEoddataSymbols_Click(object sender, EventArgs e)
        {
            btnWA_ParseEoddataSymbols.Enabled = false;

            await Task.Factory.StartNew(() =>
                Actions.Eoddata.WebArchive_Symbols.ParseData($"E:\\Quote\\WebArchive\\Symbols\\Eoddata", ShowStatus));
            
            btnWA_ParseEoddataSymbols.Enabled = true;
        }

        private async void btnWebArchiveDownloadHtmlTradingViewScreener_Click(object sender, EventArgs e)
        {
            btnWebArchiveDownloadHtmlTradingViewScreener.Enabled = false;
            await Task.Factory.StartNew(() => Actions.TradingView.WebArchive_Screener.DownloadHtmlData(ShowStatus));
            btnWebArchiveDownloadHtmlTradingViewScreener.Enabled = true;
        }

        private async void btnWebArchiveDownloadJsonTradingViewScreener_Click(object sender, EventArgs e)
        {
            btnWebArchiveDownloadHtmlTradingViewScreener.Enabled = false;
            await Task.Factory.StartNew(() => Actions.TradingView.WebArchive_Screener.DownloadJsonData(ShowStatus));
            btnWebArchiveDownloadHtmlTradingViewScreener.Enabled = true;
        }

        private async void btnWebArchiveParseTradingViewScreener_Click(object sender, EventArgs e)
        {
            btnWebArchiveParseTradingViewScreener.Enabled = false;
            await Task.Factory.StartNew(() => Actions.TradingView.WebArchive_Screener.ParseData(ShowStatus));
            btnWebArchiveParseTradingViewScreener.Enabled = true;
        }

        private async void btnWebArchiveDownloadTradingViewProfiles_Click(object sender, EventArgs e)
        {
            btnWebArchiveDownloadTradingViewProfiles.Enabled = false;
            await Task.Factory.StartNew(() => Actions.TradingView.WebArchive_Profile.DownloadData(ShowStatus));
            btnWebArchiveDownloadTradingViewProfiles.Enabled = true;
        }

        private async void btnWebArchiveParseTradingViewProfiles_Click(object sender, EventArgs e)
        {
            btnWebArchiveParseTradingViewProfiles.Enabled = false;
            await Task.Factory.StartNew(() => Actions.TradingView.WebArchive_Profile.ParseData(ShowStatus));
            btnWebArchiveParseTradingViewProfiles.Enabled = true;
        }

        private async void btnTradingViewRecommendParse_Click(object sender, EventArgs e)
        {
            btnTradingViewRecommendParse.Enabled = false;
            await Task.Factory.StartNew(() =>
            {
                var files = Directory.GetFiles(Settings.ScreenerTradingViewFolder, "*.zip").OrderBy(a => a).ToArray();
                foreach (var file in files)
                {
                    Actions.TradingView.Recommend_Parse.Parse(file, ShowStatus);
                }
            });

            // if (CsUtils.OpenFileDialogGeneric(Settings.ScreenerTradingViewFolder, @"TVScreener_202?????.zip file (*.zip)|TVScreener_202?????.zip") is string fn && !string.IsNullOrEmpty(fn))
            //  Actions.Parse.ScreenerTradingView_ParseAndSaveToDb(fn, ShowStatus);
            
            btnTradingViewRecommendParse.Enabled = true;
        }

        private async void btnWikipediaIndicesDownload_Click(object sender, EventArgs e)
        {
            btnWikipediaIndicesDownload.Enabled = false;
            await Task.Factory.StartNew(() => Actions.Wikipedia.Indices.Download(ShowStatus));
            btnWikipediaIndicesDownload.Enabled = true;
        }

        private async void btnWikipediaIndicesParse_Click(object sender, EventArgs e)
        {
            btnWikipediaIndicesParse.Enabled = false;
            await Task.Factory.StartNew(() => Actions.Wikipedia.Indices.Parse(Settings.IndicesWikipediaFolder + "IndexComponents_20230306.zip", ShowStatus));
//            if (CsUtils.OpenZipFileDialog(Settings.IndicesWikipediaFolder) is string fn && !string.IsNullOrEmpty(fn))
  //              await Task.Factory.StartNew(() => Actions.Wikipedia.Indices.Parse(fn, ShowStatus));
            btnWikipediaIndicesParse.Enabled = true;
        }

        private async void btnRussellIndicesParse_Click(object sender, EventArgs e)
        {
            btnRussellIndicesParse.Enabled = false;
            if (CsUtils.OpenZipFileDialog(Settings.IndicesRussellFolder) is string fn && !string.IsNullOrEmpty(fn))
                await Task.Factory.StartNew(() => Actions.Russell.IndexRussell.Parse(fn, ShowStatus));
            btnRussellIndicesParse.Enabled = true;
        }

        private async void btnWebArchiveWikipediaIndicesParse_Click(object sender, EventArgs e)
        {
            btnWebArchiveWikipediaIndicesParse.Enabled = false;
            await Task.Factory.StartNew(() =>
                Actions.Wikipedia.Indices.Parse(
                    @"E:\Quote\WebArchive\Indices\Wikipedia\WebArchive.Wikipedia.Indices.zip", ShowStatus));
            btnWebArchiveWikipediaIndicesParse.Enabled = true;
        }

        private async void btnStockAnalysisIPO_Click(object sender, EventArgs e)
        {
            btnStockAnalysisIPO.Enabled = false;
            await Task.Factory.StartNew(() => Actions.StockAnalysis.IPO.Start(true, ShowStatus));
            btnStockAnalysisIPO.Enabled = true;
        }

        private async void btnWebArchiveParseStockAnalysisActions_Click(object sender, EventArgs e)
        {
            ((Control)sender).Enabled = false;
            await Task.Factory.StartNew(() => Actions.StockAnalysis.WebArchiveActions.Parse(ShowStatus));
            ((Control)sender).Enabled = true;
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            // await Task.Run(() => Data.Helpers.ZipUtils.ReZipFiles(@"E:\Quote\WebData\Symbols\Polygon2003\Data - Copy", false, ShowStatus));

            /*var files = Directory.GetFiles(@"E:\Quote\WebData\Minute\Polygon2003\DataBuffer", "*.zip").OrderByDescending(a => a).Take(17).ToArray();
            foreach (var file in files)
            {
                await Task.Run(() => Data.Actions.Polygon2003.PolygonMinuteSaveLogToDb.Start(file));
            }

            Data.Helpers.Logger.AddMessage($"Finished for {files.Length} files");*/

            // var a = Data.Actions.Polygon.PolygonCommon.TestSymbols.Select(a=>a.Key).ToArray();
            // await Task.Factory.StartNew(Data.Actions.Polygon2003.PolygonSymbolsLoader2003.ParseAndSaveAllZip);
            // await Task.Factory.StartNew(Data.Actions.Polygon2003.PolygonMinuteLoader2003.StartAll);
            // await Task.Run(() => Data.Actions.Polygon2003.PolygonMinuteSaveLogToDb.Start(@"E:\Quote\WebData\Minute\Polygon2003\DataBuffer\MP2003_20171111.zip"));
            /*var files = Directory.GetFiles(@"E:\Quote\WebData\Minute\Polygon\DataBuffer-2023-12", "MinutePolygon_*.zip");
            foreach (var file in files)
            {
                var newFN = file.Replace("MinutePolygon_", "MinutePolygon2023_");
                File.Move(file, newFN);
            }*/
        }

        private async void btnRunMultiItemsLoader_Click(object sender, EventArgs e)
        {
            ((Control)sender).Enabled = false;
            dataGridView1.ReadOnly = true;

            foreach (var item in Data.Models.LoaderItem.DataGridLoaderItems)
                item.Reset();

            foreach (var item in Data.Models.LoaderItem.DataGridLoaderItems.Where(a => a.Checked))
                await item.Start();

            dataGridView1.ReadOnly = false;
            ((Control)sender).Enabled = true;
        }

        private async void btnTemp_Click(object sender, EventArgs e)
        {
            btnTemp.Enabled = false;

            /*var files = Directory.GetFiles(@"E:\Quote\WebData\Daily\Polygon\Data\", "DayPolygon_**.zip").Where(a=> string.Compare(Path.GetFileName(a), "DayPolygon_20230724.zip", StringComparison.InvariantCultureIgnoreCase) == 1).OrderBy(a=>a).ToArray();
            foreach (var file in files)
            {
                await Task.Factory.StartNew(() =>
                {
                    Data.Actions.Polygon.PolygonDailyLoader.ParseAndSaveToDb(file);
                });
            }

            Data.Helpers.DbUtils.RunProcedure("dbQ2023..pUpdateDayPolygon");*/

            // if (CsUtils.OpenZipFileDialog($@"E:\Quote\WebData\Indices\Wikipedia\IndexComponents") is string zipFileName && !string.IsNullOrEmpty(zipFileName))
            //  await Task.Factory.StartNew(() => Data.Actions.Wikipedia.WikipediaIndexLoader.ParseAndSaveToDb(zipFileName));

            btnTemp.Enabled = true;
        }

        private async void btnRussellIndicesParseZipFile_Click(object sender, EventArgs e)
        {
            btnRussellIndicesParseZipFile.Enabled = false;
            if (CsUtils.OpenZipFileDialog(Settings.IndicesRussellFolder) is string zipFileName && !string.IsNullOrEmpty(zipFileName))
                await Task.Factory.StartNew(() => Data.Actions.Russell.RussellIndexLoader.Parse(zipFileName));
            btnRussellIndicesParseZipFile.Enabled = true;
        }

        private async void btnMavSaveLogToDb_Click(object sender, EventArgs e)
        {
            btnMavSaveLogToDb.Enabled = false;

            var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = @"E:\Quote\WebData\Minute\AlphaVantage\DataBuffer",
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                await Task.Factory.StartNew(() => Data.Actions.AlphaVantage.MavSaveLogToDb.Start(dialog.FileName));
            }

            btnMavSaveLogToDb.Enabled = true;
        }

        private async void btnMavSplitDataLog_Click(object sender, EventArgs e)
        {
            btnMavSplitDataLog.Enabled = false;

            var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = @"E:\Quote\WebData\Minute\AlphaVantage\DataBuffer",
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                await Task.Factory.StartNew(() => Data.Actions.AlphaVantage.MavSplitData.Start(dialog.FileName, true));
            }

            btnMavSplitDataLog.Enabled = true;

        }

        private async void btnMavSplitDataAndSaveToZip_Click(object sender, EventArgs e)
        {
            btnMavSplitDataAndSaveToZip.Enabled = false;

            var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = @"E:\Quote\WebData\Minute\AlphaVantage\DataBuffer",
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                await Task.Factory.StartNew(() => Data.Actions.AlphaVantage.MavSplitData.Start(dialog.FileName, false));
            }

            btnMavSplitDataAndSaveToZip.Enabled = true;
        }

        private async void btnMavCopyZipInfoToDb_Click(object sender, EventArgs e)
        {
            btnMavCopyZipInfoToDb.Enabled = false;
            await Task.Factory.StartNew(Data.Actions.AlphaVantage.MavCopyZipInfoToDb.Start);
            btnMavCopyZipInfoToDb.Enabled = true;
        }

        private async void btnWAEoddataSymbols_Click(object sender, EventArgs e)
        {
            btnWAEoddataSymbols.Enabled = false;
            await Task.Factory.StartNew(Data.Actions.WebArchive.Eoddata.WA_EoddataSymbolsLoader.Start);
            btnWAEoddataSymbols.Enabled = true;
        }

        private async void btnFmpCloudSplits_Click(object sender, EventArgs e)
        {
            btnFmpCloudSplits.Enabled = false;
            // await Task.Factory.StartNew(Data.Actions.FmpCloud.FmpCloudSplitsLoader.Start);
            var filename = @"E:\Quote\WebData\Splits\FmpCloud\Data\FmpCloudSplits_20230327.zip";
            await Task.Factory.StartNew(() => Data.Actions.FmpCloud.FmpCloudSplitsLoader.ParseAndSaveToDb(filename));
            btnFmpCloudSplits.Enabled = true;

        }

        private async void btnWaEoddataSymbolsParseAndSaveToDb_Click(object sender, EventArgs e)
        {
            btnWaEoddataSymbolsParseAndSaveToDb.Enabled = false;

            /*var folder = @"E:\Quote\WebArchive\Eoddata\Symbols\WA_Eoddata_Symbols_20230327\NASDAQ";
            var files = Directory.GetFiles(folder, "*.html");
            foreach (var file in files)
            {
                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var timestamp = DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                File.SetCreationTime(file, timestamp); 
                File.SetLastWriteTime(file, timestamp);

            }*/
            var filename = @"E:\Quote\WebArchive\Eoddata\Symbols\WA_Eoddata_Symbols_20230327\WA_Eoddata_Symbols_AMEX_20230327.zip";
            await Task.Factory.StartNew(() => Data.Actions.WebArchive.Eoddata.WA_EoddataSymbolsLoader.ParseAndSaveToDb(filename));

            btnWaEoddataSymbolsParseAndSaveToDb.Enabled = true;

        }

        private async void btnMinutePolygonSaveLogToDb_Click(object sender, EventArgs e)
        {
            btnMinutePolygonSaveLogToDb.Enabled = false;

            /*var folder = @"E:\Quote\WebData\Minute\Polygon\DataBuffer-2023-12";
            var files = Directory.GetFiles(folder, "MinutePolygon2023_*.zip").OrderBy(a=>a).ToArray();
            foreach (var file in files)
            {
                await Task.Factory.StartNew(() => Data.Actions.Polygon.PolygonMinuteSaveLogToDb.Start(file));
            }*/

            if (CsUtils.OpenZipFileDialog(@"E:\Quote\WebData\Minute\Polygon\DataBuffer") is string zipFileName && File.Exists(zipFileName))
                await Task.Factory.StartNew(() => Data.Actions.Polygon.PolygonMinuteSaveLogToDb.Start(zipFileName));

            btnMinutePolygonSaveLogToDb.Enabled = true;
        }

        private async void btnMinutePolygonSplitFilesByDates_Click(object sender, EventArgs e)
        {
            btnMinutePolygonSplitFilesByDates.Enabled = false;

            if (CsUtils.OpenZipFileDialog(@"E:\Quote\WebData\Minute\Polygon\DataBuffer") is string zipFileName && !string.IsNullOrEmpty(zipFileName))
                await Task.Factory.StartNew(() => Data.Actions.Polygon.PolygonMinuteSplitData.Start(zipFileName));

            btnMinutePolygonSplitFilesByDates.Enabled = true;
        }

        private async void btnPolygonCopyZipInfoToDb_Click(object sender, EventArgs e)
        {
            btnPolygonCopyZipInfoToDb.Enabled = false;
            await Task.Factory.StartNew(Data.Actions.Polygon.PolygonCopyZipInfoToDb.Start);
            btnPolygonCopyZipInfoToDb.Enabled = true;
        }

        private async void btnPolygon2003_daily_Click(object sender, EventArgs e)
        {
            /*btnPolygon2003_daily.Enabled = false;
            await Task.Factory.StartNew(Data.Actions.Polygon2003.PolygonDailyLoader2003.Start);
            btnPolygon2003_daily.Enabled = true;*/
        }

        private async void btnPolygon2003_symbols_Click(object sender, EventArgs e)
        {
            /*btnPolygon2003_symbols.Enabled = false;
            // await Task.Factory.StartNew(Data.Actions.Polygon2003.PolygonSymbolsLoader2003.Start);
            await Task.Factory.StartNew(Data.Actions.Polygon2003.PolygonSymbolsLoader2003.ParseAndSaveAllZip);
            btnPolygon2003_symbols.Enabled = true;*/
        }

        private async void btnMinutePolygonUpdateDailyIn_Click(object sender, EventArgs e)
        {
            btnMinutePolygonUpdateDailyIn.Enabled = false;
            await Task.Factory.StartNew(Data.Actions.Polygon.PolygonDailyInUpdater.Run);
            // await Task.Factory.StartNew(Data.Actions.Polygon.PolygonMinuteLoader.RunOthers_2023_12_14);
            btnMinutePolygonUpdateDailyIn.Enabled = true;
        }

        private async void btnPolygon2003Daily_Click(object sender, EventArgs e)
        {
            btnPolygon2003Daily.Enabled = false;
            await Task.Factory.StartNew(Data.Actions.Polygon2003.PolygonDailyLoader2003.Start);
            btnPolygon2003Daily.Enabled = true;
        }

        private async void btnPolygon2003Symbols_Click(object sender, EventArgs e)
        {
            btnPolygon2003Symbols.Enabled = false;
            await Task.Factory.StartNew(Data.Actions.Polygon2003.PolygonSymbolsLoader2003.Start);
            btnPolygon2003Symbols.Enabled = true;
        }
    }
}

