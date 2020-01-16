using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        public static string GetResponseText(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            byte[] response = parser.GetResponseBinary();
            if (webPage.Response_Type == ResponseType.Binary)
            {
                return Convert.ToBase64String(response);
            }
            return Config.Instance.Encoding.GetString(response);
        }
        public async static Task<string> GetResponseTextAsync(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            byte[] response = await parser.GetResponseBinaryAsync();
            if (webPage.Response_Type == ResponseType.Binary)
            {
                return Convert.ToBase64String(response) ;
            }
            return Config.Instance.Encoding.GetString(response);
        }   
        public static byte[] GetResponseBinary(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            return parser.GetResponseBinary();
        }
        public async static Task<byte[]> GetResponseBinaryAsync(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            return await parser.GetResponseBinaryAsync();
        }           
        public static HttpWebResponse GetResponse(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            return parser.GetResponse();
        }
        public async static Task<HttpWebResponse> GetResponseAsync(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            return await parser.GetResponseAsync();
        }   
        public static object GetResponseBySetting(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            byte[] response = parser.GetResponseBinary();
            if (webPage.Response_Type==ResponseType.Binary)
            {
                return response;
            }
            return Config.Instance.Encoding.GetString(response);
        }
        public async static Task<object> GetResponseBySettingAsync(WebPage webPage)
        {
            HtmlGetter parser = new HtmlGetter(webPage);
            byte[] response =await parser.GetResponseBinaryAsync();
            if (webPage.Response_Type == ResponseType.Binary)
            {
                return response;
            }
            return Config.Instance.Encoding.GetString(response);
        }
        private byte[] GetResponseBinary()
        {
            using (Stream stream = GetResponse().GetResponseStream())
            {
                using (var mS = new MemoryStream())
                {
                    stream.CopyTo(mS);
                    return mS.ToArray();
                }
            }
        }
        private HttpWebResponse GetResponse(Action<HttpWebRequest> requestSettings=null)
        {
            if (string.IsNullOrEmpty(WebPage.Url))
            {
                throw new Exception(Strings.Get("error_urlIsEmpty") );
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
            request.ServicePoint.Expect100Continue = WebPage.Request_Expect100Continue;

            request.AllowAutoRedirect = WebPage.Request_AllowAutoRedirect;

            request.KeepAlive = WebPage.Request_KeepAlive; 
            if (request.Method == "POST")
            {
                //request.ContentLength = Config.Instance.Encoding.GetByteCount(WebPage.Request_Body);

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
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;
            requestSettings?.Invoke(request);
            //request.CookieContainer = GetCookie();
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch(Exception ex)
            {
                throw new Exception(Strings.Get("ex_GetResponse") , ex);
            }
            FixResponseCookies(request, response);
            return response;
        }

        private Task<byte[]> GetResponseBinaryAsync()
        {
            return Task.Run(GetResponseBinary);
        }      
        private Task<HttpWebResponse> GetResponseAsync(Action<HttpWebRequest> requestSettings = null)
        {
            return Task.Run(()=> GetResponse(requestSettings));
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
        private static void FixResponseCookies(HttpWebRequest request, HttpWebResponse response)
        {
            for (int i = 0; i < response.Headers.Count; i++)
            {
                string name = response.Headers.GetKey(i);
                if (name != "Set-Cookie")
                    continue;
                string value = response.Headers.Get(i);
                foreach (var singleCookie in value.Split(','))
                {
                    Match match = Regex.Match(singleCookie, "(.+?)=(.+?);");
                    if (match.Captures.Count == 0)
                        continue;
                    response.Cookies.Add(
                        new System.Net.Cookie(
                            match.Groups[1].ToString(),
                            match.Groups[2].ToString(),
                            "/",
                            request.Host.Split(':')[0]));
                }
            }
        }
    }
}
