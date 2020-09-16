using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WebPageWatcher.Data;
using WebPageWatcher.Web;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BlackWhiteListWindow : WindowBase
    {
        public WebPage WebPage { get; private set; }
        private HtmlDocument htmlDoc;
        private JToken jsonObject;

        //private HtmlNodeCollection nodes;
        public HtmlNodeCollection HtmlNodes { get; private set; }// { get => nodes; private set => SetValueAndNotify(ref nodes, value, nameof(HtmlNodes)); }

        public List<JToken> JsonTokens { get; private set; }// { get => nodes; private set => SetValueAndNotify(ref nodes, value, nameof(HtmlNodes)); }

        public BlackWhiteListWindow(WebPage webPage)
        {
            WebPage = webPage;
            InitializeComponent();
            //VirtualizingPanel.SetIsVirtualizing(tree, false);
        }

        private async Task LoadAsync()
        {
            progressDialog.Show();
            cbbBlackWhite.SelectedIndex = WebPage.BlackWhiteListMode;
            switch (WebPage.Response_Type)
            {
                case ResponseType.Html:
                    await LoadHtmlAsync();
                    break;

                case ResponseType.Json:
                    await LoadJsonAsync();
                    break;

                default:
                    throw new NotSupportedException();
            }
            progressDialog.Close();
        }

        private async Task LoadJsonAsync()
        {
            if (!await LoadJsonObjectAsync())
            {
                return;
            }
            if (jsonObject != default)
            {
                JsonTokens = jsonObject.Children().ToList();
            }
            if (WebPage.BlackWhiteList != null)
            {
                foreach (var id in WebPage.BlackWhiteList)
                {
                    if (id == null)
                    {
                        continue;
                    }
                    var line = new JsonBlackWhiteListItemLine(id.Clone());
                    line.Deleted += Line_Deleted;
                    stkIdentifies.Children.Add(line);
                }
            }
            //tree.ItemTemplate = tree.Resources["htmlTemplate"] as HierarchicalDataTemplate;

            tree.ItemsSource = JsonTokens;
        }

        private async Task LoadHtmlAsync()
        {
            if (!await LoadHtmlDocumentAsync())
            {
                return;
            }
            HtmlNodeCollection nodes = null;
            if (htmlDoc != null)
            {
                nodes = htmlDoc.DocumentNode.ChildNodes;
                foreach (var node in nodes.ToArray())
                {
                    if (node.NodeType != HtmlNodeType.Element)
                    {
                        nodes.Remove(node);
                    }
                }
            }
            if (WebPage.BlackWhiteList != null)
            {
                foreach (var id in WebPage.BlackWhiteList)
                {
                    if (id == null)
                    {
                        continue;
                    }
                    var line = new HtmlBlackWhiteListItemLine(id.Clone());
                    line.Deleted += Line_Deleted;
                    stkIdentifies.Children.Add(line);
                }
            }
            tree.ItemTemplate = tree.Resources["htmlTemplate"] as HierarchicalDataTemplate;

            HtmlNodes = nodes;
            tree.ItemsSource = HtmlNodes;
        }

        private async Task<bool> LoadHtmlDocumentAsync()
        {
            try
            {
                Exception ex = null;

                try
                {
                    string response = await HtmlGetter.GetResponseTextAsync(WebPage);
                    htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(response);
                }
                catch (Exception ex2)
                {
                    ex = ex2;
                }
                if (ex != null)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                progressDialog.Close();
                if (await dialog.ShowYesNoAsync(FindResource("error_getHtmlFailed") as string + Environment.NewLine + ex.ToString(), FindResource("error_title_getHtmlFailed") as string))
                {
                    grd.Children.Remove(tree);
                    grd.ColumnDefinitions[0].Width = grd.ColumnDefinitions[1].Width = new GridLength(0);
                }
                else
                {
                    Close();
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> LoadJsonObjectAsync()
        {
            try
            {
                string response = await HtmlGetter.GetResponseTextAsync(WebPage);
                jsonObject = JToken.Parse(response);
            }
            catch (Exception ex)
            {
                progressDialog.Close();
                if (await dialog.ShowYesNoAsync(FindResource("error_getHtmlFailed") as string + Environment.NewLine + ex.ToString(), FindResource("error_title_getHtmlFailed") as string))
                {
                    grd.Children.Remove(tree);
                    grd.ColumnDefinitions[0].Width = grd.ColumnDefinitions[1].Width = new GridLength(0);
                }
                else
                {
                    Close();
                    return false;
                }
            }
            return true;
        }

        private void Line_Deleted(object sender, EventArgs e)
        {
            stkIdentifies.Children.Remove(sender as UIElement);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            IBlackWhiteListItemLine line = null;
            switch (WebPage.Response_Type)
            {
                case ResponseType.Html:
                    line = new HtmlBlackWhiteListItemLine();
                    break;

                case ResponseType.Json:
                    line = new JsonBlackWhiteListItemLine();
                    break;

                default:
                    throw new NotSupportedException();
            }
            line.Deleted += Line_Deleted;
            stkIdentifies.Children.Add(line as UIElement);
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            menuId.Visibility = Visibility.Collapsed;
            menuXPath.Visibility = Visibility.Collapsed;
            menuPath.Visibility = Visibility.Collapsed;
            switch (WebPage.Response_Type)
            {
                case ResponseType.Html:
                    HtmlNode node = tree.SelectedItem as HtmlNode;
                    if (!string.IsNullOrEmpty(node.Id))
                    {
                        menuId.Visibility = Visibility.Visible;
                    }
                    menuXPath.Visibility = Visibility.Visible;
                    break;

                case ResponseType.Json:
                    menuPath.Visibility = Visibility.Visible;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void menuId_Click(object sender, RoutedEventArgs e)
        {
            AddHtmlLine(BlackWhiteListItemType.Id, p => p.Id);
        }

        private void AddHtmlLine(BlackWhiteListItemType type, Func<HtmlNode, string> valueFunc)
        {
            HtmlNode node = tree.SelectedItem as HtmlNode;

            var item = new BlackWhiteListItem()
            {
                Type = type,
                Value = valueFunc(node)
            };
            HtmlBlackWhiteListItemLine line = new HtmlBlackWhiteListItemLine(item);
            line.Deleted += Line_Deleted;
            stkIdentifies.Children.Add(line);
        }

        private void menuXPath_Click(object sender, RoutedEventArgs e)
        {
            AddHtmlLine(BlackWhiteListItemType.XPath, p => p.XPath);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            WebPage.BlackWhiteListMode = cbbBlackWhite.SelectedIndex;
            WebPage.BlackWhiteList = stkIdentifies.Children.Cast<IBlackWhiteListItemLine>()
                .Select(p => p.Item).ToList();
            Close();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            HtmlNode node = null;

            if (cbbSearchType.SelectedIndex == 0)//ID
            {
                node = htmlDoc.GetElementbyId(txtSearch.Text);
            }
            else
            {
                node = htmlDoc.DocumentNode.SelectSingleNode(txtSearch.Text);
            }
            if (node == null)
            {
                await dialog.ShowErrorAsync(FindResource("error_cannotFindNode") as string);
                return;
            }
            //SelectTreeViewItemByHtmlNode(node);
            SelectTreeViewItemByHtmlNode(node);// tree.ItemContainerGenerator.ContainerFromIndex(tree.Items.CurrentPosition) as TreeViewItem;
        }

        private void SelectTreeViewItemByHtmlNode(HtmlNode node)
        {
            Stack<HtmlNode> nodes = new Stack<HtmlNode>();
            while (!HtmlNodes.Contains(node))
            {
                nodes.Push(node);
                node = node.ParentNode;
            }
            TreeViewItem treeViewItem = tree.ItemContainerGenerator
                .ContainerFromIndex(HtmlNodes.IndexOf(node)) as TreeViewItem;

            Expanded(false);
            void Expanded(bool a)
            {
                if (a)
                {
                    treeViewItem = treeViewItem.ItemContainerGenerator.ContainerFromItem(node) as TreeViewItem;
                    if (nodes.Count == 0)
                    {
                        treeViewItem.IsSelected = true;
                        treeViewItem.BringIntoView();
                        return;
                    }
                }
                node = nodes.Pop();
                treeViewItem.IsExpanded = true;
                if (treeViewItem.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    Expanded(true);
                }
                else
                {
                    treeViewItem.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
                }
                //treeViewItem.Expanded += (p1, p2) => Expanded(true);
                //treeViewItem.UpdateLayout();
            }
            void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
            {
                if (treeViewItem.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    treeViewItem.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
                    Expanded(true);
                }
            }
        }

        private void menuPath_Click(object sender, RoutedEventArgs e)
        {
            JToken jToken = tree.SelectedItem as JToken;
            var item = new BlackWhiteListItem()
            {
                Type = BlackWhiteListItemType.JTokenPath,
                Value = jToken.Path,
            };
            JsonBlackWhiteListItemLine line = new JsonBlackWhiteListItemLine(item);
            line.Deleted += Line_Deleted;
            stkIdentifies.Children.Add(line);
        }

        private async void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAsync();
        }
    }
}