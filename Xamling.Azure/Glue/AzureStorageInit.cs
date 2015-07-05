//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using RZ.Contract.Data;
//using RZ.Entity.Queues;
//using Xamling.Azure.Contract;
//using Xamling.Portable.Contract;
//using XamlingCore.Portable.Contract.Config;

//namespace Xamling.Azure.Glue
//{
//    public class AzureStorageInit
//    {
//        private readonly IEntityCacheTableRepo _entityCacheTableRepo;
//        private readonly IBlobRepoFactory _blobRepoFactory;
//        private readonly IEntityCacheBlobRepo _entityCacheBlobRepo;
//        private readonly IQueueMessageRepo<CheckoutQueueMessage> _checkoutQueue;
//        private readonly IConfig _config;

//        private static bool _hasInit;

//        public AzureStorageInit(IEntityCacheTableRepo entityCacheTableRepo, 
//            IBlobRepoFactory blobRepoFactory, 
//            IEntityCacheBlobRepo entityCacheBlobRepo,
//            IQueueMessageRepo<CheckoutQueueMessage> checkoutQueue,
//            IConfig config)
//        {
//            _entityCacheTableRepo = entityCacheTableRepo;
//            _blobRepoFactory = blobRepoFactory;
//            _entityCacheBlobRepo = entityCacheBlobRepo;
//            _checkoutQueue = checkoutQueue;
//            _config = config;
//        }

//        public async Task Init()
//        {
//            if (_hasInit)
//            {
//                return;
//            }

//            _hasInit = true;
//            await _checkoutQueue.CreateQueue();

//            await _entityCacheTableRepo.CreateTable();

//            var thisRepo = _blobRepoFactory.GetBlobRepo(_config["RetailZooImages"]);
//            await thisRepo.CreateBlob();

//            await _entityCacheBlobRepo.CreateBlob();
            
//        }
//    }
//}
