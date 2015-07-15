using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using RZ.NET.Tests.Mocks;
using Xamling.Azure.Glue;
using Xamling.Azure.Portable.Contract;
using XamlingCore.NET.Glue;
using XamlingCore.Portable.Contract.Config;
using XamlingCore.Portable.Contract.Device.Location;
using XamlingCore.Portable.Contract.Network;
using XamlingCore.Portable.Data.Glue;
using XamlingCore.Portable.Glue;

namespace Xamling.Azure.IntegrationTests.Glue
{
    public class ProjectGlue : NETGlue
    {
        public override void Init()
        {
            base.Init();

            Builder.RegisterType<WebConfig>().As<IConfig>();
            Builder.RegisterModule<DefaultXCoreModule>();
            Builder.RegisterModule<DefaultNETModule>();

            Builder.RegisterModule<AzureModule_Test>();

            Builder.RegisterType<MockLogService>().As<ILogService>();

            Container = Builder.Build();
            ContainerHost.Container = Container;

        }
    }
}
