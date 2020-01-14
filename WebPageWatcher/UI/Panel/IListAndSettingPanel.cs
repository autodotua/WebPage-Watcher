using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebPageWatcher.Data;

namespace WebPageWatcher.UI
{
    public interface IListAndSettingPanel
    {
        void UpdateDisplay(object item);
        void SelectItem(object item);
    }
}
