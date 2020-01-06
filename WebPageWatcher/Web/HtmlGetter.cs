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
            if(string.IsNullOrEmpty(WebPage.Url))
            {
                throw new Exception(App.Current.FindResource("error_urlIsEmpty") as string);
            }
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
