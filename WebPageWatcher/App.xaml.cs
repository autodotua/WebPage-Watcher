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
#if (!DEBUG)
            FzLib.Program.Runtime.UnhandledException.RegistAll();

            FzLib.Program.Runtime.SingleInstance singleInstance = new FzLib.Program.Runtime.SingleInstance(Assembly.GetExecutingAssembly().FullName);
            if (await singleInstance.CheckAndOpenWindow(this, this))
            {
                return;
            }
#endif

//            RequestParser.Parse(new WebPage(), @"POST https://activity.windows.com/v3/feeds/me/$batch HTTP/1.1
//Connection: Keep-Alive
//Date: Wed, 08 Jan 2020 11:12:35 GMT
//Content-Type: multipart/mixed; boundary=""Batch_1578481955""
//Accept - Encoding: gzip, deflate
//Authorization: EwAoA3l + BAAUvDBLmTCD21dGgKWS1TSPVrFoWQcAAd / IRAgzAKIG / dm7yEWcSW7GuP / rgvkt5yO3JyeosmcuuZc9vJ8ZsixIl + xUcTjTEflOpJO4aMugpbfCl02fAaky5wdUEtSq0Wm3t5qfrgsNzvjnv0ktcjwRNZ / kZkH9Vvwdbe7vR9 + szCd9wsMFQ5w2Gd2WhRaY2xVy3I3amqRO4 + wJXwWZpnzzOzsL + apzZ / NUGo8ckA1Zj2qjzvNAUBgaeYjZaM + aHqyK7vGxiTaSdeZ3dI8Dm5ko5H / LGjh8XKaQWLTijb + SxLuogj0 + Z0eVUfb3kYoGGOV1Gpoaqb / NZr32GLU0Swsql / zE110jujMdOl / 4wN + qFhdXzMesUmUDZgAACPKqPxhvIzcn + AHeNuLxiKa9hoti6M2m + v5MdaHq4dLD25uq6XCZy / tlBCC1aTjgkEzvaH8lJykwa7Y3m / 2 / wmpP4ZWle2o9Dk13yjUfftD4Or0p5EK7aQnKgW5QHY / GxXbPUFfCwequQa16sefSKjZ / MYHAGEoExObQM9EZqF7W11oC1mEmZzR2Va8d07cwusHC75ChxEg5hQPwxoo1x7AALMZqsQQUVMnqAO4JLO0o0hbblAqQL + czAF0f4iH22r12aDo + PUKc0RelNrtkI1ONEecsnybImdaL74bJ9wISs8KR5HBh + pjJMZKJbXIgoJhyPXNnKkAU76AdEpIdAfHw0VwQRIbdiYxDAznKV39yHkbVpqwMvKT285FA0HkbrcU6XZ6DwzTphonvCCfcqW / +OcOLdAiLTsoAOvV2I4dWPtWN6THBj5yeddOJHmm / JgDwxyFWoVbKsTdbk1 / AT0dnznLObvS25htQ43Esi79qBYnZpi5tvGIApGzfxbuTweDUNrZX9QGJD82xlqTX8Ef5n2Vae3bEJCtN52KozxH2 / Pb + T5ZMxBS3n0jf4 + X7gpZIxoAdc4qFBjYHIp3CSlUlx5dyjQ4wDuqB3Zl6adkt + ls + 3HzMdbf82YKOksQ8xLvapaFB6hS3j5l7nw42m8pKHhubUypYmcOtfoV6ewVOZ8UpAg ==
//User - Agent: SGPlatform 2.0
//OData - MaxVersion: 4.0
//OData - Version: 4.0
//X - AFS - ClientInfo: os = windows; osVer = 10.0.18363.535; lcid = zh - CN; deviceType = 9; deviceModel = LENOVO / 20B7S06T00;
//            X - AFS - CV: Fh0 / x18Y0KY4 + RJv.0
//Content - Length: 1543
//Host: activity.windows.com
//Cookie: MUID=29789A7647ED68063000968C43ED6EF2; SRCHD=AF=NOFORM; SRCHUID=V=2&GUID=30D45564826D4EE7B7B02D4C90D59B9A&dmnchg=1; PPLState=1; MUIDB=29789A7647ED68063000968C43ED6EF2; _UR=MC=1; ANON=A=BCBF10684E0A4FE8117296DAFFFFFFFF&E=175c&W=1; NAP=V=1.9&E=1702&C=S4xvuGhRSqelhDiv2Fs81elpEExfNP6cZWw5jcCYgd-1-yXgeyOChw&W=1; SerpPWA=reg=1; _tarLang=default=zh-Hans; imgv=lts=20191215; ULC=P=28B4|47:@25&H=28B4|47:25&T=28B4|47:25:2; ENSEARCH=BENVER=0; ABDEF=MRB=1578293493049&MRNB=0; SRCHUSR=DOB=20190216&T=1578293491000&POEX=W; KievRPSSecAuth=FABiARRaTOJILtFsMkpLVWSG6AN6C/svRwNmAAAEgAAACHaTkoEj00BLIAGeZuWRbbgYV%2BYrKlOxSkqqUo0oCBodVT%2BIj5ZLVlhc4WYp4MnQdwa8TsvD/NtLT2svdpN201S%2BqcJjKupjoIYZgPFulx5Tk0Znxvd6t9xK637DlCz6m/gBKk9TQSlhdVI9A7ioV7hAB9CmjNz7/xoOJoJTheO0YQoZzCJ5JP4tN912ro6WRB6MuIBSEg8NcM0HGJhvKV5gf01iYuc1Gj7b5O%2B7aw8bff8v0ijGEP0NpJ%2BKmZ1ykf2WDbB/keZ9OCB%2BN0t/hM1hA1bFIWaZpgt%2B1zRXOUqSaRp7vK85Zmi7QRmmyqLD9X0nnDUtSgnauMo00aGmuafo6QdjFUBhwcdlymeXMGmYPsG8Lj%2B6zX9PHN4a/IRv6RGrYlmaEXelMRwUAFDpinPcmnyYL9Kf1/Io8b7ng8S9; _U=1QLx_oI1SWuPAIFSngBdZjUdD0wKpwCP8S_BaIFU8wHeGE65KlqDkxo4det3KxIfanKdGkljO3e2PizO2Ncl0ldjzuBUmmpkZruiELdPbmk-rT0LMrtO_3RhEG_RYLYpKLaSmMMofmysmHFowQRhU4j-lUQI5AybPxUCKnOfqzCBvtKZo4PaPnHCozjrgmWOg9aJPiWkYTVKo4YclQYC__w; WLID=yP7Kh+S6Bn1B9W7K27Lkx3FdRP+CfUFBGWgyu7YvrZnle7kPBaHNGXaFfcGAlcjeXlVhGInTy4sdmdgJvWu3PkY8mvSI/hp0V2wUAKEJkzE=; WLS=C=1d66d35e1b476e9f&N=%e9%9c%87; _EDGE_S=SID=178E84D3AFC067890F198A9FAE0B66B8; SRCHHPGUSR=CW=1204&CH=564&DPR=1.5625&UTC=480&WTS=63713981130; ipv6=hit=1578532442365&t=4; _SS=SID=178E84D3AFC067890F198A9FAE0B66B8&bIm=246&HV=1578528852


//--Batch_1578481955
//Content - Type: application / http
//Content - Transfer - Encoding: binary

//    POST https://activity.windows.com/v3/feeds/me/activities HTTP/1.1
//            Content - Length: 1307
//Content - Type: application / json

//  [
//   {
//                ""__schemaVersion"" : 1,
//      ""activityId"" : ""1AE36F68-A079-20C5-B604-FF031E01EA32"",
//      ""appActivityId"" : ""ECB32AF3-1440-4086-94E3-5311F97F89C4"",
//      ""appId"" : {
//                    ""alternateId"" : """",
//         ""packageId"" : ""C:\\Users\\admin\\AppData\\Local\\Programs\\Fiddler\\Fiddler.exe"",
//         ""windows_win32"" : ""C:\\Users\\admin\\AppData\\Local\\Programs\\Fiddler\\Fiddler.exe""
//      },
//      ""applicationId"" : [
//         {
//            ""application"" : ""C:\\Users\\admin\\AppData\\Local\\Programs\\Fiddler\\Fiddler.exe"",
//            ""platform"" : ""windows_win32""
//         },
//         {
//            ""application"" : ""C:\\Users\\admin\\AppData\\Local\\Programs\\Fiddler\\Fiddler.exe"",
//            ""platform"" : ""packageId""
//         },
//         {
//            ""application"" : """",
//            ""platform"" : ""alternateId""
//         }
//      ],
//      ""lastModifiedOnClient"" : ""2020-01-08T11:12:34.000Z"",
//      ""payload"" : ""eyJkaXNwbGF5VGV4dCI6IkZpZGRsZXIgNCIsImFjdGl2YXRpb25VcmkiOiJtcy1zaGVsbGFjdGl2aXR5OiIsImFwcERpc3BsYXlOYW1lIjoiRmlkZGxlciA0IiwiYmFja2dyb3VuZENvbG9yIjoiYmxhY2sifQ=="",
//      ""platformDeviceId"" : ""afQzg+/uiHOaR9OcozjVMwF2NifDUH/p1L8fanpw8UU="",
//      ""priority"" : 3,
//      ""read"" : false,
//      ""startTime"" : ""2020-01-08T11:12:34.000Z"",
//      ""type"" : 5,
//      ""userActionState"" : 0
//   }
//]

//--Batch_1578481955--");
            await BackgroundTask.Load();

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
                        theme.BaseTheme = MaterialDesignThemes.Wpf.BaseTheme.Light;
                    }
                    else
                    {
                        theme.BaseTheme = MaterialDesignThemes.Wpf.BaseTheme.Dark;
                    }
                    break;
                case -1:
                    theme.BaseTheme = MaterialDesignThemes.Wpf.BaseTheme.Dark;
                    break;
                case 1:
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

        public MainWindow GetMainWindow(bool notUIThread = false)
        {
            MainWindow mainWindow = SingleObject;
            if (mainWindow != null && mainWindow.IsLoaded && !mainWindow.IsClosed)
            {
                return mainWindow;
            }
            return null;
        }
    }
}
