using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xamling.Azure.Portable.Entity;
using XamlingCore.Portable.Contract.Entities;

namespace Xamling.Azure.Portable.Contract.Cache
{
    public interface IDocumentEntityCache : IEntityCache
    {
        Task<List<T>> QueryEntity<T>(params Expression<Func<XDocumentCacheItem<T>, bool>>[] queries) where T : class, new();

        Task<List<T>> QueryEntity<T>(string query)
            where T : class, new();

        Task<IQueryable<XDocumentCacheItem<T>>> GetQuery<T>() where T : class, new();
        Task<List<T>> Query<T>(IQueryable<XDocumentCacheItem<T>> query) where T : class, new();
    }
}