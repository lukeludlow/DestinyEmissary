using System;
using System.Net.Http;
using System.IO;  // Directory
using System.Reflection;  // Assembly
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EmissaryApi.Model;

namespace EmissaryApi
{
    public class Emissary
    {

        private HttpClient httpClient;

        public Emissary()
        {
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Add("X-API-KEY", GetBungieApiKey());
        }

        // constructor used for dependency injection stuff
        public Emissary(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public string GetBungieApiKey()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            string secretsFileName = "secrets.json";
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(solutionDirectory)
                .AddJsonFile(secretsFileName);
            IConfiguration config = configBuilder.Build();
            string bungieApiKeyName = "BungieApiKey";
            string bungieApiKey = config[bungieApiKeyName];
            return bungieApiKey;
        }

        public string DummyGetManifestInventoryItem()
        {
            // this is a blocking statement
            var response = httpClient.GetAsync("https://www.bungie.net/platform/Destiny/Manifest/InventoryItem/1274330687/").Result;
            var content = response.Content.ReadAsStringAsync().Result;
            dynamic item = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
            //Console.WriteLine($"full message: {item}");
            string itemName = item.Response.data.inventoryItem.itemName;
            Console.WriteLine($"found item name: {itemName}");
            return itemName;
        }

        public bool TrySearchDestinyPlayer(string displayName, out string membershipId)
        {
            int membershipType = BungieMembershipType.All;
            string requestUrl = $"https://www.bungie.net/platform/Destiny2/SearchDestinyPlayer/{membershipType}/{displayName}/";

            HttpResponseMessage httpResponse = httpClient.GetAsync(requestUrl).Result;
            string json = httpResponse.Content.ReadAsStringAsync().Result;
            SearchDestinyPlayerResponse response = JsonConvert.DeserializeObject<SearchDestinyPlayerResponse>(json);

            bool foundPlayer = false;
            membershipId = "";
            foreach (UserInfoCard userInfo in response.Response) {
                if (userInfo.DisplayName == displayName) {
                    foundPlayer = true;
                    membershipId = userInfo.MembershipId;
                    break;
                }
            }
            return foundPlayer;
        }



    }
}
