using FzLib.Basic;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebPageWatcher.Data;
using static WebPageWatcher.Data.DbHelper;

namespace WebPageWatcher.Web
{
    public abstract class ComparerBase
    {
        protected static readonly Regex rWhiteSpace = new Regex(@"\s+", RegexOptions.Compiled);

        public WebPage WebPage { get; protected set; }
        public abstract CompareResult CompareWith(byte[] oldContent, byte[] newContent);

        public async static Task<CompareResult> CompareAsync(WebPage webPage)
        {
            byte[] newContent;
            try
            {
                newContent = await HtmlGetter.GetResponseBinaryAsync(webPage);
            }
            catch (Exception ex)
            {
                throw new WebPageException("ex_getContentFailed",webPage, ex);
            }
            return await CompareAsync(webPage, webPage.GetLatestContent(), newContent);
        }
        public async static Task<CompareResult> CompareAsync(WebPage webPage, byte[] oldContent, byte[] newContent)
        {

            CompareResult result = null;
            await Task.Run(() =>
             {
                 ComparerBase comparer = webPage.Response_Type switch
                 {
                     ResponseType.Html => new HtmlComparer(webPage),
                     ResponseType.Json => new JsonComparer(webPage),
                     ResponseType.Text => new TextComparer(webPage),
                     ResponseType.Binary => new BinaryComparer(webPage),
                     _ => throw new NotSupportedException(),
                 };
                 try
                 {
                     result = comparer.CompareWith(oldContent, newContent);
                 }
                 catch (Exception ex)
                 {
                     throw new WebPageException("ex_compareFailed",webPage, ex);
                 }
             });
            return result;
        }

        protected object GetDocument<T>(byte[] oldContent, byte[] newContent, Func<string, T> getDoc) where T : class
        {
            T oldDocument = null;
            T newDocument = null;
            Exception oldEx = null;
            Exception newEx = null;
            try
            {
                oldDocument = getDoc(oldContent.ToEncodedString());
            }
            catch (Exception ex)
            {
                oldEx = ex;
            }
            try
            {
                newDocument = getDoc(newContent.ToEncodedString());
            }
            catch (Exception ex)
            {
                newEx = ex;
            }
            int errorCount = (oldEx != null ? 1 : 0) + (newEx != null ? 1 : 0);
            if (errorCount == 1)
            {
                if (!Config.Instance.RegardOneSideParseErrorAsNotSame)
                {
                    throw new WebPageException("ex_parseError", WebPage, oldEx == null ? newEx : oldEx);
                }
                else
                {
                    return new CompareResult(WebPage, false, oldContent, newContent);
                }
            }
            else if (errorCount == 2)
            {
                throw new WebPageException("ex_parseError", WebPage);
            }
            return (oldDocument, newDocument);
        }
    }
    public class TextComparer : ComparerBase
    {
        public TextComparer(WebPage webPage)
        {
            WebPage = webPage;
        }
        public override CompareResult CompareWith(byte[] oldContent, byte[] newContent)
        {

            bool same;
            if (WebPage.IgnoreWhiteSpace)
            {
                same = rWhiteSpace.Replace(newContent.ToEncodedString(), "") == rWhiteSpace.Replace(oldContent.ToEncodedString(), "");
            }
            else
            {
                same = Enumerable.SequenceEqual(newContent, oldContent);
            }
            return new CompareResult(WebPage, same, oldContent, newContent);
        }
    }
    public class BinaryComparer : ComparerBase
    {
        public BinaryComparer(WebPage webPage)
        {
            WebPage = webPage;
        }
        public override CompareResult CompareWith(byte[] oldContent, byte[] newContent)
        {
            bool same = Enumerable.SequenceEqual(newContent, oldContent);
            return new CompareResult(WebPage, same, oldContent, newContent);
        }
    }
    public class JsonComparer : ComparerBase
    {

        public JsonComparer(WebPage webPage)
        {
            WebPage = webPage;
        }

        public override CompareResult CompareWith(byte[] oldContent, byte[] newContent)
        {
            object temp = GetDocument(oldContent, newContent, p => JToken.Parse(p));
            if (temp is CompareResult)
            {
                return temp as CompareResult;
            }

            JToken oldDocument = (((JToken, JToken))temp).Item1;
            JToken newDocument = (((JToken, JToken))temp).Item2;

            List<(object Old, object New)> differentElements;
            if (WebPage.BlackWhiteListMode == 0)
            {
                differentElements = WhiteListCompare(oldDocument, newDocument);
            }
            else
            {
                differentElements = BlackListCompare(oldDocument, newDocument);
            }

            return new CompareResult(WebPage, differentElements, oldDocument, newDocument, oldContent, newContent);
        }

        private List<(object Old, object New)> WhiteListCompare(JToken oldDocument, JToken newDocument)
        {
            List<(object Old, object New)> differentElements = new List<(object Old, object New)>();

            if (WebPage.BlackWhiteList == null || WebPage.BlackWhiteList.Count == 0)
            {
                throw new WebPageException("ex_whiteListIsEmpty", WebPage);
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

        public override CompareResult CompareWith(byte[] oldContent, byte[] newContent)
        {

            object temp = GetDocument(oldContent, newContent, p =>
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(p);
                return doc;
            });
            if (temp is CompareResult)
            {
                return temp as CompareResult;
            }

            HtmlDocument oldDocument = (((HtmlDocument, HtmlDocument))temp).Item1;
            HtmlDocument newDocument = (((HtmlDocument, HtmlDocument))temp).Item2;

            List<(object Old, object New)> differentElements = null;

            if (WebPage.BlackWhiteListMode == 0)
            {
                differentElements = WhiteListCompare(oldDocument, newDocument);
            }
            else
            {
                differentElements = BlackListCompare(oldDocument, newDocument);
            }
            return new CompareResult(WebPage, differentElements, oldDocument, newDocument, oldContent, newContent);

        }

        private List<(object Old, object New)> WhiteListCompare(HtmlDocument oldDocument, HtmlDocument newDocument)
        {
            List<(object Old, object New)> differentElements = new List<(object Old, object New)>();

            if (WebPage.BlackWhiteList == null || WebPage.BlackWhiteList.Count == 0)
            {
                throw new WebPageException("ex_whiteSpaceIsEmpty", WebPage);
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
       byte[] oldContent, byte[] newContent) : this(webPage)
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
            if (DifferentNodes == null)
            {
                return null;
            }
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
}
