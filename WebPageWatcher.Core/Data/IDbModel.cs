using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher.Data
{
    public interface IDbModel:ICloneable
    {
        public int ID { get; set; }
    }
}
