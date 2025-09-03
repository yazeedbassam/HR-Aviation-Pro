using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public class AutoCacheClearMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AutoCacheClearMiddleware> _logger;
        
        public AutoCacheClearMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<AutoCacheClearMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // مسح الكاش تلقائياً كل 5 دقائق
                var lastClearTime = _cache.Get<DateTime>("LastAutoClearTime");
                if (lastClearTime == default(DateTime) || DateTime.Now - lastClearTime > TimeSpan.FromMinutes(5))
                {
                    await ClearExpiredCache();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in auto-cache clear middleware");
            }
            
            await _next(context);
        }
        
        private async Task ClearExpiredCache()
        {
            try
            {
                var keysToRemove = new List<string>();
                
                // البحث عن الكاش المنتهي الصلاحية
                // استخدام reflection للحصول على مفاتيح الكاش
                var cacheField = _cache.GetType().GetField("_coherentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (cacheField != null)
                {
                    var coherentState = cacheField.GetValue(_cache);
                    if (coherentState != null)
                    {
                        var entriesCollection = coherentState.GetType().GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (entriesCollection != null)
                        {
                            var entries = entriesCollection.GetValue(coherentState) as System.Collections.IDictionary;
                            if (entries != null)
                            {
                                foreach (var entry in entries.Keys)
                                {
                                    var key = entry.ToString();
                                    if (key.StartsWith("CacheTime_"))
                                    {
                                        try
                                        {
                                            var cacheTime = _cache.Get<DateTime>(key);
                                            if (DateTime.Now - cacheTime > TimeSpan.FromMinutes(5))
                                            {
                                                var permissionKey = key.Replace("CacheTime_", "");
                                                keysToRemove.Add(permissionKey);
                                                keysToRemove.Add(key);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogWarning(ex, "Error processing cache key: {Key}", key);
                                            // إضافة المفتاح للمسح في حالة الخطأ
                                            keysToRemove.Add(key);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                // مسح الكاش المنتهي الصلاحية
                foreach (var key in keysToRemove)
                {
                    try
                    {
                        _cache.Remove(key);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error removing cache key: {Key}", key);
                    }
                }
                
                if (keysToRemove.Count > 0)
                {
                    _logger.LogInformation("Auto-cache clear completed. Removed {Count} expired entries", keysToRemove.Count);
                }
                
                _cache.Set("LastAutoClearTime", DateTime.Now, TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in auto-cache clear middleware");
            }
        }
    }
} 