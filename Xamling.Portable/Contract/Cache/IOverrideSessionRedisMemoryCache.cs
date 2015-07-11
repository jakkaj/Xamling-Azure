using System;
using XamlingCore.Portable.Contract.Entities;

namespace Xamling.Azure.Portable.Contract.Cache
{
    public interface IOverrideSessionRedisMemoryCache : IMemoryCache
    {
        void SetUserId(Guid userId);
    }
}
