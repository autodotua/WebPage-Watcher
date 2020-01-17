using FzLib.Control.Extension;
using ICSharpCode.AvalonEdit.Highlighting;
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
    public partial class PreviewBox : ExtendedUserControl
    {
        public PreviewBox()
        {
            InitializeComponent();

        }
        public void Load(WebPage webPage)
        {
            Load(webPage.LatestContent.ToEncodedString(), webPage.Response_Type);
        }
        public void Load(string text, ResponseType type)
        {
            switch (type)
            {
                case ResponseType.Html:
                    grd.ColumnDefinitions[0].Width = new GridLength(1,GridUnitType.Star);
                    grd.ColumnDefinitions[1].Width = new GridLength(8,GridUnitType.Pixel);
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
                    grd.ColumnDefinitions[0].Width = new GridLength(0);
                    grd.ColumnDefinitions[1].Width = new GridLength(0);

                    code.Text = text;

                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void Clear()
        {
            web.Navigate("about:blank");
            code.Text = "";
        }
    }
}
