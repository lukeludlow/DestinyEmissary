using System;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Data.Sqlite;

namespace EmissaryCore
{
    public class ManifestDao : IManifestDao
    {
        private IDatabaseAccessor databaseAccessor;
        private string manifestPath;
        // private IBungieApiService bungieApiService;
        // public string CurrentVersion { get; set; }

        public ManifestDao(IDatabaseAccessor databaseAccessor)
        {
            this.databaseAccessor = databaseAccessor;
            this.manifestPath = GetManifestPath();
        }

        public ManifestItemDefinition GetItemDefinition(uint itemHash)
        {
            int itemId = ConvertHashToTableId(itemHash);
            string commandText = $"SELECT json FROM DestinyInventoryItemDefinition WHERE id = {itemId}";
            string json = databaseAccessor.ExecuteCommand(commandText, manifestPath);
            ManifestItemDefinition itemDefinition = JsonConvert.DeserializeObject<ManifestItemDefinition>(json);
            return itemDefinition;
        }

        public ManifestItemCategoryDefinition GetItemCategoryDefinition(uint categoryHash)
        {
            int itemCategoryId = ConvertHashToTableId(categoryHash);
            string commandText = $"SELECT json FROM DestinyItemCategoryDefinition WHERE id = {itemCategoryId}";
            string json = databaseAccessor.ExecuteCommand(commandText, manifestPath);
            ManifestItemCategoryDefinition categoryDefinition = JsonConvert.DeserializeObject<ManifestItemCategoryDefinition>(json);
            return categoryDefinition;
        }


        private int ConvertHashToTableId(uint hash)
        {
            // itemHash is an unsigned 32bit integer, but SQLite is interpreting the value as a signed int.
            // so just cast it to a signed int and then use that value as the id.
            int id = (int)hash;
            return id;
        }

        // TODO refactor this is very bad
        private string GetManifestPath()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            string dataDirectory = Path.Combine(solutionDirectory, "data");
            string localManifestFileName = "79433.19.11.13.1925-2.manifest";
            string localManifestFile = Path.Combine(dataDirectory, localManifestFileName);
            return localManifestFile;
        }


        // TODO make it so that this checks for the latest version and downloads the new manifest only if needed
        // public void LoadLatestManifest()
        // {
        //     // get-latest-manifest-info.json
        //     string requestUrl = "https://www.bungie.net/Platform/Destiny2/Manifest/";
        //     string json = bungieApiProxy.Get(requestUrl);
        //     dynamic response = JObject.Parse(json);
        //     // the main game world database is listed in the Response.mobileWorldContentPaths.en object 
        //     string remoteManifestPath = response.Response.mobileWorldContentPaths.en;
        //     string remoteManifestUrl = "https://www.bungie.net" + remoteManifestPath;

        //     string latestManifestVersion = response.Response.version;

        //     string workingDirectory = Environment.CurrentDirectory;
        //     string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
        //     string dataDirectory = Path.Combine(solutionDirectory, "data");
        //     string localManifestFileName = latestManifestVersion + ".manifest";
        //     string localManifestFile = Path.Combine(dataDirectory, localManifestFileName);

        //     Stream stream = bungieApiProxy.SendRequestAndReturnStream(remoteManifestUrl);
        //     using (MemoryStream memoryStream = new MemoryStream()) {
        //         stream.CopyTo(memoryStream);
        //         memoryStream.Seek(0, SeekOrigin.Begin);
        //         ZipArchive manifestZip = new ZipArchive(memoryStream);
        //         manifestZip.Entries[0].ExtractToFile(localManifestFile);
        //     }
        // }


    }
}