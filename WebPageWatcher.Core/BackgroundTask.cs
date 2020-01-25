//#define CONTINUING//忽略设置的对比间隔，每次循环进行一次对比
//#define DISABLED//禁止后台任务

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using WebPageWatcher.Data;
using WebPageWatcher.Web;
using FzLib.Basic.Collection;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace WebPageWatcher
{
    public static class BackgroundTask
    {
        private static Timer timer;
        #region Data
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
        public static event EventHandler<ExtendedObservableCollection<WebPage>> WebPagesChanged;

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
        public static event EventHandler<ExtendedObservableCollection<Script>> ScriptsChanged;

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

        #endregion
        public static async Task LoadAsync()
        {
            WebPages = new ExtendedObservableCollection<WebPage>(await DbHelper.GetWebPagesAsync());
            Scripts = new ExtendedObservableCollection<Script>(await DbHelper.GetScriptsAsync());
            Triggers = new ExtendedObservableCollection<Trigger>(await DbHelper.GetTriggersAsync());
        }

        public static void Start()
        {
            if (timer == null)
            {
                timer = new Timer(new TimerCallback(p => Do()));
            }
#if (DISABLED && DEBUG)
            return;
#endif
#if (CONTINUING && DEBUG)
            timer .Change( 0, 1000 * 10);
#else
            timer.Change(0, 1000 * 60);
#endif
        }

        private static async Task Do()
        {
            foreach (var webPage in WebPages.Where(p => p.Enabled).ToArray())
            {
                try
                {
                    bool succeed = await CheckAndExcuteWebPageAsync(webPage);
                    await CheckTriggerAsync(webPage, succeed ? TriggerEvent.ExcuteWebPageChanged : TriggerEvent.ExcuteWebPageNotChanged);
                    await DbHelper.AddLogAsync(new Log("log_webpageCompareSucceed", webPage.ToString() + " - " + succeed.ToString(), webPage.ID));
                }
                catch (Exception ex)
                {
                    await DbHelper.AddLogAsync(new Log("log_webpageCompareFailed", webPage.ToString(), webPage.ID));
                    await CheckTriggerAsync(webPage, TriggerEvent.ExcuteWebPageFailed);

                    AddAndCheckExceptions(webPage, ex);

                }
            }

            foreach (var script in Scripts.Where(p => p.Enabled).ToArray())
            {
                try
                {
                    await ExcuteScriptAsync(script);
                    await CheckTriggerAsync(script, TriggerEvent.ExcuteScriptSucceed);
                    await DbHelper.AddLogAsync(new Log("log_scriptExcuteSucceed", script.ToString()  , script.ID));
                }
                catch (Exception ex)
                {
                    await DbHelper.AddLogAsync(new Log("log_scriptExcuteFailed", script.ToString(), script.ID));

                    await CheckTriggerAsync(script, TriggerEvent.ExcuteScriptFailed);
                    AddAndCheckExceptions(script, ex);
                }
            }
        }

        private async static Task CheckTriggerAsync(IDbModel item, TriggerEvent triggerEvent)
        {
            Trigger trigger = Triggers.FirstOrDefault(p => p.Event == triggerEvent && p.Event_ID == item.ID);
            if (trigger != null)
            {
                try
                {
                    await ExcuteTrigger(trigger);
                    await DbHelper.AddLogAsync(new Log("log_triggerSucceed", item.ToString() + " - " + trigger.ToString(), trigger.ID));
                }
                catch (Exception ex)
                {
                    await DbHelper.AddLogAsync(new Log("log_triggerFailed", item.ToString() + " - " + trigger.ToString(), trigger.ID));
                }
            }
        }

        private static Trigger lastTrigger = null;
        private static IDbModel lastTriggerItem = null;
        public async static Task ExcuteTrigger(Trigger trigger)
        {
            trigger.LastExcuteTime = DateTime.Now;
            await DbHelper.UpdateAsync(trigger);
            switch (trigger.Operation)
            {
                case TriggerOperation.None:
                    break;
                case TriggerOperation.ExcuteWebPage:
                    WebPage webPage = WebPages.FirstOrDefault(p => p.ID == trigger.Operation_ID);
                    //类似下面的语句，是为了防止死循环，防止触发事件中再次触发相同事件
                    if (trigger == lastTrigger && lastTriggerItem == webPage)
                    {
                        return;
                    }
                    lastTriggerItem = webPage;
                    if (webPage != null)
                    {
                        await CheckAndExcuteWebPageAsync(webPage);
                    }
                    break;
                case TriggerOperation.ExcuteScript:
                    Script script = Scripts.FirstOrDefault(p => p.ID == trigger.Operation_ID);
                    if (trigger == lastTrigger && lastTriggerItem == script)
                    {
                        return;
                    }
                    lastTriggerItem = script;
                    if (script != null)
                    {
                        await ExcuteScriptAsync(script);
                    }
                    break;
                case TriggerOperation.ExcuteCommand:
                    await ExcuteCommandAsync(trigger);
                    break;
            }
            lastTrigger = trigger;
        }

        public static async Task<bool> CheckAndExcuteWebPageAsync(WebPage webPage, bool force = false)
        {
            DateTime now = DateTime.Now;
            byte[] latestContent = await webPage.GetLatestContentAsync();
            if (latestContent == null)
            {
                byte[] content = await HtmlGetter.GetResponseBinaryAsync(webPage);
                webPage.LastCheckTime = now;
                await UpdateWebPageDbAndUI(webPage, null);
                await UpdateWebPageUpdateDb(webPage, content);
                return false;
            }
            else
#if (!CONTINUING || !DEBUG)
                    if (webPage.LastCheckTime + TimeSpan.FromMilliseconds(webPage.Interval) < now || force)
#endif
            {

                CompareResult result = await ComparerBase.CompareAsync(webPage);
                if (result.Same == false)//网页发生变化
                {
                    WebPageChanged?.Invoke(null, new WebPageChangedEventArgs(webPage, result));
                    webPage.LastUpdateTime = now;
                    await UpdateWebPageUpdateDb(webPage, result.NewContent);
                }

                webPage.LastCheckTime = now;
                await UpdateWebPageDbAndUI(webPage, result);
                return result.Same == false;
            }
            return false;

            async static Task UpdateWebPageDbAndUI(WebPage page, CompareResult result)
            {
                await DbHelper.UpdateAsync(page);
                PropertyUpdated?.Invoke(null, new PropertyUpdatedEventArgs(page));
            }
            async static Task UpdateWebPageUpdateDb(WebPage webPage, byte[] content)
            {

                WebPageUpdate update = new WebPageUpdate(webPage.ID, content);
                await DbHelper.InsertAsync(update);
            }
        }
        public static async Task ExcuteScriptAsync(Script script, bool force = false)
        {
            DateTime now = DateTime.Now;

#if (!CONTINUING || !DEBUG)
            if (script.LastExcuteTime + TimeSpan.FromMilliseconds(script.Interval) < now || force)
            {
#endif
                ScriptParser parser = new ScriptParser(script);
                await parser.ParseAsync();
                script.LastExcuteTime = now;
                await DbHelper.UpdateAsync(script);
                PropertyUpdated?.Invoke(null, new PropertyUpdatedEventArgs(script));

#if (!CONTINUING || !DEBUG)
            }
#endif
        }
        public async static Task ExcuteCommandAsync(Trigger trigger)
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                process.ErrorDataReceived +=async (p1, p2) =>
                {
                    await DbHelper.AddLogAsync(new Log("log_triggerCommandError", p2.Data, trigger.ID));
                };
                process.OutputDataReceived +=async (p1, p2) =>
                {
                    await DbHelper.AddLogAsync(new Log("log_triggerCommandOutput", p2.Data, trigger.ID));
                };
                //startInfo.UseShellExecute = false;
                process.StartInfo = startInfo;
                process.Start();
                using StreamWriter sw = process.StandardInput;
                if (sw.BaseStream.CanWrite)
                {
                    foreach (var line in trigger.Operation_Command.Split(Environment.NewLine.ToCharArray()))
                    {
                        string str = line.Trim();
                        if (!string.IsNullOrWhiteSpace(str))
                        {
                            await sw.WriteLineAsync(str);
                        }
                    }
                    await sw.WriteLineAsync("exit");
                }
            }
            catch (Exception ex)
            {

            }
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
                ExceptionAlarm?.Invoke(null, new ExceptionAlarmEventArgs(item, exceptions[item], () => exceptionsCount[item] = 0, () => exceptionsCount[item] = -1));
            }
        }

        public static void Stop()
        {
            if (timer != null)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
        public static event EventHandler<WebPageChangedEventArgs> WebPageChanged;
        public static event EventHandler<PropertyUpdatedEventArgs> PropertyUpdated;
        public static event EventHandler<ExceptionAlarmEventArgs> ExceptionAlarm;
    }

    public class PropertyUpdatedEventArgs : EventArgs
    {
        public PropertyUpdatedEventArgs(IDbModel item)
        {
            Item = item;
        }

        public IDbModel Item { get; }
    }
    public class ExceptionAlarmEventArgs : EventArgs
    {
        public ExceptionAlarmEventArgs(IDbModel item, Exception exception, Action resetAction, Action disableAction)
        {
            Item = item;
            Exception = exception;
            ResetAction = resetAction;
            DisableAction = disableAction;
        }

        public IDbModel Item { get; }
        public Exception Exception { get; }
        public Action ResetAction { get; }
        public Action DisableAction { get; }
    }

    public class WebPageChangedEventArgs : EventArgs
    {
        public WebPageChangedEventArgs(WebPage webPage, CompareResult compareResult)
        {
            WebPage = webPage;
            CompareResult = compareResult;
        }

        public WebPage WebPage { get; }
        public CompareResult CompareResult { get; }
    }
}
