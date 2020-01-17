using HtmlAgilityPack;
using ICSharpCode.AvalonEdit.Highlighting;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WebPageWatcher.Data;
using WebPageWatcher.Web;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WebPageHistoryWindow : WindowBase
    {
        public WebPageHistoryWindow(WebPage webPage)
        {
            InitializeComponent();

            WebPage = webPage;
        }

        public WebPage WebPage { get; }
        public WebPageUpdate Update => lbx.SelectedItem == null ? null : (lbx.SelectedItem as ListBoxItem).Tag as WebPageUpdate;

        private async Task Initialize()
        {
            WebPageUpdate[] datas = (await DbHelper.GetWebPageUpdatesAsync(WebPage)).ToArray();
            if (datas.Length == 0)
            {
                await dialog.ShowErrorAsync(FindResource("error_noHistory") as string);
                Close();
                return;
            }
            foreach (var item in datas)
            {
                ListBoxItem lbxItem = new ListBoxItem
                {
                    Content = item.Time.ToString(CultureInfo.CurrentUICulture),
                    Tag = item
                };
                lbx.Items.Add(lbxItem);
            }

        }

        private async void WindowBase_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await Initialize();
        }

        private async void lbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (selectionMode)
            {
                case 1:
                    if (lbx.SelectedItem == null)
                    {
                        return;
                    }
                    Animate(2);
                    ResizeMode = System.Windows.ResizeMode.CanResize;
                    WebPageUpdate update1 = (lbx.SelectedItem as ListBoxItem).Tag as WebPageUpdate;

                    selectionMode = 2;
                    lbx.SelectedItem = lastItem;
                    selectionMode = 0;

                    var compareResult = await ComparerBase.CompareAsync(WebPage, Update.Content, update1.Content);
                    ComparisonWindow win = new ComparisonWindow(compareResult) { Owner = this };
                    win.ShowDialog();

                    break;
                case 0:
                    Notify(nameof(Update));
                    if (Update != null)
                    {
                        pre.Load(Update.Content.ToEncodedString(), WebPage.Response_Type);
                    }
                    else
                    {
                        pre.Clear();
                    }
                    break;
                default:
                    break;
            }
        }

        int selectionMode = 0;
        private object lastItem;
        private void btnCompare_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            selectionMode = 1;
            ResizeMode = System.Windows.ResizeMode.NoResize;
            Animate(1);
            lastItem = lbx.SelectedItem;
            lbx.SelectedItem = null;

        }
        private void Animate(int step)
        {

            double x1;
            double opacity;
            double x2;
            if (step == 1)
            {
                x1 = ActualWidth / 2 - lbx.ActualWidth / 2;
                opacity = 0;
                x2 = column2.ActualWidth;
            }
            else
            {
                x1 = 0;
                opacity = 1;
                x2 = 0;
            }
            ThicknessAnimation ani = new ThicknessAnimation(new System.Windows.Thickness(x1, 0, -x1, 0), TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            lbx.BeginAnimation(MarginProperty, ani);
            DoubleAnimation ani2 = new DoubleAnimation(opacity, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            splitter.BeginAnimation(OpacityProperty, ani2);
            DoubleAnimation ani3 = new DoubleAnimation(x2, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            column2Tran.BeginAnimation(TranslateTransform.XProperty, ani3);
        }
    }
}
