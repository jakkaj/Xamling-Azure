using System;
using XamlingCore.Portable.Contract.Entities;

namespace Xamling.Portable.Contract.Cache
{
    public interface IOverrideSessionEntityCache : IEntityCache
    {
        void SetUserId(Guid userId);
    }
}
