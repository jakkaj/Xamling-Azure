using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Xamling.Azure.Contract;
using Xamling.Portable.Contract;

namespace Xamling.Azure.Blob
{
    public class EntityCacheBlobRepo : BlobRepo, IEntityCacheBlobRepo
    {
        public EntityCacheBlobRepo(CloudBlobClient blobClient, ILogService logService) : base(blobClient, logService, "EntityCache")
        {
        }
    }
}
