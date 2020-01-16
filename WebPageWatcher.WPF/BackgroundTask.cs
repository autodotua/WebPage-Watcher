//#define CONTINUING
//#define DISABLED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using WebPageWatcher.Data;
using WebPageWatcher.Web;
using WebPageWatcher.UI;
using FzLib.Basic.Collection;
using System.Diagnostics;
using System.Media;
using System.IO;
using System.Runtime.InteropServices;

namespace WebPageWatcher
{
    public static class BackgroundTaskHelper
    {
        public static void Initialize()
        {
            BackgroundTask.WebPageChanged += WebPageChanged;
            BackgroundTask.PropertyUpdated += PropertyUpdated;
            BackgroundTask.ExceptionAlarm += ExceptionAlarm;
        }

        private static void ExceptionAlarm(object sender, ExceptionAlarmEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                BackgroundTaskErrorNotificationWindow win = new BackgroundTaskErrorNotificationWindow(e.Item, e.Exception);
                win.Ignore += (p1, p2) => e.DisableAction() ;
                win.IgnoreOnce += (p1, p2) => e.ResetAction() ;
                win.PopUp();
            });
        }

        private static void PropertyUpdated(object sender, PropertyUpdatedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = App.Current.GetMainWindow();
                if (mainWindow != null)
                {
                    mainWindow.UpdateDisplay(e.Item);
                }
            });
        }

        private static void WebPageChanged(object sender, WebPageChangedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                WebPageChangedNotificationWindow win = new WebPageChangedNotificationWindow(e.WebPage,e.CompareResult);
                win.Closed += (p1, p2) => StopPlayingRing();
                win.PopUp();
                PlayRing();
            });
        }
        public static uint SND_ASYNC = 0x0001;
        public static uint SND_FILENAME = 0x00020000;
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern uint mciSendString(string lpstrCommand, string lpstrReturnString, uint uReturnLength, uint hWndCallback);

        public static void PlayRing()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (Config.Instance.Ring == 0)
                {
                    return;
                }
                string path;
                if (Config.Instance.Ring == 1 || !File.Exists(Config.Instance.CustomRingPath))
                {
                    path = Path.Combine(FzLib.Program.App.ProgramDirectoryPath, "Res", "ring.mp3");
                }
                else
                {
                    path = Config.Instance.CustomRingPath;
                }
                mciSendString("close ring", null, 0, 0);
                mciSendString($"open \"{path}\" alias ring", null, 0, 0); //音乐文件
                mciSendString("play ring", null, 0, 0);
            });
        }
        public static void StopPlayingRing()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                mciSendString("stop ring", null, 0, 0);
                mciSendString("close ring", null, 0, 0);
            });
        }
    }
    
}
