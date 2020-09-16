using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WebPageWatcher.Data;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CookieWindow : WindowBase
    {
        private WebPage webPage;

        public CookieWindow(WebPage webPage)
        {
            InitializeComponent();
            this.webPage = webPage;
            if (webPage.Request_Cookies == null)
            {
                return;
            }
            var helper = new SelectorHelper<Cookie>(lvw, Cookies);
            helper.SetContextMenu();
            Reset();
        }

        private void Reset()
        {
            Cookies.Clear();
            foreach (var cookie in webPage.Request_Cookies)
            {
                Cookies.Add(cookie);
            }
        }

        public ObservableCollection<Cookie> Cookies { get; } = new ObservableCollection<Cookie>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            webPage.Request_Cookies = Cookies.ToList();

            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Cookie cookie = new Cookie();
            Cookies.Add(cookie);
            lvw.SelectedItem = cookie;
        }
    }
}