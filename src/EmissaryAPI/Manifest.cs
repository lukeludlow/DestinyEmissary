using System;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Data.Sqlite;

namespace EmissaryApi
{
    public class Manifest
    {

        // TODO turn this into an interface and mock it 
        private BungieApiProxy bungieApiProxy;

        public string CurrentVersion { get; set; }

        public Manifest()
        {
            this.bungieApiProxy = new BungieApiProxy();
        }

        public string GetDestinyInventoryItemDefinition(UInt32 itemHash)
        {
            string dbPath = GetLocalManifestFilePath();
            string queryResult = "";
            using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}")) {
                db.Open();
                // perform the query using json extensions. this makes life a lot easier, because otherwise 
                // i have to manually convert the uint32 itemHash using bitshift stuff
                string commandText = 
                        "SELECT json_extract(DestinyInventoryItemDefinition.json, '$')" +
                        "FROM DestinyInventoryItemDefinition, json_tree(DestinyInventoryItemDefinition.json, '$')" +
                        $"WHERE json_tree.key = 'hash' AND json_tree.value = {itemHash}";
                SqliteCommand selectCommand = new SqliteCommand(commandText, db);
                SqliteDataReader reader = selectCommand.ExecuteReader();
                reader.Read();
                queryResult = reader.GetString(0);
                db.Close();
            }
            return queryResult;
        }

        private string GetLocalManifestFilePath()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            string dataDirectory = Path.Combine(solutionDirectory, "data");
            string localManifestFileName = "79433.19.11.13.1925-2.manifest";
            string localManifestFile = Path.Combine(dataDirectory, localManifestFileName);
            return localManifestFile;
        }

        // TODO make it so that this checks for the latest version and downloads the new manifest only if needed
        public void LoadLatestManifest()
        {
            string requestUrl = "https://www.bungie.net/Platform/Destiny2/Manifest/";
            string json = bungieApiProxy.SendRequest(requestUrl);
            dynamic response = JObject.Parse(json);
            // the main game world database is listed in the Response.mobileWorldContentPaths.en object 
            string remoteManifestPath = response.Response.mobileWorldContentPaths.en;
            string remoteManifestUrl = "https://www.bungie.net" + remoteManifestPath;

            string latestManifestVersion = response.Response.version;

            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            string dataDirectory = Path.Combine(solutionDirectory, "data");
            string localManifestFileName = latestManifestVersion + ".manifest";
            string localManifestFile = Path.Combine(dataDirectory, localManifestFileName);

            Stream stream = bungieApiProxy.SendRequestAndReturnStream(remoteManifestUrl);
            using (MemoryStream memoryStream = new MemoryStream()) {
                stream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                ZipArchive manifestZip = new ZipArchive(memoryStream);
                manifestZip.Entries[0].ExtractToFile(localManifestFile);
            }
        }

        private bool IsMatchingVersion(string a, string b) 
        {
            throw new NotImplementedException();
        }

    }
}

// example from this thread:
// https://www.bungie.net/sr/Groups/Post?groupId=39966&postId=105901734&sort=0&path=0&showBanned=0

//     /// <summary>
//     /// where the magic happens...
//     /// Takes a SQLite manifest file, and returns a Manifest class with all the data organised into queriable json.
//     /// </summary>
//     /// <param name="manifestFile">the bungie manifest SQLite database file</param>
//     public Manifest(string manifestFile)
//     {
//         using (var sqConn = new SQLiteConnection("Data Source=" + manifestFile + ";Version=3;")) {
//             sqConn.Open();
//             //build a list of all the tables
//             List<string> tableNames = new List<string>();
//             using (var sqCmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table'", sqConn)) {
//                 using (var sqReader = sqCmd.ExecuteReader()) {
//                     while (sqReader.Read())
//                         tableNames.Add((string)sqReader["name"]);
//                 }
//             }
//             //get the json for each row, in each table, and store it in a lovely array
//             List<ManifestBranch> manifestData = new List<ManifestBranch>();
//             foreach (var tableName in tableNames) {
//                 var manifestBranch = new ManifestBranch { TableName = tableName, Json = new List<dynamic>(), JsonString = new List<string>() };
//                 using (var sqCmd = new SQLiteCommand("SELECT * FROM " + tableName, sqConn)) {
//                     using (var sqReader = sqCmd.ExecuteReader()) {
//                         while (sqReader.Read()) {
//                             byte[] jsonData = (byte[])sqReader["json"];
//                             string jsonString = Encoding.ASCII.GetString(jsonData);
//                             manifestBranch.Json.Add(JObject.Parse(jsonString)); //you don't need to do this unless you want queriable json at this level ;)
//                             manifestBranch.JsonString.Add(jsonString);
//                         }
//                     }
//                 }
//                 manifestData.Add(manifestBranch);
//             }
//             //this next bit takes all of the json, for all of the tables, and wraps it up nicely
//             //into a single json string, which is then made dynamic and thus queriable ^_^
//             string fullJson = "{\"manifest\":[";
//             foreach (var manifestBranch in manifestData) {
//                 fullJson += "{\"" + manifestBranch.TableName + "\":[" + string.Join(",", manifestBranch.JsonString) + "]},";
//             }
//             fullJson = fullJson.TrimEnd(',') + "]}";
//             this.ManifestData = JObject.Parse(fullJson);
//             //instead of the above, you can just loop through each branch and create individual files to directly replicate lowlines code example
//             //however, it's probably best to do this further up, where we're looping through each table, rather than down here at the end ;)
//             sqConn.Close();
//         }
//     }

// }