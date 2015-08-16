using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using StackExchange.Redis;
using Xamling.Azure.Portable.Contract;
using Xamling.Azure.Portable.Contract.Cache;
using Xamling.Azure.Portable.Entity;
using XamlingCore.Portable.Contract.Serialise;
using XamlingCore.Portable.Model.Cache;

namespace Xamling.Azure.DocumentDB
{
    public class DocumentEntityCache : IDocumentEntityCache

    {
        private readonly ILifetimeScope _scope;

        public DocumentEntityCache(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public async Task<T> GetEntity<T>(string key, Func<Task<T>> sourceTask, TimeSpan? maxAge = null,
            bool allowExpired = true, bool allowZeroList = true) where T : class, new()
        {
            var e = await GetEntity<T>(key);

            if (e != null)
            {
                return e;
            }

            var result = await sourceTask();

            if (result != null)
            {
                await SetEntity(key, result, maxAge);
            }

            if (result == null && allowExpired)
            {
                return await GetEntity<T>(key);
            }

            return result;
        }

        public async Task<List<T>> QueryEntity<T>(params Expression<Func<XDocumentCacheItem<T>, bool>>[] queries) where T : class, new()
        {
            var repo = _getRepo<T>();
            var typePath = _getTypePath<T>();

            var q = queries.ToList();
            q.Add(_ => _.Id.Contains($"cache:cache_{typePath}"));

            var result = await repo.GetList(q.ToArray());

            if (!result)
            {
                return null;
            }

            return result.Object.Select(_ => _.Item).ToList();
        }

        private IDocumentRepo<XDocumentCacheItem<T>> _getRepo<T>() where T : class, new()
        {
            return _scope.Resolve<IDocumentRepo<XDocumentCacheItem<T>>>();
        }

        public async Task<T> GetCacheItem<T>(string key, TimeSpan? maxAge = null) where T : class, new()
        {
            Debug.WriteLine($"DocumentCache: Getting ${key}");
            var fullName = _getFullKey<T>(key);

            var item = await _getRepo<T>().Get(fullName);

            if (!item || item.Object?.Item == null)
            {
                return null;
            }

            if (!_validateAge(item.Object, maxAge))
            {
                return null;
            }

            return item.Object.Item;
        }

        bool _validateAge<T>(XDocumentCacheItem<T> item, TimeSpan? maxAge = null)
            where T : class, new()
        {
            if (item.MaxAge == null && maxAge == null)
            {
                return true;
            }

            if (maxAge != null)
            {
                item.MaxAge = maxAge;
            }

            var dt = DateTime.UtcNow;
            var dtWithMaxAge = item.DateStamp.Add(item.MaxAge.Value);

            return dtWithMaxAge < dt;
        }



        public async Task<T> GetEntity<T>(string key) where T : class, new()
        {
            var f = await GetCacheItem<T>(key);
            return f;
        }

        public async Task<bool> SetEntity<T>(string key, T item) where T : class, new()
        {
            return await SetEntity(key, item, null);
        }

        public async Task<bool> SetEntity<T>(string key, T item, TimeSpan? maxAge) where T : class, new()
        {
            Debug.WriteLine($"DocumentCache: Setting ${key}");

            var fullName = _getFullKey<T>(key);

            var i = new XDocumentCacheItem<T>();
            i.Item = item;
            i.Id = fullName;

            i.DateStamp = DateTime.UtcNow;
            i.MaxAge = maxAge;

            var result = await _getRepo<T>().AddOrUpdate(i);

            return result != null;
        }

        public async Task<bool> Delete<T>(string key) where T : class, new()
        {
            var fullName = _getFullKey<T>(key);
            return await _getRepo<T>().Delete(fullName);
        }


        string _getFullKey<T>(string key)
        {
            var path = String.Join(":", _getDirPath<T>(), string.Format("{0}.cache", key));
            return path;
        }

        string _getDirPath<T>()
        {
            var p = string.Format("cache_{0}", _getTypePath<T>());
            p = string.Join(":", "cache", p);

            return p;
        }

        string _getTypePath<T>()
        {
            var t = typeof(T);
            var args = t.GenericTypeArguments;

            string tName = t.Name;

            if (args != null)
            {
                foreach (var a in args)
                {
                    tName += "_" + a.Name;
                    if (a.GenericTypeArguments != null)
                    {
                        foreach (var subA in a.GenericTypeArguments)
                        {
                            tName += "_" + subA.Name;
                        }
                    }
                }
            }

            tName = tName.Replace("`", "-g-");

            return tName;
        }
    }
}
