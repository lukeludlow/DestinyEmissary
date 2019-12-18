using System;
using System.IO;
using Emissary.Model;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Data.Sqlite;

namespace Emissary
{
    public class ManifestAccessor : IManifestAccessor
    {
        private IDatabaseAccessor databaseAccessor;
        private string manifestPath;
        // private IBungieApiService bungieApiService;
        // public string CurrentVersion { get; set; }

        public ManifestAccessor(IDatabaseAccessor databaseAccessor)
        {
            this.databaseAccessor = databaseAccessor;
            this.manifestPath = GetManifestPath();
        }

        public DestinyItem LookupItem(uint itemHash)
        {
            int itemId = ConvertHashToTableId(itemHash);
            string commandText = $"SELECT json FROM DestinyInventoryItemDefinition WHERE id = {itemId}";
            string json = TryExecuteCommand(commandText, manifestPath);
            DestinyItem item = JsonConvert.DeserializeObject<DestinyItem>(json);
            return item;
        }

        public DestinyItemCategory LookupItemCategory(uint itemCategoryHash)
        {
            int itemCategoryId = ConvertHashToTableId(itemCategoryHash);
            string commandText = $"SELECT json FROM DestinyItemCategoryDefinition WHERE id = {itemCategoryId}";
            string json = TryExecuteCommand(commandText, manifestPath);
            DestinyItemCategory itemCategory = JsonConvert.DeserializeObject<DestinyItemCategory>(json);
            return itemCategory;
        }

        private string TryExecuteCommand(string commandText, string manifestPath)
        {
            try {
                return databaseAccessor.ExecuteCommand(commandText, manifestPath);
            } catch (Exception e) {
                throw new EmissaryDataAccessException(e.Message);
            }
        }

        private int ConvertHashToTableId(uint hash)
        {
            int id = (int)hash;
            // TODO what does this do???
            // if ((id & (1 << (32 - 1))) != 0) {
            // id = id - (1 << 32);
            // }
            return id;
        }

        // private string ExecuteCommandOnManifestDatabase(string commandText)
        // {
        //     string dbPath = GetLocalManifestFilePath();
        //     string queryResult = "";
        //     using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}")) {
        //         db.Open();
        //         SqliteCommand selectCommand = new SqliteCommand(commandText, db);
        //         SqliteDataReader reader = selectCommand.ExecuteReader();
        //         reader.Read();
        //         queryResult = reader.GetString(0);
        //         db.Close();
        //     }
        //     return queryResult;
        // }

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