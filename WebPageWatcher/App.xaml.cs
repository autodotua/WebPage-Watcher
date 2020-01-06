using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WebPageWatcher.Data;
using WebPageWatcher.UI;

namespace WebPageWatcher
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application, ISingleObject<MainWindow>
    {
        private FzLib.Program.Runtime.TrayIcon notifyIcon = null;
        public MainWindow SingleObject { get; set; }
        public static new App Current { get; private set; }
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            Current = this;
#if DEBUG
#else
                        FzLib.Program.Runtime.UnhandledException.RegistAll();
#endif
            await BackgroundTask.Load();
            FzLib.Program.Runtime.SingleInstance singleInstance = new FzLib.Program.Runtime.SingleInstance(Assembly.GetExecutingAssembly().FullName);
            //if (await singleInstance.CheckAndOpenWindow(this, this))
            //{
            //    return;
            //}
            FzLib.Program.App.SetWorkingDirectoryToAppPath();

            AboutTheme();

            Tray();

            SelectCulture(Config.Instance.Language);

            if (!(e.Args.Length == 1 && e.Args[0] == "startup"))
            {
                CreateMainWindow();
            }

        }

        public void CreateMainWindow(WebPage para = null)
        {
            if (para != null)
            {
                SingleObject = new MainWindow(para);

            }
            else
            {
                SingleObject = new MainWindow();
            }
            SingleObject.Show();
        }

        private void AboutTheme()
        {
            MaterialDesignThemes.Wpf.BundledTheme theme = new MaterialDesignThemes.Wpf.BundledTheme();
            theme.PrimaryColor = MaterialDesignColors.PrimaryColor.Purple;
            theme.SecondaryColor = MaterialDesignColors.SecondaryColor.Lime;

            var v = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", "1");
            if (v == null || v.ToString() == "1")
            {
                AppsUseLightTheme = true;
                theme.BaseTheme = MaterialDesignThemes.Wpf.BaseTheme.Light;
            }
            else
            {
                theme.BaseTheme = MaterialDesignThemes.Wpf.BaseTheme.Dark;
            }
            Resources.MergedDictionaries.Add(theme);
            v = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", "0");
            if (v == null || v.ToString() == "1")
            {
                SystemUsesLightTheme = true;
            }
        }

        public bool SystemUsesLightTheme { get; private set; }
        public bool AppsUseLightTheme { get; private set; }

        /// <summary>
        /// 设置托盘信息
        /// </summary>
        private void Tray()
        {
            System.Drawing.Icon icon;
            if (SystemUsesLightTheme)
            {
                icon = WebPageWatcher.Properties.Resources.trayIcon_dark;
            }
            else
            {
                icon = WebPageWatcher.Properties.Resources.trayIcon;
            }
            notifyIcon = new FzLib.Program.Runtime.TrayIcon(icon, "网页内容变动提醒");
            notifyIcon.ClickToOpenOrHideWindow(this);
            notifyIcon.AddContextMenuItem("退出", () =>
            {
                notifyIcon.Dispose();
                Application.Current.Shutdown();
            });
            notifyIcon.Show();

        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            DbHelper.Dispose();
        }

        public void SelectCulture(string culture)
        {
            if (String.IsNullOrEmpty(culture))
                return;

            //Copy all MergedDictionarys into a auxiliar list.
            var dictionary = Resources.MergedDictionaries;

            //Search for the specified culture.     
            string requestedCulture = string.Format("/Properties/StringResources.{0}.xaml", culture);
            var resourceDictionary = dictionary.
                FirstOrDefault(p => p.Source != null && p.Source.OriginalString == requestedCulture);


            //If we have the requested resource, remove it from the list and place at the end.     
            //Then this language will be our string table to use.      
            if (resourceDictionary != null)
            {
                dictionary.Remove(resourceDictionary);
                dictionary.Add(resourceDictionary);
            }


            //Inform the threads of the new culture.     
            var c = new CultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = c;
            Thread.CurrentThread.CurrentUICulture = c;
        }

        public  MainWindow GetMainWindow(bool notUIThread = false)
        {
            MainWindow mainWindow =SingleObject;
            if (mainWindow != null && mainWindow.IsLoaded && !mainWindow.IsClosed)
            {
                return mainWindow;
            }
            return null;
        }
    }
}
