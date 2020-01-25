using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher.Data
{
    public class Trigger : IDbModel
    {
        private int iD;
        public int ID
        {
            get => iD;
            set
            {
                iD = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ID)));
            }
        }
        private string @name;
        public string Name
        {
            get => @name;
            set
            {
                @name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
            }
        }
        private DateTime lastExcuteTime;
        public DateTime LastExcuteTime
        {
            get => lastExcuteTime;
            set
            {
                lastExcuteTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastExcuteTime)));
            }
        }
        private TriggerEvent @event = TriggerEvent.None;
        public TriggerEvent Event
        {
            get => @event;
            set
            {
                @event = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Event)));
            }
        }
        private int event_ID = -1;
        public int Event_ID
        {
            get => event_ID;
            set
            {
                event_ID = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Event_ID)));
            }
        }
        private TriggerOperation operation = TriggerOperation.None;
        public TriggerOperation Operation
        {
            get => operation;
            set
            {
                operation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Operation)));
            }
        }
        private int operation_ID = -1;
        public int Operation_ID
        {
            get => operation_ID;
            set
            {
                operation_ID = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Operation_ID)));
            }
        }
        private string operation_Command="";
        public string Operation_Command
        {
            get => operation_Command;
            set
            {
                operation_Command = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Operation_Command)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Trigger Clone()
        {
            Trigger trigger = MemberwiseClone() as Trigger;
            return trigger;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
    }

    public enum TriggerEvent
    {
        None,
        ExcuteWebPageFailed,
        ExcuteWebPageChanged,
        ExcuteWebPageNotChanged,
        ExcuteScriptFailed,
        ExcuteScriptSucceed,
    }
    public enum TriggerOperation
    {
        None,
        ExcuteWebPage,
        ExcuteScript,
        ExcuteCommand
    }
}

