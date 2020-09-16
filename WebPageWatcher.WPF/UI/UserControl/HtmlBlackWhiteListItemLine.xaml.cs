using FzLib.UI.Extension;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WebPageWatcher.Data;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// HtmlIdentifyLine.xaml 的交互逻辑
    /// </summary>
    public partial class HtmlBlackWhiteListItemLine : ExtendedUserControl, IBlackWhiteListItemLine
    {
        private BlackWhiteListItem item;
        public BlackWhiteListItem Item { get => item; set => SetValueAndNotify(ref item, value, nameof(Item)); }

        public HtmlBlackWhiteListItemLine(BlackWhiteListItem item) : this()
        {
            Item = item;
        }

        private void TxtValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Item.Value = txtValue.Text;
        }

        public HtmlBlackWhiteListItemLine()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Deleted?.Invoke(this, new EventArgs());
        }

        public event EventHandler Deleted;
    }

    public class BlackWhiteListItem2IntConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (BlackWhiteListItemType)Enum.ToObject(typeof(BlackWhiteListItemType), (int)value);
        }
    }
}