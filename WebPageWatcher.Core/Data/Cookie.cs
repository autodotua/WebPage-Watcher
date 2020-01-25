using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher.Data
{
    public class Cookie:INotifyPropertyChanged
    {
        public Cookie()
        {
        }

        public Cookie(string name, string value)
        {
            Key = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        private string key;
        public string Key
        {
            get => key;
            set
            {
                key = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Key)));
            }
        }
        private string value;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Value
        {
            get => value;
            set
            {
                value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public override string ToString()
        {
            return Key + ": " + Value;
        }
    }
}

