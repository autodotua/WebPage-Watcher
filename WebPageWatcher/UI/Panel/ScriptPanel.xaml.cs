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
            Script webPage2 = Items.FirstOrDefault(p => p.ID == (item as Script).ID);
            if (webPage2 != null)
            {
                //if (item.WebPage != null && webPage2.ID == item.WebPage.ID)
                //{
                //    item.UpdateDisplay(webPage as WebPage);
                //}
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

        private void ExcuteButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
