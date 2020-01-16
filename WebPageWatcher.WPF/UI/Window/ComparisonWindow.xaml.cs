using HtmlAgilityPack;
using ICSharpCode.AvalonEdit.Highlighting;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using WebPageWatcher.Web;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ComparisonWindow : WindowBase
    {
        public ComparisonWindow(CompareResult compareResult)
        {
            InitializeComponent();
            switch (compareResult.WebPage.Response_Type)
            {
                case Data.ResponseType.Html:
                    web1.NavigateToString((compareResult.OldDocument as HtmlDocument).Text);
                    web2.NavigateToString((compareResult.NewDocument as HtmlDocument).Text);

                    web1.Navigated += (p1, p2) =>
              WebBrowserHelper.SetSilent(web1, true);
                    web2.Navigated += (p1, p2) =>
                 WebBrowserHelper.SetSilent(web2, true);

                    code1.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".html");
                    code2.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".html");

                    code1.Text = (compareResult.OldDocument as HtmlDocument).Text;
                    code2.Text = (compareResult.NewDocument as HtmlDocument).Text;
                    break;
                case Data.ResponseType.Text:
                    goto a;
                case Data.ResponseType.Json:

                    code1.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".json");
                    code2.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".json");

                    a:
                    grd.Children.Remove(web1);
                    grd.Children.Remove(web2);
                    grd.RowDefinitions.RemoveAt(2);
                    grd.RowDefinitions.RemoveAt(2);


                    code1.Text = (compareResult.OldDocument as JObject).ToString();
                    code2.Text = (compareResult.NewDocument as JObject).ToString();

                    break;
                default:
                    throw new NotSupportedException();
            }


        }





    }
}
