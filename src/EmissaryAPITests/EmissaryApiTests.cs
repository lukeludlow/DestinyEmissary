using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryApi;
using static EmissaryApi.EmissaryApi;

namespace EmissaryApiTests
{
    [TestClass]
    public class EmissaryApiTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Abc();
            Assert.Fail();
            // string sss = DummyGetManifestInventoryItem();
            // Assert.AreEqual("Gjallarhorn", sss);
            // Assert.Fail();
        }
    }
}
