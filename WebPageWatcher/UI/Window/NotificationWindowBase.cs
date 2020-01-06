using FzLib.Control.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            //从右向左弹出
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
                closing = true;
                Close();
            };
            BeginAnimation(LeftProperty, ani);
        }
        bool closing = false;
        protected override void OnClosing(CancelEventArgs e)
        {
            //只允许通过按钮关闭窗体
            if(!closing)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }
    }
}
