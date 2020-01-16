using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher.Data
{
    public class Cookie
    {
        public Cookie()
        {
        }

        public Cookie(string name, string value)
        {
            Key = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Key { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return Key + ": " + Value;
        }
    }
}
