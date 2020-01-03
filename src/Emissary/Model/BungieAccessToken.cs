using System;
using System.ComponentModel.DataAnnotations;

namespace EmissaryCore
{
    public class BungieAccessToken
    {
        [Key]
        public ulong DiscordId { get; set; }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public int AccessTokenExpiresInSeconds { get; set; }
        public int RefreshTokenExpiresInSeconds { get; set; }

        public DateTimeOffset AccessTokenCreatedDate { get; set; }
        public DateTimeOffset RefreshTokenCreatedDate { get; set; }


        public BungieAccessToken(ulong discordId, string accessToken, string refreshToken, int accessTokenExpiresInSeconds, int refreshTokenExpiresInSeconds, DateTimeOffset accessTokenCreatedDate, DateTimeOffset refreshTokenCreatedDate)
        {
            this.DiscordId = discordId;
            this.AccessToken = accessToken;
            this.RefreshToken = refreshToken;
            this.AccessTokenExpiresInSeconds = accessTokenExpiresInSeconds;
            this.RefreshTokenExpiresInSeconds = refreshTokenExpiresInSeconds;
            this.AccessTokenCreatedDate = accessTokenCreatedDate;
            this.RefreshTokenCreatedDate = refreshTokenCreatedDate;
        }

        public BungieAccessToken()
        {
            this.DiscordId = default;
            this.AccessToken = default;
            this.RefreshToken = default;
            this.AccessTokenExpiresInSeconds = default;
            this.RefreshTokenExpiresInSeconds = default;
            this.AccessTokenCreatedDate = default;
            this.RefreshTokenCreatedDate = default;
        }
    }
}