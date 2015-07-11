using System;

namespace Xamling.Azure.Portable.Contract.Cache
{
    public interface IOverrideSessionRedisEntityCache : IRedisEntityCache
    {
        void SetUserId(Guid userId);
    }
}
