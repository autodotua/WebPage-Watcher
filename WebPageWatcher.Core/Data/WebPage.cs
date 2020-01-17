using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebPageWatcher.Web;

namespace WebPageWatcher.Data
{
    public class WebPage : IDbModel
    {
        public WebPage()
        {
            Name = "未命名";
            LastUpdateTime = DateTime.Now;
            LastCheckTime = DateTime.Now;
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool Enabled { get; set; } = true;
        public DateTime LastUpdateTime { get; set; } = DateTime.MinValue;
        public DateTime LastCheckTime { get; set; } = DateTime.MinValue;
        public int Interval { get; set; } = 1000 * 60 * 15;



        [Computed]
        public List<BlackWhiteListItem> BlackWhiteList { get; set; } = new List<BlackWhiteListItem>();
        public string BlackWhiteListJson
        {
            get => JsonConvert.SerializeObject(BlackWhiteList);
            set => BlackWhiteList = JsonConvert.DeserializeObject<List<BlackWhiteListItem>>(value);
        }
        /// <summary>
        /// 0为白名单，1为黑名单
        /// </summary>
        public int BlackWhiteListMode { get; set; } = 1;
        public bool InnerTextOnly { get; set; } = true;
        public bool IgnoreWhiteSpace { get; set; } = true;

        [Computed]
        public List<Cookie> Request_Cookies { get; set; } = new List<Cookie>();
        public string Request_CookiesJson
        {
            get => JsonConvert.SerializeObject(Request_Cookies);
            set => Request_Cookies = JsonConvert.DeserializeObject<List<Cookie>>(value);
        }
        public string Request_Method { get; set; } = "GET";
        public string Request_Accept { get; set; }
        public string Request_Origin { get; set; }
        public string Request_Referer { get; set; }
        public string Request_UserAgent { get; set; }
        public string Request_ContentType { get; set; }
        public string Request_Body { get; set; }
        public bool Request_Expect100Continue { get; set; } = false;
        public bool Request_KeepAlive { get; set; } = true;
        public bool Request_AllowAutoRedirect { get; set; } = true;

        /// <summary>
        /// 响应格式，支持HTML、JSON
        /// </summary>
        public ResponseType Response_Type { get; set; } = ResponseType.Html;

        public WebPage Clone()
        {
            WebPage webPage = MemberwiseClone() as WebPage;
            if (BlackWhiteList != null)
            {
                webPage.BlackWhiteList = new List<BlackWhiteListItem>(BlackWhiteList);
            }
            if (Request_Cookies != null)
            {
                webPage.Request_Cookies = new List<Cookie>(Request_Cookies);
            }
            return webPage;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        public override string ToString()
        {
            return Name + ":" + Url;
        }

         
    }

    public enum ResponseType
    {
        Html,
        Json,
        Text,
        Binary
    }

}
