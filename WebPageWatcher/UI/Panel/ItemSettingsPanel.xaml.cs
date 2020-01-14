using FzLib.Control.Extension;
using HtmlAgilityPack;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            new CookieWindow(WebPage) { Owner = MainWindow }.ShowDialog();
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



        private async void BlackWhiteListButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = (Window.GetWindow(this) as MainWindow).dialog;
            if (WebPage.Response_Type == ResponseType.Text)
            {
                await dialog.ShowErrorAsync(FindResource("error_textTypeNotSupport") as string);
                return;
            }
            if (WebPage.Response_Type == ResponseType.Binary)
            {
                await dialog.ShowErrorAsync(FindResource("error_binaryTypeNotSupport") as string);
                return;
            }

            BlackWhiteListWindow htmlWindow = new BlackWhiteListWindow(WebPage) { Owner = Window.GetWindow(this) };
            htmlWindow.ShowDialog();
            Notify(nameof(WebPage));
        }

        private async void ViewLatestButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = MainWindow.dialog;
            if (WebPage.LatestContent == null)
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
            WebPage.LatestContent = webPage.LatestContent;
            Notify(nameof(WebPage));
        }

        private async void ParseHTTPHeaderButton_Click(object sender, RoutedEventArgs e)
        {

            string header = await MainWindow.inputDialog.ShowAsync(FindResource("label_HTTPHeaderInput") as string, true, TryFindResource("hint_HTTPHeader") as string);
            if (header != null)
            {
                string[] errors = RequestParser.Parse(WebPage, header);
                await MainWindow.dialog.ShowInfomationAsync(FindResource("label_headerError")
                       + Environment.NewLine + string.Join(Environment.NewLine, errors));

                Notify(nameof(WebPage));
            }
        }

        private async void ForceCompareButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.progressDialog.Show();
            try
            {
                bool result = await BackgroundTask.Excute(WebPage, true);
                MainWindow.progressDialog.Close();
                if (!result)
                {
                    await MainWindow.dialog.ShowInfomationAsync(FindResource("label_compareComplete") as string);
                }
                Notify(nameof(WebPage));
            }
            catch (Exception ex)
            {
                MainWindow.progressDialog.Close();
                await MainWindow.dialog.ShowErrorAsync(ex.ToString(), FindResource("error_forceCompare") as string);
            }
        }

        private async void ForceGetButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.progressDialog.Show();
            try
            {
                byte[] content = await HtmlGetter.GetResponseBinaryAsync(webPage);

                MainWindow.progressDialog.Close();
                if (WebPage.Response_Type == ResponseType.Binary)
                {
                    bool yes = await MainWindow.dialog.ShowYesNoAsync(FindResource("label_binaryTypePreview") as string, FindResource("label_binaryTypePreviewTitle") as string);
                    if(yes)
                    {
                        string defaultName = "";
                        string url = WebPage.Url.TrimEnd('/');
                        int index = url.LastIndexOf('/')+1;
                        if(index>0 && index<WebPage.Url.Length)
                        {
                            defaultName = url.Substring(index);
                        }
                        string path = FzLib.Control.Dialog.FileSystemDialog.GetSaveFile(null, false, false, defaultName);
                        if(path!=null)
                        {
                            File.WriteAllBytes(path, content);
                        }
                    }

                    return;
                }
                if (content.Length == 0)
                {
                    await MainWindow.dialog.ShowInfomationAsync(FindResource("label_responseIsEmpty") as string, FindResource("error_forceGet") as string);
                }
                else
                {
                    PreviewWindow win = new PreviewWindow(content.ToEncodedString(), WebPage.Response_Type) { Owner = MainWindow };
                    win.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MainWindow.progressDialog.Close();
                await MainWindow.dialog.ShowErrorAsync(ex.ToString(), FindResource("error_forceGet") as string);
            }

        }

        private MainWindow MainWindow => Window.GetWindow(this) as MainWindow;
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
    public class ResponseTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)(ResponseType)value;
            //switch(value )
            //{
            //    case ResponseType.Html:
            //        return 0;
            //    case ResponseType.Json:
            //        return 1;
            //    case ResponseType.Text:
            //        return 2;
            //    case ResponseType.Binary:
            //        return 3;
            //}
            //throw new ArgumentOutOfRangeException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (ResponseType)(int)value;
            //switch((int)value)
            //{
            //    case 0:
            //        return ResponseType.Html;
            //    case 1:
            //        return ResponseType.Json;
            //    case 2:
            //        return ResponseType.Text;
            //    case 3:
            //        return ResponseType.Binary;
            //}
            //throw new ArgumentOutOfRangeException();
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
