using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private int iD;
        public int ID
        {
            get => iD;
            set
            {
                iD = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ID)));
            }
        }
        private string @name;
        public string Name
        {
            get => @name;
            set
            {
                @name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
        private string url;
        public string Url
        {
            get => url;
            set
            {
                url = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Url)));
            }
        }
        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
            }
        }
        private DateTime lastUpdateTime = DateTime.MinValue;
        public DateTime LastUpdateTime
        {
            get => lastUpdateTime;
            set
            {
                lastUpdateTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastUpdateTime)));
            }
        }
        private DateTime lastCheckTime = DateTime.MinValue;
        public DateTime LastCheckTime
        {
            get => lastCheckTime;
            set
            {
                lastCheckTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastCheckTime)));
            }
        }
        private int interval = 1000 * 60 * 15;
        public int Interval
        {
            get => interval;
            set
            {
                interval = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Interval)));
            }
        }



        
        private List<BlackWhiteListItem> blackWhiteList = new List<BlackWhiteListItem>();
        [Computed]
        public List<BlackWhiteListItem> BlackWhiteList
        {
            get => blackWhiteList;
            set
            {
                blackWhiteList = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BlackWhiteList)));
            }
        }
        public string BlackWhiteListJson
        {
            get => JsonConvert.SerializeObject(BlackWhiteList);
            set => BlackWhiteList = JsonConvert.DeserializeObject<List<BlackWhiteListItem>>(value);
        }
        /// <summary>
        /// 0为白名单，1为黑名单
        /// </summary>
        private int blackWhiteListMode = 1;
        public int BlackWhiteListMode
        {
            get => blackWhiteListMode;
            set
            {
                blackWhiteListMode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BlackWhiteListMode)));
            }
        }
        private bool innerTextOnly = true;
        public bool InnerTextOnly
        {
            get => innerTextOnly;
            set
            {
                innerTextOnly = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InnerTextOnly)));
            }
        }
        private bool ignoreWhiteSpace = true;
        public bool IgnoreWhiteSpace
        {
            get => ignoreWhiteSpace;
            set
            {
                ignoreWhiteSpace = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IgnoreWhiteSpace)));
            }
        }

      
        private List<Cookie> request_Cookies = new List<Cookie>();
        [Computed]
        public List<Cookie> Request_Cookies
        {
            get => request_Cookies;
            set
            {
                request_Cookies = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Request_Cookies)));
            }
        }
        public string Request_CookiesJson
        {
            get => JsonConvert.SerializeObject(Request_Cookies);
            set => Request_Cookies = JsonConvert.DeserializeObject<List<Cookie>>(value);
        }
        private string request_Method = "GET";
        public string Request_Method
        {
            get => request_Method;
            set
            {
                request_Method = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Request_Method)));
            }
        }
        private string request_Accept;
        public string Request_Accept
        {
            get => request_Accept;
            set
            {
                request_Accept = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Request_Accept)));
            }
        }
        private string request_Origin;
        public string Request_Origin
        {
            get => request_Origin;
            set
            {
                request_Origin = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Request_Origin)));
            }
        }
        private string request_Referer;
        public string Request_Referer
        {
            get => request_Referer;
            set
            {
                request_Referer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Request_Referer)));
            }
        }
        private string request_UserAgent;
        public string Request_UserAgent
        {
            get => request_UserAgent;
            set
            {
                request_UserAgent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Request_UserAgent)));
            }
        }
        private string request_ContentType;
        public string Request_ContentType
        {
            get => request_ContentType;
            set
            {
                request_ContentType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Request_ContentType)));
            }
        }
        private string request_Body;
        public string Request_Body
        {
            get => request_Body;
            set
            {
                request_Body = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Request_Body)));
            }
        }
        public bool Request_Expect100Continue { get; set; } = false;
        private bool request_KeepAlive = true;
        public bool Request_KeepAlive
        {
            get => request_KeepAlive;
            set
            {
                request_KeepAlive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Request_KeepAlive)));
            }
        }
        private bool request_AllowAutoRedirect = true;
        public bool Request_AllowAutoRedirect
        {
            get => request_AllowAutoRedirect;
            set
            {
                request_AllowAutoRedirect = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Request_AllowAutoRedirect)));
            }
        }

        /// <summary>
        /// 响应格式，支持HTML、JSON
        /// </summary>
        private ResponseType response_Type = ResponseType.Html;
        public ResponseType Response_Type
        {
            get => response_Type;
            set
            {
                response_Type = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Response_Type)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

