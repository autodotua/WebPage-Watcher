using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class AboutDialog : UserControl
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        public async Task Show()
        {
            await dialog.ShowDialog(dialog.DialogContent);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            dialog.CurrentSession.Close();
        }
    }
}