using System;
using Autofac;
using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure.Storage;
using StackExchange.Redis;
using Xamling.Azure.Blob;
using Xamling.Azure.Contract;
using Xamling.Azure.DocumentDB;
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
    public class AzureModule_Test : Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            builder.Register(_ => new DocumentClient(new Uri(_.Resolve<IConfig>()["DocumentDatabaseUri"]), _.Resolve<IConfig>()["DocumentDatabaseAuth"]));

            builder.RegisterType<DocumentConnection>().As<IDocumentConnection>().SingleInstance();

            builder.RegisterType<DocumentEntityCache>().As<IDocumentEntityCache>().SingleInstance();

            builder.RegisterGeneric(typeof(DocumentRepo<>)).As(typeof(IDocumentRepo<>)).SingleInstance();

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


            builder.RegisterGeneric(typeof (QueueMessageRepo<>)).As(typeof (IQueueMessageRepo<>)).SingleInstance();

            builder.RegisterType<RedisConnection>().As<IRedisConnection>().SingleInstance();

            builder.Register(_=>_.Resolve<IRedisConnection>().GetDatabase()).As<IDatabase>();
            builder.Register(_ => _.Resolve<IRedisConnection>().GetSubscriber()).As<ISubscriber>();

            builder.RegisterType<RedisEntityCache>().As<IRedisEntityCache>();
           
            builder.RegisterType<RedisEntityCache>().As<IRedisEntityCache>();

            builder.RegisterType<RedisMemoryCache>().As<IMemoryCache>();
            builder.RegisterType<RedisMemoryCache>().As<IRedisMemoryCache>();

            
            builder.RegisterType<BlobRepo>().As<IBlobRepo>();
            builder.RegisterType<BlobRepoFactory>().As<IBlobRepoFactory>();

            builder.RegisterType<BlobStorageFileRepo>().As<IStorageFileRepo>();

            builder.RegisterType<LogService>().As<ILogService>().SingleInstance();
        }
    }
}
