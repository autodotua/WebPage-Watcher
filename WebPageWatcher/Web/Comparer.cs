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
        public abstract CompareResult CompareWith(byte[] newContent);
        [Obsolete]
        public static CompareResult Compare(WebPage webPage)
        {
            byte[] newContent;
            try
            {
                newContent = HtmlGetter.GetResponseBinary(webPage);
            }
            catch (Exception ex)
            {
                throw new Exception("获取最新的内容失败", ex);
            }
            CompareResult result = null;

            ComparerBase comparer;
            switch (webPage.Response_Type)
            {
                case ResponseType.Html:
                    comparer = new HtmlComparer(webPage);
                    break;
                case ResponseType.Json:
                    comparer = new JsonComparer(webPage);
                    break;
                case ResponseType.Text:
                    comparer = new TextComparer(webPage);
                    break;
                case ResponseType.Binary:
                    comparer = new BinaryComparer(webPage);
                    break;
                default:
                    throw new NotSupportedException();
            }
            result = comparer.CompareWith(newContent);
            return result;
        }
        public async static Task<CompareResult> CompareAsync(WebPage webPage)
        {
            byte[] newContent;
            try
            {
                newContent = await HtmlGetter.GetResponseBinaryAsync(webPage);
            }
            catch (Exception ex)
            {
                throw new Exception("获取最新的内容失败", ex);
            }
            CompareResult result = null;
            await Task.Run(() =>
             {
                 ComparerBase comparer;
                 switch (webPage.Response_Type)
                 {
                     case ResponseType.Html:
                         comparer = new HtmlComparer(webPage);
                         break;
                     case ResponseType.Json:
                         comparer = new JsonComparer(webPage);
                         break;
                     case ResponseType.Text:
                         comparer = new TextComparer(webPage);
                         break;
                     case ResponseType.Binary:
                         comparer = new BinaryComparer(webPage);
                         break;
                     default:
                         throw new NotSupportedException();
                 }
                 result = comparer.CompareWith(newContent);
             });
            return result;
        }

    }
    public class TextComparer : ComparerBase
    {
        public TextComparer(WebPage webPage)
        {
            WebPage = webPage;
        }
        public override CompareResult CompareWith(byte[] newContent)
        {

            bool same;
            if (WebPage.IgnoreWhiteSpace)
            {
                same = rWhiteSpace.Replace(newContent.ToEncodedString(), "") == rWhiteSpace.Replace(WebPage.LatestContent.ToEncodedString(), "");
            }
            else
            {
                same = Enumerable.SequenceEqual(newContent, WebPage.LatestContent);
            }
            return new CompareResult(WebPage, same, WebPage.LatestContent, newContent);
        }
    }
    public class BinaryComparer : ComparerBase
    {
        public BinaryComparer(WebPage webPage)
        {
            WebPage = webPage;
        }
        public override CompareResult CompareWith(byte[] newContent)
        {
            bool same = Enumerable.SequenceEqual(newContent, WebPage.LatestContent);
            return new CompareResult(WebPage, same, WebPage.LatestContent, newContent);
        }
    }
    public class JsonComparer : ComparerBase
    {

        public JsonComparer(WebPage webPage)
        {
            WebPage = webPage;
        }

        public override CompareResult CompareWith(byte[] newContent)
        {
            JToken oldDocument = JToken.Parse(WebPage.LatestContent.ToEncodedString());
            JToken newDocument = JToken.Parse(newContent.ToEncodedString());

            List<(object Old, object New)> differentElements = null;

            if (WebPage.BlackWhiteListMode == 0)
            {
                differentElements = WhiteListCompare(oldDocument, newDocument);
            }
            else
            {
                differentElements = BlackListCompare(oldDocument, newDocument);
            }

            return new CompareResult(WebPage, differentElements, oldDocument, newDocument, WebPage.LatestContent, newContent);
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

        public override CompareResult CompareWith(byte[] newContent)
        {
            HtmlDocument newDocument = new HtmlDocument();
            newDocument.LoadHtml(newContent.ToEncodedString());

            HtmlDocument oldDocument = new HtmlDocument();
            oldDocument.LoadHtml(WebPage.LatestContent.ToEncodedString());

            List<(object Old, object New)> differentElements = null;

            if (WebPage.BlackWhiteListMode == 0)
            {
                differentElements = WhiteListCompare(oldDocument, newDocument);
            }
            else
            {
                differentElements = BlackListCompare(oldDocument, newDocument);
            }
            return new CompareResult(WebPage, differentElements, oldDocument, newDocument, WebPage.LatestContent, newContent);

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
        public CompareResult(WebPage webPage,
            IEnumerable<(object Old, object New)> differentElements,
       HtmlDocument oldDocument, HtmlDocument newDocument,
       byte[] oldContent, byte[] newContent) : this(webPage)
        {
            Same = differentElements == null || !differentElements.Any();
            DifferentNodes = differentElements?.ToArray();
            OldDocument = oldDocument;
            NewDocument = newDocument;

            OldContent = oldContent;
            NewContent = newContent;

        }
        public CompareResult(WebPage webPage,
            IEnumerable<(object Old, object New)> differentElements,
       JToken oldToken, JToken newToken,
       byte[] oldContent, byte[] newContent) : this(webPage)
        {
            Same = differentElements == null || !differentElements.Any();
            DifferentNodes = differentElements?.ToArray();
            OldDocument = oldToken;
            NewDocument = newToken;

            OldContent = oldContent;
            NewContent = newContent;

        }

        public CompareResult(WebPage webPage, bool same,
       byte[] oldContent, byte[] newContent):this(webPage)
        {
            Same = same;
            OldContent = oldContent;
            NewContent = newContent;
        }

        private CompareResult(WebPage webPage)
        {
            WebPage = webPage;
            Type = webPage.Response_Type;
        }
        public bool Same { get; }
        public (object Old, object New)[] DifferentNodes { get; }
        public WebPage WebPage { get; }
        public ResponseType Type { get; }
        public object OldDocument { get; }
        public object NewDocument { get; }
        public byte[] OldContent { get; }
        public byte[] NewContent { get; }

        public Diff[] GetDifferences()
        {
            StringBuilder text1 = new StringBuilder();
            StringBuilder text2 = new StringBuilder();
            switch (Type)
            {
                case ResponseType.Html:
                    foreach ((object oldNode, object newNode) in DifferentNodes)
                    {
                        if (WebPage.InnerTextOnly)
                        {
                            text1.AppendLine((oldNode as HtmlNode).InnerText);
                            text2.AppendLine((newNode as HtmlNode).InnerText);
                        }
                        else
                        {
                            text1.AppendLine((oldNode as HtmlNode).OuterHtml);
                            text2.AppendLine((newNode as HtmlNode).OuterHtml);
                        }
                    }
                    break;
                case ResponseType.Json:
                    foreach ((object oldNode, object newNode) in DifferentNodes)
                    {
                        text1.AppendLine((oldNode as JToken).ToString());
                        text2.AppendLine((newNode as JToken).ToString());
                    }
                    break;
                case ResponseType.Text:
                    text1.Append(OldContent.ToEncodedString());
                    text2.Append(NewContent.ToEncodedString());
                    break;
                default:
                    throw new NotSupportedException();
            }

            return new diff_match_patch().diff_main(text1.ToString(), text2.ToString()).ToArray();
        }
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
