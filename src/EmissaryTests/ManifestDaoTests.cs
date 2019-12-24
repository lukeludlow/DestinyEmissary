using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emissary;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Moq;
using Microsoft.Data.Sqlite;

namespace EmissaryTests
{
    [TestClass]
    public class ManifestDaoTests
    {

        // [TestMethod]
        // public void LookupItem_Izanagi_ShouldReturnDestinyInventoryItemDefinitionWithCorrectValues()
        // {
        //     uint itemHash = 3211806999;  // izanagi's burden inventory item hash
        //     string jsonFileName = "item-izanagi.json";
        //     string json = TestUtils.ReadFile(jsonFileName);
        //     IDatabaseAccessor mockDatabaseAccessor = Mock.Of<IDatabaseAccessor>(m => m.ExecuteCommand(It.IsAny<string>(), It.IsRegex(@".*\.manifest$")) == json);
        //     IManifestDao manifestAccessor = new ManifestDao(mockDatabaseAccessor);
        //     // string itemName = "Izanagi's Burden";
        //     // uint[] itemCategoryHashes = new uint[] { 2, 1, 10 };
        //     DestinyItem expected = JsonConvert.DeserializeObject<DestinyItem>(json);
        //     DestinyItem actual = manifestAccessor.LookupItem(itemHash);
        //     Assert.AreEqual(expected.DisplayProperties.Name, actual.DisplayProperties.Name);
        //     Assert.AreEqual(expected.ItemCategoryHashes.Length, actual.ItemCategoryHashes.Length);
        // }

        // [TestMethod]
        // public void LookupItemCategory_KineticWeapon_ShouldReturnDestinyItemCategoryDefinitionWithCorrectValues()
        // {
        //     uint itemCategoryHash = 2;  // id for kinetic weapon category
        //     string jsonFileName = "category-kinetic-weapon.json";
        //     string json = TestUtils.ReadFile(jsonFileName);
        //     IDatabaseAccessor mockDatabaseAccessor = Mock.Of<IDatabaseAccessor>(m => m.ExecuteCommand(It.IsAny<string>(), It.IsRegex(@".*\.manifest$")) == json);
        //     IManifestDao manifestAccessor = new ManifestDao(mockDatabaseAccessor);
        //     DestinyItemCategory expected = JsonConvert.DeserializeObject<DestinyItemCategory>(json);
        //     DestinyItemCategory actual = manifestAccessor.LookupItemCategory(itemCategoryHash);
        //     Assert.AreEqual(expected.DisplayProperties.Name, actual.DisplayProperties.Name);
        // }

        // [TestMethod]
        // public void LookupItem_InvalidPath_ShouldThrowException()
        // {
        //     uint itemHash = 3211806999;  
        //     Mock<IDatabaseAccessor> mockDatabaseAccessor = new Mock<IDatabaseAccessor>();
        //     mockDatabaseAccessor.Setup(m => m.ExecuteCommand(It.IsAny<string>(), It.IsRegex(@".*\.manifest$"))).Throws(new SqliteException("", 0));
        //     IManifestDao manifestAccessor = new ManifestDao(mockDatabaseAccessor.Object);
        //     Assert.ThrowsException<DataAccessException>(() => manifestAccessor.LookupItem(itemHash));
        // }

        // [TestMethod]
        // public void LookupItem_ItemDoesNotExist_ShouldThrowException()
        // {
        //     uint itemHash = 1;  // this id does not exist in the manifest database
        //     IManifestDao manifestAccessor = new ManifestDao(new DatabaseAccessor());
        //     // Mock<IDatabaseAccessor> mockDatabaseAccessor = new Mock<IDatabaseAccessor>();
        //     // mockDatabaseAccessor.Setup(m => m.ExecuteCommand(It.IsAny<string>(), It.IsRegex(@".*\.manifest$"))).Throws(new SqliteException("", 0));
        //     // IManifestAccessor manifestAccessor = new ManifestAccessor(mockDatabaseAccessor.Object);
        //     Assert.ThrowsException<DataAccessException>(() => manifestAccessor.LookupItem(itemHash));
        // }





        // should i actually do this test? idk if it's helpful
        // [TestMethod]
        // public void LookupItem_UnableToAccessDatabase_ShouldThrowException()
        // {
        //     Assert.Fail();
        // }



        // [TestMethod]
        // public void GetDestinyInventoryItemDefinition_Izanagi_x()
        // {
        //     // id -1083160297
        //     IManifestAccessor manifestAccessor = new ManifestAccessor();
        //     uint itemHash = 3211806999;  // izanagi's burden inventory item hash

        //     DestinyInventoryItem expectedItem = new DestinyInventoryItem();
        //     expectedItem.DisplayProperties = new DestinyDisplayPropertiesDefinition();
        //     expectedItem.DisplayProperties.Name = "Izanagi's Burden";

        //     string json = manifestAccessor.LookupItem(itemHash);
        //     DestinyInventoryItem actualItem = JsonConvert.DeserializeObject<DestinyInventoryItem>(json);

        //     Assert.AreEqual(expectedItem.DisplayProperties.Name, actualItem.DisplayProperties.Name);
        // }

        // [TestMethod]
        // public void GetDestinyItemCategoryDefinition_2_ShouldReturnKineticWeapon()
        // {
        //     IManifestAccessor manifestAccessor = new ManifestAccessor();
        //     uint itemCategoryHash = 2;  // Kinetic Weapon
        //     DestinyItemCategory expectedItemCategory = new DestinyItemCategory();
        //     expectedItemCategory.DisplayProperties = new DestinyDisplayPropertiesDefinition();
        //     expectedItemCategory.DisplayProperties.Name = "Kinetic Weapon";
        //     string json = manifestAccessor.LookupItemCategory(itemCategoryHash);
        //     DestinyItemCategory actualItemCategory = JsonConvert.DeserializeObject<DestinyItemCategory>(json);
        //     Assert.AreEqual(expectedItemCategory.DisplayProperties.Name, actualItemCategory.DisplayProperties.Name);
        // }


        // TODO this test doesn't make sense. mock the bungie api proxy and file stuff
        // [TestMethod]
        // public void LoadLatestManifest_FileDoesNotExist_ShouldDownloadFile()
        // {
        //     Manifest manifest = new Manifest();

        //     string workingDirectory = Environment.CurrentDirectory;
        //     string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
        //     string dataDirectory = Path.Combine(solutionDirectory, "data");

        //     // in this case, we'll say there is no manifest file at all
        //     string[] filesBefore = Directory.GetFiles(dataDirectory, "*.manifest");
        //     Assert.AreEqual(0, filesBefore.Length);

        //     manifest.LoadLatestManifest();

        //     // the latest manifest file should now be there
        //     string[] filesAfter = Directory.GetFiles(dataDirectory, "*.manifest");
        //     Assert.AreEqual(1, filesAfter.Length);
        // }

    }
}