using FzLib.Control.Extension;
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
    public partial class HtmlBlackWhiteListItemLine : ExtendedUserControl, IBlackWhiteListItemLine
    {
        private BlackWhiteListItem item; 
        public BlackWhiteListItem Item { get => item; set => SetValueAndNotify(ref item, value, nameof(Item)); }


        public HtmlBlackWhiteListItemLine(BlackWhiteListItem item) : this()
        {
            Item = item;
            //switch (item.Type)
            //{
            //    case HtmlElementIdentifyType.Id:
            //        cbbIdentifyType.SelectedIndex = 0;
            //        break;
            //    case HtmlElementIdentifyType.XPath:
            //        cbbIdentifyType.SelectedIndex = 1;
            //        break;
            //}
            //txtValue.Text = item.Value;

            //txtValue.TextChanged += TxtValue_TextChanged;
            //cbbIdentifyType.SelectionChanged += CbbIdentifyType_SelectionChanged;
        }

        private void CbbIdentifyType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //switch (cbbIdentifyType.SelectedIndex)
            //{
            //    case 0:
            //        Item.Type = HtmlElementIdentifyType.Id;
            //        break;
            //    case 1:
            //        Item.Type = HtmlElementIdentifyType.XPath;
            //        break;
            //}
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
