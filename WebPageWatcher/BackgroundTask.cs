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
        private static List<WebPage> webPages;

        private static ExtendedDictionary<WebPage, int> exceptions = new ExtendedDictionary<WebPage, int>();

        public static async Task Load()
        {
            if (timer != null)
            {
                timer.Dispose();
            }

            webPages = (await DbHelper.GetWebPagesAsync()).ToList();
#if DEBUG
            timer = new Timer(new TimerCallback(p => Do()), null, 0, 1000 * 10);
#else
            timer = new Timer(new TimerCallback(p => Do()), null, 0, 1000 * 60);
#endif
        }

        private static void Do()
        {
            DateTime now = DateTime.Now;
            foreach (var page in webPages)
            {
                try
                {
                    if (page.LatestDocument == null)
                    {
                        page.LatestDocument = HtmlGetter.GetResponse(page);
                        page.LastCheckTime = now;

                        UpdateDbAndUI(page, null);
                    }
                    else if (page.LastCheckTime + TimeSpan.FromMilliseconds(page.Interval) < now)
                    {
                        Debug.WriteLine("比较了");
                        page.LastCheckTime = now;

                        CompareResult result = ComparerBase.Compare(page);
                        if (result.Same == false)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                WebPageChangedNotificationWindow win = new WebPageChangedNotificationWindow(page, result);
                                win.PopUp();
                            });
                            page.LastUpdateTime = now;
                            page.LatestDocument = result.NewContent;

                        }

                        UpdateDbAndUI(page, result);
                    }
                }
                catch (Exception ex)
                {
                    exceptions[page]++;
                }

            }
            CheckExceptions();
        }

        private static void UpdateDbAndUI(WebPage page, CompareResult result)
        {
            DbHelper.UpdateAsync(page).Wait();
            App.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = (App.Current as App).SingleObject;
                if (mainWindow != null && mainWindow.IsLoaded && !mainWindow.IsClosed)
                {
                    mainWindow.UpdateDisplay(page, result != null && result.Same == false);
                }
            });
        }

        private static void CheckExceptions()
        {

        }

    }

}
