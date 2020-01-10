using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOPath = System.IO.Path;

namespace WebPageWatcher
{
    public class Config : FzLib.DataStorage.Serialization.JsonSerializationBase
    {
        private static Config instance;

        public static string DataPath
        {
            get
            {
#if DEBUG
                string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FzLib.Program.App.ProgramName, "Debug");
#else
                string path= IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FzLib.Program.App.ProgramName);
#endif
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = OpenOrCreate<Config>(IOPath.Combine(DataPath, "config.json"));
                }
                return instance;
            }
        }
        /// <summary>
        /// 程序语言，支持zh-CN和en-US
        /// </summary>
        public string Language { get; set; } = "zh-CN";
        /// <summary>
        /// 两色暗色主题。1为亮色，-1为暗色，0为跟随系统
        /// </summary>
        public int Theme { get; set; } = 0;
        /// <summary>
        /// 0为不播放，1为默认，2为
        /// </summary>
        public int Ring { get; set; } = 0;
        public string CustomRingName { get; set; } = null;
        [Newtonsoft.Json.JsonIgnore]
        public string CustomRingPath => IOPath.Combine(DataPath, CustomRingName);
        [Newtonsoft.Json.JsonIgnore]
        public Encoding Encoding => Encoding.UTF8;

    }
}
