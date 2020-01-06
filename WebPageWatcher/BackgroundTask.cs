﻿#define TEST

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
#if (TEST && DEBUG)
            timer = new Timer(new TimerCallback(p => Do()), null, 0, 1000 * 10);
#else
            timer = new Timer(new TimerCallback(p => Do()), null, 0, 1000 * 60);
#endif
        }

        private static void Do()
        {
            DateTime now = DateTime.Now;
            foreach (var webPage in WebPages.ToArray())
            {
                try
                {
                    if (webPage.LatestDocument == null)
                    {
                        webPage.LatestDocument = HtmlGetter.GetResponse(webPage);
                        webPage.LastCheckTime = now;

                        UpdateDbAndUI(webPage, null).Wait();
                    }
                    else
#if (!TEST || !DEBUG)
                    if (webPage.LastCheckTime + TimeSpan.FromMilliseconds(webPage.Interval) < now)
#endif
                    {
                        Debug.WriteLine("比较了");
                        webPage.LastCheckTime = now;

                        CompareResult result = ComparerBase.Compare(webPage);
                        if (result.Same == false)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                WebPageChangedNotificationWindow win = new WebPageChangedNotificationWindow(webPage, result);
                                win.PopUp();
                            });
                            webPage.LastUpdateTime = now;
                            webPage.LatestDocument = result.NewContent;

                        }

                        UpdateDbAndUI(webPage, result).Wait();
                    }
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

        private async static Task UpdateDbAndUI(WebPage page, CompareResult result)
        {
            await DbHelper.UpdateAsync(page);
            App.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = App.Current.GetMainWindow(true);
                if (mainWindow != null )
                {
                    mainWindow.UpdateDisplay(page);
                }
            });
        }

        private static void CheckExceptions(WebPage webPage)
        {
            if (exceptionsCount[webPage]>=5)
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

    }

}
