using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryApi;
using EmissaryApi.Model;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace EmissaryApiTests
{
    [TestClass]
    public class ManifestTests
    {

        [TestMethod]
        public void GetDestinyInventoryItemDefinition_Izanagi_x()
        {
            // id -1083160297
            Manifest manifest = new Manifest();
            UInt32 itemHash = 3211806999; // izanagi's burden inventory item hash

            DestinyInventoryItem expectedItem = new DestinyInventoryItem();
            expectedItem.DisplayProperties = new DestinyDisplayPropertiesDefinition();
            expectedItem.DisplayProperties.Name = "Izanagi's Burden";

            string json = manifest.GetDestinyInventoryItemDefinition(itemHash);
            DestinyInventoryItem actualItem = JsonConvert.DeserializeObject<DestinyInventoryItem>(json);

            Assert.AreEqual(expectedItem.DisplayProperties.Name, actualItem.DisplayProperties.Name);
        }


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