using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xamling.Azure.IntegrationTests.Glue;

namespace RZ.NET.Tests.Base
{
    public class TestBase
    {
        protected IContainer Container;
        public TestBase()
        {
            var glue = new ProjectGlue();
            glue.Init();

            Container = glue.Container;
        }

       

        public void RunTest(Func<Task> task)
        {
            var msr = new ManualResetEvent(false);

            Task.Run(async () =>
            {
                await task();
                msr.Set();
            });

            var msrResult = msr.WaitOne(115000);
            Assert.IsTrue(msrResult, "MSR not set, means assertion failed in task");
        }

        public T Resolve<T>() where T : class
        {
            return Container.Resolve<T>();
        }
    }
}