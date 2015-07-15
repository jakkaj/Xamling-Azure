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
            //var d = new DocumentClient(new Uri("https://ptdb.documents.azure.com:443/"),
            //    "Epn2VV6081ypHRc6B+tgHK3fBpVmiP19KeVqi3lxHWnWQMLvA/gFYAJpc+nyZj3LSOn8S+VoZSMpEWwGoFsHpw==");

            ////var db = Resolve<IDocumentConnection>();

            //var db = d.CreateDatabaseQuery()
            //            .Where(d2 => d2.Id == "TestDatabase")
            //            .AsEnumerable()
            //            .FirstOrDefault();

            //var cb = Resolve<Database>();
            ////var cbb = Resolve<DocumentCollection>();

            var c = Resolve<IDocumentEntityCache>();

            var doc = new TestKeyEntity();
            doc.PersonName = "Jordan Kniht";

            var result = await c.SetEntity("SOme key!", doc);

            Assert.IsTrue(result);

            var getRes = await c.GetEntity<TestKeyEntity>("SOme key!");

            Assert.IsNotNull(getRes);

        }
    }
}
