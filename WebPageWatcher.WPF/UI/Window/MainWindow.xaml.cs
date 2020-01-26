using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WebPageWatcher.Data;
using WebPageWatcher.Web;
using System;
using System.Windows.Input;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowBase
    {

        public MainWindow(IDbModel needToSelect) : this()
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
                //ScriptParser parser = new ScriptParser();
                //parser.Output += (p1, p2) => Debug.WriteLine(p2);
                //await parser.ParseAsync(c);
            }
            catch (ScriptException ex)
            {
                await dialog.ShowErrorAsync(ex.ToString());
            }

        }
        public void UpdateDisplay(IDbModel item)
        {
            switch (item)
            {
                case WebPage webPage:
                    (webPagePanel as TabItemPanelBase<WebPage>).UpdateDisplay(webPage);
                    break;
                case Script script:
                    (scriptPanel as TabItemPanelBase<Script>).UpdateDisplay(script);
                    break;
                case Data.Trigger trigger:
                    throw new NotImplementedException();
                    break;
            }
        }

        public void SelectItem(IDbModel item)
        {
            switch (item)
            {
                case WebPage webPage:
                    (webPagePanel as TabItemPanelBase<WebPage>).SelectItem(webPage);
                    break;
                case Script script:
                    (scriptPanel as TabItemPanelBase<Script>).SelectItem(script);
                    break;
                case Data.Trigger trigger:
                    throw new NotImplementedException();
                    break;
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void SettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow win = new SettingWindow() { Owner = this };
            win.ShowDialog();
        }

        private void ShutdownMenuItem_Click(object sender, RoutedEventArgs e)
        {
            BackgroundTask.Stop();
            Application.Current.Shutdown();
        }

        private void ScriptHelperMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new ScriptHelpWindow() { Owner = this }.ShowDialog();
        }

        private async void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await aboutDialog.Show();
        }

        private void AllHistoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new WebPageHistoryWindow().Show();
        }

        private async void ExportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string path = FzLib.UI.Dialog.FileSystemDialog.GetSaveFile(new (string, string)[] { ("SQLite", "db") }, false, true,
                   FindResource("app_name") as string + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            if (path != null)
            {
                try
                {
                    DbHelper.Export(path);
                    await dialog.ShowInfomationAsync(FindResource("info_exportSucceed") as string, FindResource("title_export") as string);
                }
                catch (Exception ex)
                {
                    await dialog.ShowExceptionAsync(ex, FindResource("error_exportFailed") as string, FindResource("title_export") as string);
                }
            }
        }
        private async void ImportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string path = FzLib.UI.Dialog.FileSystemDialog.GetOpenFile(new (string, string)[] { ("SQLite", "db") }, false);
            if (path != null)
            {
                try
                {
                    DbHelper.Import(path);
                    await dialog.ShowInfomationAsync(FindResource("info_importSucceed") as string, FindResource("title_import") as string);
                    Close();
                    await BackgroundTask.LoadAsync();
                    App.Current.GetNewMainWindow().Show();
                }
                catch (Exception ex)
                {
                    await dialog.ShowExceptionAsync(ex, FindResource("error_importFailed") as string, FindResource("title_import") as string);
                }
            }
        }

        private void LogsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new LogWindow().Show();
        }
    }
}
