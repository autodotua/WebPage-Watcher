using FzLib.Control.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
            cbbLanguage.SelectedItem = cbbLanguage.Items.Cast<ComboBoxItem>().First(p => p.Tag.Equals(Config.Language));
            cbbTheme.SelectedItem = cbbTheme.Items.Cast<ComboBoxItem>().First(p => p.Tag.Equals(Config.Theme.ToString()));
            chkStartup.IsChecked = FzLib.Program.Startup.IsRegistryKeyExist() == FzLib.IO.Shortcut.ShortcutStatus.Exist;
            cbbLanguage.SelectionChanged += cbbLanguage_SelectionChanged;
            cbbTheme.SelectionChanged += cbbTheme_SelectionChanged;

            switch(Config.Ring)
            {
                case 0:
                    rbtnRingDisabled.IsChecked = true;
                    break;
                case 1:
                    rbtnRingDefault.IsChecked = true;
                    break;
                default:
                    if (string.IsNullOrEmpty(Config.CustomRingPath) || !File.Exists(Config.CustomRingPath))
                    {
                        Config.Ring = 1;
                        rbtnRingDefault.IsChecked = true;
                        Config.CustomRingName = null;
                        Config.Save();
                    }
                    else
                    {
                        rbtnRingCustom.IsChecked = true;
                    }
                    break;
            }
            rbtnRingDisabled.Checked += RadioButton_Checked;
            rbtnRingDefault.Checked += RadioButton_Checked;
            rbtnRingCustom.Checked += RadioButton_Checked;

            Notify(nameof(Config));
        }


        private void cbbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.Language = (cbbLanguage.SelectedItem as ComboBoxItem).Tag as string;
            App.Current.SetCulture();
            Config.Save();
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
            Config.Theme = int.Parse((cbbTheme.SelectedItem as ComboBoxItem).Tag as string);
            App.Current.SetTheme();
            Config.Save();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            switch((sender as RadioButton).Name)
            {
                case nameof(rbtnRingDisabled):
                    Config.Ring = 0;
                    break;
                case nameof(rbtnRingDefault):
                    Config.Ring = 1;
                    break;
                case nameof(rbtnRingCustom):
                    Config.Ring = 2;
                    break;
            }
            Config.Save();
        }

        public Config Config => Config.Instance;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string path = FzLib.Control.Dialog.FileSystemDialog.GetOpenFile(new (string, string)[] { ("mp3", "mp3") });
            if(path!=null)
            {
                string name = Path.GetFileName(path);
                Config.CustomRingName = name;
                Notify(nameof(Config));
                await Task.Run(() =>
                {
                    if(File.Exists(Config.CustomRingPath))
                    {
                        File.Delete(Config.CustomRingPath);
                    }
                    File.Copy(path, Config.CustomRingPath);
                });
                Config.Save();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            BackgroundTaskHelper.PlayRing();
        }

        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BackgroundTaskHelper.StopPlayingRing();
        }

        private void RegardOneSideParseErrorAsNotSameCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Config.Save();
        }
    }
}
