using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AIProject.Interfaces;

namespace AIProject.Services
{
    /// <summary>
    /// Implementación en memoria del servicio de caché.
    /// Esta implementación es útil para desarrollo y pruebas.
    /// </summary>
    public class InMemoryCacheService : ICacheService
    {
        private class CacheItem
        {
            public object Value { get; set; }
            public DateTime ExpirationTime { get; set; }
        }

        private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (_cache.TryGetValue(key, out var item))
            {
                if (DateTime.UtcNow < item.ExpirationTime)
                {
                    return Task.FromResult(item.Value as T);
                }
                
                // Remove expired item
                _cache.TryRemove(key, out _);
            }

            return Task.FromResult<T?>(null);
        }

        public Task<bool> SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var cacheItem = new CacheItem
            {
                Value = value,
                ExpirationTime = DateTime.UtcNow.Add(expiration)
            };

            _cache[key] = cacheItem;
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return Task.FromResult(_cache.TryRemove(key, out _));
        }

        public Task<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (_cache.TryGetValue(key, out var item))
            {
                if (DateTime.UtcNow < item.ExpirationTime)
                {
                    return Task.FromResult(true);
                }
                
                // Remove expired item
                _cache.TryRemove(key, out _);
            }

            return Task.FromResult(false);
        }

        public Task<bool> UpdateExpirationAsync(string key, TimeSpan expiration)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (_cache.TryGetValue(key, out var item))
            {
                if (DateTime.UtcNow < item.ExpirationTime)
                {
                    item.ExpirationTime = DateTime.UtcNow.Add(expiration);
                    return Task.FromResult(true);
                }
                
                // Remove expired item
                _cache.TryRemove(key, out _);
            }

            return Task.FromResult(false);
        }
    }
}
