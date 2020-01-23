using FzLib.UI.Extension;
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
    public partial class JsonBlackWhiteListItemLine : ExtendedUserControl,IBlackWhiteListItemLine
    {
        private BlackWhiteListItem item=new BlackWhiteListItem() { Type = BlackWhiteListItemType.JTokenPath };
        public BlackWhiteListItem Item { get => item; set => SetValueAndNotify(ref item, value, nameof(Item)); }

        public JsonBlackWhiteListItemLine(BlackWhiteListItem item):this()
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
