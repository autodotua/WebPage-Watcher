using FzLib.Basic;
using HtmlAgilityPack;
using ICSharpCode.AvalonEdit.Highlighting;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WebPageWatcher.Web;
using static WebPageWatcher.Data.Helper;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ComparisonWindow : WindowBase
    {
        public ComparisonWindow(CompareResult compareResult) : this()
        {
            CompareResult = compareResult;
        }
        public ComparisonWindow()
        {
            InitializeComponent();
        }
        public CompareResult CompareResult { get; }

        private void WindowBase_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //if(CompareResult.OldContent==null || CompareResult.NewDocument==null)
            //{
            //}
            //switch (CompareResult.WebPage.Response_Type)
            //{
            //    case Data.ResponseType.Html:
            //        web1.NavigateToString(CompareResult.OldContent.ToEncodedString());
            //        web2.NavigateToString(CompareResult.NewContent.ToEncodedString());

            //        web1.Navigated += (p1, p2) =>
            //  WebBrowserHelper.SetSilent(web1, true);
            //        web2.Navigated += (p1, p2) =>
            //     WebBrowserHelper.SetSilent(web2, true);

            //        code1.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".html");
            //        code2.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".html");

            //        break;
            //    case Data.ResponseType.Text:
            //        goto a;
            //    case Data.ResponseType.Json:

            //        code1.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".json");
            //        code2.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".json");

            //    a:
            //        grd.Children.Remove(web1);
            //        grd.Children.Remove(web2);
            //        grd.RowDefinitions.RemoveAt(2);
            //        grd.RowDefinitions.RemoveAt(2);

            //        break;
            //    default:
            //        throw new NotSupportedException();
            //}
            //code1.Text = CompareResult.OldContent.ToEncodedString();
            //code2.Text = CompareResult.NewContent.ToEncodedString();
            string text1 = CompareResult.OldContent.ToEncodedString();
            string text2 = CompareResult.NewContent.ToEncodedString();
            box1.Load(text1, CompareResult.Type);
            box2.Load(text2, CompareResult.Type);
            diff_match_patch diff = new diff_match_patch();
            var diffs = diff.diff_main(text1, text2); 
            rtb.Document.Blocks.Clear();
            foreach (var item in diffs)
            {
                Paragraph paragraph = new Paragraph();
                rtb.Document.Blocks.Add(paragraph);

                Run run = new Run(item.text);
                paragraph.Inlines.Add(run);
                TextRange range = new TextRange(run.ContentStart, run.ContentEnd);
                switch (item.operation)
                {
                    case Operation.DELETE:
                        range.ApplyPropertyValue(TextElement.FontSizeProperty, 12.0);
                        range.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
                        break;
                    case Operation.INSERT:
                        range.ApplyPropertyValue(TextElement.FontSizeProperty, 12.0); range.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
                        range.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
                        //range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                        break;
                    case Operation.EQUAL:
                        range.ApplyPropertyValue(TextElement.FontSizeProperty, 9.0);
                        range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Gray);
                        break;
                }
            }
        }
    }
}
