using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WebPageWatcher.Data;
using WebPageWatcher.Web;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// WebPageChangedNotificationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BackgroundTaskErrorNotificationWindow : NotificationWindowBase
    {
        public BackgroundTaskErrorNotificationWindow(IDbModel model, Exception exception)
        {
            Model = model;
            InitializeComponent();

            tbkMessage.Text = exception.Message;
            tbkContent.Text = exception.ToString();
            tbkTime.Text = DateTime.Now.ToString("t",CultureInfo.CurrentUICulture);
        }

        public IDbModel Model { get; }
        public CompareResult CompareResult { get; }
        public EventHandler IgnoreOnce;
        public EventHandler Ignore;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Ignore?.Invoke(this, new EventArgs());
            TakeBack();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow win = App.Current.GetMainWindow();
            if(win!=null)
            {
                win.SelectItem(Model);
                win.BringToFront();
            }
            else
            {
               App.Current.CreateMainWindow(Model);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            IgnoreOnce?.Invoke(this, new EventArgs());
            TakeBack();
        }
    }
}
