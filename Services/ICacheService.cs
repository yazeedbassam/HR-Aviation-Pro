using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public interface ICacheService
    {
        // Basic cache operations
        T? Get<T>(string key);
        Task<T?> GetAsync<T>(string key);
        void Set<T>(string key, T value, TimeSpan? expiration = null);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        void Remove(string key);
        Task RemoveAsync(string key);
        bool TryGetValue<T>(string key, out T? value);
        Task<(bool success, T? value)> TryGetValueAsync<T>(string key);

        // Advanced cache operations
        void RemoveByPattern(string pattern);
        Task RemoveByPatternAsync(string pattern);
        void ClearAll();
        Task ClearAllAsync();
        
        // Cache statistics
        CacheStatistics GetStatistics();
        Task<CacheStatistics> GetStatisticsAsync();
        
        // Cache management
        void Refresh<T>(string key, Func<T> factory, TimeSpan? expiration = null);
        Task RefreshAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
        
        // Bulk operations
        void SetMany<T>(Dictionary<string, T> keyValuePairs, TimeSpan? expiration = null);
        Task SetManyAsync<T>(Dictionary<string, T> keyValuePairs, TimeSpan? expiration = null);
        void RemoveMany(IEnumerable<string> keys);
        Task RemoveManyAsync(IEnumerable<string> keys);
    }

    public class CacheStatistics
    {
        public long TotalItems { get; set; }
        public long MemoryUsageBytes { get; set; }
        public long HitCount { get; set; }
        public long MissCount { get; set; }
        public double HitRate => TotalRequests > 0 ? (double)HitCount / TotalRequests : 0;
        public long TotalRequests => HitCount + MissCount;
        public DateTime LastReset { get; set; }
    }
} 