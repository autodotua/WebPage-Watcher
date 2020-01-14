using FzLib.Basic.Collection;
using FzLib.Control.Extension;
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
    /// HtmlIdentifyLine.xaml 的交互逻辑
    /// </summary>
    public partial class WebPagePanel : ExtendedUserControl, IListAndSettingPanel
    {
        public WebPagePanel()
        {
            BackgroundTask.WebPagesChanged += (p1, p2) => Notify(nameof(WebPages));
            InitializeComponent();
            UIHelper.SetContextMenuForSelector(lvwWebPages, WebPages, (p1, p2) => DbHelper.DeleteAsync(p2));
            item.Update = UpdateItem;

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            WebPage webPage = await DbHelper.AddWebPageAsync();
            WebPages.Add(webPage);
            lvwWebPages.SelectedItem = webPage;
        }
        private async Task UpdateItem(WebPage oldItem, WebPage newItem)
        {
            int index = WebPages.IndexOf(oldItem);
            WebPages.Remove(oldItem);
            WebPages.Insert(index, newItem);
            await DbHelper.UpdateAsync(newItem);
            lvwWebPages.SelectedItem = newItem;
        }
        public void UpdateDisplay(object webPage)
        {
            WebPage webPage2 = WebPages.FirstOrDefault(p => p.ID == (webPage as WebPage).ID);
            if (webPage2 != null)
            {
                if (item.WebPage != null && webPage2.ID == item.WebPage.ID)
                {
                    item.UpdateDisplay(webPage as WebPage);
                }
            }
        }
        public void SelectItem(object webPage)
        {
            lvwWebPages.SelectedItem = webPage;
        }
        public ExtendedObservableCollection<WebPage> WebPages => BackgroundTask.WebPages;

    }
}
