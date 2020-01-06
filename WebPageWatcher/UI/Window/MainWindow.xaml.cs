using FzLib.Basic.Collection;
using FzLib.Control.Extension;
using HtmlAgilityPack;
using MaterialDesignThemes.Wpf;
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

namespace WebPageWatcher.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowBase
    {

        public MainWindow()
        {
            InitializeComponent();
            UIHelper.SetContextMenuForSelector(lvwWebPages, WebPages,(p1,p2)=>DbHelper.DeleteAsync(p2));
            item.Update = UpdateItem;
            //item.Reset = ResetItem;
        }

        private async Task UpdateItem(WebPage oldItem, WebPage newItem)
        {
            int index = WebPages.IndexOf(oldItem);
            WebPages.Remove(oldItem);
            WebPages.Insert(index, newItem);
            await DbHelper.UpdateAsync(newItem);
            lvwWebPages.SelectedItem = newItem;

            await BackgroundTask.Load();
        }

        public void UpdateDisplay(WebPage webPage,bool changed)
        {
            WebPage webPage2 = WebPages.FirstOrDefault(p => p.ID == webPage.ID);
            if(webPage2!=null)
            {
                webPage2.LastCheckTime = webPage.LastCheckTime;
                    webPage2.LastUpdateTime = webPage.LastUpdateTime;
                    webPage2.LatestDocument = webPage.LatestDocument;

                if(item.WebPage!=null &&webPage2.ID==item.WebPage.ID)
                {
                    item.UpdateDisplay(webPage, changed);
                }
            }
        }

        public ExtendedObservableCollection<WebPage> WebPages { get; } = new ExtendedObservableCollection<WebPage>();
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WebPages.AddRange(await DbHelper.GetWebPagesAsync());
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
    }

}
