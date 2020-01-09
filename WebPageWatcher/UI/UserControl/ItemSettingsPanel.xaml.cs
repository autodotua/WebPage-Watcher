using FzLib.Control.Extension;
using HtmlAgilityPack;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebPageWatcher.Data;
using WebPageWatcher.Web;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// ItemSettingsPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ItemSettingsPanel : ExtendedUserControl
    {
        private WebPage rawWebPage;
        private WebPage webPage;
        public Func<WebPage, WebPage, Task> Update { get; set; }

        public ItemSettingsPanel()
        {
            InitializeComponent();
        }

        public WebPage WebPage
        {
            get => webPage;
            set
            {
                rawWebPage = value;
                webPage = value == null ? null : value.Clone();
                Notify(nameof(WebPage));
                //SetValueAndNotify(ref webPage, value, nameof(WebPage));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new CookieWindow(WebPage) { Owner = Window.GetWindow(this) }.ShowDialog();
            Notify(nameof(WebPage));
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //WebPage.Interval = 1000 * 60 * (cbbInterval.SelectedIndex == 0 ? 1 : 60) * (int)(sldInterval.Value);
            //itemJson = null;
            await Update(rawWebPage, webPage);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            WebPage = rawWebPage;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(WebPage.LatestDocument);

        }

        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var dialog = (Window.GetWindow(this) as MainWindow).dialog;
            if (WebPage.Response_Type == "TEXT")
            {
                await dialog.ShowErrorAsync(FindResource("error_textTypeNotSupport") as string);
                return;
            }

            BlackWhiteListWindow htmlWindow = new BlackWhiteListWindow(WebPage) { Owner = Window.GetWindow(this) };
            htmlWindow.ShowDialog();
            Notify(nameof(WebPage));
        }

        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var dialog = (Window.GetWindow(this) as MainWindow).dialog;
            if (WebPage.LatestDocument == null)
            {
                await dialog.ShowErrorAsync(FindResource("error_notGetYet") as string);
                return;
            }
            PreviewWindow win = new PreviewWindow(WebPage) { Owner = Window.GetWindow(this) };
            win.ShowDialog();
        }

        public void UpdateDisplay(WebPage webPage)
        {

            WebPage.LastCheckTime = webPage.LastCheckTime;

            WebPage.LastUpdateTime = webPage.LastUpdateTime;
            WebPage.LatestDocument = webPage.LatestDocument;
            Notify(nameof(WebPage));
        }

        private async void Button_Click_6(object sender, RoutedEventArgs e)
        {
            string header = await (Window.GetWindow(this) as MainWindow).inputDialog.ShowAsync(FindResource("label_HTTPHeaderInput") as string, true, TryFindResource("hint_HTTPHeader") as string);
            if (header != null)
            {
                string[] errors = RequestParser.Parse(WebPage, header);
                await (Window.GetWindow(this) as MainWindow).dialog.ShowInfomationAsync(FindResource("label_headerError")
                       + Environment.NewLine + string.Join(Environment.NewLine, errors));

                Notify(nameof(WebPage));
            }
        }
    }

    public class IsNotNullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }

    public class TimeSpan2Ms : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int interval = (int)value;
            return DateTime.Today + TimeSpan.FromMilliseconds(interval);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? interval = value as DateTime?;
            if (interval.HasValue)
            {
                int ms = (int)(interval.Value - DateTime.Today).TotalMilliseconds;
                if (ms == 0)
                {
                    ms = 1000 * 60;
                }
                return ms;
            }
            return 1000 * 600;

        }
    }
}
