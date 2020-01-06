using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher
{
   public class Config : FzLib.DataStorage.Serialization.JsonSerializationBase
    {
        private static Config instance;
        public static Config Instance
        {
            get
            {
                if(instance==null)
                {
                    instance = OpenOrCreate<Config>();
                }
                return instance;
            }
        }
        public bool WaitForScriptComplete { get; set; } = true;
        public string Language { get; set; } = "en-US";
        //public string Language { get; set; } = "zh-CN";
    }
}
