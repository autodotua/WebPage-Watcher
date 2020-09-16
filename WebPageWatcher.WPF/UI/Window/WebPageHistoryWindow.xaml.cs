using FzLib.Extension;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WebPageWatcher.Data;
using WebPageWatcher.Web;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WebPageHistoryWindow : WindowBase
    {
        public WebPageHistoryWindow(WebPage webPage) : this()
        {
            WebPage = webPage;
        }

        public WebPageHistoryWindow()
        {
            InitializeComponent();
        }

        public WebPage WebPage { get; }
        public WebPageUpdate update;

        public WebPageUpdate Update
        {
            get => update;
            set
            {
                this.SetValueAndNotify(ref update, value, nameof(Update));
                ListViewSelectionChanged();
            }
        }

        private async Task Initialize()
        {
            WebPageUpdate[] datas;
            if (WebPage == null)
            {
                datas = (await DbHelper.GetWebPageUpdatesAsync()).ToArray();
            }
            else
            {
                datas = (await DbHelper.GetWebPageUpdatesAsync(WebPage)).ToArray();
                (lvw.View as GridView).Columns.RemoveAt(0);
            }
            if (datas.Length == 0)
            {
                await dialog.ShowErrorAsync(FindResource("error_noHistory") as string);
                Close();
                return;
            }
            Updates = new ObservableCollection<WebPageUpdate>(datas);
            this.Notify(nameof(Updates));
        }

        public ObservableCollection<WebPageUpdate> Updates { get; private set; }

        private async void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await Initialize();
        }

        private async void ListViewSelectionChanged()
        {
            switch (selectionMode)
            {
                case 1:
                    if (Update == null)
                    {
                        return;
                    }
                    Animate(2);
                    ResizeMode = System.Windows.ResizeMode.CanResize;
                    WebPageUpdate update1 = Update;

                    selectionMode = 2;
                    Update = lastItem;
                    selectionMode = 0;
                    CompareResult compareResult;
                    try
                    {
                        compareResult = await ComparerBase.CompareAsync(WebPage, Update.Content, update1.Content);
                    }
                    catch (Exception ex)
                    {
                        await dialog.ShowErrorAsync(ex.ToString(), FindResource("error_compareFailed") as string);
                        return;
                    }
                    ComparisonWindow win = new ComparisonWindow(compareResult) { Owner = this };
                    win.ShowDialog();

                    break;

                case 0:
                    if (Update != null)
                    {
                        pre.Load(Update.Content.ToEncodedString(), (await DbHelper.GetWebPageAsync(Update.ID)).Response_Type);
                    }
                    else
                    {
                        pre.Clear();
                    }
                    break;

                default:
                    break;
            }
        }

        private int selectionMode = 0;
        private WebPageUpdate lastItem;

        private void btnCompare_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            selectionMode = 1;
            ResizeMode = System.Windows.ResizeMode.NoResize;
            Animate(1);
            lastItem = Update;
            Update = null;
        }

        private void Animate(int step)
        {
            double x1;
            double opacity;
            double x2;
            if (step == 1)
            {
                x1 = ActualWidth / 2 - lvw.ActualWidth / 2;
                opacity = 0;
                x2 = column2.ActualWidth;
            }
            else
            {
                x1 = 0;
                opacity = 1;
                x2 = 0;
            }
            ThicknessAnimation ani = new ThicknessAnimation(new System.Windows.Thickness(x1, 0, -x1, 0), TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            lvw.BeginAnimation(MarginProperty, ani);
            DoubleAnimation ani2 = new DoubleAnimation(opacity, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            splitter.BeginAnimation(OpacityProperty, ani2);
            DoubleAnimation ani3 = new DoubleAnimation(x2, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            column2Tran.BeginAnimation(TranslateTransform.XProperty, ani3);
        }
    }

    public class WebPageIDConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            WebPage webPage = BackgroundTask.WebPages.FirstOrDefault(p => p.ID == (int)value);
            if (webPage == null)
            {
                return "ID=" + value;
            }
            else
            {
                return webPage.Name;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}