using System;
using System.Globalization;
using System.Windows;
using WebPageWatcher.Data;
using WebPageWatcher.Web;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// WebPageChangedNotificationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BackgroundTaskErrorNotificationWindow : NotificationWindowBase
    {
        public BackgroundTaskErrorNotificationWindow(ITaskDbModel model, Exception exception)
        {
            Model = model;
            InitializeComponent();

            tbkMessage.Text = exception.Message;
            tbkContent.Text = exception.ToString();
            tbkTime.Text = DateTime.Now.ToString("t", CultureInfo.CurrentUICulture);
        }

        public ITaskDbModel Model { get; }
        public CompareResult CompareResult { get; }
        public EventHandler IgnoreOnce;
        public EventHandler Ignore;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Ignore?.Invoke(this, new EventArgs());
            TakeBack();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow win = App.Current.GetMainWindow();
            if (win != null)
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