using FzLib.UI.Extension;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebPageWatcher.UI
{
    /// <summary>
    /// Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : ExtendedUserControl
    {
        public InputDialog()
        {
            InitializeComponent();
        }
        public async Task<string> ShowAsync(string title, bool multipleLines,string hint="", string defaultContent = "")
        {
            tbkDialogTitle.Text = title;
            HintAssist.SetHint(textArea, hint);
            textLine.Visibility = multipleLines ? Visibility.Collapsed : Visibility.Visible;
            textArea.Visibility = multipleLines ? Visibility.Visible : Visibility.Collapsed;
            InputContent = defaultContent;
            Notify(nameof(InputContent));
            await dialog.ShowDialog(dialog.DialogContent);
            return Result ? InputContent : "";
        }
        public string InputContent { get; set; }
        public bool Result { get; set; }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender == btnOk)
            {
                Result = true;
            }
            else if (sender == btnCancel)
            {
                Result = false;
            }

            dialog.CurrentSession.Close();
        }
    }
}
