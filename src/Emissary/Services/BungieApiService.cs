using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Emissary
{
    public class BungieApiService : IBungieApiService
    {
        private HttpClient httpClient;
        private string bungieApiKey;

        public BungieApiService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.bungieApiKey = GetBungieApiKey();
            // this.httpClient = new HttpClient();
            // this.httpClient.DefaultRequestHeaders.Add("X-API-KEY", GetBungieApiKey());
        }

        public OAuthResponse GetOAuthAccessToken(OAuthRequest request)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage();
            httpRequest.RequestUri = new Uri("https://www.bungie.net/Platform/App/OAuth/Token/");
            httpRequest.Method = HttpMethod.Post;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("grant_type", "authorization_code");
            dic.Add("code", request.AuthCode);
            dic.Add("client_id", request.ClientId);
            dic.Add("client_secret", request.ClientSecret);
            httpRequest.Content = new FormUrlEncodedContent(dic);

            httpRequest.Content.Headers.Add("X-API-KEY", bungieApiKey);
            // httpRequest.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            HttpResponseMessage httpResponse = httpClient.SendAsync(httpRequest).Result;
            string responseJson = httpResponse.Content.ReadAsStringAsync().Result;
            OAuthResponse response = JsonConvert.DeserializeObject<OAuthResponse>(responseJson);
            return response;
        }

        public UserMembershipsResponse GetMembershipsForUser(UserMembershipsRequest request)
        {
            throw new NotImplementedException();
        }

        public ProfileCharactersResponse GetProfileCharacters(ProfileCharactersRequest request)
        {
            throw new NotImplementedException();
        }

        public CharacterEquipmentResponse GetCharacterEquipment(CharacterEquipmentRequest request)
        {
            throw new NotImplementedException();
        }

        public EquipItemsResponse EquipItems(EquipItemsRequest request)
        {
            throw new NotImplementedException();
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