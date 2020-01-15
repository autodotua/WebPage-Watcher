using FzLib.Basic.Collection;
using FzLib.Control.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using WebPageWatcher.Data;

namespace WebPageWatcher.UI
{

    public abstract class WebPagePanelBase : TabItemPanelBase<WebPage>
    {

    }
    public abstract class ScriptPanelBase : TabItemPanelBase<Script>
    {

    }
    public abstract class TabItemPanelBase<T> : ExtendedUserControl where T : class, IDbModel, new()
    {
        protected TabItemPanelBase()
        {
            Margin = new System.Windows.Thickness(0, 8, 0, 0);
            SetResourceReference(BackgroundProperty, "MaterialDesignPaper");
            SetResourceReference(TextElement.ForegroundProperty, "MaterialDesignBody");
            Loaded += Panel_Loaded;
        }

        private void Panel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //使用Binding引发了NullReferenceException错误，只能使用事件驱动。
            //再者，此处绑定使用OneWayToSouce模式，其实和事件没什么优劣之分
            List.SelectionChanged += List_SelectionChanged;

            SelectorHelper = new SelectorHelper<T>(List, Items);
            SelectorHelper.Delete += async (p1, p2) => await DbHelper.DeleteAsync(p2);
            SelectorHelper.Clone += async (p1, p2) => Items.Add(await DbHelper.CloneAsync(p2));
            SelectorHelper.SetContextMenu();
        }

        public SelectorHelper<T> SelectorHelper { get; private set; }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!updating)
            {
                Item = e.AddedItems.Count > 0 ? e.AddedItems[0] as T : null;
            }
        }

        public abstract void UpdateDisplay(T item);
        public void SelectItem(T item)
        {
            List.SelectedItem = item;
        }
        private bool updating = false;
        protected async Task UpdateItem()
        {
            updating = true;
            int index = Items.IndexOf(rawItem);
            Items.Remove(rawItem);
            Items.Insert(index, selectedItem);
            await DbHelper.UpdateAsync(selectedItem);
            updating = false;
            List.SelectedItem = selectedItem;
        }

        protected void ResetItem()
        {
            Item = rawItem;
        }


        public abstract ListView List { get; }
        private T rawItem;
        private T selectedItem;

        public abstract ExtendedObservableCollection<T> Items { get; }
        public T Item
        {
            get => selectedItem;
            set
            {
                rawItem = value;
                selectedItem = value == null ? null : value.Clone() as T;
                Notify(nameof(Item));
                //SetValueAndNotify(ref webPage, value, nameof(WebPage));
            }
        }
    }
}
