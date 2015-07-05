using Microsoft.WindowsAzure.Storage.Blob;
using Xamling.Portable.Contract;

namespace Xamling.Azure.Blob
{
    public class BlobRepoFactory : IBlobRepoFactory
    {
        private readonly CloudBlobClient _blobClient;
        private readonly ILogService _logService;

        public BlobRepoFactory(CloudBlobClient blobClient, ILogService logService)
        {
            _blobClient = blobClient;
            _logService = logService;
        }

        public IBlobRepo GetBlobRepo(string containerName)
        {
            return new BlobRepo(_blobClient, _logService, containerName);
        }
    }
}
