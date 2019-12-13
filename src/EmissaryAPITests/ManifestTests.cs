using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryApi;
using EmissaryApi.Model;
using System.Collections.Generic;
using System.IO;

namespace EmissaryApiTests
{
    [TestClass]
    public class ManifestTests
    {

        // TODO this test doesn't make sense. mock the bungie api proxy and file stuff
        [TestMethod]
        public void LoadLatestManifest_FileDoesNotExist_ShouldDownloadFile()
        {
            Manifest manifest = new Manifest();

            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            string dataDirectory = Path.Combine(solutionDirectory, "data");

            // in this case, we'll say there is no manifest file at all
            string[] filesBefore = Directory.GetFiles(dataDirectory, "*.manifest");
            Assert.AreEqual(0, filesBefore.Length);

            manifest.LoadLatestManifest();

            // the latest manifest file should now be there
            string[] filesAfter = Directory.GetFiles(dataDirectory, "*.manifest");
            Assert.AreEqual(1, filesAfter.Length);
        }

    }
}