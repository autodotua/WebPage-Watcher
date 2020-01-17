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

        public PreviewWindow(string text, ResponseType type):this()
        {
            box.Load(text,type);
        }
        public PreviewWindow()
        {
            InitializeComponent();
        }

        private void WindowBase_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }
    }
}
