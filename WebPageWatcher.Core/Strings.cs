using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Text;

namespace WebPageWatcher.Web
{
    internal static class Strings
    {
        internal static ResourceManager resourceManager = new ResourceManager("WebPageWatcher.StringResources", Assembly.GetExecutingAssembly());
        public static string Get(string key)
        {
            return resourceManager.GetString(key);
        }
    }
}
