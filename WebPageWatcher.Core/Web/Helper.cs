using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher.Web
{
    public static class Helper
    {
        public static string ToEncodedString(this byte[] bytes)
        {
            return Config.Instance.Encoding.GetString(bytes);
        }
        public static byte[] ToByteArray(this string text)
        {
            return Config.Instance.Encoding.GetBytes(text);
        }
    }
}
