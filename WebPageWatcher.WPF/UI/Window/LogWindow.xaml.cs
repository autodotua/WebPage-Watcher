using FzLib.Basic;
using FzLib.Extension;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WebPageWatcher.Data;

namespace WebPageWatcher.UI
{
    public partial class LogWindow : WindowBase
    {
        public LogWindow()
        {
            InitializeComponent();
            grdBar.Children.OfType<DatePicker>().ForEach(p =>
            p.BlackoutDates.Add(new CalendarDateRange(DateTime.Today.AddDays(1), DateTime.MaxValue)));
        }

        private DateTime beginTime = DateTime.Today;

        public DateTime BeginTime
        {
            get => beginTime;
            set
            {
                if (value > EndTime)
                {
                    value = EndTime;
                }

                beginTime = value;
                this.Notify();
            }
        }

        private DateTime endTime = DateTime.Today;

        public DateTime EndTime
        {
            get => endTime;
            set
            {
                if (value < BeginTime)
                {
                    value = BeginTime;
                }
                endTime = value;
                this.Notify();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            lvw.ItemsSource = (await DbHelper.GetLogsAsync(BeginTime, EndTime)).ToArray();
        }
    }

    public class PastDateValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!DateTime.TryParse((value ?? "").ToString(),
                CultureInfo.CurrentCulture,
                DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces,
                out DateTime time)) return new ValidationResult(false, "");

            return time.Date > DateTime.Now.Date
                ? new ValidationResult(false, "")
                : ValidationResult.ValidResult;
        }
    }
}