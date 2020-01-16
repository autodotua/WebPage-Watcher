using System;
using WebPageWatcher.Data;

namespace WebPageWatcher.UI
{
    interface IBlackWhiteListItemLine
    {
        BlackWhiteListItem Item { get; set; }
        event EventHandler Deleted;
    }
}
