using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RZ.NET.Tests.Base;
using Xamling.Azure.DocumentDB;

namespace Xamling.Azure.IntegrationTests.Tests
{
    [TestClass]
    public class DatabaseCreation : TestBase
    {
        [TestMethod]
        public async Task TestCreateDatabaseAndCollection()
        {
            var c = Resolve<IDocumentConnection>();

            var db = await c.GetDatabase("TestDatabase");

            Assert.IsNotNull(db);

            var collection = await c.GetCollection("TestCollection", db);

            Assert.IsNotNull(collection);
        }


        [TestMethod]
        public async Task TestAddAndReadDocument()
        {
            var c = Resolve<IDocumentConnection>();

            var db = await c.GetDatabase("TestDatabase");

            Assert.IsNotNull(db);

            var collection = await c.GetCollection("TestCollection", db);

            Assert.IsNotNull(collection);
        }

        public class SomeClass
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }
    }
}
