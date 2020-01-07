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
           lvwWebPages.SelectedItem  = webPage;
        }
        //private ExtendedObservableCollection<WebPage> webPages;
        //public ExtendedObservableCollection<WebPage> WebPages
        //{
        //    get => webPages;
        //    set => SetValueAndNotify(ref webPages, value, nameof(WebPages));
        //}
        public ExtendedObservableCollection<WebPage> WebPages => BackgroundTask.WebPages;
        private  void Window_Loaded(object sender, RoutedEventArgs e)
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
    }

}
