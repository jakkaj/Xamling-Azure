using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Xamling.Azure.Contract;
using XamlingCore.Portable.Contract.Repos.Base;
using XamlingCore.Portable.Contract.Serialise;

namespace Xamling.Azure.Storage
{
    public class BlobStorageFileRepo : IStorageFileRepo
    {
        private readonly IEntityCacheBlobRepo _blobRepo;
        private readonly IEntitySerialiser _entitySerialiser;
        public BlobStorageFileRepo(IEntityCacheBlobRepo blobRepo,
            IEntitySerialiser entitySerialiser)
        {
            _blobRepo = blobRepo;
            _entitySerialiser = entitySerialiser;
        }

        public bool DisableMultitenant { get; set; }

        public async Task<bool> Delete(string fileName)
        {
            var t = _getFileName(fileName);
            var deleteResult = await _blobRepo.DeleteAsync(t);
            return deleteResult;
        }

        public async Task<bool> Set<T>(T entity, string fileName) where T : class, new()
        {
            var t = _getFileName(fileName);

            var data = _entitySerialiser.Serialise(entity);

            var result = await _blobRepo.UploadTextAsync(t, data);

            return result;
        }

        public async Task<T> Get<T>(string fileName) where T : class, new()
        {
            var t = _getFileName(fileName);

            var ent = await _blobRepo.DownloadStringAsync(t);

            if (!ent.IsSuccess || ent.Object == null)
            {
                return null;
            }

            var obj = _entitySerialiser.Deserialise<T>(ent.Object);

            return obj;
        }

        public async Task<List<T>> GetAll<T>(string folderName, bool recurse) where T : class, new()
        {
            throw new NotImplementedException("Blob Repo get all not implemented");
        }

        public async Task DeleteAll(string folderName, bool recurse)
        {
            throw new NotImplementedException();
        }

        string _getFileName(string fileName)
        {
            fileName = fileName.Replace("\\", "/");

            fileName = HttpUtility.UrlEncode(fileName);

            fileName = fileName.Replace("%2f", "/");

            string userId = "";

            fileName = $"{userId}{fileName}";

            return fileName;
        }

    }
}
