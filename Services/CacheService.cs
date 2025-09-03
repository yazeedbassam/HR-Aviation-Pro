using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;
        private readonly CacheStatistics _statistics;
        private readonly object _statisticsLock = new object();

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _statistics = new CacheStatistics { LastReset = DateTime.UtcNow };
        }

        #region Basic Cache Operations

        public T? Get<T>(string key)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out var value))
                {
                    IncrementHitCount();
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return (T?)value;
                }

                IncrementMissCount();
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
                return default(T);
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            return await Task.FromResult(Get<T>(key));
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new MemoryCacheEntryOptions();
                
                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration;
                }
                else
                {
                    // Default expiration based on key type
                    options.AbsoluteExpirationRelativeToNow = GetDefaultExpiration(key);
                }

                // Add cache entry callbacks
                options.RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
                {
                    _logger.LogDebug("Cache entry evicted: {Key}, Reason: {Reason}", evictedKey, reason);
                    DecrementItemCount();
                });

                _memoryCache.Set(key, value, options);
                IncrementItemCount();
                
                _logger.LogDebug("Cache set for key: {Key}, Expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            await Task.Run(() => Set(key, value, expiration));
        }

        public void Remove(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                DecrementItemCount();
                _logger.LogDebug("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            await Task.Run(() => Remove(key));
        }

        public bool TryGetValue<T>(string key, out T? value)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out var cachedValue))
                {
                    value = (T?)cachedValue;
                    IncrementHitCount();
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return true;
                }

                value = default(T);
                IncrementMissCount();
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TryGetValue for key: {Key}", key);
                value = default(T);
                return false;
            }
        }

        public async Task<(bool success, T? value)> TryGetValueAsync<T>(string key)
        {
            var result = TryGetValue<T>(key, out var value);
            return await Task.FromResult((result, value));
        }

        #endregion

        #region Advanced Cache Operations

        public void RemoveByPattern(string pattern)
        {
            try
            {
                var regex = new Regex(pattern.Replace("*", ".*"));
                var keysToRemove = new List<string>();

                // Get all cache keys using reflection (this is a limitation of IMemoryCache)
                var field = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field?.GetValue(_memoryCache) is IDictionary<object, object> entries)
                {
                    foreach (var key in entries.Keys)
                    {
                        try
                        {
                            if (key is string stringKey && regex.IsMatch(stringKey))
                            {
                                keysToRemove.Add(stringKey);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error processing cache key: {Key}", key);
                        }
                    }
                }

                foreach (var key in keysToRemove)
                {
                    try
                    {
                        Remove(key);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error removing cache key: {Key}", key);
                    }
                }

                _logger.LogInformation("Removed {Count} cache entries matching pattern: {Pattern}", keysToRemove.Count, pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache entries by pattern: {Pattern}", pattern);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            await Task.Run(() => RemoveByPattern(pattern));
        }

        public void ClearAll()
        {
            try
            {
                // IMemoryCache doesn't have a Clear method, so we need to use reflection
                var field = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field?.GetValue(_memoryCache) is IDictionary<object, object> entries)
                {
                    var keys = entries.Keys.OfType<string>().ToList();
                    var removedCount = 0;
                    
                    foreach (var key in keys)
                    {
                        try
                        {
                            _memoryCache.Remove(key);
                            removedCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error removing cache key: {Key}", key);
                        }
                    }
                    
                    lock (_statisticsLock)
                    {
                        _statistics.TotalItems = 0;
                    }
                    
                    _logger.LogInformation("Cleared all cache entries. Total removed: {Count}", removedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all cache entries");
            }
        }

        public async Task ClearAllAsync()
        {
            await Task.Run(() => ClearAll());
        }

        #endregion

        #region Cache Statistics

        public CacheStatistics GetStatistics()
        {
            lock (_statisticsLock)
            {
                return new CacheStatistics
                {
                    TotalItems = _statistics.TotalItems,
                    MemoryUsageBytes = _statistics.MemoryUsageBytes,
                    HitCount = _statistics.HitCount,
                    MissCount = _statistics.MissCount,
                    LastReset = _statistics.LastReset
                };
            }
        }

        public async Task<CacheStatistics> GetStatisticsAsync()
        {
            return await Task.FromResult(GetStatistics());
        }

        #endregion

        #region Cache Management

        public void Refresh<T>(string key, Func<T> factory, TimeSpan? expiration = null)
        {
            try
            {
                var value = factory();
                Set(key, value, expiration);
                _logger.LogDebug("Cache refreshed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache for key: {Key}", key);
            }
        }

        public async Task RefreshAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            try
            {
                var value = await factory();
                await SetAsync(key, value, expiration);
                _logger.LogDebug("Cache refreshed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache for key: {Key}", key);
            }
        }

        #endregion

        #region Bulk Operations

        public void SetMany<T>(Dictionary<string, T> keyValuePairs, TimeSpan? expiration = null)
        {
            try
            {
                foreach (var kvp in keyValuePairs)
                {
                    Set(kvp.Key, kvp.Value, expiration);
                }
                
                _logger.LogDebug("Set {Count} cache entries in bulk", keyValuePairs.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting multiple cache entries");
            }
        }

        public async Task SetManyAsync<T>(Dictionary<string, T> keyValuePairs, TimeSpan? expiration = null)
        {
            await Task.Run(() => SetMany(keyValuePairs, expiration));
        }

        public void RemoveMany(IEnumerable<string> keys)
        {
            try
            {
                foreach (var key in keys)
                {
                    Remove(key);
                }
                
                _logger.LogDebug("Removed {Count} cache entries in bulk", keys.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing multiple cache entries");
            }
        }

        public async Task RemoveManyAsync(IEnumerable<string> keys)
        {
            await Task.Run(() => RemoveMany(keys));
        }

        #endregion

        #region Private Helper Methods

        private TimeSpan GetDefaultExpiration(string key)
        {
            // Default expiration based on key type
            if (key.StartsWith("permission_"))
                return TimeSpan.FromMinutes(5);
            if (key.StartsWith("Configuration_"))
                return TimeSpan.FromMinutes(30);
            if (key.StartsWith("menu_"))
                return TimeSpan.FromMinutes(10);
            if (key.StartsWith("user_"))
                return TimeSpan.FromMinutes(15);
            
            return TimeSpan.FromMinutes(20); // Default
        }

        private void IncrementHitCount()
        {
            lock (_statisticsLock)
            {
                _statistics.HitCount++;
            }
        }

        private void IncrementMissCount()
        {
            lock (_statisticsLock)
            {
                _statistics.MissCount++;
            }
        }

        private void IncrementItemCount()
        {
            lock (_statisticsLock)
            {
                _statistics.TotalItems++;
            }
        }

        private void DecrementItemCount()
        {
            lock (_statisticsLock)
            {
                if (_statistics.TotalItems > 0)
                    _statistics.TotalItems--;
            }
        }

        #endregion
    }
} 