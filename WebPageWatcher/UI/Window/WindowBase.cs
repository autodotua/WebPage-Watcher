using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace WebPageWatcher.UI
{
   public class WindowBase : FzLib.Control.Extension.ExtendedWindow
    {
        public WindowBase()
        {
            SetResourceReference(BackgroundProperty, "MaterialDesignPaper");
            SetResourceReference(ForegroundProperty, "MaterialDesignBody");
            //Background = FindResource("MaterialDesignPaper") as Brush;
            //TextElement.SetForeground(this, FindResource("MaterialDesignBody") as Brush);

        }
    }
}
