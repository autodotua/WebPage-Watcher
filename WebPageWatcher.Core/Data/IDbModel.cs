using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher.Data
{
    public interface IDbModel:ICloneable,INotifyPropertyChanged
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
