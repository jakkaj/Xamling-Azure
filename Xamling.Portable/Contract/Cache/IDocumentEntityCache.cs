using System;
using System.Threading.Tasks;
using XamlingCore.Portable.Contract.Entities;

namespace Xamling.Azure.Portable.Contract.Cache
{
    public interface IDocumentEntityCache : IEntityCache
    {
        //Task<T> GetEntity<T>(string key, Func<Task<T>> sourceTask, TimeSpan? maxAge = null,
        //    bool allowExpired = true, bool allowZeroList = true) where T : class, new();

        //Task<T> GetCacheItem<T>(string key, TimeSpan? maxAge = null) where T : class, new();
        //Task<T> GetEntity<T>(string key) where T : class, new();
        //Task<bool> SetEntity<T>(string key, T item) where T : class, new();
        //Task<bool> SetEntity<T>(string key, T item, TimeSpan? maxAge) where T : class, new();
    }
}