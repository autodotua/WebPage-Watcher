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
using WebPageWatcher.Web;

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
            Test();
#if (!DEBUG)
            FzLib.Program.Runtime.UnhandledException.RegistAll();

            FzLib.Program.Runtime.SingleInstance singleInstance = new FzLib.Program.Runtime.SingleInstance(Assembly.GetExecutingAssembly().FullName);
            if (await singleInstance.CheckAndOpenWindow(this, this))
            {
                return;
            }
#endif
            await BackgroundTask.Load();
            BackgroundTaskHelper.Initialize();

            FzLib.Program.App.SetWorkingDirectoryToAppPath();

            InitializeTheme();
            SetTheme();

            Tray();

            SetCulture();

            if (!(e.Args.Length == 1 && e.Args[0] == "startup"))
            {
                CreateMainWindow();
            }

        }

        public void CreateMainWindow(IDbModel para = null)
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

        private void InitializeTheme()
        {
            var v = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", "1");
            if (v == null || v.ToString() == "1")
            {
                AppsUseLightTheme = true;
            }
            v = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", "0");
            if (v == null || v.ToString() == "1")
            {
                SystemUsesLightTheme = true;
            }
        }
        public void SetTheme()
        {
            MaterialDesignThemes.Wpf.BundledTheme theme = new MaterialDesignThemes.Wpf.BundledTheme();
            theme.PrimaryColor = MaterialDesignColors.PrimaryColor.Purple;
            theme.SecondaryColor = MaterialDesignColors.SecondaryColor.Lime;
            switch (Config.Instance.Theme)
            {
                case 0:
                    if (AppsUseLightTheme)
                    {
                        goto l;
                    }
                    goto d;
                case -1:
                d:
                    theme.BaseTheme = MaterialDesignThemes.Wpf.BaseTheme.Dark;
                    break;
                case 1:
                l:
                    theme.BaseTheme = MaterialDesignThemes.Wpf.BaseTheme.Light;
                    break;
            }

            Resources.MergedDictionaries.Add(theme);

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
                icon = WebPageWatcher.Properties.Resources.tray_icon_dark;
            }
            else
            {
                icon = WebPageWatcher.Properties.Resources.tray_icon;
            }
            notifyIcon = new FzLib.Program.Runtime.TrayIcon(icon, "网页内容变动提醒");
            notifyIcon.ClickToOpenOrHideWindow(this);
            notifyIcon.AddContextMenuItem("退出", () =>
            {
                Application.Current.Shutdown();
            });
            notifyIcon.ReShowWhenDisplayChanged = true;
            notifyIcon.Show();

        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            DbHelper.Dispose();
            BackgroundTask.Stop();
            notifyIcon.Dispose();
        }

        public void SetCulture()
        {
            string culture = Config.Instance.Language;

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

        public MainWindow GetMainWindow()
        {
            MainWindow mainWindow = SingleObject;
            if (mainWindow != null && mainWindow.IsLoaded && !mainWindow.IsClosed)
            {
                return mainWindow;
            }
            return null;
        }

        private void Test()
        {
            string a = @"<div class=""word"">
    <div class=""word-cont"">
		<h1 class=""keyword"" tip=""音节划分：dis·crep·an·cy"">discrepancy</h1>
					<a class=""wordbook"" href=""javascript:void(0);"" wid=""67412545"" act=""addword"" title=""添加该生词到生词本中"">　</a>
											<span class=""level-title"" level=""海词3星常用词汇，属常用6000词。"">	常用词汇				</span>
						<a href=""javascript:void(0);"" class=""level_3"" level=""海词3星常用词汇，属常用6000词。"">　</a>
			</div>

		<div class=""phonetic"">
					<span>英
					        <bdo lang=""EN-US"">[dɪs'krepənsi]</bdo>
	    								<i class=""sound fsound"" naudio=""fbTd30O58f9563892b9d21d019d10c1ef13e59eb.mp3?t=discrepancy"" title=""女生版发音"">　</i>
										<i class=""sound"" naudio=""mbTd30O538d5d828f1b418df0fadc882c3d34c20.mp3?t=discrepancy"" title=""男生版发音"">　</i>
					    </span>
		
					<span>美
							<bdo lang=""EN-US"">[dɪs'krepənsi]</bdo>
	    		    		    		<i class=""sound fsound"" naudio=""fuTd30O5bc413ae1f0a1329501ebb085e3a34b7c.mp3?t=discrepancy"" title=""女生版发音"">　</i>
	    		    		    		<i class=""sound"" naudio=""muTd30O57bb37a8dc0f64e79083e6059c9642d49.mp3?t=discrepancy"" title=""男生版发音"">　</i>
	    				</span>
			</div>
	
			<div class=""basic clearfix"">
			<ul class=""dict-basic-ul"">
									<li>
																		<span>n.</span>
																								<strong>差异；不一致；分歧</strong>
																</li>
				                <li style=""padding-top: 25px;"">
                												<script type=""text/javascript"">
							if(less1280){
								document.write(""<div id='div-gpt-ad-1456897273340-5' style='width:270px; height:100px;'><script type='text/javascript'>googletag.cmd.push(function() { googletag.display('div-gpt-ad-1456897273340-5'); });<\/script><\/div>"");	
							}else{
								document.write(""<div id='div-gpt-ad-1456897273340-4' style='width:470px; height:90px;'><script type='text/javascript'>googletag.cmd.push(function() { googletag.display('div-gpt-ad-1456897273340-4'); });<\/script><\/div>"");
							}
						</script><div id=""div-gpt-ad-1456897273340-4"" style=""width: 470px; height: 90px;""><script type=""text/javascript"">googletag.cmd.push(function() { googletag.display('div-gpt-ad-1456897273340-4'); });</script><div id=""google_ads_iframe_/146434140/search_in_w_0__container__"" style=""border: 0pt none; width: 470px; height: 90px;""></div></div>
														</li>
			</ul>
				        	<div class=""dict-chart"" id=""dict-chart-basic"" data=""%7B%221%22%3A%7B%22percent%22%3A81%2C%22sense%22%3A%22%5Cu5dee%5Cu5f02%22%7D%2C%222%22%3A%7B%22percent%22%3A14%2C%22sense%22%3A%22%5Cu4e0d%5Cu4e00%5Cu81f4%22%7D%2C%223%22%3A%7B%22percent%22%3A5%2C%22sense%22%3A%22%5Cu5206%5Cu6b67%22%7D%7D"" data-highcharts-chart=""0""><div class=""highcharts-container"" id=""highcharts-0"" style=""position: relative; overflow: hidden; width: 280px; height: 200px; text-align: left; line-height: normal; z-index: 0; -webkit-tap-highlight-color: rgba(0, 0, 0, 0); font-family: &quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif; font-size: 12px;""><svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1"" width=""280"" height=""200""><desc>Created with Highcharts 3.0.2</desc><defs><clipPath id=""highcharts-1""><rect fill=""none"" x=""0"" y=""0"" width=""280"" height=""145""></rect></clipPath></defs><rect rx=""5"" ry=""5"" fill=""#FFFFFF"" x=""0"" y=""0"" width=""280"" height=""200""></rect><g class=""highcharts-series-group"" zIndex=""3""><g class=""highcharts-series highcharts-tracker"" visibility=""visible"" zIndex=""0.1"" transform=""translate(0,35) scale(1 1)"" style=""cursor:pointer;""><path fill=""none"" d=""M 82.07368091356298 108.48339544790976 L 92.86249706364204 104.54771157079462 L 98.65131321372108 100.6120276936795"" stroke=""#808080"" stroke-width=""1"" visibility=""visible""></path><path fill=""none"" d=""M 191.6450944763785 28.679512082934025 L 181.5432872680246 33.47237794886311 L 176.4414800596707 38.26524381479221"" stroke=""#808080"" stroke-width=""1"" visibility=""visible""></path><path fill=""none"" d=""M 208.2109954525741 62.48151439115391 L 196.2972928249488 63.57728625462145 L 189.3835901973235 64.67305811808899"" stroke=""#808080"" stroke-width=""1"" visibility=""visible""></path><path fill=""#19B29F"" d=""M 190 72.5 A 50 50 0 1 1 158.34205095434262 25.985817573687285 L 140 72.5 A 0 0 0 1 0 140 72.5 Z"" stroke=""#FFFFFF"" stroke-width=""1"" stroke-linejoin=""round"" transform=""translate(0,0)""></path><path fill=""#53C8BA"" d=""M 158.38855595799188 26.0041828786739 A 50 50 0 0 1 187.53694756866645 57.000367234871156 L 140 72.5 A 0 0 0 0 0 140 72.5 Z"" stroke=""#FFFFFF"" stroke-width=""1"" stroke-linejoin=""round"" transform=""translate(0,0)""></path><path fill=""#62DDCF"" d=""M 187.5524234303765 57.04791192433275 A 50 50 0 0 1 189.99996487617636 72.44073465489818 L 140 72.5 A 0 0 0 0 0 140 72.5 Z"" stroke=""#FFFFFF"" stroke-width=""1"" stroke-linejoin=""round"" transform=""translate(0,0)""></path></g><g class=""highcharts-markers"" visibility=""visible"" zIndex=""0.1"" transform=""translate(0,35) scale(1 1)""></g></g><text x=""0"" y=""15"" style=""font-family:&quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif;font-size:12px;color:#808080;fill:#808080;"" text-anchor=""start"" class=""highcharts-title"" zIndex=""4""><tspan x=""0"">释义常用度分布图</tspan></text><g class=""highcharts-data-labels highcharts-tracker"" visibility=""visible"" zIndex=""6"" transform=""translate(0,35) scale(1 1)"" style=""cursor:pointer;""><g zIndex=""1"" style=""cursor:default;"" transform=""translate(43,98)"" visibility=""visible""></g><g zIndex=""1"" style=""cursor:default;"" transform=""translate(188,19)"" visibility=""visible""></g><g zIndex=""1"" style=""cursor:default;"" transform=""translate(204,52)"" visibility=""visible""></g></g><g class=""highcharts-legend"" zIndex=""7""><rect rx=""5"" ry=""5"" fill=""none"" x=""0.5"" y=""0.5"" width=""7"" height=""7"" stroke=""#909090"" stroke-width=""1"" visibility=""hidden""></rect><g zIndex=""1""><g></g></g></g><g class=""highcharts-tooltip"" zIndex=""8"" style=""cursor:default;padding:0;white-space:nowrap;"" visibility=""hidden"" transform=""translate(148,68)"" opacity=""0""><rect rx=""3"" ry=""3"" fill=""none"" x=""0.5"" y=""0.5"" width=""75"" height=""26.578125"" fill-opacity=""0.85"" isShadow=""true"" stroke=""black"" stroke-opacity=""0.049999999999999996"" stroke-width=""5"" transform=""translate(1, 1)""></rect><rect rx=""3"" ry=""3"" fill=""none"" x=""0.5"" y=""0.5"" width=""75"" height=""26.578125"" fill-opacity=""0.85"" isShadow=""true"" stroke=""black"" stroke-opacity=""0.09999999999999999"" stroke-width=""3"" transform=""translate(1, 1)""></rect><rect rx=""3"" ry=""3"" fill=""none"" x=""0.5"" y=""0.5"" width=""75"" height=""26.578125"" fill-opacity=""0.85"" isShadow=""true"" stroke=""black"" stroke-opacity=""0.15"" stroke-width=""1"" transform=""translate(1, 1)""></rect><rect rx=""3"" ry=""3"" fill=""rgb(255,255,255)"" x=""0.5"" y=""0.5"" width=""75"" height=""26.578125"" fill-opacity=""0.85"" stroke=""#19B29F"" stroke-width=""1"" anchorX=""87.51731330476946"" anchorY=""12.0146484375""></rect><text x=""4"" y=""17"" style=""font-family:&quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif;font-size:12px;color:#333333;line-height:20px;fill:#333333;"" zIndex=""1""><tspan x=""4"">常用度:81 %</tspan></text></g><text x=""278"" y=""196"" style=""font-family:&quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif;font-size:12px;cursor:default;color:#808080;fill:#808080;"" text-anchor=""end"" zIndex=""8""><tspan x=""278"">海词统计</tspan></text></svg><div class=""highcharts-data-labels"" style=""position: absolute; left: 0px; top: 35px;""><div class=""null"" style=""position: absolute; left: 43px; top: 98px; visibility: visible;""><span style=""position: absolute; white-space: nowrap; font-family: &quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif; font-size: 12px; color: rgb(102, 102, 102); line-height: 14px; margin-left: 0px; margin-top: 0px; left: 3px; top: 3px;"" zindex=""1""><font title=""差异"">差异</font></span></div><div class=""null"" style=""position: absolute; left: 188px; top: 19px; visibility: visible;""><span style=""position: absolute; white-space: nowrap; font-family: &quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif; font-size: 12px; color: rgb(102, 102, 102); line-height: 14px; margin-left: 0px; margin-top: 0px; left: 3px; top: 3px;"" zindex=""1""><font title=""不一致"">不一致</font></span></div><div class=""null"" style=""position: absolute; left: 204px; top: 52px; visibility: visible;""><span style=""position: absolute; white-space: nowrap; font-family: &quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif; font-size: 12px; color: rgb(102, 102, 102); line-height: 14px; margin-left: 0px; margin-top: 0px; left: 3px; top: 3px;"" zindex=""1""><font title=""分歧"">分歧</font></span></div></div></div></div>
	        		</div>

	
			<div class=""shape"">
							<label>名词复数:</label>
				<a href=""http://dict.cn/discrepancies"">
				discrepancies				</a>
					</div>
	</div>";
            string b = @"<div class=""word"">
    <div class=""word-cont"">
		<h1 class=""keyword"" tip=""音节划分：dis·crep·an·cy"">discrepancy</h1>
					<a class=""wordbook"" href=""javascript:void(0);"" wid=""67412545"" act=""addword"" title=""添加该生词到生词本中"">　</a>
											<span class=""level-title"" level=""海词3星常用词汇，属常用6000词。"">	常用词汇				</span>
						<a href=""javascript:void(0);"" class=""level_3"" level=""海词5星常用词汇，属常用6000词。"">　</a>
			</div>

		<div class=""phonetic"">
					<span>英
					        <bdo lang=""EN-US"">[dɪs'krepənsi]</bdo>
	    								<i class=""sound fsound"" naudio=""fbTd30O58f9563892b9d21d019d10c1ef13e59eb.mp3?t=discrepancy"" title=""女生版发音"">　</i>
										<i class=""sound"" naudio=""mbTd30O538d5d828f1b418df0fadc882c3d34c20.mp3?t=discrepancy"" title=""男生版发音"">　</i>
					    </span>
		
					<span>美
							<bdo lang=""EN-US"">[dɪs'krepənsi]</bdo>
	    		    		    		<i class=""sound fsound"" naudio=""fuTd30O5bc413ae1f0a1329501ebb085e3a34b7c.mp3?t=discrepancy"" title=""女生2版发音"">　</i>
	    		    		    		<i class=""sound"" naudio=""muTd30O57bb37a8dc0f64e79083e6059c9642d49.mp3?t=discrepancy"" title=""男生2版发音"">　</i>
	    				</span>
			</div>
	
			<div class=""basic clearfix"">
			<ul class=""dict-basic-ul"">
									<li>
																		<span>n.</span>
																								<strong>差异；不一致；分歧</strong>
																</li>
				                <li style=""padding-top: 25px;"">
                												<script type=""text/javascript"">
							if(less1280){
								document.write(""<div id='div-gpt-ad-1456897273340-5' style='width:270px; height:100px;'><script type='text/javascript'>googletag.cmd.push(function() { googletag.display('div-gpt-ad-1456897273340-5'); });<\/script><\/div>"");	
							}else{
								document.write(""<div id='div-gpt-ad-1456897273340-4' style='width:470px; height:90px;'><script type='text/javascript'>googletag.cmd.push(function() { googletag.display('div-gpt-ad-1456897273340-4'); });<\/script><\/div>"");
							}
						</script><div id=""div-gpt-ad-1456897273340-4"" style=""width: 470px; height: 90px;""><script type=""text/javascript"">googletag.cmd.push(function() { googletag.display('div-gpt-ad-1456897273340-4'); });</script><div id=""google_ads_iframe_/146434140/search_in_w_0__container__"" style=""border: 0pt none; width: 470px; height: 90px;""></div></div>
														</li>
			</ul>
				        	<div class=""dict-chart"" id=""dict-chart-basic"" data=""%7B%221%22%3A%7B%22percent%22%3A81%2C%22sense%22%3A%22%5Cu5dee%5Cu5f02%22%7D%2C%222%22%3A%7B%22percent%22%3A14%2C%22sense%22%3A%22%5Cu4e0d%5Cu4e00%5Cu81f4%22%7D%2C%223%22%3A%7B%22percent%22%3A5%2C%22sense%22%3A%22%5Cu5206%5Cu6b67%22%7D%7D"" data-highcharts-chart=""0""><div class=""highcharts-container"" id=""highcharts-0"" style=""position: relative; overflow: hidden; width: 280px; height: 200px; text-align: left; line-height: normal; z-index: 0; -webkit-tap-highlight-color: rgba(0, 0, 0, 0); font-family: &quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif; font-size: 12px;""><svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1"" width=""280"" height=""200""><desc>Created with Highcharts 3.0.2</desc><defs><clipPath id=""highcharts-1""><rect fill=""none"" x=""0"" y=""0"" width=""280"" height=""145""></rect></clipPath></defs><rect rx=""5"" ry=""5"" fill=""#FFFFFF"" x=""0"" y=""0"" width=""280"" height=""200""></rect><g class=""highcharts-series-group"" zIndex=""3""><g class=""highcharts-series highcharts-tracker"" visibility=""visible"" zIndex=""0.1"" transform=""translate(0,35) scale(1 1)"" style=""cursor:pointer;""><path fill=""none"" d=""M 82.07368091356298 108.48339544790976 L 92.86249706364204 104.54771157079462 L 98.65131321372108 100.6120276936795"" stroke=""#808080"" stroke-width=""1"" visibility=""visible""></path><path fill=""none"" d=""M 191.6450944763785 28.679512082934025 L 181.5432872680246 33.47237794886311 L 176.4414800596707 38.26524381479221"" stroke=""#808080"" stroke-width=""1"" visibility=""visible""></path><path fill=""none"" d=""M 208.2109954525741 62.48151439115391 L 196.2972928249488 63.57728625462145 L 189.3835901973235 64.67305811808899"" stroke=""#808080"" stroke-width=""1"" visibility=""visible""></path><path fill=""#19B29F"" d=""M 190 72.5 A 50 50 0 1 1 158.34205095434262 25.985817573687285 L 140 72.5 A 0 0 0 1 0 140 72.5 Z"" stroke=""#FFFFFF"" stroke-width=""1"" stroke-linejoin=""round"" transform=""translate(0,0)""></path><path fill=""#53C8BA"" d=""M 158.38855595799188 26.0041828786739 A 50 50 0 0 1 187.53694756866645 57.000367234871156 L 140 72.5 A 0 0 0 0 0 140 72.5 Z"" stroke=""#FFFFFF"" stroke-width=""1"" stroke-linejoin=""round"" transform=""translate(0,0)""></path><path fill=""#62DDCF"" d=""M 187.5524234303765 57.04791192433275 A 50 50 0 0 1 189.99996487617636 72.44073465489818 L 140 72.5 A 0 0 0 0 0 140 72.5 Z"" stroke=""#FFFFFF"" stroke-width=""1"" stroke-linejoin=""round"" transform=""translate(0,0)""></path></g><g class=""highcharts-markers"" visibility=""visible"" zIndex=""0.1"" transform=""translate(0,35) scale(1 1)""></g></g><text x=""0"" y=""15"" style=""font-family:&quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif;font-size:12px;color:#808080;fill:#808080;"" text-anchor=""start"" class=""highcharts-title"" zIndex=""4""><tspan x=""0"">释义常用度分布图</tspan></text><g class=""highcharts-data-labels highcharts-tracker"" visibility=""visible"" zIndex=""6"" transform=""translate(0,35) scale(1 1)"" style=""cursor:pointer;""><g zIndex=""1"" style=""cursor:default;"" transform=""translate(43,98)"" visibility=""visible""></g><g zIndex=""1"" style=""cursor:default;"" transform=""translate(188,19)"" visibility=""visible""></g><g zIndex=""1"" style=""cursor:default;"" transform=""translate(204,52)"" visibility=""visible""></g></g><g class=""highcharts-legend"" zIndex=""7""><rect rx=""5"" ry=""5"" fill=""none"" x=""0.5"" y=""0.5"" width=""7"" height=""7"" stroke=""#909090"" stroke-width=""1"" visibility=""hidden""></rect><g zIndex=""1""><g></g></g></g><g class=""highcharts-tooltip"" zIndex=""8"" style=""cursor:default;padding:0;white-space:nowrap;"" visibility=""hidden"" transform=""translate(148,68)"" opacity=""0""><rect rx=""3"" ry=""3"" fill=""none"" x=""0.5"" y=""0.5"" width=""75"" height=""26.578125"" fill-opacity=""0.85"" isShadow=""true"" stroke=""black"" stroke-opacity=""0.049999999999999996"" stroke-width=""5"" transform=""translate(1, 1)""></rect><rect rx=""3"" ry=""3"" fill=""none"" x=""0.5"" y=""0.5"" width=""75"" height=""26.578125"" fill-opacity=""0.85"" isShadow=""true"" stroke=""black"" stroke-opacity=""0.09999999999999999"" stroke-width=""3"" transform=""translate(1, 1)""></rect><rect rx=""3"" ry=""3"" fill=""none"" x=""0.5"" y=""0.5"" width=""75"" height=""26.578125"" fill-opacity=""0.85"" isShadow=""true"" stroke=""black"" stroke-opacity=""0.15"" stroke-width=""1"" transform=""translate(1, 1)""></rect><rect rx=""3"" ry=""3"" fill=""rgb(255,255,255)"" x=""0.5"" y=""0.5"" width=""75"" height=""26.578125"" fill-opacity=""0.85"" stroke=""#19B29F"" stroke-width=""1"" anchorX=""87.51731330476946"" anchorY=""12.0146484375""></rect><text x=""4"" y=""17"" style=""font-family:&quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif;font-size:12px;color:#333333;line-height:20px;fill:#333333;"" zIndex=""1""><tspan x=""4"">常用度:81 %</tspan></text></g><text x=""278"" y=""196"" style=""font-family:&quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif;font-size:12px;cursor:default;color:#808080;fill:#808080;"" text-anchor=""end"" zIndex=""8""><tspan x=""278"">海词统计</tspan></text></svg><div class=""highcharts-data-labels"" style=""position: absolute; left: 0px; top: 35px;""><div class=""null"" style=""position: absolute; left: 43px; top: 98px; visibility: visible;""><span style=""position: absolute; white-space: nowrap; font-family: &quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif; font-size: 12px; color: rgb(102, 102, 102); line-height: 14px; margin-left: 0px; margin-top: 0px; left: 3px; top: 3px;"" zindex=""1""><font title=""差异"">差异</font></span></div><div class=""null"" style=""position: absolute; left: 188px; top: 19px; visibility: visible;""><span style=""position: absolute; white-space: nowrap; font-family: &quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif; font-size: 12px; color: rgb(102, 102, 102); line-height: 14px; margin-left: 0px; margin-top: 0px; left: 3px; top: 3px;"" zindex=""1""><font title=""不一致"">不一致</font></span></div><div class=""null"" style=""position: absolute; left: 204px; top: 52px; visibility: visible;""><span style=""position: absolute; white-space: nowrap; font-family: &quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Verdana, Arial, Helvetica, sans-serif; font-size: 12px; color: rgb(102, 102, 102); line-height: 14px; margin-left: 0px; margin-top: 0px; left: 3px; top: 3px;"" zindex=""1""><font title=""分歧"">分歧</font></span></div></div></div></div>
	        		</div>

	
			<div class=""shape"">
							<label>名词复数:</label>
				<a href=""http://dict.cn/discrepancies"">
				discrepancies				</a>
					</div>
	</div>";
            //CompareResult compareResult = new CompareResult(new WebPage() { }, null, null, null, null, null, a,b);


        }
    }
}
