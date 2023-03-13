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
            new LoaderItem {Id = "ScreenerTradingView", Name = "TradingView Screener", Action = Actions.TradingView.ScreenerLoader.Start},
            new LoaderItem {Id = "ScreenerNasdaqStock", Name = "Nasdaq Stock Screener", Status = ItemStatus.None},
            new LoaderItem {Id = "ScreenerNasdaqEtf", Name = "Nasdaq Etf Screener", Status = ItemStatus.None}
        };
        
        public static Image GetAnimatedImage() => GetImage(ItemStatus.Working);


        private static Dictionary<string, Bitmap> _imgResources;
        private static string[] _itemStatusImageName = new[] {"Blank", "Blank", "Wait", "Done", "Error"};

        private static void TestAction(Action<string> showStatus)
        {
            showStatus("Started");
            Thread.Sleep(1200);
            showStatus("Finished");
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

        public string Id;

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
        public long? Duration => Started.HasValue && _finished.HasValue ? Convert.ToInt64((_finished.Value - Started.Value).TotalSeconds) : (long?)null;

        public string Name { get; private set; }
        public Action<Action<string>> Action = TestAction;

        public ItemStatus Status { get; set; } = ItemStatus.None;

        public void Reset()
        {
            Started = null;
            _finished = null;
            Status = ItemStatus.None;
            UpdateUI();
        }
        public async Task Start(Action<string> showStatus)
        {
            Started = DateTime.Now;
            _finished = null;
            Status = ItemStatus.Working;
            UpdateUI();

            await Task.Factory.StartNew(() => Action?.Invoke(showStatus));

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
