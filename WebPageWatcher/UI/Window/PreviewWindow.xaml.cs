using HtmlAgilityPack;
using ICSharpCode.AvalonEdit.Highlighting;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using WebPageWatcher.Data;
using WebPageWatcher.Web;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PreviewWindow : WindowBase
    {
        public PreviewWindow(WebPage webPage) : this(webPage.LatestContent.ToEncodedString(), webPage.Response_Type)
        {
        }
        public PreviewWindow(string text, ResponseType type)
        {
            InitializeComponent();
            switch (type)
            {
                case ResponseType.Html:
                    web.NavigateToString(text);

                    web.Navigated += (p1, p2) => WebBrowserHelper.SetSilent(web, true);

                    code.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".html");

                    code.Text = text;
                    break;
                case ResponseType.Text:
                    goto a;
                case ResponseType.Json:

                    code.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".json");

                a:
                    grd.Children.Remove(web);
                    grd.ColumnDefinitions.RemoveAt(0);
                    grd.ColumnDefinitions.RemoveAt(0);


                    code.Text = text;

                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
