using HtmlAgilityPack;
using ICSharpCode.AvalonEdit.Highlighting;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
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
    public partial class ScriptHelpWindow : WindowBase
    {

        public ScriptHelpWindow()
        {
            if (CultureInfo.CurrentCulture.Name == "zh-CN")
            {
                Text = Properties.Resources.script_help_zh_CN.ToEncodedString();
            }
            else
            {
                Text = Properties.Resources.script_help_en_US.ToEncodedString();

            }
            InitializeComponent();
        }
        public string Text { get; set; }
    }
}

