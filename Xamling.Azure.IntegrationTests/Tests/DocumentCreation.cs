using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RZ.NET.Tests.Base;
using Xamling.Azure.DocumentDB;
using Xamling.Azure.Entities;
using Xamling.Azure.Portable.Contract.Cache;

namespace Xamling.Azure.IntegrationTests.Tests
{
    [TestClass]
    public class DocumentCreation : TestBase
    {
        [TestMethod]
        public async Task CreateTestDocument()
        {
            

            var db = Resolve<IDocumentConnection>();

            //var db = d.CreateDatabaseQuery()
            //            .Where(d2 => d2.Id == "TestDatabase")
            //            .AsEnumerable()
            //            .FirstOrDefault();

            //var cb = Resolve<Database>();
            ////var cbb = Resolve<DocumentCollection>();

            var c = Resolve<IDocumentEntityCache>();

            var doc = new TestKeyEntity();

            doc.PersonName = "Jordan Kniht";

            doc.SubKey = new TestKeyEntity
            {
                Key = "Subkey item"
            };

            var result = await c.SetEntity("SOme key!", doc);

            Assert.IsTrue(result);

            var getRes = await c.GetEntity<TestKeyEntity>("SOme key!");
            Assert.IsNotNull(getRes);
            var collection = await db.GetCollection();

            var q = db.Client.CreateDocumentQuery<TestKeyEntity>(collection.DocumentsLink)
                .Where(d => d.SubKey.Key == "Subkey item").ToList();


            var i = q.FirstOrDefault();

            Assert.IsNotNull(i);

           

        }
    }
}
