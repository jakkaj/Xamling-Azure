using System;

namespace Xamling.Portable.Contract.Cache
{
    public interface IOverrideSessionRedisEntityCache : IRedisEntityCache
    {
        void SetUserId(Guid userId);
    }
}
