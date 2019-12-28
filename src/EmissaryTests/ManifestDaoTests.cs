using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryCore;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Moq;
using Microsoft.Data.Sqlite;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace EmissaryTests
{
    [TestClass]
    public class ManifestDaoTests
    {

        [TestMethod]
        public void GetItemDefinition_Izanagi_ShouldReturnItemDefinitionWithCorrectValues()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Emissary:ManifestPath"] == "data/dummy.manifest");
            uint izanagiHash = 3211806999;
            string jsonResponse = TestUtils.ReadFile("item-izanagi.json");
            IDatabaseAccessor databaseAccessor = Mock.Of<IDatabaseAccessor>();
            Mock.Get(databaseAccessor)
                .Setup(m => m.ExecuteCommand(
                        It.Is<string>(s => s.Contains(((int)izanagiHash).ToString())),
                        It.IsRegex(@".*\.manifest$")))
                .Returns(jsonResponse);

            ManifestItemDefinition expected = new ManifestItemDefinition("Izanagi's Burden",
                    new List<uint>() { 2, 1, 10 });

            ManifestDao manifestDao = new ManifestDao(config, databaseAccessor);
            ManifestItemDefinition actual = manifestDao.GetItemDefinition(izanagiHash);
            Assert.AreEqual(expected.DisplayName, actual.DisplayName);
            Assert.IsTrue(expected.ItemCategoryHashes.SequenceEqual(actual.ItemCategoryHashes));
        }

        [TestMethod]
        public void GetItemDefinition_ItemNotFound_ShouldReturnNull()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Emissary:ManifestPath"] == "data/dummy.manifest");
            uint nonexistentItemHash = 0;
            IDatabaseAccessor databaseAccessor = Mock.Of<IDatabaseAccessor>();
            Mock.Get(databaseAccessor)
                .Setup(m => m.ExecuteCommand(
                        It.Is<string>(s => s.Contains(((int)nonexistentItemHash).ToString())),
                        It.IsRegex(@".*\.manifest$")))
                .Returns("");
            ManifestDao manifestDao = new ManifestDao(config, databaseAccessor);
            ManifestItemDefinition actual = manifestDao.GetItemDefinition(nonexistentItemHash);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void LookupItem_DatabaseNotFound_ShouldThrowDataAccessException()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Emissary:ManifestPath"] == "data/dummy.manifest");
            uint izanagiHash = 3211806999;
            IDatabaseAccessor databaseAccessor = Mock.Of<IDatabaseAccessor>();
            Mock.Get(databaseAccessor)
                .Setup(m => m.ExecuteCommand(
                        It.Is<string>(s => s.Contains(((int)izanagiHash).ToString())),
                        It.IsRegex(@".*\.manifest$")))
                .Throws(new DataAccessException("database file not found"));
            ManifestDao manifestDao = new ManifestDao(config, databaseAccessor);
            DataAccessException exception = Assert.ThrowsException<DataAccessException>(() => manifestDao.GetItemDefinition(izanagiHash));
            Assert.IsTrue(exception.Message.Contains("database file not found"));
        }

        [TestMethod]
        public void LookupItemCategory_KineticWeapon_ShouldReturnItemDefinitionWithCorrectValues()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Emissary:ManifestPath"] == "data/dummy.manifest");
            uint kineticWeaponCategoryHash = 2;
            string jsonResponse = TestUtils.ReadFile("category-kinetic-weapon.json");
            IDatabaseAccessor databaseAccessor = Mock.Of<IDatabaseAccessor>();
            Mock.Get(databaseAccessor)
                .Setup(m => m.ExecuteCommand(
                        It.Is<string>(s => s.Contains(((int)kineticWeaponCategoryHash).ToString())),
                        It.IsRegex(@".*\.manifest$")))
                .Returns(jsonResponse);
            ManifestItemCategoryDefinition expected = new ManifestItemCategoryDefinition("Kinetic Weapon");
            ManifestDao manifestDao = new ManifestDao(config, databaseAccessor);
            ManifestItemCategoryDefinition actual = manifestDao.GetItemCategoryDefinition(kineticWeaponCategoryHash);
            Assert.AreEqual(expected.CategoryName, actual.CategoryName);
        }

        [TestMethod]
        public void LookupItemCategory_CategoryNotFound_ShouldReturnNull()
        {
            IConfiguration config = Mock.Of<IConfiguration>(m => m["Emissary:ManifestPath"] == "data/dummy.manifest");
            uint nonexistentCategoryHash = 0;
            IDatabaseAccessor databaseAccessor = Mock.Of<IDatabaseAccessor>();
            Mock.Get(databaseAccessor)
                .Setup(m => m.ExecuteCommand(
                        It.Is<string>(s => s.Contains(((int)nonexistentCategoryHash).ToString())),
                        It.IsRegex(@".*\.manifest$")))
                .Returns("");
            ManifestDao manifestDao = new ManifestDao(config, databaseAccessor);
            ManifestItemCategoryDefinition actual = manifestDao.GetItemCategoryDefinition(nonexistentCategoryHash);
            Assert.IsNull(actual);
        }


    }
}