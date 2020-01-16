using FzLib.Basic;
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
    public partial class WebPageChangedNotificationWindow : NotificationWindowBase
    {
        public WebPageChangedNotificationWindow(WebPage webPage, CompareResult compareResult)
        {
            WebPage = webPage;
            CompareResult = compareResult;
            InitializeComponent();
            //IEnumerable<string> strs = null;
            switch (compareResult.WebPage.Response_Type)
            {
                case ResponseType.Html:
                //strs = compareResult.DifferentNodes.Select(p => Regex.Replace((p.New as HtmlNode).InnerText.Trim(), "\\s+", " "));
                //break;
                case ResponseType.Json:
                //strs = compareResult.DifferentNodes.Select(p => Regex.Replace((p.New as JToken).ToString().Trim(), "\\s+", " "));
                //break;
                case ResponseType.Text:
                    //strs = new string[] { compareResult.NewContent.ToEncodedString() };
                    SetDifferencesText(compareResult);
                    break;
                case ResponseType.Binary:
                    btnView.IsEnabled = false;
                    //strs = new string[] { $"{FindResource("type_binary") as string} ({CompareResult.NewContent.Length})" };
                    rtb.Document.Blocks.Add(new Paragraph(new Run($"{FindResource("type_binary") as string} ({CompareResult.NewContent.Length})")));
                    break;
                default:
                    throw new NotSupportedException();
            }

            tbkTime.Text = DateTime.Now.ToString("t", CultureInfo.CurrentUICulture);
            btnOpen.IsEnabled = webPage.Request_Method == "GET";
        }

        private void SetDifferencesText(CompareResult compareResult)
        {
            rtb.Document.Blocks.Clear();
            Diff[] diffs = compareResult.GetDifferences();
            foreach (var diff in diffs)
            {
                switch (diff.operation)
                {
                    case Operation.DELETE:
                        {
                            Paragraph paragraph = new Paragraph();
                            rtb.Document.Blocks.Add(paragraph);

                            Run run = new Run(diff.text);
                            paragraph.Inlines.Add(run);

                            TextRange range = new TextRange(run.ContentStart, run.ContentEnd);
                            range.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
                        }
                        break;
                    case Operation.INSERT:
                        {
                            Paragraph paragraph = new Paragraph();
                            rtb.Document.Blocks.Add(paragraph);

                            Run run = new Run(diff.text);
                            paragraph.Inlines.Add(run);

                            TextRange range = new TextRange(run.ContentStart, run.ContentEnd);
                            //range.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
                        }
                        break;
                    case Operation.EQUAL:
                        break;
                }
            }
        }

        public WebPage WebPage { get; }
        public CompareResult CompareResult { get; }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            TakeBack();
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(WebPage.Url);
                TakeBack();
            }
            catch (Exception ex)
            {
                await dialog.ShowErrorAsync("打开失败");
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            ComparisonWindow win = new ComparisonWindow(CompareResult);
            win.Show();
            TakeBack();
        }
    }
}
