using FzLib.Basic.Collection;
using FzLib.Control.Extension;
using HtmlAgilityPack;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowBase
    {

        public MainWindow(WebPage needToSelect) : this()
        {
            SelectItem(needToSelect);
        }
        public MainWindow()
        {
            BackgroundTask.WebPagesChanged += (p1, p2) => Notify(nameof(WebPages));
            InitializeComponent();
            UIHelper.SetContextMenuForSelector(lvwWebPages, WebPages, (p1, p2) => DbHelper.DeleteAsync(p2));
            item.Update = UpdateItem;
            //item.Reset = ResetItem;

#if DEBUG
            Button btn = new Button();
            btn.Content = "测试";
            outGrd.Children.Add(btn);
            btn.HorizontalAlignment = HorizontalAlignment.Right;
            btn.Click += TestButton_Click;
#endif
        }

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string c = @"let a1 webpage a1
let a1r responseText a1
let token responseJsonValue a1r.data.token

let a2 webpage a2 clone
let data string id=43&token={token}
set a2 Request_Body data
let a2r responseText a2

let a3Url responseJsonValue a2r.data.url
let a3 webpage a3 clone
set a3 Url a3Url
let a3r response a3

let a4 webpage a4
set a4 Cookies a3r
cp a4";
                ScriptParser parser = new ScriptParser();
                parser.Output += (p1, p2) => Debug.WriteLine(p2);
                await parser.ParseAsync(c);
            }
            catch (ScriptException ex)
            {
                await dialog.ShowErrorAsync(ex.ToString());
            }

        }

        private async Task UpdateItem(WebPage oldItem, WebPage newItem)
        {
            int index = WebPages.IndexOf(oldItem);
            WebPages.Remove(oldItem);
            WebPages.Insert(index, newItem);
            await DbHelper.UpdateAsync(newItem);
            lvwWebPages.SelectedItem = newItem;

        }

        public void UpdateDisplay(WebPage webPage)
        {
            WebPage webPage2 = WebPages.FirstOrDefault(p => p.ID == webPage.ID);
            if (webPage2 != null)
            {
                if (item.WebPage != null && webPage2.ID == item.WebPage.ID)
                {
                    item.UpdateDisplay(webPage);
                }
            }
        }

        public void SelectItem(WebPage webPage)
        {
            lvwWebPages.SelectedItem = webPage;
        }
        //private ExtendedObservableCollection<WebPage> webPages;
        //public ExtendedObservableCollection<WebPage> WebPages
        //{
        //    get => webPages;
        //    set => SetValueAndNotify(ref webPages, value, nameof(WebPages));
        //}
        public ExtendedObservableCollection<WebPage> WebPages => BackgroundTask.WebPages;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            WebPage webPage = await DbHelper.AddWebPageAsync();
            WebPages.Add(webPage);
            lvwWebPages.SelectedItem = webPage;

        }

        private void lvwWebPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //item.WebPage = lvwWebPages.SelectedItem as WebPage;
        }

        private void win_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow win = new SettingWindow() { Owner = this };
            win.ShowDialog();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            BackgroundTask.Stop();
            Application.Current.Shutdown();
        }
    }

}
