using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace EmissaryCore
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IConfiguration config;
        private readonly IBungieApiService bungieApiService;
        private readonly IEmissaryDao emissaryDao;

        public AuthorizationService(IConfiguration config, IBungieApiService bungieApiService, IEmissaryDao emissaryDao)
        {
            this.config = config;
            this.bungieApiService = bungieApiService;
            this.emissaryDao = emissaryDao;
        }

        public string GetAccessToken(ulong discordId)
        {
            BungieAccessToken accessToken = emissaryDao.GetAccessTokenByDiscordId(discordId);
            if (accessToken == null) {
                return null;
            }
            if (AccessTokenIsExpired(accessToken)) {
                if (RefreshTokenIsExpired(accessToken)) {
                    return null;
                }
                OAuthResponse refreshResponse = bungieApiService.RefreshAccessToken(accessToken.RefreshToken);
                accessToken.AccessToken = refreshResponse.AccessToken;
                accessToken.AccessTokenExpiresInSeconds = refreshResponse.AccessTokenExpiresInSeconds;
                accessToken.AccessTokenCreatedDate = DateTimeOffset.UtcNow;
                accessToken.RefreshTokenExpiresInSeconds = refreshResponse.RefreshTokenExpiresInSeconds;
                emissaryDao.AddOrUpdateAccessToken(accessToken);
            }
            return accessToken.AccessToken;
        }

        private bool RefreshTokenIsExpired(BungieAccessToken accessToken)
        {
            DateTimeOffset currentTime = DateTimeOffset.UtcNow;
            DateTimeOffset expireTime = accessToken.RefreshTokenCreatedDate.AddSeconds(accessToken.RefreshTokenExpiresInSeconds);
            return currentTime >= expireTime;
        }

        private bool AccessTokenIsExpired(BungieAccessToken accessToken)
        {
            DateTimeOffset currentTime = DateTimeOffset.UtcNow;
            DateTimeOffset expireTime = accessToken.AccessTokenCreatedDate.AddSeconds(accessToken.AccessTokenExpiresInSeconds);
            return currentTime >= expireTime;
        }

        public OAuthResponse AuthorizeUser(ulong discordId, string authCode)
        {
            OAuthRequest oauthRequest = new OAuthRequest(authCode);
            OAuthResponse oauthResponse = bungieApiService.GetOAuthAccessToken(oauthRequest);
            if (string.IsNullOrWhiteSpace(oauthResponse.AccessToken)) {
                return oauthResponse;
            }
            BungieAccessToken accessToken = new BungieAccessToken();
            accessToken.DiscordId = discordId;
            accessToken.AccessToken = oauthResponse.AccessToken;
            accessToken.RefreshToken = oauthResponse.RefreshToken;
            accessToken.AccessTokenExpiresInSeconds = oauthResponse.AccessTokenExpiresInSeconds;
            accessToken.RefreshTokenExpiresInSeconds = oauthResponse.RefreshTokenExpiresInSeconds;
            accessToken.AccessTokenCreatedDate = DateTimeOffset.UtcNow;
            accessToken.RefreshTokenCreatedDate = DateTimeOffset.UtcNow;
            emissaryDao.AddOrUpdateAccessToken(accessToken);
            return oauthResponse;
        }


        // public OAuthResponse GetOAuthAccessToken(OAuthRequest oauthRequest)
        // {
        //     HttpRequestMessage request = new HttpRequestMessage();
        //     request.RequestUri = new Uri("https://www.bungie.net/Platform/App/OAuth/Token/");
        //     request.Method = HttpMethod.Post;
        //     Dictionary<string, string> dic = new Dictionary<string, string>();
        //     dic.Add("grant_type", "authorization_code");
        //     dic.Add("code", oauthRequest.AuthCode);
        //     dic.Add("client_id", config["Bungie:ClientId"]);
        //     dic.Add("client_secret", config["Bungie:ClientSecret"]);
        //     request.Content = new FormUrlEncodedContent(dic);
        //     request.Content.Headers.Add("X-API-KEY", config["Bungie:ApiKey"]);
        //     // httpRequest.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
        //     HttpResponseMessage response = httpClient.SendAsync(request).Result;
        //     string json = response.Content.ReadAsStringAsync().Result;
        //     OAuthResponse oauthResponse = JsonConvert.DeserializeObject<OAuthResponse>(json);
        //     return oauthResponse;
        // }

    }
}