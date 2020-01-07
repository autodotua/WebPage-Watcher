using FzLib.Control.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using WebPageWatcher.Data;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : WindowBase
    {
        public SettingWindow()
        {
            InitializeComponent();
            cbbLanguage.SelectedItem = cbbLanguage.Items.Cast<ComboBoxItem>().First(p => p.Tag.Equals(Config.Instance.Language));
            chkStartup.IsChecked = FzLib.Program.Startup.IsRegistryKeyExist() == FzLib.IO.Shortcut.ShortcutStatus.Exist;

        }


        private void cbbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.Instance.Language = (cbbLanguage.SelectedItem as ComboBoxItem).Tag as string;
            App.Current.SetCulture();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if(chkStartup.IsChecked==true)
            {
                FzLib.Program.Startup.CreateRegistryKey("startup");
            }
            else
            {
                FzLib.Program.Startup.DeleteRegistryKey();
            }
        }
    }
}
