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
    public static class BackgroundTask
    {
        private static Timer timer;

        private static ExtendedObservableCollection<WebPage> webPages;
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

        private static ExtendedObservableCollection<Script> scripts;
        public static ExtendedObservableCollection<Script> Scripts
        {
            get => scripts;
            private set
            {
                scripts = value;
                ScriptsChanged?.Invoke(null, Scripts);
            }
        }
        public static EventHandler<ExtendedObservableCollection<Script>> ScriptsChanged;

        private static ExtendedObservableCollection<Trigger> triggers;
        public static ExtendedObservableCollection<Trigger> Triggers
        {
            get => triggers;
            private set
            {
                triggers = value;
                TriggersChanged?.Invoke(null, Triggers);
            }
        }
        public static EventHandler<ExtendedObservableCollection<Trigger>> TriggersChanged;


        private static ExtendedDictionary<IDbModel, int> exceptionsCount = new ExtendedDictionary<IDbModel, int>();
        private static ExtendedDictionary<IDbModel, Exception> exceptions = new ExtendedDictionary<IDbModel, Exception>();


        public static async Task Load()
        {

            if (timer != null)
            {
                timer.Dispose();
            }

            WebPages = new ExtendedObservableCollection<WebPage>(await DbHelper.GetWebPagesAsync());
            Scripts = new ExtendedObservableCollection<Script>(await DbHelper.GetScriptsAsync());
            Triggers = new ExtendedObservableCollection<Trigger>(await DbHelper.GetTriggersAsync());
#if (DISABLED && DEBUG)
            return;
#endif
#if (CONTINUING && DEBUG)
            timer = new Timer(new TimerCallback(async p => await Do()), null, 0, 1000 * 10);
#else
            timer = new Timer(new TimerCallback(p => Do()), null, 0, 1000 * 60);
#endif
        }

        private static async Task Do()
        {
            foreach (var webPage in WebPages.Where(p => p.Enabled).ToArray())
            {
                try
                {
                    await CheckAndExcuteWebPageAsync(webPage);
                }
                catch (Exception ex)
                {
                    AddAndCheckExceptions(webPage, ex);
                }
            }

            foreach (var script in Scripts.Where(p => p.Enabled).ToArray())
            {
                try
                {
                    await CheckAndExcuteScriptAsync(script);
                }
                catch (Exception ex)
                {
                    AddAndCheckExceptions(script, ex);
                }
            }
        }

        public static async Task<bool> CheckAndExcuteWebPageAsync(WebPage webPage, bool force = false)
        {
            DateTime now = DateTime.Now;
            if (webPage.LatestContent == null || webPage.LatestContent.Length == 0)
            {
                byte[] content = await HtmlGetter.GetResponseBinaryAsync(webPage);
                webPage.LatestContent = content;
                webPage.LastCheckTime = now;

                await UpdateWebPageDbAndUI(webPage, null);
                return false;
            }
            else
#if (!CONTINUING || !DEBUG)
                    if (webPage.LastCheckTime + TimeSpan.FromMilliseconds(webPage.Interval) < now || force)
#endif
            {

                CompareResult result = await ComparerBase.CompareAsync(webPage);
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
                await UpdateWebPageDbAndUI(webPage, result);
                return result.Same == false;
            }
            return false;

            async static Task UpdateWebPageDbAndUI(WebPage page, CompareResult result)
            {
                await DbHelper.UpdateAsync(page);
                App.Current.Dispatcher.Invoke(() =>
                {
                    var mainWindow = App.Current.GetMainWindow();
                    if (mainWindow != null)
                    {
                        mainWindow.UpdateDisplay(page);
                    }
                });
            }
        }
        public static async Task CheckAndExcuteScriptAsync(Script script, bool force = false)
        {
            DateTime now = DateTime.Now;

#if (!CONTINUING || !DEBUG)
            if (script.LastExcuteTime + TimeSpan.FromMilliseconds(script.Interval) < now || force)

            {
#endif
                ScriptParser parser = new ScriptParser();
                await parser.ParseAsync(script.Code);
                script.LastExcuteTime = now;
                await DbHelper.UpdateAsync(script);
                App.Current.Dispatcher.Invoke(() =>
                {
                    var mainWindow = App.Current.GetMainWindow();
                    if (mainWindow != null)
                    {
                        mainWindow.UpdateDisplay(script);
                    }
                });
#if (!CONTINUING || !DEBUG)
            }
#endif
        }


        private static void AddAndCheckExceptions(IDbModel item, Exception ex)
        {
            if (exceptionsCount[item] == -1)
            {
                return;
            }

            exceptionsCount[item]++;
            exceptions[item] = ex;

            if (exceptionsCount[item] >= 5)
            {
                exceptionsCount[item] = -1;//暂时先不记录，等关闭窗口以后继续累计
                App.Current.Dispatcher.Invoke(() =>
                {
                    BackgroundTaskErrorNotificationWindow win = new BackgroundTaskErrorNotificationWindow(item, exceptions[item]);
                    win.Ignore += (p1, p2) => exceptionsCount[item] = -1;
                    win.IgnoreOnce += (p1, p2) => exceptionsCount[item] = 0;
                    win.PopUp();
                });
            }
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
