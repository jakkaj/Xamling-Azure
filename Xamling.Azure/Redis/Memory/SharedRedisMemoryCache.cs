using System;
using System.Threading.Tasks;
using Xamling.Azure.Portable.Contract.Cache;
using XamlingCore.Portable.Contract.Entities;
using XamlingCore.Portable.Model.Cache;

namespace Xamling.Azure.Redis.Memory
{
    public class SharedRedisMemoryCache : ISharedRedisMemoryCache
    {
        private readonly IRedisEntityCache _redisCache;

        public SharedRedisMemoryCache(IRedisEntityCache redisCache)
        {
            _redisCache = redisCache;
        }

        private bool _enabled = true;
        public async Task Clear()
        {
            //throw new NotImplementedException();
        }

        public async Task Disable()
        {
            _enabled = false;
        }

        public async Task Enable()
        {
            _enabled = true;
        }

        public async Task<XCacheItem<T>> Get<T>(string key) where T : class, new()
        {
            if (!_enabled)
            {
                return null;
            }

            return await _redisCache.GetEntity<XCacheItem<T>>(key);
        }

        public async Task<XCacheItem<T>> Set<T>(string key, T item, TimeSpan? maxAge) where T : class, new()
        {
            var cacheItem = new XCacheItem<T>();

            cacheItem.DateStamp = DateTime.UtcNow;

            cacheItem.Item = item;
            cacheItem.Key = key;
            cacheItem.MaxAge = maxAge;

            if (maxAge != null && maxAge.Value > TimeSpan.FromDays(7))
            {
                maxAge = TimeSpan.FromDays(7);
            }

            var result = await _redisCache.SetEntity(key, cacheItem, maxAge);

            return cacheItem;
        }

        public async Task<bool> Delete<T>(string key) where T : class, new()
        {
            return await _redisCache.Delete<XCacheItem<T>>(key);
        }
    }
}
