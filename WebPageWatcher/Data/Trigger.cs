using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher.Data
{
    public class Trigger : IDbModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public DateTime LastExcuteTime { get; set; }

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
}
