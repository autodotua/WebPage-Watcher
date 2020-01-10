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
        private WebPage WebPage { get; set; }
        private HtmlGetter(WebPage webPage)
        {
            WebPage = webPage;
        }


        //public static HtmlDocument GetHtmlDocument(WebPage webPage)
        //{
        //    HtmlGetter parser = new HtmlGetter(webPage);
        //    string html = parser.GetResponseText();
        //    var doc = new HtmlDocument();
        //    doc.LoadHtml(html);
        //    return doc;
        //}
        //public async static Task<HtmlDocument> GetHtmlDocumentAsync(WebPage webPage)
        //{
        //    HtmlGetter parser = new HtmlGetter(webPage);
        //    string html =await parser.GetResponseTextAsync();
        //    var doc = new HtmlDocument();
        //    doc.LoadHtml(html);
        //    return doc;
        //}
        //public static JToken GetJson(WebPage webPage)
        //{
        //    HtmlGetter parser = new HtmlGetter(webPage);
        //    string json = parser.GetResponseText();
        //    try
        //    {
        //        return JToken.Parse(json);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("转换为JSON失败", ex);
        //    }
        //}   
        //public async static Task<JToken> GetJsonAsync(WebPage webPage)
        //{
        //    HtmlGetter parser = new HtmlGetter(webPage);
        //    string json =await parser.GetResponseTextAsync();
        //    try
        //    {
        //        return JToken.Parse(json);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("转换为JSON失败", ex);
        //    }
        //}
        public static string GetResponseTextOrBase64(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            byte[] response = parser.GetResponse();
            if (webPage.Response_Type == ResponseType.Binary)
            {
                return Convert.ToBase64String(response);
            }
            return Config.Instance.Encoding.GetString(response);
        }
        public async static Task<string> GetResponseTextOrBase64Async(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            byte[] response = await parser.GetResponseAsync();
            if (webPage.Response_Type == ResponseType.Binary)
            {
                return Convert.ToBase64String(response) ;
            }
            return Config.Instance.Encoding.GetString(response);
        }   
        public static byte[] GetResponseBinary(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            return parser.GetResponse();
        }
        public async static Task<byte[]> GetResponseBinaryAsync(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            return await parser.GetResponseAsync();
        }   
        public static object GetResponseBySetting(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            byte[] response = parser.GetResponse();
            if (webPage.Response_Type==ResponseType.Binary)
            {
                return response;
            }
            return Config.Instance.Encoding.GetString(response);
        }
        public async static Task<object> GetResponseBySettingAsync(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            byte[] response =await parser.GetResponseAsync();
            if (webPage.Response_Type == ResponseType.Binary)
            {
                return response;
            }
            return Config.Instance.Encoding.GetString(response);
        }
        private byte[] GetResponse()
        {
            if (string.IsNullOrEmpty(WebPage.Url))
            {
                throw new Exception(App.Current.FindResource("error_urlIsEmpty") as string);
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(WebPage.Url);
            request.CookieContainer = GetCookies();
            if (!string.IsNullOrEmpty(WebPage.Request_Method))
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
            if (request.Method == "POST")
            {
                if (WebPage.Request_Body != null && WebPage.Request_Body.Length > 0)
                {
                    using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(WebPage.Request_Body);
                    }
                }
                else
                {
                    request.ContentLength = 0;
                }
            }
            //request.CookieContainer = GetCookie();
            HttpWebResponse response=null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch(Exception ex)
            {
                throw new Exception(App.Current.FindResource("ex_GetResponse") as string, ex);
            }
            using (Stream stream = response.GetResponseStream())
            {
                using (var mS = new MemoryStream())
                {
                    stream.CopyTo(mS);
                    return mS.ToArray();
                }
            }
        }

        private Task<byte[]> GetResponseAsync()
        {
            return Task.Run(GetResponse);
        }


        private CookieContainer GetCookies()
        {
            CookieContainer cookies = new CookieContainer();
            if (WebPage.Request_Cookies != null)
            {

                foreach (var c in WebPage.Request_Cookies)
                {
                    cookies.Add(new System.Net.Cookie(c.Key, c.Value, "/", new Uri(WebPage.Url).Host));
                }
            }
            return cookies;
        }
    }
}
