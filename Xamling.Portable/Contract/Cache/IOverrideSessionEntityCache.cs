using System;
using XamlingCore.Portable.Contract.Entities;

namespace Xamling.Azure.Portable.Contract.Cache
{
    public interface IOverrideSessionEntityCache : IEntityCache
    {
        void SetUserId(Guid userId);
    }
}
