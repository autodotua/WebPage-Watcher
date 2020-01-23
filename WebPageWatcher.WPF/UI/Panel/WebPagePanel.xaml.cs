using FzLib.Basic.Collection;
using FzLib.UI.Extension;
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
    /// HtmlIdentifyLine.xaml 的交互逻辑
    /// </summary>
    public partial class WebPagePanel : WebPagePanelBase
    {
        public WebPagePanel()
        {
            BackgroundTask.WebPagesChanged += (p1, p2) => Notify(nameof(Items));
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            WebPage webPage = await DbHelper.InsertAsync<WebPage>();
            Items.Add(webPage);
            lvw.SelectedItem = webPage;
        }

        public override void UpdateDisplay(WebPage webPage3)
        {
            WebPage webPage = webPage3 as WebPage;
            WebPage webPage2 = Items.FirstOrDefault(p => p.ID == (webPage as WebPage).ID);
            if (webPage2 != null)
            {
                if (Item != null && webPage2.ID == Item.ID)
                {
                    Item.LastCheckTime = webPage.LastCheckTime;
                    Item.LastUpdateTime = webPage.LastUpdateTime;
                    Notify(nameof(Item));
                }
            }
        }

        public override ExtendedObservableCollection<WebPage> Items => BackgroundTask.WebPages;




        private void CookieButton_Click(object sender, RoutedEventArgs e)
        {
            new CookieWindow(Item) { Owner = MainWindow }.ShowDialog();
            Notify(nameof(Item));
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //WebPage.Interval = 1000 * 60 * (cbbInterval.SelectedIndex == 0 ? 1 : 60) * (int)(sldInterval.Value);
            //itemJson = null;
            await UpdateItem();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ResetItem();
        }



        private async void BlackWhiteListButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = (Window.GetWindow(this) as MainWindow).dialog;
            if (Item.Response_Type == ResponseType.Text)
            {
                await dialog.ShowErrorAsync(FindResource("error_textTypeNotSupport") as string);
                return;
            }
            if (Item.Response_Type == ResponseType.Binary)
            {
                await dialog.ShowErrorAsync(FindResource("error_binaryTypeNotSupport") as string);
                return;
            }

            BlackWhiteListWindow htmlWindow = new BlackWhiteListWindow(Item) { Owner = Window.GetWindow(this) };
            htmlWindow.ShowDialog();
            Notify(nameof(Item));
        }

        private async void ViewLatestButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = MainWindow.dialog;
            byte[] content = await Item.GetLatestContentAsync();
            if (content == null)
            {
                await dialog.ShowErrorAsync(FindResource("error_notGetYet") as string);
                return;
            }
            PreviewWindow win = new PreviewWindow(content.ToEncodedString(), Item.Response_Type) { Owner = Window.GetWindow(this) };
            win.ShowDialog();
        }


        private async void ParseHTTPHeaderButton_Click(object sender, RoutedEventArgs e)
        {

            string header = await MainWindow.inputDialog.ShowAsync(FindResource("label_HTTPHeaderInput") as string, true, TryFindResource("hint_HTTPHeader") as string);
            if (header != null)
            {
                string[] errors = RequestParser.Parse(Item, header);
                await MainWindow.dialog.ShowInfomationAsync(FindResource("label_headerError")
                       + Environment.NewLine + string.Join(Environment.NewLine, errors));

                Notify(nameof(Item));
            }
        }

        private async void ForceCompareButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.progressDialog.Show();
            try
            {
                bool result = await BackgroundTask.CheckAndExcuteWebPageAsync(Item, true);
                MainWindow.progressDialog.Close();
                if (!result)
                {
                    await MainWindow.dialog.ShowInfomationAsync(FindResource("label_compareComplete") as string);
                }
                Notify(nameof(Item));
            }
            catch (Exception ex)
            {
                MainWindow.progressDialog.Close();
                await MainWindow.dialog.ShowErrorAsync(ex.ToString(), FindResource("error_compareFailed") as string);
            }
        }

        private async void ForceGetButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.progressDialog.Show();
            try
            {
                byte[] content = await HtmlGetter.GetResponseBinaryAsync(Item);

                MainWindow.progressDialog.Close();
                if (Item.Response_Type == ResponseType.Binary)
                {
                    bool yes = await MainWindow.dialog.ShowYesNoAsync(FindResource("label_binaryTypePreview") as string, FindResource("label_binaryTypePreviewTitle") as string);
                    if (yes)
                    {
                        string defaultName = "";
                        string url = Item.Url.TrimEnd('/');
                        int index = url.LastIndexOf('/') + 1;
                        if (index > 0 && index < Item.Url.Length)
                        {
                            defaultName = url.Substring(index);
                        }
                        string path = FzLib.UI.Dialog.FileSystemDialog.GetSaveFile(null, false, false, defaultName);
                        if (path != null)
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
                    PreviewWindow win = new PreviewWindow(content.ToEncodedString(), Item.Response_Type) { Owner = MainWindow };
                    win.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MainWindow.progressDialog.Close();
                await MainWindow.dialog.ShowErrorAsync(ex.ToString(), FindResource("error_forceGet") as string);
            }

        }

        public override ListView List => lvw;

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            WebPageHistoryWindow win = new WebPageHistoryWindow(Item);
            win.Show();
        }
    }
}
