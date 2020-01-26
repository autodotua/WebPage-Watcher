using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebPageWatcher.Web;

namespace WebPageWatcher.Data
{
    public class Script : ITaskDbModel
    {
        public Script()
        {
        }

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
        private string @name = Strings.Get("model_unnamed");
        public string Name
        {
            get => @name;
            set
            {
                @name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
        private string code;
        public string Code
        {
            get => code;
            set
            {
                code = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Code)));
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
        private int interval = 1000 * 60 * 15;
        public int Interval
        {
            get => interval;
            set
            {
                interval = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Interval)));
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

        public event PropertyChangedEventHandler PropertyChanged;

        public Script Clone()
        {
            Script script = MemberwiseClone() as Script;

            return script;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        public override string ToString()
        {
            return Name;
        }
    }
}

