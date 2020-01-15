using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
            InitializeComponent();
            //item.Reset = ResetItem;

#if DEBUG
            Button btn = new Button();
            btn.Content = "测试";
            outGrd.Children.Add(btn);
            btn.HorizontalAlignment = HorizontalAlignment.Right;
            btn.VerticalAlignment = VerticalAlignment.Top;
            btn.Click += TestButton_Click;
            Grid.SetColumn(btn, 1);
#endif

            //tab.SetResourceReference(BackgroundProperty, "MaterialDesignPaper");
            //tab.SetResourceReference(TextElement.ForegroundProperty, "MaterialDesignBody");
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
        public void UpdateDisplay<T>(T item) where T : class, IDbModel, new()
        {
            TabItemPanelBase<T> panel = null;
            if (item is WebPage)
            {
                panel = webPagePanel as TabItemPanelBase<T>;
            }
            panel.UpdateDisplay(item);
        }

        public void SelectItem<T>(T item) where T : class, IDbModel, new()
        {
            TabItemPanelBase<T> panel = null;
            if (item is WebPage)
            {
                panel = webPagePanel as TabItemPanelBase<T>;
            }
            panel.SelectItem(item);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            new ScriptHelpWindow() { Owner = this }.ShowDialog();
        }

        private async void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            await aboutDialog.Show();
        }
    }

}
