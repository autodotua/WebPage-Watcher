using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher.Data
{
    public class WebPageUpdate : IDbModel
    {
        public int ID { get; set; }
        public int WebPage_ID { get; set; }
        public DateTime Time { get; set; }
        public byte[] NewContent { get; set; }

        public WebPageUpdate Clone()
        {
            WebPageUpdate update = MemberwiseClone() as WebPageUpdate;

            return update;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
