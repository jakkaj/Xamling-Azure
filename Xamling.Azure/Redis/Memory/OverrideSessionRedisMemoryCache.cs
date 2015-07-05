using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamling.Portable.Contract;
using Xamling.Portable.Contract.Cache;

namespace Xamling.Azure.Redis.Memory
{
    public class OverrideSessionSessionRedisMemoryCache : SharedRedisMemoryCache, IOverrideSessionRedisMemoryCache
    {
        private readonly IOverrideSessionRedisEntityCache _redisCache;

        public OverrideSessionSessionRedisMemoryCache(IOverrideSessionRedisEntityCache redisCache) 
            : base(redisCache)
        {
            _redisCache = redisCache;
        }

        public void SetUserId(Guid userId)
        {
            _redisCache.SetUserId(userId);
        }
    }
}
