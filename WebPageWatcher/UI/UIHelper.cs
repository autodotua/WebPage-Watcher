using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WebPageWatcher.UI
{
   public static class UIHelper
    {
        public static void SetContextMenuForSelector<T>(Selector listView, ObservableCollection<T> source,EventHandler<T> deleted=null) where T:class
        {
            MenuItem menuDelete = new MenuItem() { Header = "删除" };
            menuDelete.Click += (p1, p2) =>
            {
                T item = listView.SelectedItem as T;
                source.Remove(item);
                deleted?.Invoke(p1, item);
            };

            ContextMenu menu = new ContextMenu();
            menu.Items.Add(menuDelete);

            listView.ContextMenu = menu;
        }
    }
}
