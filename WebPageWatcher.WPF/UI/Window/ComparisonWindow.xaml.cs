using HtmlAgilityPack;
using ICSharpCode.AvalonEdit.Highlighting;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using WebPageWatcher.Web;
using static WebPageWatcher.Data.Helper;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ComparisonWindow : WindowBase
    {
        public ComparisonWindow(CompareResult compareResult):this()
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

            box1.Load(CompareResult.OldContent.ToEncodedString(), CompareResult.Type);
            box2.Load(CompareResult.NewContent.ToEncodedString(), CompareResult.Type);
        }
    }
}
