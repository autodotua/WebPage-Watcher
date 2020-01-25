using FzLib.Basic.Collection;
using FzLib.UI.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using WebPageWatcher.Data;
using WebPageWatcher.Web;
using Trigger = WebPageWatcher.Data.Trigger;

namespace WebPageWatcher.UI
{
    public partial class TriggerPanel : TriggerPanelBase
    {
        private void UpdateEventSelector()
        {
            bool webPageVisiable = Item.Event == TriggerEvent.ExcuteWebPageFailed || Item.Event == TriggerEvent.ExcuteWebPageChanged||Item.Event==TriggerEvent.ExcuteWebPageNotChanged;
            bool scriptVisiable = Item.Event == TriggerEvent.ExcuteScriptFailed || Item.Event == TriggerEvent.ExcuteScriptSucceed;
            tbkEventWebPage.Visibility = webPageVisiable ? Visibility.Visible : Visibility.Collapsed;
            cbbEventWebPage.Visibility = webPageVisiable ? Visibility.Visible : Visibility.Collapsed;
            tbkEventScript.Visibility = scriptVisiable ? Visibility.Visible : Visibility.Collapsed;
            cbbEventScript.Visibility = scriptVisiable ? Visibility.Visible : Visibility.Collapsed;

            if (webPageVisiable)
            {
                EventItem = BackgroundTask.WebPages.FirstOrDefault(p => p.ID == Item.Event_ID);
            }
            else if (scriptVisiable)
            {
                EventItem = BackgroundTask.Scripts.FirstOrDefault(p => p.ID == Item.Event_ID);
            }
        }


        private void UpdateOperationSelector()
        {
            bool webPageVisiable = Item.Operation == TriggerOperation.ExcuteWebPage;
            bool scriptVisiable = Item.Operation == TriggerOperation.ExcuteScript;
            bool commandVisiable = Item.Operation == TriggerOperation.ExcuteCommand;

            tbkOperationWebPage.Visibility = webPageVisiable ? Visibility.Visible : Visibility.Collapsed;
            cbbOperationWebPage.Visibility = webPageVisiable ? Visibility.Visible : Visibility.Collapsed;
      
            tbkOperationScript.Visibility = scriptVisiable ? Visibility.Visible : Visibility.Collapsed;
            cbbOperationScript.Visibility = scriptVisiable ? Visibility.Visible : Visibility.Collapsed;

            tbkOperationCommand.Visibility = commandVisiable ? Visibility.Visible : Visibility.Collapsed;
            txtOperationCommand.Visibility = commandVisiable ? Visibility.Visible : Visibility.Collapsed;

            if (webPageVisiable)
            {
                OperationItem = BackgroundTask.WebPages.FirstOrDefault(p => p.ID == Item.Operation_ID);
            }
            else if (scriptVisiable)
            {
                OperationItem = BackgroundTask.Scripts.FirstOrDefault(p => p.ID == Item.Operation_ID);
            }
        }

        private IDbModel eventItem;
        public IDbModel EventItem
        {
            get => eventItem;
            set => SetValueAndNotify(ref eventItem, value, nameof(EventItem));
        }
        private IDbModel operationItem;
        public IDbModel OperationItem
        {
            get => operationItem;
            set => SetValueAndNotify(ref operationItem, value, nameof(OperationItem));
        }

        public TriggerPanel()
        {
            BackgroundTask.WebPagesChanged += (p1, p2) => Notify(nameof(Item));
            InitializeComponent();

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Trigger script = await DbHelper.InsertAsync<Trigger>();
            Items.Add(script);
            lvw.SelectedItem = script;
        }

        public override void UpdateDisplay(Trigger item)
        {
            Trigger here = Items.FirstOrDefault(p => p.ID == (item as Trigger).ID);
            if (here != null)
            {
                if (Item != null && here.ID == Item.ID)
                {
                    Item.LastExcuteTime = item.LastExcuteTime;
                    Notify(nameof(Item));
                }
            }
        }
        public IEnumerable<TriggerEvent> Events { get; } = Enum.GetValues(typeof(TriggerEvent)).Cast<TriggerEvent>();
        public IEnumerable<TriggerOperation> Operations { get; } = Enum.GetValues(typeof(TriggerOperation)).Cast<TriggerOperation>();
        public override ExtendedObservableCollection<Trigger> Items => BackgroundTask.Triggers;

        public override ListView List => lvw;



        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Item.Event_ID = EventItem == null ? -1 : EventItem.ID;
            Item.Operation_ID = OperationItem == null ? -1 : OperationItem.ID;
            await UpdateItem();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetItem();
        }


        private void TriggerPanelBase_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Item):
                    UpdateEventSelector();
                    UpdateOperationSelector(); 
                    Item.PropertyChanged += Item_PropertyChanged;

                    break;
                case nameof(Trigger.Event):
                    UpdateEventSelector();
                    break;
                case nameof(Trigger.Operation):
                    UpdateOperationSelector();
                    break;
            }
        }

        private void TriggerPanelBase_Initialized(object sender, EventArgs e)
        {
            PropertyChanged += Item_PropertyChanged;
        }
    }
}
