using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamling.Portable.Contract;
using Xamling.Portable.Contract.Cache;
using XamlingCore.Portable.Contract.Entities;
using XamlingCore.Portable.Contract.Repos.Base;
using XamlingCore.Portable.Data.Entities;

namespace Xamling.Azure.EntityCaches
{
    public class SharedEntityCache : EntityCache, ISharedEntityCache
    {
        public SharedEntityCache(IStorageFileRepo storageFileRepo, 
            ISharedRedisMemoryCache cache) : base(storageFileRepo, cache)
        {
        }

        public override string GetKey(string key)
        {
            return base.GetKey("shared\\" + key);
        }
    }
}
