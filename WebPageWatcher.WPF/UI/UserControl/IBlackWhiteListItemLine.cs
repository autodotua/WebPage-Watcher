using System;
using WebPageWatcher.Data;

namespace WebPageWatcher.UI
{
    internal interface IBlackWhiteListItemLine
    {
        BlackWhiteListItem Item { get; set; }

        event EventHandler Deleted;
    }
}