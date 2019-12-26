using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EmissaryCore
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

        public OAuthResponse GetOAuthAccessToken(OAuthRequest oauthRequest)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://www.bungie.net/Platform/App/OAuth/Token/");
            request.Method = HttpMethod.Post;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("grant_type", "authorization_code");
            dic.Add("code", oauthRequest.AuthCode);
            dic.Add("client_id", oauthRequest.ClientId);
            dic.Add("client_secret", oauthRequest.ClientSecret);
            request.Content = new FormUrlEncodedContent(dic);
            request.Content.Headers.Add("X-API-KEY", bungieApiKey);
            // httpRequest.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            HttpResponseMessage response = httpClient.SendAsync(request).Result;
            string json = response.Content.ReadAsStringAsync().Result;
            OAuthResponse oauthResponse = JsonConvert.DeserializeObject<OAuthResponse>(json);
            return oauthResponse;
        }

        public UserMembershipsResponse GetMembershipsForUser(UserMembershipsRequest membershipsRequest)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://www.bungie.net/Platform/User/GetMembershipsForCurrentUser/");
            request.Method = HttpMethod.Get;
            request.Headers.Add("X-API-KEY", bungieApiKey);
            request.Headers.Add("Authorization", $"Bearer {membershipsRequest.AccessToken}");
            HttpResponseMessage response = httpClient.SendAsync(request).Result;
            if (response.StatusCode == HttpStatusCode.Unauthorized) {
                throw new BungieApiException("Unauthorized: Access is denied due to invalid credentials.");
            }
            // TODO what about other exceptions? like when the token is empty string and it returns a 500 error code?
            string json = response.Content.ReadAsStringAsync().Result;
            UserMembershipsResponse membershipsResponse = JsonConvert.DeserializeObject<UserMembershipsResponse>(json);
            return membershipsResponse;
        }

        public ProfileCharactersResponse GetProfileCharacters(ProfileCharactersRequest charactersRequest)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"https://www.bungie.net/Platform/Destiny2/{charactersRequest.MembershipType}/Profile/{charactersRequest.DestinyMembershipId}/?components=200");
            request.Headers.Add("X-API-KEY", bungieApiKey);
            HttpResponseMessage response = httpClient.SendAsync(request).Result;
            if (response.IsSuccessStatusCode) {
                string json = response.Content.ReadAsStringAsync().Result;
                ProfileCharactersResponse charactersResponse = JsonConvert.DeserializeObject<ProfileCharactersResponse>(json);
                return charactersResponse;
            } else if (response.StatusCode == HttpStatusCode.InternalServerError) {
                dynamic errorResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                throw new BungieApiException($"{errorResponse.ErrorStatus}: {errorResponse.Message}");
            } else {
                throw new BungieApiException($"{response.StatusCode}: {response.Content.ReadAsStringAsync().Result}");
            }
        }

        public CharacterEquipmentResponse GetCharacterEquipment(CharacterEquipmentRequest equipmentRequest)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"https://www.bungie.net/Platform/Destiny2/{equipmentRequest.MembershipType}/Profile/{equipmentRequest.MembershipId}/Character/{equipmentRequest.DestinyCharacterId}/?components=205");
            request.Headers.Add("X-API-KEY", bungieApiKey);
            HttpResponseMessage response = httpClient.SendAsync(request).Result;
            if (response.IsSuccessStatusCode) {
                string json = response.Content.ReadAsStringAsync().Result;
                CharacterEquipmentResponse equipmentResponse = JsonConvert.DeserializeObject<CharacterEquipmentResponse>(json);
                if (equipmentResponse == null || equipmentResponse.Items == null || equipmentResponse.Items.Length <= 0) {
                    throw new BungieApiException("character not found: Account exists but could not get equipment for the given character.");
                }
                return equipmentResponse;
            } else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.InternalServerError) {
                dynamic errorResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                throw new BungieApiException($"{errorResponse.ErrorStatus}: {errorResponse.Message}");
            } else {
                throw new NotImplementedException();
            }
        }

        public EquipItemsResponse EquipItems(EquipItemsRequest equipRequest)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://www.bungie.net/Platform/Destiny2/Actions/Items/EquipItems/");
            request.Method = HttpMethod.Post;
            string contentString = JsonConvert.SerializeObject(equipRequest);
            request.Content = new StringContent(contentString, Encoding.UTF8, "application/json");
            request.Headers.Add("X-API-KEY", bungieApiKey);
            request.Headers.Add("Authorization", $"Bearer {equipRequest.AccessToken}");
            HttpResponseMessage response = httpClient.SendAsync(request).Result;
            if (response.IsSuccessStatusCode) {
                string json = response.Content.ReadAsStringAsync().Result;
                EquipItemsResponse equipResponse = JsonConvert.DeserializeObject<EquipItemsResponse>(json);
                return equipResponse;
            } else if (response.StatusCode == HttpStatusCode.Unauthorized) {
                throw new BungieApiException("Unauthorized: Access is denied due to invalid credentials.");
            } else {
                throw new BungieApiException("idk");
            }
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