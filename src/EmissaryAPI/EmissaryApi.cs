using System;
using System.Net.Http;
using System.IO;  // Directory
using System.Reflection;  // Assembly
using Microsoft.Extensions.Configuration;

namespace EmissaryApi
{
    public class EmissaryApi
    {

        public static string GetBungieApiKey()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            string secretsFileName = "secrets.json";
            Console.WriteLine($"looking for secrets at {solutionDirectory}/{secretsFileName}");
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(solutionDirectory)
                .AddJsonFile(secretsFileName);
            IConfiguration config = configBuilder.Build();
            string bungieApiKeyName = "BungieApiKey";
            string bungieApiKey = config[bungieApiKeyName];
            return bungieApiKey;
        }

        public static void CurrentlyEquipped()
        {
        }

        public static string DummyGetManifestInventoryItem()
        {
            using (var client = new HttpClient()) {
                client.DefaultRequestHeaders.Add("X-API-KEY", GetBungieApiKey());
                // this is a blocking statement
                var response = client.GetAsync("https://www.bungie.net/platform/Destiny/Manifest/InventoryItem/1274330687/").Result;
                var content = response.Content.ReadAsStringAsync().Result;
                dynamic item = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                string itemName = item.Response.data.inventoryItem.itemName;
                Console.WriteLine($"found item name: {itemName}");
                return itemName;
            }
        }

    }
}
