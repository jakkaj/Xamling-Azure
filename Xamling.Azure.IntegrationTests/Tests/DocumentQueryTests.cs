using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RZ.NET.Tests.Base;
using Xamling.Azure.Entities;
using Xamling.Azure.Portable.Contract;
using Xamling.Azure.Portable.Contract.Cache;

namespace Xamling.Azure.IntegrationTests.Tests
{
    [TestClass]
    public class DocumentQueryTests : TestBase
    {
        [TestMethod]
        public async Task TestDocumentQueries()
        {
            var i = Resolve<IDocumentEntityCache>();

            var t = new TestKeyEntity()
            {
                Id = "Key1",
                PersonName = "PersonName",
                SubKey = new TestKeyEntity
                {
                    Id = "Key2",
                    PersonName = "PersonNameInner",
                }
            };

            var t2 = new TestKeyEntity()
            {
                Id = "Key3",
                PersonName = "PersonName2",
                SubKey = new TestKeyEntity
                {
                    Id = "Key4",
                    PersonName = "PersonNameInner2",
                }
            };

            var o = new OtherTestKeyEntity()
            {
                Id = "KeyOtherTestKeyEntity1",
                PersonName = "PersonName",
                SubKey = new OtherTestKeyEntity
                {
                    Id = "Key2",
                    PersonName = "PersonNameInner",
                }
            };

            var save = await i.SetEntity(t.Id, t);
            var save2 = await i.SetEntity(t2.Id, t2);
            var save3 = await i.SetEntity(o.Id, o);

            var load = await i.GetEntity<TestKeyEntity>(t.Id);
            var load2 = await i.GetEntity<TestKeyEntity>(t2.Id);

            Assert.IsNotNull(load);
            Assert.IsNotNull(load2);

            Assert.AreEqual(t.PersonName, load.PersonName);
            Assert.AreEqual(t2.PersonName, load2.PersonName);


            var query1 = await i.QueryEntity<TestKeyEntity>(_ => _.Item.PersonName.Contains("PersonName"));

            Assert.IsNotNull(query1);
            Assert.IsTrue(query1.Count > 1);

            var query2 = await i.QueryEntity<OtherTestKeyEntity>(_ => _.Item.PersonName == "PersonName");

            Assert.IsNotNull(query2);
            Assert.IsTrue(query2.Count > 0);
        }
    }
}
