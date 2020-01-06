using FzLib.Control.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WebPageWatcher.UI
{
    public class NotificationWindowBase : ExtendedWindow
    {
        public NotificationWindowBase():base()
        {
            AllowsTransparency = true;
            WindowStyle = System.Windows.WindowStyle.None;
            ShowInTaskbar = false;
            SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            Opacity = 0.8;
            Background = Brushes.Black;
            Foreground = Brushes.White;
            Topmost = true;
        }

        public void PopUp()
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right;
            Top = desktopWorkingArea.Bottom;
            Loaded += NotificationWindowBase_Loaded;
            Show();
        }

        private void NotificationWindowBase_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= NotificationWindowBase_Loaded;
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;

            this.Top = desktopWorkingArea.Bottom-ActualHeight;
            DoubleAnimation ani = new DoubleAnimation(desktopWorkingArea.Right - ActualWidth, TimeSpan.FromSeconds(1)) { EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut } };
            BeginAnimation(LeftProperty, ani);
        }

        public void TakeBack()
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;

            DoubleAnimation ani = new DoubleAnimation(desktopWorkingArea.Right, TimeSpan.FromSeconds(1)) { EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut } };
            ani.Completed += (p1, p2) =>
            {
                Close();
            };
            BeginAnimation(LeftProperty, ani);
        }
    }
}
