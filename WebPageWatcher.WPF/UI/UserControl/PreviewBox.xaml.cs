using FzLib.UI.Extension;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Windows;
using WebPageWatcher.Data;

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

        public void Load(string text, ResponseType type)
        {
            switch (type)
            {
                case ResponseType.Html:
                    grd.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    grd.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
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