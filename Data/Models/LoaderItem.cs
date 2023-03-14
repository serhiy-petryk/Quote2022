using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.Models
{
    public class LoaderItem: NotifyPropertyChangedAbstract
    {
        public enum ItemStatus { Disabled, None, Working, Done, Error }

        public static BindingList<LoaderItem> DataGridLoaderItems = new BindingList<LoaderItem>
        {
            new LoaderItem {Name = "TradingView Screener", Action = Actions.TradingView.TvScreenerLoader.Start, Checked = false},
            new LoaderItem {Name = "Nasdaq Stock/Etf Screeners", Action = Actions.Nasdaq.NasdaqScreenerLoader.Start, Checked = false},
            new LoaderItem {Name = "Eoddata Symbols", Action = Actions.Eoddata.EoddataSymbolsLoader.Start, Checked = false},
            new LoaderItem {Name = "Yahoo Profiles", Action=Actions.Yahoo.YahooProfileLoader.Start},
            new LoaderItem {Name = "Yahoo Indices & Update Trading days", Action = Actions.Yahoo.YahooIndicesLoader.Start, Checked = false},
            new LoaderItem {Name = "Eoddata Daily Quotes", Action = Actions.Eoddata.EoddataDailyLoader.Start},
        };

        public static Image GetAnimatedImage() => GetImage(ItemStatus.Working);

        private static Dictionary<string, Bitmap> _imgResources;
        private static string[] _itemStatusImageName = new[] {"Blank", "Blank", "Wait", "Done", "Error"};

        private static void TestLogEvent(Action<string> logEvent)
        {
            logEvent("Started");
            Thread.Sleep(1200);
            logEvent("Finished");
        }

        private static Bitmap GetImage(ItemStatus status)
        {
            if (_imgResources == null)
            {
                _imgResources=new Dictionary<string, Bitmap>();
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var rm = new System.Resources.ResourceManager("Data.Images", asm);
                foreach (var o in rm.GetResourceSet(CultureInfo.InvariantCulture, true, false))
                {
                    if (o is DictionaryEntry de && de.Key is string key && de.Value is Bitmap value)
                        _imgResources.Add(key, value);
                }
            }

            return _imgResources[_itemStatusImageName[(int)status]];
        }

        private bool _checked = true;
        public bool Checked
        {
            get => _checked;
            set
            {
                _checked = value;
                OnPropertiesChanged(nameof(Checked));
            }
        }

        public System.Drawing.Bitmap Image => GetImage(Status);

        public DateTime? Started { get; private set; }
        private DateTime? _finished;

        public long? Duration => Started.HasValue && _finished.HasValue
            ? Convert.ToInt64((_finished.Value - Started.Value).TotalSeconds)
            : (Started.HasValue ? Convert.ToInt64((DateTime.Now - Started.Value).TotalSeconds) : (long?) null);

        public string Name { get; private set; }
        public Action<Action<string>> Action = TestLogEvent;

        public ItemStatus Status { get; set; } = ItemStatus.None;

        public void Reset()
        {
            Started = null;
            _finished = null;
            Status = ItemStatus.None;
            UpdateUI();
        }
        public async Task Start(Logger loger)
        {
            Started = DateTime.Now;
            _finished = null;
            Status = ItemStatus.Working;
            UpdateUI();

            await Task.Factory.StartNew(() => Action?.Invoke(loger.AddMessage));

            _finished = DateTime.Now;
            Checked = false;
            Status = ItemStatus.Done;
            UpdateUI();
        }

        public override void UpdateUI()
        {
            OnPropertiesChanged(nameof(Started), nameof(ItemStatus), nameof(Duration), nameof(Image));
            // OnPropertiesChanged(nameof(Started));
        }
    }
}
