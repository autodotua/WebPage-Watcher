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
            cbbTheme.SelectedItem = cbbTheme.Items.Cast<ComboBoxItem>().First(p => p.Tag.Equals(Config.Instance.Theme.ToString()));
            chkStartup.IsChecked = FzLib.Program.Startup.IsRegistryKeyExist() == FzLib.IO.Shortcut.ShortcutStatus.Exist;
            cbbLanguage.SelectionChanged += cbbLanguage_SelectionChanged;
            cbbTheme.SelectionChanged += cbbTheme_SelectionChanged;

            switch(Config.Instance.Ring)
            {
                case 0:
                    rbtnRingDisabled.IsChecked = true;
                    break;
                case 1:
                    rbtnRingDefault.IsChecked = true;
                    break;
                default:
                    rbtnRingCustom.IsChecked = true;
                    break;
            }
            rbtnRingDisabled.Checked += RadioButton_Checked;
            rbtnRingDefault.Checked += RadioButton_Checked;
            rbtnRingCustom.Checked += RadioButton_Checked;
        }


        private void cbbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.Instance.Language = (cbbLanguage.SelectedItem as ComboBoxItem).Tag as string;
            App.Current.SetCulture();
            Config.Instance.Save();
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

        private void cbbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.Instance.Theme = int.Parse((cbbTheme.SelectedItem as ComboBoxItem).Tag as string);
            App.Current.SetTheme();
            Config.Instance.Save();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            switch((sender as RadioButton).Name)
            {
                case nameof(rbtnRingDisabled):
                    Config.Instance.Ring = 0;
                    break;
                case nameof(rbtnRingDefault):
                    Config.Instance.Ring = 1;
                    break;
                case nameof(rbtnRingCustom):
                    Config.Instance.Ring = 2;
                    break;
            }
            Config.Instance.Save();
        }

        public Config Config => Config.Instance;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string path = FzLib.Control.Dialog.FileSystemDialog.GetOpenFile(new (string, string)[] { ("mp3", "mp3") });
            if(path!=null)
            {
                Config.RingPath = path;
                Notify(nameof(Config));
            }
        }
    }
}
