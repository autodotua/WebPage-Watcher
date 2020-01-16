using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebPageWatcher.Data;

namespace WebPageWatcher.Web
{
    public class RequestParser
    {
        public WebPage WebPage { get; }

        /*POST https://www.bing.com/fd/ls/lsp.aspx HTTP/1.1
Host: www.bing.com
Connection: keep-alive
Content-Length: 585
Origin: https://www.bing.com
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36
DNT: 1
Content-Type: text/xml
Accept: 
Sec-Fetch-Site: same-origin
Sec-Fetch-Mode: cors
Referer: https://www.bing.com/?mkt=zh-CN&mkt=zh-CN
Accept-Encoding: gzip, deflate, br
Accept-Language: zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7
Cookie: MUID=29789A7647ED68063000968C43ED6EF2; SRCHD=AF=NOFORM; SRCHUID=V=2&GUID=30D45564826D4EE7B7B02D4C90D59B9A&dmnchg=1; PPLState=1; MUIDB=29789A7647ED68063000968C43ED6EF2; _UR=MC=1; ANON=A=BCBF10684E0A4FE8117296DAFFFFFFFF&E=175c&W=1; NAP=V=1.9&E=1702&C=S4xvuGhRSqelhDiv2Fs81elpEExfNP6cZWw5jcCYgd-1-yXgeyOChw&W=1; SerpPWA=reg=1; _tarLang=default=zh-Hans; imgv=lts=20191215; ULC=P=28B4|47:@25&H=28B4|47:25&T=28B4|47:25:2; ENSEARCH=BENVER=0; ABDEF=MRB=1578293493049&MRNB=0; SRCHUSR=DOB=20190216&T=1578293491000&POEX=W; KievRPSSecAuth=FABiARRaTOJILtFsMkpLVWSG6AN6C/svRwNmAAAEgAAACHaTkoEj00BLIAGeZuWRbbgYV%2BYrKlOxSkqqUo0oCBodVT%2BIj5ZLVlhc4WYp4MnQdwa8TsvD/NtLT2svdpN201S%2BqcJjKupjoIYZgPFulx5Tk0Znxvd6t9xK637DlCz6m/gBKk9TQSlhdVI9A7ioV7hAB9CmjNz7/xoOJoJTheO0YQoZzCJ5JP4tN912ro6WRB6MuIBSEg8NcM0HGJhvKV5gf01iYuc1Gj7b5O%2B7aw8bff8v0ijGEP0NpJ%2BKmZ1ykf2WDbB/keZ9OCB%2BN0t/hM1hA1bFIWaZpgt%2B1zRXOUqSaRp7vK85Zmi7QRmmyqLD9X0nnDUtSgnauMo00aGmuafo6QdjFUBhwcdlymeXMGmYPsG8Lj%2B6zX9PHN4a/IRv6RGrYlmaEXelMRwUAFDpinPcmnyYL9Kf1/Io8b7ng8S9; _U=1QLx_oI1SWuPAIFSngBdZjUdD0wKpwCP8S_BaIFU8wHeGE65KlqDkxo4det3KxIfanKdGkljO3e2PizO2Ncl0ldjzuBUmmpkZruiELdPbmk-rT0LMrtO_3RhEG_RYLYpKLaSmMMofmysmHFowQRhU4j-lUQI5AybPxUCKnOfqzCBvtKZo4PaPnHCozjrgmWOg9aJPiWkYTVKo4YclQYC__w; WLID=yP7Kh+S6Bn1B9W7K27Lkx3FdRP+CfUFBGWgyu7YvrZnle7kPBaHNGXaFfcGAlcjeXlVhGInTy4sdmdgJvWu3PkY8mvSI/hp0V2wUAKEJkzE=; WLS=C=1d66d35e1b476e9f&N=%e9%9c%87; _EDGE_S=SID=178E84D3AFC067890F198A9FAE0B66B8; SRCHHPGUSR=CW=1204&CH=564&DPR=1.5625&UTC=480&WTS=63713981130; ipv6=hit=1578532442365&t=4; _SS=SID=178E84D3AFC067890F198A9FAE0B66B8&bIm=246&HV=1578528852

<ClientInstRequest><Events><E><T>Event.ClientInst</T><IG>3652527CE5B84079A1CA85EB3BFC9288</IG><TS>1578528880988</TS><D><![CDATA[[{"T":"CI.BoxModel","FID":"CI","Name":"v2.8","SV":"4","P":{"C":5,"N":4,"I":"5sd","S":"C+K+U","M":"V+L+M+MT+E+N+C+K+BD","T":30382,"K":"arz+nfq+keydown","F":1},"L":"@j/j/INPUT#sb_form_q//6z/3i/dm/u/a/K/2","M":"lw+67f+-1+nfx+12+0+0+0+0+1ba+1","N":"a8g/10//@3/ls%2Flsp.aspx/@8/nm/@4/a8n/a9c/@i/@i/aw1/aw3+nfo/11//@3/@k//0/@4/-1/-1/-1/-1/-1/-1","C":"mo1/c////av/0/+nfp/4////9r/k/","K":"@j/j/@k/0"}]]]></D></E></Events><STS>1578528880988</STS></ClientInstRequest>
*/

        public static string[] Parse(WebPage webPage, string header)
        {
            RequestParser parser = new RequestParser(webPage);
            return parser.Parse(header);
        }

        public RequestParser(WebPage webPage)
        {
            WebPage = webPage;

            ParseFuncs = new Func<string, bool>[]
            {
                ParseTarget,
                ParseCookies,
                l=>ParseSingleLine(l,"Content-Type",w=>WebPage.Request_ContentType=w),
                l=>ParseSingleLine(l,"User-Agent",w=>WebPage.Request_UserAgent=w),
                l=>ParseSingleLine(l,"Referer",w=>WebPage.Request_Referer=w),
                l=>ParseSingleLine(l,"Origin",w=>WebPage.Request_Origin=w),
                l=>ParseSingleLine(l,"Accept",w=>WebPage.Request_Accept=w),
            };
        }

        /// <summary>
        /// 解析HTTP头
        /// </summary>
        /// <param name="header">头字符串</param>
        /// <returns>无法解析的行</returns>
        public string[] Parse(string header)
        {
            List<string> errorLines = new List<string>();
            foreach (var line in header.Split('\r', '\n'))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                bool succeed = false;
                try
                {
                    foreach (var func in ParseFuncs)
                    {
                        if (func(line.Trim()))
                        {
                            succeed = true;
                            break;
                        }
                    }
                }
                catch(Exception ex)
                {

                }
                if (!succeed)
                {
                    errorLines.Add(line);
                }
            }
            return errorLines.ToArray();
        }

        private Func<string, bool>[] ParseFuncs;

        Regex rTarget = new Regex(@"^(?<Method>(GET|POST)) (?<Url>[^ ]+) HTTP/[0-9\.]+$", RegexOptions.Compiled);
        private bool ParseTarget(string line)
        {
            Match match = rTarget.Match(line);
            if (!match.Success)
            {
                return false;
            }
            WebPage.Request_Method = match.Groups["Method"].Value;
            WebPage.Url = match.Groups["Url"].Value;
            return true;
        }

        private bool ParseSingleLine(string line, string key, Action<string> set)
        {
            int index = line.IndexOf(':');
            if (index > 0)
            {
                if (line.Substring(0, index) != key)
                {
                    return false;
                }
                string content = line.Substring(index + 1).Trim();
                set(content);
                return true;
            }
            return false;
        }

        private bool ParseCookies(string line)
        {
            int index = line.IndexOf(':');
            if (index > 0)
            {
                List<Cookie> tempList = new List<Cookie>();
                if (line.Substring(0, index) != "Cookie")
                {
                    return false;
                }
                string content = line.Substring(index + 1).Trim();

                string[] cookies = content.Split(';');
                foreach (var cookie in cookies)
                {
                    int isIndex = cookie.IndexOf('=');
                    string key = cookie.Substring(0, isIndex).Trim();
                    string value = cookie.Substring(isIndex + 1).Trim();
                    tempList.Add(new Cookie(key, value));
                }
                if (WebPage.Request_Cookies==null)
                {
                    WebPage.Request_Cookies = new List<Cookie>();
                }
                WebPage.Request_Cookies.Clear();
                WebPage.Request_Cookies.AddRange(tempList);
                return true;
            }
            return false;

        }
    }
}
