#define TEST
#define DISABLED

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
    public static class BackgroundTask
    {
        private static Timer timer;
        public static ExtendedObservableCollection<WebPage> WebPages
        {
            get => webPages;
            private set
            {
                webPages = value;
                WebPagesChanged?.Invoke(null, WebPages);
            }
        }
        public static EventHandler<ExtendedObservableCollection<WebPage>> WebPagesChanged;

        private static ExtendedDictionary<WebPage, int> exceptionsCount = new ExtendedDictionary<WebPage, int>();
        private static ExtendedDictionary<WebPage, Exception> exceptions = new ExtendedDictionary<WebPage, Exception>();
        private static ExtendedObservableCollection<WebPage> webPages;

        public static async Task Load()
        {

            if (timer != null)
            {
                timer.Dispose();
            }

            WebPages = new ExtendedObservableCollection<WebPage>(await DbHelper.GetWebPagesAsync());
#if (DISABLED && DEBUG)
            return;
#endif
#if (TEST && DEBUG)
            timer = new Timer(new TimerCallback(async p =>await Do()), null, 0, 1000 * 10);
#else
            timer = new Timer(new TimerCallback(p => Do()), null, 0, 1000 * 60);
#endif
        }

        private static async Task Do()
        {
            foreach (var webPage in WebPages.Where(p=>p.Enabled).ToArray())
            {
                try
                {
                    await Excute(webPage);
                }
                catch (Exception ex)
                {
                    if (exceptionsCount[webPage] != -1)
                    {
                        exceptionsCount[webPage]++;
                        exceptions[webPage] = ex;
                    }
                    CheckExceptions(webPage);
                }

            }
        }

        public static async Task<bool> Excute(WebPage webPage, bool force = false)
        {
            DateTime now = DateTime.Now;
            if (webPage.LatestContent == null)
            {
                webPage.LatestContent =await HtmlGetter.GetResponseBinaryAsync(webPage);
                webPage.LastCheckTime = now;

                await UpdateDbAndUI(webPage, null);
                return false;
            }
            else
#if (!TEST || !DEBUG)
                    if (webPage.LastCheckTime + TimeSpan.FromMilliseconds(webPage.Interval) < now || force)
#endif
            {

                CompareResult result =await ComparerBase.CompareAsync(webPage);
                if (result.Same == false)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        WebPageChangedNotificationWindow win = new WebPageChangedNotificationWindow(webPage, result);
                        win.Closed += (p1, p2) => StopPlayRing();
                        win.PopUp();
                    });
                    webPage.LastUpdateTime = now;
                    webPage.LatestContent = result.NewContent;
                    PlayRing();
                }

                webPage.LastCheckTime = now;
                await UpdateDbAndUI(webPage, result);
                return result.Same == false;
            }
            return false;
        }

        private async static Task UpdateDbAndUI(WebPage page, CompareResult result)
        {
            await DbHelper.UpdateAsync(page);
            App.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = App.Current.GetMainWindow(true);
                if (mainWindow != null)
                {
                    mainWindow.UpdateDisplay(page);
                }
            });
        }

        private static void CheckExceptions(WebPage webPage)
        {
            if (exceptionsCount[webPage] >= 5)
            {
                exceptionsCount[webPage] = -1;//暂时先不记录，等关闭窗口以后继续累计
                App.Current.Dispatcher.Invoke(() =>
                {
                    BackgroundTaskErrorNotificationWindow win = new BackgroundTaskErrorNotificationWindow(webPage, exceptions[webPage]);
                    win.Ignore += (p1, p2) => exceptionsCount[webPage] = -1;
                    win.IgnoreOnce += (p1, p2) => exceptionsCount[webPage] = 0;
                    win.PopUp();
                });
            }
        }
        public static uint SND_ASYNC = 0x0001;
        public static uint SND_FILENAME = 0x00020000;
        [DllImport("winmm.dll")]
        public static extern uint mciSendString(string lpstrCommand, string lpstrReturnString, uint uReturnLength, uint hWndCallback);

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
                    path = Path.Combine(FzLib.Program.App.ProgramDirectoryPath, "Audio", "ring.mp3");
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
        public static void StopPlayRing()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                mciSendString("stop ring", null, 0, 0);
                mciSendString("close ring", null, 0, 0);
            });
        }

        public static void Stop()
        {
            if (timer != null)
            {
                timer.Dispose();
            }
        }
    }

}
