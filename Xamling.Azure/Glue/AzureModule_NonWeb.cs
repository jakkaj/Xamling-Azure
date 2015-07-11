using Autofac;
using Microsoft.WindowsAzure.Storage;
using StackExchange.Redis;
using Xamling.Azure.Blob;
using Xamling.Azure.Contract;
using Xamling.Azure.EntityCaches;
using Xamling.Azure.EntityList;
using Xamling.Azure.Logger;
using Xamling.Azure.Portable.Contract;
using Xamling.Azure.Portable.Contract.Cache;
using Xamling.Azure.Queue;
using Xamling.Azure.Redis;
using Xamling.Azure.Redis.EntityCaches;
using Xamling.Azure.Redis.Memory;
using Xamling.Azure.Storage;
using XamlingCore.Portable.Contract.Config;
using XamlingCore.Portable.Contract.Entities;
using XamlingCore.Portable.Contract.Repos.Base;

namespace Xamling.Azure.Glue
{
    public class AzureModule_NonWeb : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(_ => CloudStorageAccount.Parse(_.Resolve<IConfig>()["StorageConnectionString"]))
                .AsSelf()
                .SingleInstance();

            builder.Register(_ => _.Resolve<CloudStorageAccount>().CreateCloudTableClient())
                .AsSelf()
                .SingleInstance();

            builder.Register(_ => _.Resolve<CloudStorageAccount>().CreateCloudBlobClient())
                .AsSelf()
                .SingleInstance();

            builder.Register(_ => _.Resolve<CloudStorageAccount>().CreateCloudQueueClient())
                .AsSelf()
                .SingleInstance();

            builder.RegisterGeneric(typeof(QueueMessageRepo<>)).As(typeof(IQueueMessageRepo<>)).SingleInstance();

            builder.RegisterType<RedisConnection>().As<IRedisConnection>().SingleInstance();

            builder.Register(_=>_.Resolve<IRedisConnection>().GetDatabase()).As<IDatabase>();
            builder.Register(_ => _.Resolve<IRedisConnection>().GetSubscriber()).As<ISubscriber>();

            builder.RegisterType<SharedRedisEntityCache>().As<IRedisEntityCache>();
           
            builder.RegisterType<SharedRedisEntityCache>().As<ISharedRedisEntityCache>();
           

            builder.RegisterType<SharedRedisMemoryCache>().As<IMemoryCache>();
            builder.RegisterType<SharedRedisMemoryCache>().As<ISharedRedisMemoryCache>();
            builder.RegisterType<SecureSessionSessionRedisMemoryCache>().As<ISecureSessionRedisMemoryCache>();
            builder.RegisterType<OverrideSessionSessionRedisMemoryCache>().As<IOverrideSessionRedisMemoryCache>().InstancePerDependency();

            builder.RegisterType<BlobRepo>().As<IBlobRepo>();
            builder.RegisterType<BlobRepoFactory>().As<IBlobRepoFactory>();

            builder.RegisterType<BlobStorageFileRepo>().As<IStorageFileRepo>();

            builder.RegisterType<SharedEntityCache>().As<ISharedEntityCache>();

          
            builder.RegisterGeneric(typeof(EntityLister<>)).As(typeof(IEntityLister<>));
            builder.RegisterType<LogService>().As<ILogService>().SingleInstance();
         
        }
    }
}
