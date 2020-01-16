using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher.Data
{
    public class BlackWhiteListItem
    {
        public BlackWhiteListItemType Type { get; set; }
        public string Value { get; set; }
        public BlackWhiteListItem Clone()
        {
            return MemberwiseClone() as BlackWhiteListItem;
        }
    }
   public enum BlackWhiteListItemType
    {
        Id=0,
        XPath=1,
        JTokenPath=2,
    }
}
