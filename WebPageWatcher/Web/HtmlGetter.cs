using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebPageWatcher.Data;

namespace WebPageWatcher.Web
{
    public class HtmlGetter
    {
        private WebPage WebPage { get;  set; }
        private HtmlGetter(WebPage webPage)
        {
            WebPage = webPage;
        }


        public static HtmlDocument GetHtmlDocument(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            string html = parser. GetResponse();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }     
        public static JObject GetJsonObject(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            string json = parser.GetResponse();
            return JObject.Parse(json);
        }
        public static string GetResponse(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            return parser.GetResponse();
        }
        private string GetResponse()
        {
            string str = @"Host: app.zafu.edu.cn
Proxy-Connection: keep-alive
Content-Length: 0
Accept: application/json
X-Requested-With: XMLHttpRequest
User-Agent: Mozilla/5.0 (Linux; Android 9; MI 6 Build/PQ3A.190801.002; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/77.0.3865.92 Mobile Safari/537.36
Origin: http://app.zafu.edu.cn
Referer: http://app.zafu.edu.cn/h5app/studentscore.htm
Accept-Encoding: gzip, deflate
Accept-Language: zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7
Cookie: Hm_lvt_6f798e51a1cd93937ee8293eece39b1a=1568115202;Hm_lpvt_6f798e51a1cd93937ee8293eece39b1a=1569408778;CNZZDATA5718743=cnzz_eid%3D465618644-1559608103-%26ntime%3D1569405599; JSESSIONID=9775BB7370CA66601E73D4760CBF8D69
";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(WebPage.Url);
            request.CookieContainer = GetCookies();
            if(!string.IsNullOrEmpty(WebPage.Request_Method))
            {
                request.Method = WebPage.Request_Method;
            }
            else
            {
                request.Method = "GET";
            }
            if (!string.IsNullOrEmpty(WebPage.Request_Accept))
            {
                request.Accept = WebPage.Request_Accept;
            }
            if (!string.IsNullOrEmpty(WebPage.Request_ContentType))
            {
                request.ContentType = WebPage.Request_ContentType;
            }
            if (!string.IsNullOrEmpty(WebPage.Request_UserAgent))
            {
                request.UserAgent = WebPage.Request_UserAgent;
            }     
            if (!string.IsNullOrEmpty(WebPage.Request_Referer))
            {
                request.Referer = WebPage.Request_Referer;
            }
            //request.CookieContainer = GetCookie();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();

            using (var reader = new StreamReader(stream))
            {
                string html = reader.ReadToEnd();
                return html;
            }
        }



        private CookieContainer GetCookies()
        {
            CookieContainer cookies = new CookieContainer();
            if (WebPage.Request_Cookies != null)
            {

                foreach (var c in WebPage.Request_Cookies)
                {
                    cookies.Add(new System.Net.Cookie(c.Name, c.Value, "/", new Uri(WebPage.Url).Host));
                }
            }
            return cookies;
        }
    }
}
