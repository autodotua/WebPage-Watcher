using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressDialog : UserControl
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }

        public void Show()
        {
            if (dialog.IsOpen)
            {
                return;
            }
            dialog.ShowDialog(dialog.DialogContent);
        }

        public void Close()
        {
            dialog.CurrentSession?.Close();
        }
    }
}