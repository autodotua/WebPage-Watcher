using FzLib.Basic.Collection;
using FzLib.UI.Extension;
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
    public partial class ScriptPanel : ScriptPanelBase
    {
        public ScriptPanel()
        {
            BackgroundTask.WebPagesChanged += (p1, p2) => Notify(nameof(Item));
            InitializeComponent();

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Script script = await DbHelper.InsertAsync<Script>();
            Items.Add(script);
            lvw.SelectedItem = script;
        }

        public override void UpdateDisplay(Script item)
        {
            Script here = Items.FirstOrDefault(p => p.ID == (item as Script).ID);
            if (here != null)
            {
                if (Item != null && here.ID == Item.ID)
                {
                    Item.LastExcuteTime = item.LastExcuteTime;
                    Notify(nameof(Item));
                }
            }
        }
        public override ExtendedObservableCollection<Script> Items => BackgroundTask.Scripts;

        public override ListView List => lvw;

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await UpdateItem();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ResetItem();
        }

        private async void ExcuteButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.progressDialog.Show();
            try
            {
                ScriptParser parser = new ScriptParser(Item);
                StringBuilder logs = new StringBuilder();
                parser.Output += (p1, p2) =>
                {
                    logs.AppendLine(p2);
                };
                await parser.ParseAsync();
             
                MainWindow.progressDialog.Close();
                await MainWindow.dialog.ShowInfomationAsync(logs.ToString(),FindResource("info_excuteSucceed") as string); ;
            }
            catch (Exception ex)
            {
                MainWindow.progressDialog.Close();
                MainWindow.progressDialog.Close();
                await MainWindow.dialog.ShowErrorAsync(ex.ToString(), FindResource("info_excuteSucceed") as string);
            }
        }
    }
}
