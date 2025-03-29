using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace JSViewer_MVC_Core.utils
{
    public class ReportDataCache
    {
        private static MemoryCache cache = new MemoryCache(new MemoryCacheOptions()
        {
            ExpirationScanFrequency = new System.TimeSpan(0, 0, 10)
        });

        public static void setCache(string key, object value)
        {
            var options = new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
            cache.Set(key, value, options);
        }

        public static object getCache(string key, object defaultValue = null)
        {
            object value = null;
            if((value = cache.Get(key)) == null)
            {
                value = defaultValue;
            }
            return value;
        }
    }
}
