﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RZ.NET.Tests.Base;
using Xamling.Azure.Entities;
using Xamling.Azure.Portable.Contract.Cache;

namespace Xamling.Azure.IntegrationTests.Tests
{
    [TestClass]
    public class Test_DocumentEntityCache : TestBase
    {
        [TestMethod]
        public async Task TestCreateAndGetEntitiesUsingCache()
        {
            var c = Resolve<IDocumentEntityCache>();

            var t = new TestKeyEntity();
            t.Id = "JordanKey5";
            t.SubKey = new TestKeyEntity
            {
                PersonName = "Knight"
            };

            t.PersonName = "Jordan3";

            var result = await c.SetEntity("Jordan test key2", t);

            Assert.IsTrue(result);


            var resultGet = await c.GetEntity<TestKeyEntity>("Jordan test key2");

            Assert.IsNotNull(resultGet);

            Assert.IsTrue(resultGet.PersonName == "Jordan3");
        }
    }
}
