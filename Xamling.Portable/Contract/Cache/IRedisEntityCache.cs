using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamlingCore.Portable.Contract.Entities;

namespace Xamling.Portable.Contract.Cache
{
    public interface IRedisEntityCache : IEntityCache
    {
        Task<List<T>> GetEntityList<T>(string key, bool clear = false) where T : class, new();
        Task<bool> SetEntityList<T>(string key, T item, TimeSpan? maxAge = null) where T : class, new();
        Task<bool> DeleteItemFromList<T>(string key, T item) where T : class, new();
        Task<List<string>> GetEntityList(string key, bool clear = false);
        Task<bool> SetEntityList(string key, string item, TimeSpan? maxAge = null);
        Task<bool> DeleteItemFromList(string key, string item);
        Task SubscribeChannel(string channel, Action<string> handler);
        Task PublishChannel(string channel, string message);
        Task<string> GetEntityListRightPop(string key);
        Task<long> GetListLength(string key);
        string GetKey(string key);
    }
}
