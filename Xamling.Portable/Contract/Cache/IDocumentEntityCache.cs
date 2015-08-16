using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xamling.Azure.Portable.Entity;
using XamlingCore.Portable.Contract.Entities;

namespace Xamling.Azure.Portable.Contract.Cache
{
    public interface IDocumentEntityCache : IEntityCache
    {
        Task<List<T>> QueryEntity<T>(params Expression<Func<XDocumentCacheItem<T>, bool>>[] queries) where T : class, new();
    }
}