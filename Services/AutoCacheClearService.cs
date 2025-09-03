using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public class AutoCacheClearService : BackgroundService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<AutoCacheClearService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(2); // كل دقيقتين
        
        public AutoCacheClearService(IMemoryCache cache, ILogger<AutoCacheClearService> logger)
        {
            _cache = cache;
            _logger = logger;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ClearExpiredCache();
                    await Task.Delay(_period, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in auto-cache clear service");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // انتظار دقيقة في حالة الخطأ
                }
            }
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
                                            if (DateTime.Now - cacheTime > TimeSpan.FromMinutes(3))
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing expired cache");
            }
        }
    }
} 