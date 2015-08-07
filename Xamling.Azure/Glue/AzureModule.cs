using System;
using Autofac;
using AutoMapper;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure.Storage;
using StackExchange.Redis;
using Xamling.Azure.Blob;
using Xamling.Azure.Contract;
using Xamling.Azure.DocumentDB;
using Xamling.Azure.EntityCaches;
using Xamling.Azure.EntityList;
using Xamling.Azure.Logger;
using Xamling.Azure.Portable.Contract;
using Xamling.Azure.Portable.Contract.Cache;
using Xamling.Azure.Portable.Entity;
using Xamling.Azure.Queue;
using Xamling.Azure.Redis;
using Xamling.Azure.Redis.EntityCaches;
using Xamling.Azure.Redis.Memory;
using Xamling.Azure.Storage;
using XamlingCore.Portable.Contract.Config;
using XamlingCore.Portable.Contract.Entities;
using XamlingCore.Portable.Contract.Repos.Base;
using XamlingCore.Portable.Model.Other;

namespace Xamling.Azure.Glue
{
    public class AzureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            Mapper.CreateMap<XSeverityLevel, SeverityLevel>();

            builder.Register(
                _ =>
                    new DocumentClient(new Uri(_.Resolve<IConfig>()["DocumentDatabaseUri"]),
                        _.Resolve<IConfig>()["DocumentDatabaseAuth"])).InstancePerRequest();

            builder.RegisterType<DocumentConnection>().As<IDocumentConnection>().InstancePerRequest();

            builder.RegisterType<DocumentEntityCache>().As<IDocumentEntityCache>().InstancePerRequest();

            builder.RegisterGeneric(typeof(DocumentRepo<>)).As(typeof(IDocumentRepo<>)).InstancePerRequest();

            builder.Register(_ => CloudStorageAccount.Parse(_.Resolve<IConfig>()["StorageConnectionString"]))
                .AsSelf()
                .InstancePerRequest();

            builder.Register(_ => _.Resolve<CloudStorageAccount>().CreateCloudTableClient())
                .AsSelf()
                .InstancePerRequest();

            builder.Register(_ => _.Resolve<CloudStorageAccount>().CreateCloudBlobClient())
                .AsSelf()
                .InstancePerRequest();

            builder.Register(_ => _.Resolve<CloudStorageAccount>().CreateCloudQueueClient())
                .AsSelf()
                .InstancePerRequest();

            builder.RegisterGeneric(typeof(QueueMessageRepo<>)).As(typeof(IQueueMessageRepo<>)).InstancePerRequest();

            

            builder.RegisterType<RedisConnection>().As<IRedisConnection>().SingleInstance();

            builder.Register(_=>_.Resolve<IRedisConnection>().GetDatabase()).As<IDatabase>();
            builder.Register(_ => _.Resolve<IRedisConnection>().GetSubscriber()).As<ISubscriber>();

            builder.RegisterType<SharedRedisEntityCache>().As<IRedisEntityCache>();
            builder.RegisterType<SharedRedisEntityCache>().As<ISharedRedisEntityCache>();
           
            builder.RegisterType<SharedRedisMemoryCache>().As<IMemoryCache>();
            builder.RegisterType<SharedRedisMemoryCache>().As<ISharedRedisMemoryCache>();
            builder.RegisterType<SecureSessionSessionRedisMemoryCache>().As<ISecureSessionRedisMemoryCache>();
            builder.RegisterType<OverrideSessionSessionRedisMemoryCache>().As<IOverrideSessionRedisMemoryCache>().InstancePerDependency();

            builder.RegisterType<BlobRepo>().As<IBlobRepo>().InstancePerRequest();
            builder.RegisterType<BlobRepoFactory>().As<IBlobRepoFactory>().InstancePerRequest();

            builder.RegisterType<BlobStorageFileRepo>().As<IStorageFileRepo>().InstancePerRequest();

            builder.RegisterType<SharedEntityCache>().As<ISharedEntityCache>().InstancePerRequest();

            builder.RegisterType<LogService>().As<ILogService>().SingleInstance();

            builder.RegisterGeneric(typeof(EntityLister<>)).As(typeof(IEntityLister<>));
           
        }
    }
}
