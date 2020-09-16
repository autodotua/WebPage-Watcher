using FzLib.UI.Extension;
using System;
using System.Windows;
using System.Windows.Controls;
using WebPageWatcher.Data;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// HtmlIdentifyLine.xaml 的交互逻辑
    /// </summary>
    public partial class JsonBlackWhiteListItemLine : ExtendedUserControl, IBlackWhiteListItemLine
    {
        private BlackWhiteListItem item = new BlackWhiteListItem() { Type = BlackWhiteListItemType.JTokenPath };
        public BlackWhiteListItem Item { get => item; set => SetValueAndNotify(ref item, value, nameof(Item)); }

        public JsonBlackWhiteListItemLine(BlackWhiteListItem item) : this()
        {
            Item = item;
        }

        private void TxtValue_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        public JsonBlackWhiteListItemLine()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Deleted?.Invoke(this, new EventArgs());
        }

        public event EventHandler Deleted;
    }
}