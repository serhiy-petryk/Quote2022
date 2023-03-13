
using Data.Models;

namespace Quote2022
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpLoaderNew = new System.Windows.Forms.TabPage();
            this.btnRunMultiItemsLoader = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Image = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Started = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Duration = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.loaderItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tabLoader = new System.Windows.Forms.TabPage();
            this.btnNasdaqScreenerParse = new System.Windows.Forms.Button();
            this.btnScreenerNasdaqDownload = new System.Windows.Forms.Button();
            this.btnProfileYahooParse = new System.Windows.Forms.Button();
            this.btnDayAlphaVantageParse = new System.Windows.Forms.Button();
            this.btnDayAlphaVantageDownload = new System.Windows.Forms.Button();
            this.btnMinuteAlphaVantageSplitData = new System.Windows.Forms.Button();
            this.btnMinuteAlphaVantageDownloadStop = new System.Windows.Forms.Button();
            this.btnIntradayAlphaVantageRefreshProxyList = new System.Windows.Forms.Button();
            this.btnMinuteAlphaVantageSaveLogToDb = new System.Windows.Forms.Button();
            this.btnMinuteAlphaVantageDownload = new System.Windows.Forms.Button();
            this.btnDayYahooDownload = new System.Windows.Forms.Button();
            this.btnSymbolsYahooLookupParse = new System.Windows.Forms.Button();
            this.btnSymbolsYahooLookupDownload = new System.Windows.Forms.Button();
            this.btnUpdateTradingDays = new System.Windows.Forms.Button();
            this.btnTimeSalesNasdaqSaveSummary = new System.Windows.Forms.Button();
            this.btnRefreshSpitsData = new System.Windows.Forms.Button();
            this.btnTimeSalesNasdaqSaveLog = new System.Windows.Forms.Button();
            this.btnSymbolsQuantumonlineParse = new System.Windows.Forms.Button();
            this.btnSymbolsQuantumonlineDownload = new System.Windows.Forms.Button();
            this.btnTimeSalesNasdaqDownload = new System.Windows.Forms.Button();
            this.btnRefreshSymbolsData = new System.Windows.Forms.Button();
            this.btnSymbolsNasdaqParse = new System.Windows.Forms.Button();
            this.btnSymbolsStockanalysisParse = new System.Windows.Forms.Button();
            this.btnSymbolsStockanalysisDownload = new System.Windows.Forms.Button();
            this.btnQuantumonlineProfilesParse = new System.Windows.Forms.Button();
            this.btnSplitInvestingHistoryParse = new System.Windows.Forms.Button();
            this.btnStockSplitHistoryParse = new System.Windows.Forms.Button();
            this.btnScreenerNasdaqParse = new System.Windows.Forms.Button();
            this.btnSplitEoddataParse = new System.Windows.Forms.Button();
            this.btnSplitInvestingParse = new System.Windows.Forms.Button();
            this.btnSplitYahooParse = new System.Windows.Forms.Button();
            this.btnSymbolsEoddataParse = new System.Windows.Forms.Button();
            this.btnDayEoddataParse = new System.Windows.Forms.Button();
            this.btnNanexSymbols = new System.Windows.Forms.Button();
            this.btnDayYahooIndexesParse = new System.Windows.Forms.Button();
            this.btnDayYahooParse = new System.Windows.Forms.Button();
            this.tabLayers = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.gbDataSet = new System.Windows.Forms.GroupBox();
            this.cb2013 = new System.Windows.Forms.CheckBox();
            this.cb2022 = new System.Windows.Forms.CheckBox();
            this.btnAlgorithm1 = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.btnStockAnalysisIPO = new System.Windows.Forms.Button();
            this.btnWebArchiveWikipediaIndicesParse = new System.Windows.Forms.Button();
            this.btnRussellIndicesParse = new System.Windows.Forms.Button();
            this.btnWikipediaIndicesParse = new System.Windows.Forms.Button();
            this.btnWikipediaIndicesDownload = new System.Windows.Forms.Button();
            this.btnTradingViewRecommendParse = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnIntradayYahooQuotesSaveToDB = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.nudIntradayFees = new System.Windows.Forms.NumericUpDown();
            this.cbIntradayStopInPercent = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.nudIntradayStop = new System.Windows.Forms.NumericUpDown();
            this.btnIntradayStatisticsSaveToDB = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.nudInterval = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.cbCloseInNextFrame = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.nudToMinute = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.nudToHour = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.nudFromMinute = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.nudFromHour = new System.Windows.Forms.NumericUpDown();
            this.btnIntradayByTimeReportsClosedInNextFrame = new System.Windows.Forms.Button();
            this.btnIntradayByTimeReports = new System.Windows.Forms.Button();
            this.btnIntradayPrintDetails = new System.Windows.Forms.Button();
            this.gbIntradayDataList = new System.Windows.Forms.GroupBox();
            this.clbIntradayDataList = new System.Windows.Forms.CheckedListBox();
            this.btnIntradayGenerateReport = new System.Windows.Forms.Button();
            this.btnPrepareYahooMinuteTextCache = new System.Windows.Forms.Button();
            this.btnCheckYahooMinuteData = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnMinuteYahooSaveLogToDb = new System.Windows.Forms.Button();
            this.btnMinuteYahooErrorCheck = new System.Windows.Forms.Button();
            this.btnDailyEoddataCheck = new System.Windows.Forms.Button();
            this.btnCompareMinuteYahooZips = new System.Windows.Forms.Button();
            this.btnMinuteYahooLog = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnWebArchiveParseStockAnalysisActions = new System.Windows.Forms.Button();
            this.btnWebArchiveParseTradingViewProfiles = new System.Windows.Forms.Button();
            this.btnWebArchiveDownloadTradingViewProfiles = new System.Windows.Forms.Button();
            this.btnWebArchiveDownloadJsonTradingViewScreener = new System.Windows.Forms.Button();
            this.btnWebArchiveParseTradingViewScreener = new System.Windows.Forms.Button();
            this.btnWebArchiveDownloadHtmlTradingViewScreener = new System.Windows.Forms.Button();
            this.btnWA_ParseEoddataSymbols = new System.Windows.Forms.Button();
            this.btnWA_DownloadEoddataSymbols = new System.Windows.Forms.Button();
            this.btnToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.btnTemp = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tpLoaderNew.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.loaderItemBindingSource)).BeginInit();
            this.tabLoader.SuspendLayout();
            this.tabLayers.SuspendLayout();
            this.gbDataSet.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntradayFees)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntradayStop)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudToMinute)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudToHour)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFromMinute)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFromHour)).BeginInit();
            this.gbIntradayDataList.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 437);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1166, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(67, 17);
            this.StatusLabel.Text = "StatusLabel";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpLoaderNew);
            this.tabControl1.Controls.Add(this.tabLoader);
            this.tabControl1.Controls.Add(this.tabLayers);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1166, 437);
            this.tabControl1.TabIndex = 12;
            // 
            // tpLoaderNew
            // 
            this.tpLoaderNew.Controls.Add(this.btnTemp);
            this.tpLoaderNew.Controls.Add(this.btnRunMultiItemsLoader);
            this.tpLoaderNew.Controls.Add(this.button4);
            this.tpLoaderNew.Controls.Add(this.dataGridView1);
            this.tpLoaderNew.Location = new System.Drawing.Point(4, 22);
            this.tpLoaderNew.Name = "tpLoaderNew";
            this.tpLoaderNew.Padding = new System.Windows.Forms.Padding(3);
            this.tpLoaderNew.Size = new System.Drawing.Size(1158, 411);
            this.tpLoaderNew.TabIndex = 6;
            this.tpLoaderNew.Text = "Loader (new)";
            this.tpLoaderNew.UseVisualStyleBackColor = true;
            // 
            // btnRunMultiItemsLoader
            // 
            this.btnRunMultiItemsLoader.Location = new System.Drawing.Point(357, 18);
            this.btnRunMultiItemsLoader.Name = "btnRunMultiItemsLoader";
            this.btnRunMultiItemsLoader.Size = new System.Drawing.Size(128, 23);
            this.btnRunMultiItemsLoader.TabIndex = 6;
            this.btnRunMultiItemsLoader.Text = "Run multiItems loader";
            this.btnRunMultiItemsLoader.UseVisualStyleBackColor = true;
            this.btnRunMultiItemsLoader.Click += new System.EventHandler(this.btnRunMultiItemsLoader_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(542, 194);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 5;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.ColumnHeadersVisible = false;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn1,
            this.Image,
            this.dataGridViewTextBoxColumn1,
            this.Started,
            this.Duration});
            this.dataGridView1.DataSource = this.loaderItemBindingSource;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView1.Location = new System.Drawing.Point(8, 15);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 19;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(321, 331);
            this.dataGridView1.TabIndex = 2;
            this.dataGridView1.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellMouseEnter);
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
            // 
            // dataGridViewCheckBoxColumn1
            // 
            this.dataGridViewCheckBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.dataGridViewCheckBoxColumn1.DataPropertyName = "Checked";
            this.dataGridViewCheckBoxColumn1.HeaderText = "Checked";
            this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            this.dataGridViewCheckBoxColumn1.Width = 5;
            // 
            // Image
            // 
            this.Image.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.Image.DataPropertyName = "Image";
            this.Image.HeaderText = "Image";
            this.Image.Name = "Image";
            this.Image.ReadOnly = true;
            this.Image.Width = 5;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Name";
            this.dataGridViewTextBoxColumn1.HeaderText = "Name";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // Started
            // 
            this.Started.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.Started.DataPropertyName = "Started";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.Format = "HH:mm:ss";
            this.Started.DefaultCellStyle = dataGridViewCellStyle1;
            this.Started.HeaderText = "Started";
            this.Started.MinimumWidth = 2;
            this.Started.Name = "Started";
            this.Started.ReadOnly = true;
            this.Started.Width = 2;
            // 
            // Duration
            // 
            this.Duration.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.Duration.DataPropertyName = "Duration";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Duration.DefaultCellStyle = dataGridViewCellStyle2;
            this.Duration.HeaderText = "Time";
            this.Duration.MinimumWidth = 2;
            this.Duration.Name = "Duration";
            this.Duration.ReadOnly = true;
            this.Duration.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Duration.ToolTipText = "Duration, seconds";
            this.Duration.Width = 2;
            // 
            // loaderItemBindingSource
            // 
            this.loaderItemBindingSource.DataSource = typeof(Data.Models.LoaderItem);
            // 
            // tabLoader
            // 
            this.tabLoader.Controls.Add(this.btnNasdaqScreenerParse);
            this.tabLoader.Controls.Add(this.btnScreenerNasdaqDownload);
            this.tabLoader.Controls.Add(this.btnProfileYahooParse);
            this.tabLoader.Controls.Add(this.btnDayAlphaVantageParse);
            this.tabLoader.Controls.Add(this.btnDayAlphaVantageDownload);
            this.tabLoader.Controls.Add(this.btnMinuteAlphaVantageSplitData);
            this.tabLoader.Controls.Add(this.btnMinuteAlphaVantageDownloadStop);
            this.tabLoader.Controls.Add(this.btnIntradayAlphaVantageRefreshProxyList);
            this.tabLoader.Controls.Add(this.btnMinuteAlphaVantageSaveLogToDb);
            this.tabLoader.Controls.Add(this.btnMinuteAlphaVantageDownload);
            this.tabLoader.Controls.Add(this.btnDayYahooDownload);
            this.tabLoader.Controls.Add(this.btnSymbolsYahooLookupParse);
            this.tabLoader.Controls.Add(this.btnSymbolsYahooLookupDownload);
            this.tabLoader.Controls.Add(this.btnUpdateTradingDays);
            this.tabLoader.Controls.Add(this.btnTimeSalesNasdaqSaveSummary);
            this.tabLoader.Controls.Add(this.btnRefreshSpitsData);
            this.tabLoader.Controls.Add(this.btnTimeSalesNasdaqSaveLog);
            this.tabLoader.Controls.Add(this.btnSymbolsQuantumonlineParse);
            this.tabLoader.Controls.Add(this.btnSymbolsQuantumonlineDownload);
            this.tabLoader.Controls.Add(this.btnTimeSalesNasdaqDownload);
            this.tabLoader.Controls.Add(this.btnRefreshSymbolsData);
            this.tabLoader.Controls.Add(this.btnSymbolsNasdaqParse);
            this.tabLoader.Controls.Add(this.btnSymbolsStockanalysisParse);
            this.tabLoader.Controls.Add(this.btnSymbolsStockanalysisDownload);
            this.tabLoader.Controls.Add(this.btnQuantumonlineProfilesParse);
            this.tabLoader.Controls.Add(this.btnSplitInvestingHistoryParse);
            this.tabLoader.Controls.Add(this.btnStockSplitHistoryParse);
            this.tabLoader.Controls.Add(this.btnScreenerNasdaqParse);
            this.tabLoader.Controls.Add(this.btnSplitEoddataParse);
            this.tabLoader.Controls.Add(this.btnSplitInvestingParse);
            this.tabLoader.Controls.Add(this.btnSplitYahooParse);
            this.tabLoader.Controls.Add(this.btnSymbolsEoddataParse);
            this.tabLoader.Controls.Add(this.btnDayEoddataParse);
            this.tabLoader.Controls.Add(this.btnNanexSymbols);
            this.tabLoader.Controls.Add(this.btnDayYahooIndexesParse);
            this.tabLoader.Controls.Add(this.btnDayYahooParse);
            this.tabLoader.Location = new System.Drawing.Point(4, 22);
            this.tabLoader.Name = "tabLoader";
            this.tabLoader.Padding = new System.Windows.Forms.Padding(3);
            this.tabLoader.Size = new System.Drawing.Size(1158, 411);
            this.tabLoader.TabIndex = 0;
            this.tabLoader.Text = "Loader";
            this.tabLoader.UseVisualStyleBackColor = true;
            // 
            // btnNasdaqScreenerParse
            // 
            this.btnNasdaqScreenerParse.Location = new System.Drawing.Point(572, 171);
            this.btnNasdaqScreenerParse.Name = "btnNasdaqScreenerParse";
            this.btnNasdaqScreenerParse.Size = new System.Drawing.Size(157, 23);
            this.btnNasdaqScreenerParse.TabIndex = 59;
            this.btnNasdaqScreenerParse.Text = "Nasdaq Screener Parse";
            this.btnNasdaqScreenerParse.UseVisualStyleBackColor = true;
            this.btnNasdaqScreenerParse.Click += new System.EventHandler(this.btnNasdaqScreenerParse_Click);
            // 
            // btnScreenerNasdaqDownload
            // 
            this.btnScreenerNasdaqDownload.Location = new System.Drawing.Point(572, 142);
            this.btnScreenerNasdaqDownload.Name = "btnScreenerNasdaqDownload";
            this.btnScreenerNasdaqDownload.Size = new System.Drawing.Size(157, 23);
            this.btnScreenerNasdaqDownload.TabIndex = 58;
            this.btnScreenerNasdaqDownload.Text = "Nasdaq Screener Download";
            this.btnScreenerNasdaqDownload.UseVisualStyleBackColor = true;
            this.btnScreenerNasdaqDownload.Click += new System.EventHandler(this.btnScreenerNasdaqDownload_Click);
            // 
            // btnProfileYahooParse
            // 
            this.btnProfileYahooParse.Location = new System.Drawing.Point(384, 330);
            this.btnProfileYahooParse.Name = "btnProfileYahooParse";
            this.btnProfileYahooParse.Size = new System.Drawing.Size(146, 23);
            this.btnProfileYahooParse.TabIndex = 57;
            this.btnProfileYahooParse.Text = "Profile Yahoo Parse";
            this.btnProfileYahooParse.UseVisualStyleBackColor = true;
            this.btnProfileYahooParse.Click += new System.EventHandler(this.btnProfileYahooParse_Click);
            // 
            // btnDayAlphaVantageParse
            // 
            this.btnDayAlphaVantageParse.Location = new System.Drawing.Point(214, 160);
            this.btnDayAlphaVantageParse.Name = "btnDayAlphaVantageParse";
            this.btnDayAlphaVantageParse.Size = new System.Drawing.Size(159, 23);
            this.btnDayAlphaVantageParse.TabIndex = 56;
            this.btnDayAlphaVantageParse.Text = "DailyAlphaVantage parse (zip)";
            this.btnDayAlphaVantageParse.UseVisualStyleBackColor = true;
            this.btnDayAlphaVantageParse.Click += new System.EventHandler(this.btnDayAlphaVantageParse_Click);
            // 
            // btnDayAlphaVantageDownload
            // 
            this.btnDayAlphaVantageDownload.Location = new System.Drawing.Point(214, 131);
            this.btnDayAlphaVantageDownload.Name = "btnDayAlphaVantageDownload";
            this.btnDayAlphaVantageDownload.Size = new System.Drawing.Size(159, 23);
            this.btnDayAlphaVantageDownload.TabIndex = 55;
            this.btnDayAlphaVantageDownload.Text = "DailyAlphaVantage download";
            this.btnDayAlphaVantageDownload.UseVisualStyleBackColor = true;
            this.btnDayAlphaVantageDownload.Click += new System.EventHandler(this.btnDayAlphaVantageDownload_Click);
            // 
            // btnMinuteAlphaVantageSplitData
            // 
            this.btnMinuteAlphaVantageSplitData.Location = new System.Drawing.Point(3, 292);
            this.btnMinuteAlphaVantageSplitData.Name = "btnMinuteAlphaVantageSplitData";
            this.btnMinuteAlphaVantageSplitData.Size = new System.Drawing.Size(178, 40);
            this.btnMinuteAlphaVantageSplitData.TabIndex = 54;
            this.btnMinuteAlphaVantageSplitData.Text = "MinuteAlphaVantage split files by dates";
            this.btnMinuteAlphaVantageSplitData.UseVisualStyleBackColor = true;
            this.btnMinuteAlphaVantageSplitData.Click += new System.EventHandler(this.btnMinuteAlphaVantageSplitData_Click);
            // 
            // btnMinuteAlphaVantageDownloadStop
            // 
            this.btnMinuteAlphaVantageDownloadStop.Location = new System.Drawing.Point(8, 53);
            this.btnMinuteAlphaVantageDownloadStop.Name = "btnMinuteAlphaVantageDownloadStop";
            this.btnMinuteAlphaVantageDownloadStop.Size = new System.Drawing.Size(178, 43);
            this.btnMinuteAlphaVantageDownloadStop.TabIndex = 53;
            this.btnMinuteAlphaVantageDownloadStop.Text = "STOP download of MinuteAlphaVantage";
            this.btnMinuteAlphaVantageDownloadStop.UseVisualStyleBackColor = true;
            this.btnMinuteAlphaVantageDownloadStop.Click += new System.EventHandler(this.btnMinuteAlphaVantageDownloadStop_Click);
            // 
            // btnIntradayAlphaVantageRefreshProxyList
            // 
            this.btnIntradayAlphaVantageRefreshProxyList.Location = new System.Drawing.Point(8, 113);
            this.btnIntradayAlphaVantageRefreshProxyList.Name = "btnIntradayAlphaVantageRefreshProxyList";
            this.btnIntradayAlphaVantageRefreshProxyList.Size = new System.Drawing.Size(178, 23);
            this.btnIntradayAlphaVantageRefreshProxyList.TabIndex = 52;
            this.btnIntradayAlphaVantageRefreshProxyList.Text = "Refresh Proxy list";
            this.btnIntradayAlphaVantageRefreshProxyList.UseVisualStyleBackColor = true;
            this.btnIntradayAlphaVantageRefreshProxyList.Click += new System.EventHandler(this.btnIntradayAlphaVantageRefreshProxyList_Click);
            // 
            // btnMinuteAlphaVantageSaveLogToDb
            // 
            this.btnMinuteAlphaVantageSaveLogToDb.Location = new System.Drawing.Point(3, 252);
            this.btnMinuteAlphaVantageSaveLogToDb.Name = "btnMinuteAlphaVantageSaveLogToDb";
            this.btnMinuteAlphaVantageSaveLogToDb.Size = new System.Drawing.Size(195, 23);
            this.btnMinuteAlphaVantageSaveLogToDb.TabIndex = 50;
            this.btnMinuteAlphaVantageSaveLogToDb.Text = "MinuteAlphaVantage Save Log to DB";
            this.btnMinuteAlphaVantageSaveLogToDb.UseVisualStyleBackColor = true;
            this.btnMinuteAlphaVantageSaveLogToDb.Click += new System.EventHandler(this.btnMinuteAlphaVantageSaveLogToDb_Click);
            // 
            // btnMinuteAlphaVantageDownload
            // 
            this.btnMinuteAlphaVantageDownload.Location = new System.Drawing.Point(8, 15);
            this.btnMinuteAlphaVantageDownload.Name = "btnMinuteAlphaVantageDownload";
            this.btnMinuteAlphaVantageDownload.Size = new System.Drawing.Size(178, 23);
            this.btnMinuteAlphaVantageDownload.TabIndex = 49;
            this.btnMinuteAlphaVantageDownload.Text = "MinuteAlphaVantage download";
            this.btnMinuteAlphaVantageDownload.UseVisualStyleBackColor = true;
            this.btnMinuteAlphaVantageDownload.Click += new System.EventHandler(this.btnMinuteAlphaVantageDownload_Click);
            // 
            // btnDayYahooDownload
            // 
            this.btnDayYahooDownload.Location = new System.Drawing.Point(214, 15);
            this.btnDayYahooDownload.Name = "btnDayYahooDownload";
            this.btnDayYahooDownload.Size = new System.Drawing.Size(146, 23);
            this.btnDayYahooDownload.TabIndex = 45;
            this.btnDayYahooDownload.Text = "?DayYahoo Download";
            this.btnDayYahooDownload.UseVisualStyleBackColor = true;
            this.btnDayYahooDownload.Click += new System.EventHandler(this.btnDayYahooDownload_Click);
            // 
            // btnSymbolsYahooLookupParse
            // 
            this.btnSymbolsYahooLookupParse.Location = new System.Drawing.Point(384, 73);
            this.btnSymbolsYahooLookupParse.Name = "btnSymbolsYahooLookupParse";
            this.btnSymbolsYahooLookupParse.Size = new System.Drawing.Size(181, 23);
            this.btnSymbolsYahooLookupParse.TabIndex = 44;
            this.btnSymbolsYahooLookupParse.Text = "Symbols Yahoo Lookup Parse";
            this.btnSymbolsYahooLookupParse.UseVisualStyleBackColor = true;
            this.btnSymbolsYahooLookupParse.Click += new System.EventHandler(this.btnSymbolsYahooLookupParse_Click);
            // 
            // btnSymbolsYahooLookupDownload
            // 
            this.btnSymbolsYahooLookupDownload.Location = new System.Drawing.Point(384, 44);
            this.btnSymbolsYahooLookupDownload.Name = "btnSymbolsYahooLookupDownload";
            this.btnSymbolsYahooLookupDownload.Size = new System.Drawing.Size(181, 23);
            this.btnSymbolsYahooLookupDownload.TabIndex = 43;
            this.btnSymbolsYahooLookupDownload.Text = "Symbols Yahoo Lookup Download";
            this.btnSymbolsYahooLookupDownload.UseVisualStyleBackColor = true;
            this.btnSymbolsYahooLookupDownload.Click += new System.EventHandler(this.btnSymbolsYahooLookupDownload_Click);
            // 
            // btnUpdateTradingDays
            // 
            this.btnUpdateTradingDays.Location = new System.Drawing.Point(214, 292);
            this.btnUpdateTradingDays.Name = "btnUpdateTradingDays";
            this.btnUpdateTradingDays.Size = new System.Drawing.Size(146, 23);
            this.btnUpdateTradingDays.TabIndex = 42;
            this.btnUpdateTradingDays.Text = "Update Trading Days";
            this.btnUpdateTradingDays.UseVisualStyleBackColor = true;
            this.btnUpdateTradingDays.Click += new System.EventHandler(this.btnUpdateTradingDays_Click);
            // 
            // btnTimeSalesNasdaqSaveSummary
            // 
            this.btnTimeSalesNasdaqSaveSummary.Enabled = false;
            this.btnTimeSalesNasdaqSaveSummary.Location = new System.Drawing.Point(952, 171);
            this.btnTimeSalesNasdaqSaveSummary.Name = "btnTimeSalesNasdaqSaveSummary";
            this.btnTimeSalesNasdaqSaveSummary.Size = new System.Drawing.Size(178, 23);
            this.btnTimeSalesNasdaqSaveSummary.TabIndex = 41;
            this.btnTimeSalesNasdaqSaveSummary.Text = "TimeSalesNasdaq Save Summary";
            this.btnToolTip.SetToolTip(this.btnTimeSalesNasdaqSaveSummary, "Copy data from text file to DB and then convert DB data to text and compare data");
            this.btnTimeSalesNasdaqSaveSummary.UseVisualStyleBackColor = true;
            this.btnTimeSalesNasdaqSaveSummary.Click += new System.EventHandler(this.btnTimeSalesNasdaqSaveSummary_Click);
            // 
            // btnRefreshSpitsData
            // 
            this.btnRefreshSpitsData.Location = new System.Drawing.Point(754, 330);
            this.btnRefreshSpitsData.Name = "btnRefreshSpitsData";
            this.btnRefreshSpitsData.Size = new System.Drawing.Size(146, 23);
            this.btnRefreshSpitsData.TabIndex = 40;
            this.btnRefreshSpitsData.Text = "Refresh Spits Data";
            this.btnRefreshSpitsData.UseVisualStyleBackColor = true;
            this.btnRefreshSpitsData.Click += new System.EventHandler(this.btnRefreshSpitsData_Click);
            // 
            // btnTimeSalesNasdaqSaveLog
            // 
            this.btnTimeSalesNasdaqSaveLog.Enabled = false;
            this.btnTimeSalesNasdaqSaveLog.Location = new System.Drawing.Point(952, 142);
            this.btnTimeSalesNasdaqSaveLog.Name = "btnTimeSalesNasdaqSaveLog";
            this.btnTimeSalesNasdaqSaveLog.Size = new System.Drawing.Size(178, 23);
            this.btnTimeSalesNasdaqSaveLog.TabIndex = 38;
            this.btnTimeSalesNasdaqSaveLog.Text = "TimeSalesNasdaq Save Log";
            this.btnToolTip.SetToolTip(this.btnTimeSalesNasdaqSaveLog, "Copy data from text file to DB and then convert DB data to text and compare data");
            this.btnTimeSalesNasdaqSaveLog.UseVisualStyleBackColor = true;
            this.btnTimeSalesNasdaqSaveLog.Click += new System.EventHandler(this.btnTimeSalesNasdaqSaveLog_Click);
            // 
            // btnSymbolsQuantumonlineParse
            // 
            this.btnSymbolsQuantumonlineParse.Location = new System.Drawing.Point(952, 53);
            this.btnSymbolsQuantumonlineParse.Name = "btnSymbolsQuantumonlineParse";
            this.btnSymbolsQuantumonlineParse.Size = new System.Drawing.Size(187, 23);
            this.btnSymbolsQuantumonlineParse.TabIndex = 37;
            this.btnSymbolsQuantumonlineParse.Text = "SymbolsQuantumonline Parse";
            this.btnSymbolsQuantumonlineParse.UseVisualStyleBackColor = true;
            this.btnSymbolsQuantumonlineParse.Click += new System.EventHandler(this.btnSymbolsQuantumonlineParse_Click);
            // 
            // btnSymbolsQuantumonlineDownload
            // 
            this.btnSymbolsQuantumonlineDownload.Location = new System.Drawing.Point(952, 15);
            this.btnSymbolsQuantumonlineDownload.Name = "btnSymbolsQuantumonlineDownload";
            this.btnSymbolsQuantumonlineDownload.Size = new System.Drawing.Size(187, 23);
            this.btnSymbolsQuantumonlineDownload.TabIndex = 36;
            this.btnSymbolsQuantumonlineDownload.Text = "SymbolsQuantumonline Download";
            this.btnSymbolsQuantumonlineDownload.UseVisualStyleBackColor = true;
            this.btnSymbolsQuantumonlineDownload.Click += new System.EventHandler(this.btnSymbolsQuantumonlineDownload_Click);
            // 
            // btnTimeSalesNasdaqDownload
            // 
            this.btnTimeSalesNasdaqDownload.Enabled = false;
            this.btnTimeSalesNasdaqDownload.Location = new System.Drawing.Point(214, 91);
            this.btnTimeSalesNasdaqDownload.Name = "btnTimeSalesNasdaqDownload";
            this.btnTimeSalesNasdaqDownload.Size = new System.Drawing.Size(159, 23);
            this.btnTimeSalesNasdaqDownload.TabIndex = 35;
            this.btnTimeSalesNasdaqDownload.Text = "TimeSalesNasdaq Download";
            this.btnTimeSalesNasdaqDownload.UseVisualStyleBackColor = true;
            this.btnTimeSalesNasdaqDownload.Click += new System.EventHandler(this.btnTimeSalesNasdaqDownload_Click);
            // 
            // btnRefreshSymbolsData
            // 
            this.btnRefreshSymbolsData.Location = new System.Drawing.Point(384, 368);
            this.btnRefreshSymbolsData.Name = "btnRefreshSymbolsData";
            this.btnRefreshSymbolsData.Size = new System.Drawing.Size(146, 23);
            this.btnRefreshSymbolsData.TabIndex = 34;
            this.btnRefreshSymbolsData.Text = "Refresh Symbols Data";
            this.btnRefreshSymbolsData.UseVisualStyleBackColor = true;
            this.btnRefreshSymbolsData.Click += new System.EventHandler(this.btnRefreshSymbolsData_Click);
            // 
            // btnSymbolsNasdaqParse
            // 
            this.btnSymbolsNasdaqParse.Enabled = false;
            this.btnSymbolsNasdaqParse.Location = new System.Drawing.Point(384, 252);
            this.btnSymbolsNasdaqParse.Name = "btnSymbolsNasdaqParse";
            this.btnSymbolsNasdaqParse.Size = new System.Drawing.Size(146, 23);
            this.btnSymbolsNasdaqParse.TabIndex = 31;
            this.btnSymbolsNasdaqParse.Text = "Symbols Nasdaq Parse";
            this.btnSymbolsNasdaqParse.UseVisualStyleBackColor = true;
            this.btnSymbolsNasdaqParse.Click += new System.EventHandler(this.btnSymbolsNasdaqParse_Click);
            // 
            // btnSymbolsStockanalysisParse
            // 
            this.btnSymbolsStockanalysisParse.Location = new System.Drawing.Point(715, 73);
            this.btnSymbolsStockanalysisParse.Name = "btnSymbolsStockanalysisParse";
            this.btnSymbolsStockanalysisParse.Size = new System.Drawing.Size(194, 23);
            this.btnSymbolsStockanalysisParse.TabIndex = 30;
            this.btnSymbolsStockanalysisParse.Text = "Symbols Stockanalysis Parse";
            this.btnSymbolsStockanalysisParse.UseVisualStyleBackColor = true;
            this.btnSymbolsStockanalysisParse.Click += new System.EventHandler(this.btnSymbolsStockanalysisParse_Click);
            // 
            // btnSymbolsStockanalysisDownload
            // 
            this.btnSymbolsStockanalysisDownload.Location = new System.Drawing.Point(715, 44);
            this.btnSymbolsStockanalysisDownload.Name = "btnSymbolsStockanalysisDownload";
            this.btnSymbolsStockanalysisDownload.Size = new System.Drawing.Size(194, 23);
            this.btnSymbolsStockanalysisDownload.TabIndex = 29;
            this.btnSymbolsStockanalysisDownload.Text = "Symbols Stockanalysis Download";
            this.btnSymbolsStockanalysisDownload.UseVisualStyleBackColor = true;
            this.btnSymbolsStockanalysisDownload.Click += new System.EventHandler(this.btnSymbolsStockanalysisDownload_Click);
            // 
            // btnQuantumonlineProfilesParse
            // 
            this.btnQuantumonlineProfilesParse.Location = new System.Drawing.Point(952, 91);
            this.btnQuantumonlineProfilesParse.Name = "btnQuantumonlineProfilesParse";
            this.btnQuantumonlineProfilesParse.Size = new System.Drawing.Size(164, 23);
            this.btnQuantumonlineProfilesParse.TabIndex = 28;
            this.btnQuantumonlineProfilesParse.Text = "Quantumonline Profiles Parse";
            this.btnQuantumonlineProfilesParse.UseVisualStyleBackColor = true;
            this.btnQuantumonlineProfilesParse.Click += new System.EventHandler(this.btnQuantumonlineProfilesParse_Click);
            // 
            // btnSplitInvestingHistoryParse
            // 
            this.btnSplitInvestingHistoryParse.Location = new System.Drawing.Point(715, 15);
            this.btnSplitInvestingHistoryParse.Name = "btnSplitInvestingHistoryParse";
            this.btnSplitInvestingHistoryParse.Size = new System.Drawing.Size(146, 23);
            this.btnSplitInvestingHistoryParse.TabIndex = 27;
            this.btnSplitInvestingHistoryParse.Text = "Split InvestingHistory Parse";
            this.btnSplitInvestingHistoryParse.UseVisualStyleBackColor = true;
            this.btnSplitInvestingHistoryParse.Click += new System.EventHandler(this.btnSplitInvestingHistoryParse_Click);
            // 
            // btnStockSplitHistoryParse
            // 
            this.btnStockSplitHistoryParse.Location = new System.Drawing.Point(754, 226);
            this.btnStockSplitHistoryParse.Name = "btnStockSplitHistoryParse";
            this.btnStockSplitHistoryParse.Size = new System.Drawing.Size(146, 23);
            this.btnStockSplitHistoryParse.TabIndex = 26;
            this.btnStockSplitHistoryParse.Text = "StockSplitHistory Parse";
            this.btnStockSplitHistoryParse.UseVisualStyleBackColor = true;
            this.btnStockSplitHistoryParse.Click += new System.EventHandler(this.btnStockSplitHistoryParse_Click);
            // 
            // btnScreenerNasdaqParse
            // 
            this.btnScreenerNasdaqParse.Enabled = false;
            this.btnScreenerNasdaqParse.Location = new System.Drawing.Point(384, 212);
            this.btnScreenerNasdaqParse.Name = "btnScreenerNasdaqParse";
            this.btnScreenerNasdaqParse.Size = new System.Drawing.Size(173, 23);
            this.btnScreenerNasdaqParse.TabIndex = 25;
            this.btnScreenerNasdaqParse.Text = "Nasdaq Stock Screener Parse";
            this.btnScreenerNasdaqParse.UseVisualStyleBackColor = true;
            this.btnScreenerNasdaqParse.Click += new System.EventHandler(this.btnParseScreenerNasdaqParse_Click);
            // 
            // btnSplitEoddataParse
            // 
            this.btnSplitEoddataParse.Location = new System.Drawing.Point(754, 142);
            this.btnSplitEoddataParse.Name = "btnSplitEoddataParse";
            this.btnSplitEoddataParse.Size = new System.Drawing.Size(146, 23);
            this.btnSplitEoddataParse.TabIndex = 23;
            this.btnSplitEoddataParse.Text = "Split Eoddata Parse";
            this.btnSplitEoddataParse.UseVisualStyleBackColor = true;
            this.btnSplitEoddataParse.Click += new System.EventHandler(this.btnSplitEoddataParse_Click);
            // 
            // btnSplitInvestingParse
            // 
            this.btnSplitInvestingParse.Location = new System.Drawing.Point(754, 182);
            this.btnSplitInvestingParse.Name = "btnSplitInvestingParse";
            this.btnSplitInvestingParse.Size = new System.Drawing.Size(146, 23);
            this.btnSplitInvestingParse.TabIndex = 22;
            this.btnSplitInvestingParse.Text = "Split Investing.com Parse";
            this.btnSplitInvestingParse.UseVisualStyleBackColor = true;
            this.btnSplitInvestingParse.Click += new System.EventHandler(this.btnSplitInvestingParse_Click);
            // 
            // btnSplitYahooParse
            // 
            this.btnSplitYahooParse.Location = new System.Drawing.Point(754, 268);
            this.btnSplitYahooParse.Name = "btnSplitYahooParse";
            this.btnSplitYahooParse.Size = new System.Drawing.Size(146, 23);
            this.btnSplitYahooParse.TabIndex = 21;
            this.btnSplitYahooParse.Text = "Split Yahoo Parse (zip)";
            this.btnSplitYahooParse.UseVisualStyleBackColor = true;
            this.btnSplitYahooParse.Click += new System.EventHandler(this.btnSplitYahooParse_Click);
            // 
            // btnSymbolsEoddataParse
            // 
            this.btnSymbolsEoddataParse.Location = new System.Drawing.Point(384, 290);
            this.btnSymbolsEoddataParse.Name = "btnSymbolsEoddataParse";
            this.btnSymbolsEoddataParse.Size = new System.Drawing.Size(146, 23);
            this.btnSymbolsEoddataParse.TabIndex = 16;
            this.btnSymbolsEoddataParse.Text = "Symbols Eoddata Parse";
            this.btnSymbolsEoddataParse.UseVisualStyleBackColor = true;
            this.btnSymbolsEoddataParse.Click += new System.EventHandler(this.btnSymbolsEoddataParse_Click);
            // 
            // btnDayEoddataParse
            // 
            this.btnDayEoddataParse.Location = new System.Drawing.Point(214, 330);
            this.btnDayEoddataParse.Name = "btnDayEoddataParse";
            this.btnDayEoddataParse.Size = new System.Drawing.Size(146, 23);
            this.btnDayEoddataParse.TabIndex = 15;
            this.btnDayEoddataParse.Text = "Daily Eoddata Parse";
            this.btnDayEoddataParse.UseVisualStyleBackColor = true;
            this.btnDayEoddataParse.Click += new System.EventHandler(this.btnDayEoddataParse_Click);
            // 
            // btnNanexSymbols
            // 
            this.btnNanexSymbols.Location = new System.Drawing.Point(384, 15);
            this.btnNanexSymbols.Name = "btnNanexSymbols";
            this.btnNanexSymbols.Size = new System.Drawing.Size(146, 23);
            this.btnNanexSymbols.TabIndex = 14;
            this.btnNanexSymbols.Text = "?Nanex Symbols";
            this.btnNanexSymbols.UseVisualStyleBackColor = true;
            this.btnNanexSymbols.Click += new System.EventHandler(this.btnSymbolsNanex_Click);
            // 
            // btnDayYahooIndexesParse
            // 
            this.btnDayYahooIndexesParse.Location = new System.Drawing.Point(214, 252);
            this.btnDayYahooIndexesParse.Name = "btnDayYahooIndexesParse";
            this.btnDayYahooIndexesParse.Size = new System.Drawing.Size(146, 23);
            this.btnDayYahooIndexesParse.TabIndex = 13;
            this.btnDayYahooIndexesParse.Text = "DayYahoo Indexes Parse";
            this.btnDayYahooIndexesParse.UseVisualStyleBackColor = true;
            this.btnDayYahooIndexesParse.Click += new System.EventHandler(this.btnDayYahooIndexesParse_Click);
            // 
            // btnDayYahooParse
            // 
            this.btnDayYahooParse.Location = new System.Drawing.Point(214, 44);
            this.btnDayYahooParse.Name = "btnDayYahooParse";
            this.btnDayYahooParse.Size = new System.Drawing.Size(146, 23);
            this.btnDayYahooParse.TabIndex = 12;
            this.btnDayYahooParse.Text = "?DayYahoo Parse";
            this.btnDayYahooParse.UseVisualStyleBackColor = true;
            this.btnDayYahooParse.Click += new System.EventHandler(this.btnDayYahooParse_Click);
            // 
            // tabLayers
            // 
            this.tabLayers.Controls.Add(this.button1);
            this.tabLayers.Controls.Add(this.gbDataSet);
            this.tabLayers.Controls.Add(this.btnAlgorithm1);
            this.tabLayers.Location = new System.Drawing.Point(4, 22);
            this.tabLayers.Name = "tabLayers";
            this.tabLayers.Padding = new System.Windows.Forms.Padding(3);
            this.tabLayers.Size = new System.Drawing.Size(1158, 411);
            this.tabLayers.TabIndex = 1;
            this.tabLayers.Text = "Layers";
            this.tabLayers.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(426, 97);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // gbDataSet
            // 
            this.gbDataSet.Controls.Add(this.cb2013);
            this.gbDataSet.Controls.Add(this.cb2022);
            this.gbDataSet.Location = new System.Drawing.Point(23, 17);
            this.gbDataSet.Name = "gbDataSet";
            this.gbDataSet.Size = new System.Drawing.Size(76, 68);
            this.gbDataSet.TabIndex = 1;
            this.gbDataSet.TabStop = false;
            this.gbDataSet.Text = "Data Set";
            // 
            // cb2013
            // 
            this.cb2013.AutoSize = true;
            this.cb2013.Location = new System.Drawing.Point(6, 42);
            this.cb2013.Name = "cb2013";
            this.cb2013.Size = new System.Drawing.Size(50, 17);
            this.cb2013.TabIndex = 3;
            this.cb2013.Text = "2013";
            this.cb2013.UseVisualStyleBackColor = true;
            // 
            // cb2022
            // 
            this.cb2022.AutoSize = true;
            this.cb2022.Checked = true;
            this.cb2022.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb2022.Location = new System.Drawing.Point(6, 19);
            this.cb2022.Name = "cb2022";
            this.cb2022.Size = new System.Drawing.Size(50, 17);
            this.cb2022.TabIndex = 2;
            this.cb2022.Text = "2022";
            this.cb2022.UseVisualStyleBackColor = true;
            // 
            // btnAlgorithm1
            // 
            this.btnAlgorithm1.Location = new System.Drawing.Point(23, 97);
            this.btnAlgorithm1.Name = "btnAlgorithm1";
            this.btnAlgorithm1.Size = new System.Drawing.Size(75, 23);
            this.btnAlgorithm1.TabIndex = 0;
            this.btnAlgorithm1.Text = "Algorithm 1";
            this.btnAlgorithm1.UseVisualStyleBackColor = true;
            this.btnAlgorithm1.Click += new System.EventHandler(this.btnAlgorithm1_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.btnStockAnalysisIPO);
            this.tabPage4.Controls.Add(this.btnWebArchiveWikipediaIndicesParse);
            this.tabPage4.Controls.Add(this.btnRussellIndicesParse);
            this.tabPage4.Controls.Add(this.btnWikipediaIndicesParse);
            this.tabPage4.Controls.Add(this.btnWikipediaIndicesDownload);
            this.tabPage4.Controls.Add(this.btnTradingViewRecommendParse);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(1158, 411);
            this.tabPage4.TabIndex = 5;
            this.tabPage4.Text = "EOD";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // btnStockAnalysisIPO
            // 
            this.btnStockAnalysisIPO.Location = new System.Drawing.Point(566, 17);
            this.btnStockAnalysisIPO.Name = "btnStockAnalysisIPO";
            this.btnStockAnalysisIPO.Size = new System.Drawing.Size(135, 23);
            this.btnStockAnalysisIPO.TabIndex = 55;
            this.btnStockAnalysisIPO.Text = "StockAnalysis IPO";
            this.btnToolTip.SetToolTip(this.btnStockAnalysisIPO, "Download, parse and save to database");
            this.btnStockAnalysisIPO.UseVisualStyleBackColor = true;
            this.btnStockAnalysisIPO.Click += new System.EventHandler(this.btnStockAnalysisIPO_Click);
            // 
            // btnWebArchiveWikipediaIndicesParse
            // 
            this.btnWebArchiveWikipediaIndicesParse.Location = new System.Drawing.Point(217, 17);
            this.btnWebArchiveWikipediaIndicesParse.Name = "btnWebArchiveWikipediaIndicesParse";
            this.btnWebArchiveWikipediaIndicesParse.Size = new System.Drawing.Size(203, 23);
            this.btnWebArchiveWikipediaIndicesParse.TabIndex = 54;
            this.btnWebArchiveWikipediaIndicesParse.Text = "WebArchive Wikipedia Indices Parse";
            this.btnWebArchiveWikipediaIndicesParse.UseVisualStyleBackColor = true;
            this.btnWebArchiveWikipediaIndicesParse.Click += new System.EventHandler(this.btnWebArchiveWikipediaIndicesParse_Click);
            // 
            // btnRussellIndicesParse
            // 
            this.btnRussellIndicesParse.Location = new System.Drawing.Point(415, 87);
            this.btnRussellIndicesParse.Name = "btnRussellIndicesParse";
            this.btnRussellIndicesParse.Size = new System.Drawing.Size(135, 23);
            this.btnRussellIndicesParse.TabIndex = 53;
            this.btnRussellIndicesParse.Text = "Russell Indices Parse";
            this.btnRussellIndicesParse.UseVisualStyleBackColor = true;
            this.btnRussellIndicesParse.Click += new System.EventHandler(this.btnRussellIndicesParse_Click);
            // 
            // btnWikipediaIndicesParse
            // 
            this.btnWikipediaIndicesParse.Location = new System.Drawing.Point(217, 87);
            this.btnWikipediaIndicesParse.Name = "btnWikipediaIndicesParse";
            this.btnWikipediaIndicesParse.Size = new System.Drawing.Size(178, 23);
            this.btnWikipediaIndicesParse.TabIndex = 52;
            this.btnWikipediaIndicesParse.Text = "Wikipedia Indices Parse";
            this.btnWikipediaIndicesParse.UseVisualStyleBackColor = true;
            this.btnWikipediaIndicesParse.Click += new System.EventHandler(this.btnWikipediaIndicesParse_Click);
            // 
            // btnWikipediaIndicesDownload
            // 
            this.btnWikipediaIndicesDownload.Location = new System.Drawing.Point(217, 58);
            this.btnWikipediaIndicesDownload.Name = "btnWikipediaIndicesDownload";
            this.btnWikipediaIndicesDownload.Size = new System.Drawing.Size(178, 23);
            this.btnWikipediaIndicesDownload.TabIndex = 51;
            this.btnWikipediaIndicesDownload.Text = "Wikipedia Indices Download";
            this.btnWikipediaIndicesDownload.UseVisualStyleBackColor = true;
            this.btnWikipediaIndicesDownload.Click += new System.EventHandler(this.btnWikipediaIndicesDownload_Click);
            // 
            // btnTradingViewRecommendParse
            // 
            this.btnTradingViewRecommendParse.Location = new System.Drawing.Point(19, 17);
            this.btnTradingViewRecommendParse.Name = "btnTradingViewRecommendParse";
            this.btnTradingViewRecommendParse.Size = new System.Drawing.Size(178, 23);
            this.btnTradingViewRecommendParse.TabIndex = 50;
            this.btnTradingViewRecommendParse.Text = "TradingView Recommend Parse";
            this.btnTradingViewRecommendParse.UseVisualStyleBackColor = true;
            this.btnTradingViewRecommendParse.Click += new System.EventHandler(this.btnTradingViewRecommendParse_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnIntradayYahooQuotesSaveToDB);
            this.tabPage1.Controls.Add(this.label7);
            this.tabPage1.Controls.Add(this.nudIntradayFees);
            this.tabPage1.Controls.Add(this.cbIntradayStopInPercent);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.nudIntradayStop);
            this.tabPage1.Controls.Add(this.btnIntradayStatisticsSaveToDB);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.btnIntradayByTimeReportsClosedInNextFrame);
            this.tabPage1.Controls.Add(this.btnIntradayByTimeReports);
            this.tabPage1.Controls.Add(this.btnIntradayPrintDetails);
            this.tabPage1.Controls.Add(this.gbIntradayDataList);
            this.tabPage1.Controls.Add(this.btnIntradayGenerateReport);
            this.tabPage1.Controls.Add(this.btnPrepareYahooMinuteTextCache);
            this.tabPage1.Controls.Add(this.btnCheckYahooMinuteData);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1158, 411);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "Intraday";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnIntradayYahooQuotesSaveToDB
            // 
            this.btnIntradayYahooQuotesSaveToDB.Location = new System.Drawing.Point(740, 22);
            this.btnIntradayYahooQuotesSaveToDB.Name = "btnIntradayYahooQuotesSaveToDB";
            this.btnIntradayYahooQuotesSaveToDB.Size = new System.Drawing.Size(184, 41);
            this.btnIntradayYahooQuotesSaveToDB.TabIndex = 39;
            this.btnIntradayYahooQuotesSaveToDB.Text = "Save Yahoo Intraday Quotes to DB (09:45-15:45)";
            this.btnIntradayYahooQuotesSaveToDB.UseVisualStyleBackColor = true;
            this.btnIntradayYahooQuotesSaveToDB.Click += new System.EventHandler(this.btnIntradayYahooQuotesSaveToDB_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label7.Location = new System.Drawing.Point(276, 234);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(145, 15);
            this.label7.TabIndex = 38;
            this.label7.Text = "Broker fees ($ per share):";
            // 
            // nudIntradayFees
            // 
            this.nudIntradayFees.DecimalPlaces = 3;
            this.nudIntradayFees.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudIntradayFees.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudIntradayFees.Location = new System.Drawing.Point(356, 257);
            this.nudIntradayFees.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudIntradayFees.Name = "nudIntradayFees";
            this.nudIntradayFees.Size = new System.Drawing.Size(65, 23);
            this.nudIntradayFees.TabIndex = 37;
            this.nudIntradayFees.Value = new decimal(new int[] {
            2,
            0,
            0,
            196608});
            // 
            // cbIntradayStopInPercent
            // 
            this.cbIntradayStopInPercent.AutoSize = true;
            this.cbIntradayStopInPercent.Checked = true;
            this.cbIntradayStopInPercent.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIntradayStopInPercent.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbIntradayStopInPercent.Location = new System.Drawing.Point(279, 168);
            this.cbIntradayStopInPercent.Name = "cbIntradayStopInPercent";
            this.cbIntradayStopInPercent.Size = new System.Drawing.Size(177, 19);
            this.cbIntradayStopInPercent.TabIndex = 36;
            this.cbIntradayStopInPercent.Text = "Is the stop value in percent?";
            this.cbIntradayStopInPercent.UseVisualStyleBackColor = true;
            this.cbIntradayStopInPercent.CheckedChanged += new System.EventHandler(this.cbIntradayStopInPercent_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.Location = new System.Drawing.Point(276, 195);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 15);
            this.label6.TabIndex = 35;
            this.label6.Text = "Stop value:";
            // 
            // nudIntradayStop
            // 
            this.nudIntradayStop.DecimalPlaces = 2;
            this.nudIntradayStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudIntradayStop.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudIntradayStop.Location = new System.Drawing.Point(356, 193);
            this.nudIntradayStop.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudIntradayStop.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudIntradayStop.Name = "nudIntradayStop";
            this.nudIntradayStop.Size = new System.Drawing.Size(65, 23);
            this.nudIntradayStop.TabIndex = 34;
            this.nudIntradayStop.Value = new decimal(new int[] {
            3,
            0,
            0,
            131072});
            // 
            // btnIntradayStatisticsSaveToDB
            // 
            this.btnIntradayStatisticsSaveToDB.Location = new System.Drawing.Point(494, 101);
            this.btnIntradayStatisticsSaveToDB.Name = "btnIntradayStatisticsSaveToDB";
            this.btnIntradayStatisticsSaveToDB.Size = new System.Drawing.Size(220, 23);
            this.btnIntradayStatisticsSaveToDB.TabIndex = 33;
            this.btnIntradayStatisticsSaveToDB.Text = "Save Statistics of Intraday Quotes to DB";
            this.btnIntradayStatisticsSaveToDB.UseVisualStyleBackColor = true;
            this.btnIntradayStatisticsSaveToDB.Click += new System.EventHandler(this.btnIntradayStaisticsSaveToDB_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.nudInterval);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.cbCloseInNextFrame);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.nudToMinute);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.nudToHour);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.nudFromMinute);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.nudFromHour);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox2.Location = new System.Drawing.Point(268, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(159, 142);
            this.groupBox2.TabIndex = 32;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Time Frames";
            // 
            // nudInterval
            // 
            this.nudInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudInterval.Increment = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudInterval.Location = new System.Drawing.Point(110, 78);
            this.nudInterval.Maximum = new decimal(new int[] {
            390,
            0,
            0,
            0});
            this.nudInterval.Name = "nudInterval";
            this.nudInterval.Size = new System.Drawing.Size(38, 23);
            this.nudInterval.TabIndex = 10;
            this.nudInterval.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(8, 80);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 15);
            this.label5.TabIndex = 9;
            this.label5.Text = "Interval (minutes)";
            // 
            // cbCloseInNextFrame
            // 
            this.cbCloseInNextFrame.AutoSize = true;
            this.cbCloseInNextFrame.Checked = true;
            this.cbCloseInNextFrame.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCloseInNextFrame.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbCloseInNextFrame.Location = new System.Drawing.Point(11, 108);
            this.cbCloseInNextFrame.Name = "cbCloseInNextFrame";
            this.cbCloseInNextFrame.Size = new System.Drawing.Size(131, 19);
            this.cbCloseInNextFrame.TabIndex = 8;
            this.cbCloseInNextFrame.Text = "Close in next frame";
            this.cbCloseInNextFrame.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(96, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(12, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = ":";
            // 
            // nudToMinute
            // 
            this.nudToMinute.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudToMinute.Increment = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudToMinute.Location = new System.Drawing.Point(110, 49);
            this.nudToMinute.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nudToMinute.Name = "nudToMinute";
            this.nudToMinute.Size = new System.Drawing.Size(38, 23);
            this.nudToMinute.TabIndex = 6;
            this.nudToMinute.Value = new decimal(new int[] {
            45,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(23, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "To";
            // 
            // nudToHour
            // 
            this.nudToHour.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudToHour.Location = new System.Drawing.Point(54, 49);
            this.nudToHour.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudToHour.Minimum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.nudToHour.Name = "nudToHour";
            this.nudToHour.Size = new System.Drawing.Size(38, 23);
            this.nudToHour.TabIndex = 4;
            this.nudToHour.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(96, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(12, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = ":";
            // 
            // nudFromMinute
            // 
            this.nudFromMinute.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudFromMinute.Increment = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudFromMinute.Location = new System.Drawing.Point(110, 20);
            this.nudFromMinute.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nudFromMinute.Name = "nudFromMinute";
            this.nudFromMinute.Size = new System.Drawing.Size(38, 23);
            this.nudFromMinute.TabIndex = 2;
            this.nudFromMinute.Value = new decimal(new int[] {
            45,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(8, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "From";
            // 
            // nudFromHour
            // 
            this.nudFromHour.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudFromHour.Location = new System.Drawing.Point(54, 20);
            this.nudFromHour.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudFromHour.Minimum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.nudFromHour.Name = "nudFromHour";
            this.nudFromHour.Size = new System.Drawing.Size(38, 23);
            this.nudFromHour.TabIndex = 0;
            this.nudFromHour.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
            // 
            // btnIntradayByTimeReportsClosedInNextFrame
            // 
            this.btnIntradayByTimeReportsClosedInNextFrame.Location = new System.Drawing.Point(494, 328);
            this.btnIntradayByTimeReportsClosedInNextFrame.Name = "btnIntradayByTimeReportsClosedInNextFrame";
            this.btnIntradayByTimeReportsClosedInNextFrame.Size = new System.Drawing.Size(138, 42);
            this.btnIntradayByTimeReportsClosedInNextFrame.TabIndex = 31;
            this.btnIntradayByTimeReportsClosedInNextFrame.Text = "Generate ByTime reports (closedInNextFrame)";
            this.btnIntradayByTimeReportsClosedInNextFrame.UseVisualStyleBackColor = true;
            this.btnIntradayByTimeReportsClosedInNextFrame.Click += new System.EventHandler(this.btnIntradayByTimeReportsClosedInNextFrame_Click);
            // 
            // btnIntradayByTimeReports
            // 
            this.btnIntradayByTimeReports.Location = new System.Drawing.Point(494, 299);
            this.btnIntradayByTimeReports.Name = "btnIntradayByTimeReports";
            this.btnIntradayByTimeReports.Size = new System.Drawing.Size(138, 23);
            this.btnIntradayByTimeReports.TabIndex = 30;
            this.btnIntradayByTimeReports.Text = "Generate ByTime reports";
            this.btnIntradayByTimeReports.UseVisualStyleBackColor = true;
            this.btnIntradayByTimeReports.Click += new System.EventHandler(this.btnIntradayByTimeReports_Click);
            // 
            // btnIntradayPrintDetails
            // 
            this.btnIntradayPrintDetails.Location = new System.Drawing.Point(556, 256);
            this.btnIntradayPrintDetails.Name = "btnIntradayPrintDetails";
            this.btnIntradayPrintDetails.Size = new System.Drawing.Size(75, 23);
            this.btnIntradayPrintDetails.TabIndex = 29;
            this.btnIntradayPrintDetails.Text = "Print Details";
            this.btnIntradayPrintDetails.UseVisualStyleBackColor = true;
            this.btnIntradayPrintDetails.Click += new System.EventHandler(this.btnIntradayPrintDetails_Click);
            // 
            // gbIntradayDataList
            // 
            this.gbIntradayDataList.Controls.Add(this.clbIntradayDataList);
            this.gbIntradayDataList.Location = new System.Drawing.Point(8, 6);
            this.gbIntradayDataList.Name = "gbIntradayDataList";
            this.gbIntradayDataList.Size = new System.Drawing.Size(254, 370);
            this.gbIntradayDataList.TabIndex = 28;
            this.gbIntradayDataList.TabStop = false;
            this.gbIntradayDataList.Text = "Data List";
            // 
            // clbIntradayDataList
            // 
            this.clbIntradayDataList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.clbIntradayDataList.CheckOnClick = true;
            this.clbIntradayDataList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clbIntradayDataList.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.clbIntradayDataList.FormattingEnabled = true;
            this.clbIntradayDataList.Location = new System.Drawing.Point(3, 16);
            this.clbIntradayDataList.Name = "clbIntradayDataList";
            this.clbIntradayDataList.Size = new System.Drawing.Size(248, 351);
            this.clbIntradayDataList.TabIndex = 27;
            // 
            // btnIntradayGenerateReport
            // 
            this.btnIntradayGenerateReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnIntradayGenerateReport.Location = new System.Drawing.Point(494, 209);
            this.btnIntradayGenerateReport.Name = "btnIntradayGenerateReport";
            this.btnIntradayGenerateReport.Size = new System.Drawing.Size(109, 23);
            this.btnIntradayGenerateReport.TabIndex = 27;
            this.btnIntradayGenerateReport.Text = "Generate report";
            this.btnIntradayGenerateReport.UseVisualStyleBackColor = true;
            this.btnIntradayGenerateReport.Click += new System.EventHandler(this.btnIntradayGenerateReport_Click);
            // 
            // btnPrepareYahooMinuteTextCache
            // 
            this.btnPrepareYahooMinuteTextCache.Location = new System.Drawing.Point(494, 60);
            this.btnPrepareYahooMinuteTextCache.Name = "btnPrepareYahooMinuteTextCache";
            this.btnPrepareYahooMinuteTextCache.Size = new System.Drawing.Size(184, 23);
            this.btnPrepareYahooMinuteTextCache.TabIndex = 18;
            this.btnPrepareYahooMinuteTextCache.Text = "Prepare Yahoo Minute Text Cache";
            this.btnPrepareYahooMinuteTextCache.UseVisualStyleBackColor = true;
            this.btnPrepareYahooMinuteTextCache.Click += new System.EventHandler(this.btnPrepareYahooMinuteZipCache_Click);
            // 
            // btnCheckYahooMinuteData
            // 
            this.btnCheckYahooMinuteData.Location = new System.Drawing.Point(494, 22);
            this.btnCheckYahooMinuteData.Name = "btnCheckYahooMinuteData";
            this.btnCheckYahooMinuteData.Size = new System.Drawing.Size(184, 23);
            this.btnCheckYahooMinuteData.TabIndex = 17;
            this.btnCheckYahooMinuteData.Text = "Check Yahoo Minute Data";
            this.btnCheckYahooMinuteData.UseVisualStyleBackColor = true;
            this.btnCheckYahooMinuteData.Click += new System.EventHandler(this.btnCheckYahooMinuteData_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnMinuteYahooSaveLogToDb);
            this.tabPage2.Controls.Add(this.btnMinuteYahooErrorCheck);
            this.tabPage2.Controls.Add(this.btnDailyEoddataCheck);
            this.tabPage2.Controls.Add(this.btnCompareMinuteYahooZips);
            this.tabPage2.Controls.Add(this.btnMinuteYahooLog);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1158, 411);
            this.tabPage2.TabIndex = 3;
            this.tabPage2.Text = "Checks";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnMinuteYahooSaveLogToDb
            // 
            this.btnMinuteYahooSaveLogToDb.Location = new System.Drawing.Point(24, 103);
            this.btnMinuteYahooSaveLogToDb.Name = "btnMinuteYahooSaveLogToDb";
            this.btnMinuteYahooSaveLogToDb.Size = new System.Drawing.Size(146, 23);
            this.btnMinuteYahooSaveLogToDb.TabIndex = 54;
            this.btnMinuteYahooSaveLogToDb.Text = "Minute Yahoo Save Log to DB";
            this.btnMinuteYahooSaveLogToDb.UseVisualStyleBackColor = true;
            this.btnMinuteYahooSaveLogToDb.Click += new System.EventHandler(this.btnMinuteYahooSaveLogToDb_Click);
            // 
            // btnMinuteYahooErrorCheck
            // 
            this.btnMinuteYahooErrorCheck.Location = new System.Drawing.Point(24, 58);
            this.btnMinuteYahooErrorCheck.Name = "btnMinuteYahooErrorCheck";
            this.btnMinuteYahooErrorCheck.Size = new System.Drawing.Size(146, 23);
            this.btnMinuteYahooErrorCheck.TabIndex = 53;
            this.btnMinuteYahooErrorCheck.Text = "Minute Yahoo Error Check";
            this.btnMinuteYahooErrorCheck.UseVisualStyleBackColor = true;
            this.btnMinuteYahooErrorCheck.Click += new System.EventHandler(this.btnMinuteYahooErrorCheck_Click);
            // 
            // btnDailyEoddataCheck
            // 
            this.btnDailyEoddataCheck.Enabled = false;
            this.btnDailyEoddataCheck.Location = new System.Drawing.Point(203, 20);
            this.btnDailyEoddataCheck.Name = "btnDailyEoddataCheck";
            this.btnDailyEoddataCheck.Size = new System.Drawing.Size(146, 23);
            this.btnDailyEoddataCheck.TabIndex = 52;
            this.btnDailyEoddataCheck.Text = "Daily Eoddata Check";
            this.btnDailyEoddataCheck.UseVisualStyleBackColor = true;
            this.btnDailyEoddataCheck.Click += new System.EventHandler(this.btnDailyEoddataCheck_Click);
            // 
            // btnCompareMinuteYahooZips
            // 
            this.btnCompareMinuteYahooZips.Location = new System.Drawing.Point(24, 148);
            this.btnCompareMinuteYahooZips.Name = "btnCompareMinuteYahooZips";
            this.btnCompareMinuteYahooZips.Size = new System.Drawing.Size(146, 38);
            this.btnCompareMinuteYahooZips.TabIndex = 51;
            this.btnCompareMinuteYahooZips.Text = "Compare 2 zip of Minute Yahoo";
            this.btnCompareMinuteYahooZips.UseVisualStyleBackColor = true;
            this.btnCompareMinuteYahooZips.Click += new System.EventHandler(this.btnCompareMinuteYahooZips_Click);
            // 
            // btnMinuteYahooLog
            // 
            this.btnMinuteYahooLog.Location = new System.Drawing.Point(24, 20);
            this.btnMinuteYahooLog.Name = "btnMinuteYahooLog";
            this.btnMinuteYahooLog.Size = new System.Drawing.Size(146, 23);
            this.btnMinuteYahooLog.TabIndex = 50;
            this.btnMinuteYahooLog.Text = "Minute Yahoo Log (for zip)";
            this.btnMinuteYahooLog.UseVisualStyleBackColor = true;
            this.btnMinuteYahooLog.Click += new System.EventHandler(this.btnMinuteYahooLog_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnWebArchiveParseStockAnalysisActions);
            this.tabPage3.Controls.Add(this.btnWebArchiveParseTradingViewProfiles);
            this.tabPage3.Controls.Add(this.btnWebArchiveDownloadTradingViewProfiles);
            this.tabPage3.Controls.Add(this.btnWebArchiveDownloadJsonTradingViewScreener);
            this.tabPage3.Controls.Add(this.btnWebArchiveParseTradingViewScreener);
            this.tabPage3.Controls.Add(this.btnWebArchiveDownloadHtmlTradingViewScreener);
            this.tabPage3.Controls.Add(this.btnWA_ParseEoddataSymbols);
            this.tabPage3.Controls.Add(this.btnWA_DownloadEoddataSymbols);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1158, 411);
            this.tabPage3.TabIndex = 4;
            this.tabPage3.Text = "Web Archive";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnWebArchiveParseStockAnalysisActions
            // 
            this.btnWebArchiveParseStockAnalysisActions.Location = new System.Drawing.Point(667, 16);
            this.btnWebArchiveParseStockAnalysisActions.Name = "btnWebArchiveParseStockAnalysisActions";
            this.btnWebArchiveParseStockAnalysisActions.Size = new System.Drawing.Size(173, 23);
            this.btnWebArchiveParseStockAnalysisActions.TabIndex = 7;
            this.btnWebArchiveParseStockAnalysisActions.Text = "Parse StockAnalysis Actions";
            this.btnWebArchiveParseStockAnalysisActions.UseVisualStyleBackColor = true;
            this.btnWebArchiveParseStockAnalysisActions.Click += new System.EventHandler(this.btnWebArchiveParseStockAnalysisActions_Click);
            // 
            // btnWebArchiveParseTradingViewProfiles
            // 
            this.btnWebArchiveParseTradingViewProfiles.Location = new System.Drawing.Point(469, 45);
            this.btnWebArchiveParseTradingViewProfiles.Name = "btnWebArchiveParseTradingViewProfiles";
            this.btnWebArchiveParseTradingViewProfiles.Size = new System.Drawing.Size(173, 23);
            this.btnWebArchiveParseTradingViewProfiles.TabIndex = 6;
            this.btnWebArchiveParseTradingViewProfiles.Text = "Parse TradingView Profiles";
            this.btnWebArchiveParseTradingViewProfiles.UseVisualStyleBackColor = true;
            this.btnWebArchiveParseTradingViewProfiles.Click += new System.EventHandler(this.btnWebArchiveParseTradingViewProfiles_Click);
            // 
            // btnWebArchiveDownloadTradingViewProfiles
            // 
            this.btnWebArchiveDownloadTradingViewProfiles.Location = new System.Drawing.Point(469, 16);
            this.btnWebArchiveDownloadTradingViewProfiles.Name = "btnWebArchiveDownloadTradingViewProfiles";
            this.btnWebArchiveDownloadTradingViewProfiles.Size = new System.Drawing.Size(173, 23);
            this.btnWebArchiveDownloadTradingViewProfiles.TabIndex = 5;
            this.btnWebArchiveDownloadTradingViewProfiles.Text = "Download TradingView Profiles";
            this.btnWebArchiveDownloadTradingViewProfiles.UseVisualStyleBackColor = true;
            this.btnWebArchiveDownloadTradingViewProfiles.Click += new System.EventHandler(this.btnWebArchiveDownloadTradingViewProfiles_Click);
            // 
            // btnWebArchiveDownloadJsonTradingViewScreener
            // 
            this.btnWebArchiveDownloadJsonTradingViewScreener.Location = new System.Drawing.Point(231, 45);
            this.btnWebArchiveDownloadJsonTradingViewScreener.Name = "btnWebArchiveDownloadJsonTradingViewScreener";
            this.btnWebArchiveDownloadJsonTradingViewScreener.Size = new System.Drawing.Size(215, 23);
            this.btnWebArchiveDownloadJsonTradingViewScreener.TabIndex = 4;
            this.btnWebArchiveDownloadJsonTradingViewScreener.Text = "Download TradingView Screener (json)";
            this.btnWebArchiveDownloadJsonTradingViewScreener.UseVisualStyleBackColor = true;
            this.btnWebArchiveDownloadJsonTradingViewScreener.Click += new System.EventHandler(this.btnWebArchiveDownloadJsonTradingViewScreener_Click);
            // 
            // btnWebArchiveParseTradingViewScreener
            // 
            this.btnWebArchiveParseTradingViewScreener.Location = new System.Drawing.Point(231, 74);
            this.btnWebArchiveParseTradingViewScreener.Name = "btnWebArchiveParseTradingViewScreener";
            this.btnWebArchiveParseTradingViewScreener.Size = new System.Drawing.Size(175, 23);
            this.btnWebArchiveParseTradingViewScreener.TabIndex = 3;
            this.btnWebArchiveParseTradingViewScreener.Text = "Parse TradingView Screener";
            this.btnWebArchiveParseTradingViewScreener.UseVisualStyleBackColor = true;
            this.btnWebArchiveParseTradingViewScreener.Click += new System.EventHandler(this.btnWebArchiveParseTradingViewScreener_Click);
            // 
            // btnWebArchiveDownloadHtmlTradingViewScreener
            // 
            this.btnWebArchiveDownloadHtmlTradingViewScreener.Location = new System.Drawing.Point(231, 16);
            this.btnWebArchiveDownloadHtmlTradingViewScreener.Name = "btnWebArchiveDownloadHtmlTradingViewScreener";
            this.btnWebArchiveDownloadHtmlTradingViewScreener.Size = new System.Drawing.Size(215, 23);
            this.btnWebArchiveDownloadHtmlTradingViewScreener.TabIndex = 2;
            this.btnWebArchiveDownloadHtmlTradingViewScreener.Text = "Download TradingView Screener (html)";
            this.btnWebArchiveDownloadHtmlTradingViewScreener.UseVisualStyleBackColor = true;
            this.btnWebArchiveDownloadHtmlTradingViewScreener.Click += new System.EventHandler(this.btnWebArchiveDownloadHtmlTradingViewScreener_Click);
            // 
            // btnWA_ParseEoddataSymbols
            // 
            this.btnWA_ParseEoddataSymbols.Location = new System.Drawing.Point(30, 45);
            this.btnWA_ParseEoddataSymbols.Name = "btnWA_ParseEoddataSymbols";
            this.btnWA_ParseEoddataSymbols.Size = new System.Drawing.Size(175, 23);
            this.btnWA_ParseEoddataSymbols.TabIndex = 1;
            this.btnWA_ParseEoddataSymbols.Text = "Parse Eoddata Symbols";
            this.btnWA_ParseEoddataSymbols.UseVisualStyleBackColor = true;
            this.btnWA_ParseEoddataSymbols.Click += new System.EventHandler(this.btnWA_ParseEoddataSymbols_Click);
            // 
            // btnWA_DownloadEoddataSymbols
            // 
            this.btnWA_DownloadEoddataSymbols.Location = new System.Drawing.Point(30, 16);
            this.btnWA_DownloadEoddataSymbols.Name = "btnWA_DownloadEoddataSymbols";
            this.btnWA_DownloadEoddataSymbols.Size = new System.Drawing.Size(175, 23);
            this.btnWA_DownloadEoddataSymbols.TabIndex = 0;
            this.btnWA_DownloadEoddataSymbols.Text = "Download Eoddata Symbols";
            this.btnWA_DownloadEoddataSymbols.UseVisualStyleBackColor = true;
            this.btnWA_DownloadEoddataSymbols.Click += new System.EventHandler(this.btnWA_DownloadEoddataSymbols_Click);
            // 
            // btnTemp
            // 
            this.btnTemp.Location = new System.Drawing.Point(988, 323);
            this.btnTemp.Name = "btnTemp";
            this.btnTemp.Size = new System.Drawing.Size(81, 23);
            this.btnTemp.TabIndex = 18;
            this.btnTemp.Text = "Temp";
            this.btnTemp.UseVisualStyleBackColor = true;
            this.btnTemp.Click += new System.EventHandler(this.btnTemp_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1166, 459);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tpLoaderNew.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.loaderItemBindingSource)).EndInit();
            this.tabLoader.ResumeLayout(false);
            this.tabLayers.ResumeLayout(false);
            this.gbDataSet.ResumeLayout(false);
            this.gbDataSet.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntradayFees)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntradayStop)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudToMinute)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudToHour)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFromMinute)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFromHour)).EndInit();
            this.gbIntradayDataList.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabLoader;
        private System.Windows.Forms.TabPage tabLayers;
        private System.Windows.Forms.Button btnSplitInvestingParse;
        private System.Windows.Forms.Button btnSplitYahooParse;
        private System.Windows.Forms.Button btnSymbolsEoddataParse;
        private System.Windows.Forms.Button btnDayEoddataParse;
        private System.Windows.Forms.Button btnNanexSymbols;
        private System.Windows.Forms.Button btnDayYahooIndexesParse;
        private System.Windows.Forms.Button btnDayYahooParse;
        private System.Windows.Forms.Button btnSplitEoddataParse;
        private System.Windows.Forms.Button btnAlgorithm1;
        private System.Windows.Forms.GroupBox gbDataSet;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox cb2013;
        private System.Windows.Forms.CheckBox cb2022;
        private System.Windows.Forms.Button btnScreenerNasdaqParse;
        private System.Windows.Forms.Button btnStockSplitHistoryParse;
        private System.Windows.Forms.Button btnSplitInvestingHistoryParse;
        private System.Windows.Forms.ToolTip btnToolTip;
        private System.Windows.Forms.Button btnQuantumonlineProfilesParse;
        private System.Windows.Forms.Button btnSymbolsStockanalysisDownload;
        private System.Windows.Forms.Button btnSymbolsStockanalysisParse;
        private System.Windows.Forms.Button btnSymbolsNasdaqParse;
        private System.Windows.Forms.Button btnRefreshSymbolsData;
        private System.Windows.Forms.Button btnTimeSalesNasdaqDownload;
        private System.Windows.Forms.Button btnSymbolsQuantumonlineDownload;
        private System.Windows.Forms.Button btnSymbolsQuantumonlineParse;
        private System.Windows.Forms.Button btnTimeSalesNasdaqSaveLog;
        private System.Windows.Forms.Button btnRefreshSpitsData;
        private System.Windows.Forms.Button btnTimeSalesNasdaqSaveSummary;
        private System.Windows.Forms.Button btnUpdateTradingDays;
        private System.Windows.Forms.Button btnSymbolsYahooLookupDownload;
        private System.Windows.Forms.Button btnSymbolsYahooLookupParse;
        private System.Windows.Forms.Button btnDayYahooDownload;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btnCheckYahooMinuteData;
        private System.Windows.Forms.Button btnPrepareYahooMinuteTextCache;
        private System.Windows.Forms.Button btnIntradayGenerateReport;
        private System.Windows.Forms.GroupBox gbIntradayDataList;
        private System.Windows.Forms.CheckedListBox clbIntradayDataList;
        private System.Windows.Forms.Button btnIntradayPrintDetails;
        private System.Windows.Forms.Button btnIntradayByTimeReports;
        private System.Windows.Forms.Button btnIntradayByTimeReportsClosedInNextFrame;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown nudFromMinute;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudFromHour;
        private System.Windows.Forms.CheckBox cbCloseInNextFrame;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudToMinute;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nudToHour;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudInterval;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnIntradayStatisticsSaveToDB;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nudIntradayStop;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown nudIntradayFees;
        private System.Windows.Forms.CheckBox cbIntradayStopInPercent;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnCompareMinuteYahooZips;
        private System.Windows.Forms.Button btnMinuteYahooLog;
        private System.Windows.Forms.Button btnDailyEoddataCheck;
        private System.Windows.Forms.Button btnMinuteYahooErrorCheck;
        private System.Windows.Forms.Button btnIntradayYahooQuotesSaveToDB;
        private System.Windows.Forms.Button btnMinuteAlphaVantageDownload;
        private System.Windows.Forms.Button btnMinuteAlphaVantageSaveLogToDb;
        private System.Windows.Forms.Button btnMinuteYahooSaveLogToDb;
        private System.Windows.Forms.Button btnMinuteAlphaVantageDownloadStop;
        private System.Windows.Forms.Button btnIntradayAlphaVantageRefreshProxyList;
        private System.Windows.Forms.Button btnMinuteAlphaVantageSplitData;
        private System.Windows.Forms.Button btnDayAlphaVantageDownload;
        private System.Windows.Forms.Button btnDayAlphaVantageParse;
        private System.Windows.Forms.Button btnProfileYahooParse;
        private System.Windows.Forms.Button btnScreenerNasdaqDownload;
        private System.Windows.Forms.Button btnNasdaqScreenerParse;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnWA_DownloadEoddataSymbols;
        private System.Windows.Forms.Button btnWA_ParseEoddataSymbols;
        private System.Windows.Forms.Button btnWebArchiveDownloadHtmlTradingViewScreener;
        private System.Windows.Forms.Button btnWebArchiveParseTradingViewScreener;
        private System.Windows.Forms.Button btnWebArchiveDownloadJsonTradingViewScreener;
        private System.Windows.Forms.Button btnWebArchiveDownloadTradingViewProfiles;
        private System.Windows.Forms.Button btnWebArchiveParseTradingViewProfiles;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button btnTradingViewRecommendParse;
        private System.Windows.Forms.Button btnWikipediaIndicesDownload;
        private System.Windows.Forms.Button btnWikipediaIndicesParse;
        private System.Windows.Forms.Button btnRussellIndicesParse;
        private System.Windows.Forms.Button btnWebArchiveWikipediaIndicesParse;
        private System.Windows.Forms.Button btnStockAnalysisIPO;
        private System.Windows.Forms.Button btnWebArchiveParseStockAnalysisActions;
        private System.Windows.Forms.TabPage tpLoaderNew;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn checkedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource loaderItemBindingSource;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private System.Windows.Forms.DataGridViewImageColumn Image;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Started;
        private System.Windows.Forms.DataGridViewTextBoxColumn Duration;
        private System.Windows.Forms.Button btnRunMultiItemsLoader;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.Button btnTemp;
    }
}

