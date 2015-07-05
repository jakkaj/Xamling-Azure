﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamling.Portable.Contract;
using Xamling.Portable.Contract.Cache;

namespace Xamling.Azure.Redis.Memory
{
    public class SecureSessionSessionRedisMemoryCache : SharedRedisMemoryCache, ISecureSessionRedisMemoryCache
    {
        public SecureSessionSessionRedisMemoryCache(ISecureSessionRedisEntityCache cache)
            : base(cache)
        {
            
        }
    }
}