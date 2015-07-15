using System;
using System.Threading.Tasks;

namespace Xamling.Azure.Portable.Contract.Cache
{
    public interface IDocumentEntityCache
    {
        Task<T> GetEntity<T>(string key, Func<Task<T>> sourceTask, TimeSpan? maxAge = null,
            bool allowExpired = true, bool allowZeroList = true) where T : class, IKeyEntity, new();

        Task<T> GetCacheItem<T>(string key, TimeSpan? maxAge = null) where T : class, IKeyEntity, new();
        Task<T> GetEntity<T>(string key) where T : class, IKeyEntity, new();
        Task<bool> SetEntity<T>(string key, T item) where T : class, IKeyEntity, new();
        Task<bool> SetEntity<T>(string key, T item, TimeSpan? maxAge) where T : class, IKeyEntity, new();
    }
}