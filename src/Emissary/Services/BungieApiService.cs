using System;
using System.IO;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Emissary
{
    public class BungieApiService : IBungieApiService
    {
        private HttpClient httpClient;

        public BungieApiService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            // this.httpClient = new HttpClient();
            // this.httpClient.DefaultRequestHeaders.Add("X-API-KEY", GetBungieApiKey());
        }

        public DestinyProfileCharactersResponse GetCharacters(int membershipType, long bungieId)
        {
            throw new NotImplementedException();
        }

        public DestinyProfileCharacterEquipmentResponse GetEquipment(int membershipType, long bungieId)
        {
            throw new NotImplementedException();
        }

        public SearchDestinyPlayerResponse SearchDestinyPlayer(int membershipType, string displayName)
        {
            string requestPath = $"SearchDestinyPlayer/{membershipType}/{displayName}";
            string jsonResponse = Get(requestPath);
            SearchDestinyPlayerResponse response = JsonConvert.DeserializeObject<SearchDestinyPlayerResponse>(jsonResponse);
            return response;
        }


        // send a GET request to the specified URL to retrieve information (from the bungie api)
        private string Get(string requestUrl)
        {
            // note: this is blocking! (because i'm too lazy to implement async requests right now)
            HttpResponseMessage httpResponse = httpClient.GetAsync(requestUrl).Result;
            string json = httpResponse.Content.ReadAsStringAsync().Result;
            return json;
        }

        // TODO refactor this
        public Stream SendRequestAndReturnStream(string requestUrl)
        {
            // very much blocking. might want to change this later since it's a kinda big file.
            HttpResponseMessage httpResponse = httpClient.GetAsync(requestUrl).Result;
            return httpResponse.Content.ReadAsStreamAsync().Result;
        }

        private string GetBungieApiKey()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            string dataDirectory = Path.Combine(solutionDirectory, "data");
            string secretsFileName = "secrets.json";
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(dataDirectory)
                .AddJsonFile(secretsFileName);
            IConfiguration config = configBuilder.Build();
            string bungieApiKeyName = "BungieApiKey";
            string bungieApiKey = config[bungieApiKeyName];
            return bungieApiKey;
        }

    }
}