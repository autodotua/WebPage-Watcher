using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher.Data
{
    public class Script: IDbModel
    {
        public Script()
        {
            Name = "未命名";
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool Enabled { get; set; } = true;
        public int Interval { get; set; } = 1000 * 60 * 15; 
        public DateTime LastExcuteTime { get; set; }
        public Script Clone()
        {
            Script script = MemberwiseClone() as Script;
       
            return script;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
