using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebPageWatcher.Web;

namespace WebPageWatcher.Data
{
    public class Log : IDbModel
    {
        public Log()
        {
        }

        public Log(string typeKey, string message, int item_ID)
        {
            Type = typeKey;
            Message = message;
            Item_ID = item_ID;
            Time = DateTime.Now;
        }

        public int ID { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public int Item_ID { get; set; }
        public DateTime Time { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}

