using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebPageWatcher.Data;

namespace WebPageWatcher.Web
{
    public abstract class ComparerBase
    {
        protected static readonly Regex rWhiteSpace = new Regex(@"\s+", RegexOptions.Compiled);

        public WebPage WebPage { get; protected set; }
        public abstract CompareResult CompareWith(string newContent);

        public static CompareResult Compare(WebPage webPage)
        {
            string newContent;
            try
            {
                newContent = HtmlGetter.GetResponseText(webPage);
            }
            catch (Exception ex)
            {
                throw new Exception("获取最新的内容失败", ex);
            }
            switch (webPage.Response_Type)
            {
                case "HTML":
                    HtmlComparer htmlComparer = new HtmlComparer(webPage);
                    return htmlComparer.CompareWith(newContent);
                case "JSON":
                    JsonComparer jsonComparer = new JsonComparer(webPage);
                    return jsonComparer.CompareWith(newContent);
            }
            throw new NotImplementedException();
        }
        public async static Task<CompareResult> CompareAsync(WebPage webPage)
        {
            string newContent;
            try
            {
                newContent = await HtmlGetter.GetResponseTextAsync(webPage);
            }
            catch (Exception ex)
            {
                throw new Exception("获取最新的内容失败", ex);
            }
            CompareResult result = null;
            await Task.Run(() =>
             {
                 switch (webPage.Response_Type)
                 {
                     case "HTML":
                         HtmlComparer htmlComparer = new HtmlComparer(webPage);
                         result = htmlComparer.CompareWith(newContent);
                         break;
                     case "JSON":
                         JsonComparer jsonComparer = new JsonComparer(webPage);
                         result = jsonComparer.CompareWith(newContent);
                         break;
                 }
             });
            return result;
            throw new NotImplementedException();
        }

    }
    public class JsonComparer : ComparerBase
    {

        public JsonComparer(WebPage webPage)
        {
            WebPage = webPage;
        }

        public override CompareResult CompareWith(string newContent)
        {
            JToken oldDocument = JToken.Parse(WebPage.LatestDocument);
            JToken newDocument = JToken.Parse(newContent);

            List<(object Old, object New)> differentElements = null;

            if (WebPage.BlackWhiteListMode == 0)
            {
                differentElements = WhiteListCompare(oldDocument, newDocument);
            }
            else
            {
                differentElements = BlackListCompare(oldDocument, newDocument);
            }

            return new CompareResult("JSON", differentElements, oldDocument, newDocument, p => (p as JToken).ToString());
        }

        private List<(object Old, object New)> WhiteListCompare(JToken oldDocument, JToken newDocument)
        {
            List<(object Old, object New)> differentElements = new List<(object Old, object New)>();

            if (WebPage.BlackWhiteList == null || WebPage.BlackWhiteList.Count == 0)
            {
                throw new Exception("白名单为空");
            }
            foreach (var identify in WebPage.BlackWhiteList)
            {
                JToken oldNode = oldDocument.SelectToken(identify.Value);
                JToken newNode = newDocument.SelectToken(identify.Value);

                if (IsDifferent(oldNode, newNode))
                {
                    differentElements.Add((oldNode, newNode));
                }

            }
            return differentElements;
        }
        private List<(object Old, object New)> BlackListCompare(JToken oldDocument, JToken newDocument)
        {
            List<(object Old, object New)> differentElements = new List<(object Old, object New)>();

            if (WebPage.BlackWhiteList != null && WebPage.BlackWhiteList.Count > 0)
            {
                foreach (var identify in WebPage.BlackWhiteList)
                {
                    JToken oldNode = oldDocument.SelectToken(identify.Value);
                    JToken newNode = newDocument.SelectToken(identify.Value);

                    oldNode?.Remove();
                    newNode?.Remove();
                }
            }
            if (IsDifferent(oldDocument, newDocument))
            {
                differentElements.Add((oldDocument, newDocument));
            }
            return differentElements;
        }

        private bool IsDifferent(JToken oldNode, JToken newNode)
        {
            string value1 = oldNode.ToString();
            string value2 = newNode.ToString();

            if (WebPage.IgnoreWhiteSpace)
            {
                value1 = rWhiteSpace.Replace(value1, "");
                value2 = rWhiteSpace.Replace(value2, "");
            }

            return value1 != value2;
        }

    }


    public class HtmlComparer : ComparerBase
    {

        public HtmlComparer(WebPage webPage)
        {
            WebPage = webPage;
        }

        public override CompareResult CompareWith(string newContent)
        {
            HtmlDocument newDocument = new HtmlDocument();
            newDocument.LoadHtml(newContent);

            HtmlDocument oldDocument = new HtmlDocument();
            oldDocument.LoadHtml(WebPage.LatestDocument);

            List<(object Old, object New)> differentElements = null;

            if (WebPage.BlackWhiteListMode == 0)
            {
                differentElements = WhiteListCompare(oldDocument, newDocument);
            }
            else
            {
                differentElements = BlackListCompare(oldDocument, newDocument);
            }
            return new CompareResult("HTML", differentElements, oldDocument, newDocument, p => (p as HtmlDocument).Text);

        }

        private List<(object Old, object New)> WhiteListCompare(HtmlDocument oldDocument, HtmlDocument newDocument)
        {
            List<(object Old, object New)> differentElements = new List<(object Old, object New)>();

            if (WebPage.BlackWhiteList == null || WebPage.BlackWhiteList.Count == 0)
            {
                throw new Exception("白名单为空");
            }
            foreach (var identify in WebPage.BlackWhiteList)
            {
                HtmlNode oldNode = null;
                HtmlNode newNode = null;
                switch (identify.Type)
                {
                    case BlackWhiteListItemType.Id:
                        {
                            oldNode = oldDocument.GetElementbyId(identify.Value);
                            newNode = newDocument.GetElementbyId(identify.Value);
                            break;
                        }
                    case BlackWhiteListItemType.XPath:
                        {
                            oldNode = oldDocument.DocumentNode.SelectSingleNode(identify.Value);
                            newNode = newDocument.DocumentNode.SelectSingleNode(identify.Value);
                            break;
                        }
                }
                if (IsDifferent(oldNode, newNode))
                {
                    differentElements.Add((oldNode, newNode));
                }

            }
            return differentElements;
        }
        private List<(object Old, object New)> BlackListCompare(HtmlDocument oldDocument, HtmlDocument newDocument)
        {
            List<(object Old, object New)> differentElements = new List<(object Old, object New)>();

            if (WebPage.BlackWhiteList != null && WebPage.BlackWhiteList.Count > 0)
            {
                foreach (var item in WebPage.BlackWhiteList)
                {
                    HtmlNode oldNode = null;
                    HtmlNode newNode = null;
                    switch (item.Type)
                    {
                        case BlackWhiteListItemType.Id:
                            {
                                oldNode = oldDocument.GetElementbyId(item.Value);
                                newNode = newDocument.GetElementbyId(item.Value);
                                break;
                            }
                        case BlackWhiteListItemType.XPath:
                            {
                                oldNode = oldDocument.DocumentNode.SelectSingleNode(item.Value);
                                newNode = newDocument.DocumentNode.SelectSingleNode(item.Value);
                                break;
                            }
                    }
                    if (newNode != null)
                    {
                        newNode.Remove();
                    }
                    if (oldNode != null)
                    {
                        oldNode.Remove();
                    }

                }
            }
            if (IsDifferent(oldDocument.DocumentNode, newDocument.DocumentNode))
            {
                differentElements.Add((oldDocument.DocumentNode, newDocument.DocumentNode));
            }
            return differentElements;

        }
        private bool IsDifferent(HtmlNode oldNode, HtmlNode newNode)
        {
            string value1;
            string value2;
            if (WebPage.InnerTextOnly)
            {
                value1 = oldNode.InnerText;
                value2 = newNode.InnerText;
            }
            else
            {
                value1 = oldNode.OuterHtml;
                value2 = newNode.OuterHtml;
            }

            if (WebPage.IgnoreWhiteSpace)
            {
                value1 = rWhiteSpace.Replace(value1, "");
                value2 = rWhiteSpace.Replace(value2, "");
            }

            return value1 != value2;
        }
    }

    public class CompareResult
    {
        public CompareResult(string type,
            IEnumerable<(object Old, object New)> differentElements,
       object oldDocument, object newDocument, Func<object, string> getContent)
        {
            Type = type;
            Same = !differentElements.Any();
            DifferentNodes = differentElements.ToArray();
            OldDocument = oldDocument;
            NewDocument = newDocument;

            OldContent = getContent(oldDocument);
            NewContent = getContent(newDocument);

        }
        public string Type { get; }
        public bool Same { get; }
        public (object Old, object New)[] DifferentNodes { get; }
        public object OldDocument { get; }
        public object NewDocument { get; }
        public string OldContent { get; }
        public string NewContent { get; }
    }

    //public  class HtmlCompareResult: CompareResult<HtmlDocument,HtmlNode>
    //{
    //public HtmlCompareResult(IEnumerable<(HtmlNode Old, HtmlNode New)> differentElements,
    //    HtmlDocument oldDocument, HtmlDocument newDocument)
    //{
    //    Same = !differentElements.Any();
    //    DifferentElements.AddRange(differentElements);
    //    OldDocument = oldDocument;
    //    NewDocument = newDocument;
    //}

    //    public override bool Same { get;  }
    //    public override List<(HtmlNode Old, HtmlNode New)> DifferentElements { get; } = new List<(HtmlNode Old, HtmlNode New)>();
    //    public override HtmlDocument OldDocument { get; }
    //    public override HtmlDocument NewDocument { get; }
    //}
}
