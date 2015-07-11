using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using Xamling.Azure.Portable.Contract.Cache;
using XamlingCore.Portable.Contract.Serialise;
using XamlingCore.Portable.Data.Entities;
using XamlingCore.Portable.Model.Resiliency;

namespace Xamling.Azure.Redis.EntityCaches
{
    public class SharedRedisEntityCache : KeyEntityBase, ISharedRedisEntityCache
    {
        private readonly IDatabase _database;
        private readonly ISubscriber _subs;
        private readonly IEntitySerialiser _entitySerialiser;

        public SharedRedisEntityCache(IDatabase database, ISubscriber subs, IEntitySerialiser serialiser)
        {
            _database = database;
            _subs = subs;
            _entitySerialiser = serialiser;
        }

        public async Task SubscribeChannel(string channel, Action<string> handler)
        {
            if (!_subs.Multiplexer.IsConnected)
            {
                return;
            }
            await _subs.SubscribeAsync(channel, (redisChannel, value) => handler(value));
        }

        public async Task PublishChannel(string channel, string message)
        {
            if (!_subs.Multiplexer.IsConnected)
            {
                return;
            }

            await _subs.PublishAsync(channel, message);
        }

        public override string GetKey(string key)
        {
            return base.GetKey(key);
        }

        string _getFullKey<T>(string key)
        {
            var path = String.Join(":", _getDirPath<T>(), string.Format("{0}.cache", key));
            return GetKey(path);
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

        public async Task<T> GetCacheItem<T>(string key, TimeSpan? maxAge = null) where T : class, new()
        {
            if (!_database.Multiplexer.IsConnected)
            {
                return null;
            }

            var fullName = _getFullKey<T>(key);

            var dataFromRedis = await _database.StringGetAsync(fullName);

            var f = Deserialise<T>(dataFromRedis);
            return f;
        }

        public async Task<List<T>> GetEntityList<T>(string key, bool clear = false) where T : class, new()
        {
            if (!_database.Multiplexer.IsConnected)
            {
                return null;
            }

            var fullName = _getFullKey<T>(key);

            var tran = _database.CreateTransaction();

            var dataFromRedisTask = tran.ListRangeAsync(fullName, 0, -1);

            if (clear)
            {
                var delTask = tran.KeyDeleteAsync(fullName);
            }


            await tran.ExecuteAsync();

            var dataFromRedis = await dataFromRedisTask;

            if (dataFromRedis == null || dataFromRedis.Length == 0)
            {
                return null;
            }

            return dataFromRedis.Select(item => Deserialise<T>(item)).ToList();
        }

        public async Task<List<string>> GetEntityList(string key, bool clear = false)
        {
            if (!_database.Multiplexer.IsConnected)
            {
                return null;
            }

            var fullName = _getFullKey<string>(key);

            var tran = _database.CreateTransaction();

            var dataFromRedisTask = tran.ListRangeAsync(fullName, 0, -1);

            if (clear)
            {
                var delTask = tran.KeyDeleteAsync(fullName);
            }

            await tran.ExecuteAsync();

            var dataFromRedis = await dataFromRedisTask;

            if (dataFromRedis == null || dataFromRedis.Length == 0)
            {
                return null;
            }

            return dataFromRedis.Select(item => item.ToString()).ToList();
        }

        public async Task<string> GetEntityListRightPop(string key)
        {
            if (!_database.Multiplexer.IsConnected)
            {
                return null;
            }

            var fullName = _getFullKey<string>(key);
            
            var dataFromRedis = await _database.ListRightPopAsync(fullName);

            return dataFromRedis;
        }

        public async Task<long> GetListLength(string key)
        {
            if (!_database.Multiplexer.IsConnected)
            {
                return 0;
            }

            var fullName = _getFullKey<string>(key);
            var dataFromRedis = await _database.ListLengthAsync(fullName);
            return dataFromRedis;
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
            if (!_database.Multiplexer.IsConnected)
            {
                return false;
            }

            var fullName = _getFullKey<T>(key);

            var serialised = Serialise(item);

            if (maxAge == null)
            {
                maxAge = TimeSpan.FromDays(7);
            }

            return await XResiliant.Default.RunBool(() => _database.StringSetAsync(fullName, serialised, maxAge));
        }

        public async Task<bool> SetEntityList<T>(string key, T item, TimeSpan? maxAge = null) where T : class, new()
        {
            if (!_database.Multiplexer.IsConnected)
            {
                return false;
            }

            var fullName = _getFullKey<T>(key);

            var serialised = Serialise(item);

            if (maxAge == null)
            {
                maxAge = TimeSpan.FromDays(30);
            }

            var result = await XResiliant.Default.RunBool(
                async () => await _database.ListLeftPushAsync(fullName, serialised) != -1);

            if (!result)
            {
                return false;
            }

            await XResiliant.Default.RunBool(async () =>
            {
                await _database.KeyExpireAsync(fullName, DateTime.UtcNow.Add(maxAge.Value));
                return true;
            });
           

            return true;
        }

        public async Task<bool> SetEntityList(string key, string item, TimeSpan? maxAge = null)
        {
            if (!_database.Multiplexer.IsConnected)
            {
                return false;
            }

            var fullName = _getFullKey<string>(key);

            if (maxAge == null)
            {
                maxAge = TimeSpan.FromDays(30);
            }

            var result = await XResiliant.Default.RunBool(
                async () => await _database.ListLeftPushAsync(fullName, item) != -1);

            if (!result)
            {
                return false;
            }

            await XResiliant.Default.RunBool(async () =>
            {
                await _database.KeyExpireAsync(fullName, DateTime.UtcNow.Add(maxAge.Value));
                return true;
            });

            return true;
        }

        public async Task<bool> DeleteItemFromList<T>(string key, T item) where T : class, new()
        {
            if (!_database.Multiplexer.IsConnected)
            {
                return false;
            }

            var fullName = _getFullKey<T>(key);

            var serialised = Serialise(item);

            var result = await XResiliant.Default.RunBool(async () =>
            {
                await _database.ListRemoveAsync(fullName, serialised);
                return true;
            });
            
            return true;
        }

        public async Task<bool> DeleteItemFromList(string key, string item)
        {
            if (!_database.Multiplexer.IsConnected)
            {
                return false;
            }

            var fullName = _getFullKey<string>(key);

            var result = await XResiliant.Default.RunBool(async () =>
            {
                await _database.ListRemoveAsync(fullName, item);
                return true;
            });
            
            return true;
        }

        public Task Clear()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Delete<T>(string key) where T : class, new()
        {
            if (!_database.Multiplexer.IsConnected)
            {
                return false;
            }

            var fullName = _getFullKey<T>(key);
            return await _database.KeyDeleteAsync(fullName);
        }

        public async Task DisableMemoryCache()
        {
            throw new NotImplementedException();
        }

        public async Task EnableMemoryCache()
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> GetAll<T>() where T : class, new()
        {
            throw new NotImplementedException();
        }

        public Task DeleteAll<T>() where T : class, new()
        {
            throw new NotImplementedException();
        }

        public async Task<TimeSpan?> GetAge<T>(string key) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateAge<T>(string key) where T : class, new()
        {
            throw new NotImplementedException();
        }

    

        protected T Deserialise<T>(string entity)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(entity))
            {
                return null;
            }

            try
            {

                var e = _entitySerialiser.Deserialise<T>(entity);
                return e;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Des problem: " + ex.Message);
            }

            return null;
        }

        protected string Serialise<T>(T entity)
        {
            return _entitySerialiser.Serialise(entity);
        }

    }
}
