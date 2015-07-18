using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private DocumentCollection _collection;
       
        private DocumentClient _client;

        public DocumentEntityCache(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public async Task<T> GetEntity<T>(string key, Func<Task<T>> sourceTask, TimeSpan? maxAge = null,
            bool allowExpired = true, bool allowZeroList = true) where T: class, new()
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

        private IDocumentRepo<XDocumentCacheItem<T>>_getRepo<T>() where T :class, new()
        {
            return _scope.Resolve<IDocumentRepo<XDocumentCacheItem<T>>>();
        }

        public async Task<T> GetCacheItem<T>(string key, TimeSpan? maxAge = null) where T : class, new()
        {
            var fullName = _getFullKey<T>(key);

            var item = await _getRepo<T>().Get(fullName); 

            if (!item)
            {
                return null;
            }

            return item.Object.Item;
        }

        //public async Task<List<T>> GetEntityList<T>(string key, bool clear = false) where T : class, new()
        //{
        //    if (!_database.Multiplexer.IsConnected)
        //    {
        //        return null;
        //    }

        //    var fullName = _getFullKey<T>(key);

        //    var tran = _database.CreateTransaction();

        //    var dataFromRedisTask = tran.ListRangeAsync(fullName, 0, -1);

        //    if (clear)
        //    {
        //        var delTask = tran.KeyDeleteAsync(fullName);
        //    }


        //    await tran.ExecuteAsync();

        //    var dataFromRedis = await dataFromRedisTask;

        //    if (dataFromRedis == null || dataFromRedis.Length == 0)
        //    {
        //        return null;
        //    }

        //    return dataFromRedis.Select(item => Deserialise<T>(item)).ToList();
        //}

        //public async Task<List<string>> GetEntityList(string key, bool clear = false)
        //{
        //    if (!_database.Multiplexer.IsConnected)
        //    {
        //        return null;
        //    }

        //    var fullName = _getFullKey<string>(key);

        //    var tran = _database.CreateTransaction();

        //    var dataFromRedisTask = tran.ListRangeAsync(fullName, 0, -1);

        //    if (clear)
        //    {
        //        var delTask = tran.KeyDeleteAsync(fullName);
        //    }

        //    await tran.ExecuteAsync();

        //    var dataFromRedis = await dataFromRedisTask;

        //    if (dataFromRedis == null || dataFromRedis.Length == 0)
        //    {
        //        return null;
        //    }

        //    return dataFromRedis.Select(item => item.ToString()).ToList();
        //}

        //public async Task<string> GetEntityListRightPop(string key)
        //{
        //    if (!_database.Multiplexer.IsConnected)
        //    {
        //        return null;
        //    }

        //    var fullName = _getFullKey<string>(key);

        //    var dataFromRedis = await _database.ListRightPopAsync(fullName);

        //    return dataFromRedis;
        //}

        //public async Task<long> GetListLength(string key)
        //{
        //    if (!_database.Multiplexer.IsConnected)
        //    {
        //        return 0;
        //    }

        //    var fullName = _getFullKey<string>(key);
        //    var dataFromRedis = await _database.ListLengthAsync(fullName);
        //    return dataFromRedis;
        //}


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
            var fullName = _getFullKey<T>(key);

            var i = new XDocumentCacheItem<T>();
            i.Item = item;
            i.Id = fullName;
            
            i.DateStamp = DateTime.UtcNow;
            i.MaxAge = maxAge;

            var result = await _getRepo<T>().AddOrUpdate(i);

            return result != null;
        }

        //public async Task<bool> SetEntityList<T>(string key, T item, TimeSpan? maxAge = null) where T : class, new()
        //{
        //    if (!_database.Multiplexer.IsConnected)
        //    {
        //        return false;
        //    }

        //    var fullName = _getFullKey<T>(key);

        //    var serialised = Serialise(item);

        //    if (maxAge == null)
        //    {
        //        maxAge = TimeSpan.FromDays(30);
        //    }

        //    var result = await XResiliant.Default.RunBool(
        //        async () => await _database.ListLeftPushAsync(fullName, serialised) != -1);

        //    if (!result)
        //    {
        //        return false;
        //    }

        //    await XResiliant.Default.RunBool(async () =>
        //    {
        //        await _database.KeyExpireAsync(fullName, DateTime.UtcNow.Add(maxAge.Value));
        //        return true;
        //    });


        //    return true;
        //}

        //public async Task<bool> SetEntityList(string key, string item, TimeSpan? maxAge = null)
        //{
        //    if (!_database.Multiplexer.IsConnected)
        //    {
        //        return false;
        //    }

        //    var fullName = _getFullKey<string>(key);

        //    if (maxAge == null)
        //    {
        //        maxAge = TimeSpan.FromDays(30);
        //    }

        //    var result = await XResiliant.Default.RunBool(
        //        async () => await _database.ListLeftPushAsync(fullName, item) != -1);

        //    if (!result)
        //    {
        //        return false;
        //    }

        //    await XResiliant.Default.RunBool(async () =>
        //    {
        //        await _database.KeyExpireAsync(fullName, DateTime.UtcNow.Add(maxAge.Value));
        //        return true;
        //    });

        //    return true;
        //}

        //public async Task<bool> DeleteItemFromList<T>(string key, T item) where T : class, new()
        //{
        //    if (!_database.Multiplexer.IsConnected)
        //    {
        //        return false;
        //    }

        //    var fullName = _getFullKey<T>(key);

        //    var serialised = Serialise(item);

        //    var result = await XResiliant.Default.RunBool(async () =>
        //    {
        //        await _database.ListRemoveAsync(fullName, serialised);
        //        return true;
        //    });

        //    return true;
        //}

        //public async Task<bool> DeleteItemFromList(string key, string item)
        //{
        //    if (!_database.Multiplexer.IsConnected)
        //    {
        //        return false;
        //    }

        //    var fullName = _getFullKey<string>(key);

        //    var result = await XResiliant.Default.RunBool(async () =>
        //    {
        //        await _database.ListRemoveAsync(fullName, item);
        //        return true;
        //    });

        //    return true;
        //}

        //public Task Clear()
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<bool> Delete<T>(string key) where T : class, new()
        //{
        //    if (!_database.Multiplexer.IsConnected)
        //    {
        //        return false;
        //    }

        //    var fullName = _getFullKey<T>(key);
        //    return await _database.KeyDeleteAsync(fullName);
        //}
        
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
