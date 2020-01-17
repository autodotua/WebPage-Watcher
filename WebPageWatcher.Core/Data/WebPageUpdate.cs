using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher.Data
{
    public class WebPageUpdate : IDbModel
    {
        public WebPageUpdate()
        {
        }

        public WebPageUpdate(int webPageID,byte[] newContent)
        {
            WebPage_ID = webPageID;
            Content = newContent;
            Time = DateTime.Now;
        }
        public int ID { get; set; }
        public int WebPage_ID { get; set; }
        public DateTime Time { get; set; }
        public byte[] Content { get; set; }

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
