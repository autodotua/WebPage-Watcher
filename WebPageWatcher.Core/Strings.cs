using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Text;

namespace WebPageWatcher
{
    public static class Strings
    {
        private static ResourceManager resourceManager = new ResourceManager("WebPageWatcher.StringResources", Assembly.GetExecutingAssembly());
        public static string Get(string key)
        {
            return resourceManager.GetString(key);
        }
    }
}
