using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamling.Azure.Contract;
using Xamling.Azure.Entities;
using XamlingCore.Portable.Contract.Repos.Base;
using XamlingCore.Portable.Contract.Serialise;

namespace Xamling.Azure.Storage
{
    public class TableStorageFileRepo : IStorageFileRepo
    {
        private readonly IEntityCacheTableRepo _entityCacheTableRepo;
        private readonly IEntitySerialiser _entitySerialiser;

        public TableStorageFileRepo(IEntityCacheTableRepo entityCacheTableRepo,
            IEntitySerialiser entitySerialiser)
        {
            _entityCacheTableRepo = entityCacheTableRepo;
            _entitySerialiser = entitySerialiser;
        }

        public bool DisableMultitenant { get; set; }

        public async Task<bool> Delete(string fileName)
        {
            var t = _getFileName(fileName);

            var e = await _entityCacheTableRepo.Get(t.Item1, t.Item2);

            if (!e.IsSuccess || e.Object == null)
            {
                return false;
            }

            var deleteResult = await _entityCacheTableRepo.Delete(e.Object);
            return deleteResult.IsSuccess;
        }

        public async Task<bool> Set<T>(T entity, string fileName) where T : class, new()
        {
            var t = _getFileName(fileName);

            var data = _entitySerialiser.Serialise(entity);

            var dEntity = new EntityCacheTableEntity
            {
                Data = data,
                PartitionKey = t.Item1,
                RowKey = t.Item2,
                Timestamp = DateTime.Now
            };

            var result = await _entityCacheTableRepo.AddOrUpdate(dEntity);

            return result.IsSuccess;
        }

        public async Task<T> Get<T>(string fileName) where T : class, new()
        {
            var t = _getFileName(fileName);

            var ent = await _entityCacheTableRepo.Get(t.Item1, t.Item2);

            if (!ent.IsSuccess || ent.Object == null || string.IsNullOrWhiteSpace(ent.Object.Data))
            {
                return null;
            }

            var obj = _entitySerialiser.Deserialise<T>(ent.Object.Data);

            return obj;
        }

        public async Task<List<T>> GetAll<T>(string folderName, bool recurse) where T : class, new()
        {
            var fileName = _getFolderName(folderName);
            var all = await _entityCacheTableRepo.Get(fileName);

            if (!all.IsSuccess || all.Object == null)
            {
                return null;
            }

            return (from item in all.Object where !string.IsNullOrWhiteSpace(item.Data) select _entitySerialiser.Deserialise<T>(item.Data)).ToList();
        }

        public async Task DeleteAll(string folderName, bool recurse)
        {
            var fileName = _getFolderName(folderName);

            var all = await _entityCacheTableRepo.Get(fileName);

            if (!all.IsSuccess || all.Object == null)
            {
                return;
            }

            foreach (var item in all.Object)
            {
                await _entityCacheTableRepo.Delete(item);
            }
        }

        string _getFolderName(string folderName)
        {
            var dir = folderName.Replace("#", "-");
            dir = dir.Replace("?", "-");

            dir = dir.Replace("\\", "/");

            dir = dir.Replace("/", "-");

            string userId = "all";


            dir = dir.Replace("cache-cache", userId);

            if (!dir.Contains(userId))
            {
                dir = userId + dir;
            }

            return dir.ToLower();
        }

        Tuple<string, string> _getFileName(string fileName)
        {
            fileName = fileName.Replace("#", "-");
            fileName = fileName.Replace("?", "-");

            fileName = fileName.Replace("\\", "/");

            if (!fileName.Contains('/'))
            {
                throw new Exception("TableStorageFileRepo cannot work with filename " + fileName);
            }

            var dir = Path.GetDirectoryName(fileName);
            var fn = Path.GetFileName(fileName);

            dir = dir.Replace("/", "-");
            dir = dir.Replace("\\", "-");

            string userId = "all";

         

            dir = dir.Replace("cache-cache", userId);
            if (!dir.Contains(userId))
            {
                dir = userId + dir;
            }

            return new Tuple<string, string>(dir.ToLower(), fn.ToLower());
        }

    }
}
