using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryCore;
using Moq;
using Microsoft.Extensions.Configuration;

namespace EmissaryTests.Service
{
    [TestClass]
    public class AuthorizationServiceTests
    {

        [TestMethod]
        public void GetAccessToken_UserHasNoTokenYet_ShouldReturnNull()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();
            ulong discordId = 69;
            Mock.Get(emissaryDao).Setup(m => m.GetAccessTokenByDiscordId(discordId)).Returns(value: null);
            IAuthorizationService authorizationService = new AuthorizationService(config, bungieApiService, emissaryDao);
            string actual = authorizationService.GetAccessToken(discordId);
            Assert.IsTrue(string.IsNullOrWhiteSpace(actual));
            Mock.Get(bungieApiService).Verify(m => m.RefreshAccessToken(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void GetAccessToken_UserHasValidToken_ShouldReturnToken()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();

            ulong discordId = 69;
            BungieAccessToken accessToken = new BungieAccessToken();
            accessToken.DiscordId = discordId;
            accessToken.AccessToken = "access.token";
            accessToken.RefreshToken = "refresh.token";
            accessToken.AccessTokenExpiresInSeconds = 3600;
            accessToken.RefreshTokenExpiresInSeconds = 7776000;
            accessToken.AccessTokenCreatedDate = DateTimeOffset.UtcNow;
            accessToken.RefreshTokenCreatedDate = DateTimeOffset.UtcNow;

            Mock.Get(emissaryDao).Setup(m => m.GetAccessTokenByDiscordId(discordId)).Returns(accessToken);

            IAuthorizationService authorizationService = new AuthorizationService(config, bungieApiService, emissaryDao);

            string actual = authorizationService.GetAccessToken(discordId);
            Assert.AreEqual("access.token", actual);
            Mock.Get(bungieApiService).Verify(m => m.RefreshAccessToken(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void GetAccessToken_TokenHasExpiredButCanBeRefreshed_ShouldSendRequestToBungieApiAndUpdateDatabaseAndReturnNewToken()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();

            ulong discordId = 69;
            BungieAccessToken accessToken = new BungieAccessToken();
            accessToken.DiscordId = discordId;
            accessToken.AccessToken = "access.token";
            accessToken.RefreshToken = "refresh.token";
            accessToken.AccessTokenExpiresInSeconds = 3600;
            accessToken.RefreshTokenExpiresInSeconds = 7776000;

            DateTimeOffset currentTime = DateTimeOffset.UtcNow;
            DateTimeOffset timeSixtyMinsAgo = currentTime.AddSeconds(-3601);
            accessToken.AccessTokenCreatedDate = timeSixtyMinsAgo;
            accessToken.RefreshTokenCreatedDate = timeSixtyMinsAgo;

            Mock.Get(emissaryDao).Setup(m => m.GetAccessTokenByDiscordId(discordId)).Returns(accessToken);

            OAuthResponse refreshResponse = new OAuthResponse();
            refreshResponse.AccessToken = "new.access.token";
            refreshResponse.RefreshToken = "refresh.token";
            refreshResponse.AccessTokenExpiresInSeconds = 3600;
            refreshResponse.RefreshTokenExpiresInSeconds = 7772399; // (7776000 - 3601). but i'm not sure what this should be
            Mock.Get(bungieApiService).Setup(m => m.RefreshAccessToken("refresh.token")).Returns(refreshResponse);

            IAuthorizationService authorizationService = new AuthorizationService(config, bungieApiService, emissaryDao);

            string actual = authorizationService.GetAccessToken(discordId);
            Assert.AreEqual("new.access.token", actual);
            Mock.Get(bungieApiService).Verify(m => m.RefreshAccessToken("refresh.token"), Times.Once());
            Mock.Get(emissaryDao).Verify(m => m.AddOrUpdateAccessToken(It.Is<BungieAccessToken>(r => r.AccessToken == "new.access.token" && r.AccessTokenCreatedDate > timeSixtyMinsAgo)), Times.Once());
        }

        [TestMethod]
        public void GetAccessToken_TokenHasExpiredAndCantBeRefreshed_ShouldReturnNull()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();

            ulong discordId = 69;
            BungieAccessToken accessToken = new BungieAccessToken();
            accessToken.DiscordId = discordId;
            accessToken.AccessToken = "access.token";
            accessToken.RefreshToken = "refresh.token";
            accessToken.AccessTokenExpiresInSeconds = 3600;
            int oneDayInSeconds = 86400;
            accessToken.RefreshTokenExpiresInSeconds = oneDayInSeconds;  // i'm not sure what this should be

            Mock.Get(emissaryDao).Setup(m => m.GetAccessTokenByDiscordId(discordId)).Returns(accessToken);

            DateTimeOffset currentTime = DateTimeOffset.UtcNow;
            DateTimeOffset timeTwoDaysAgo = currentTime.AddDays(-2);
            accessToken.RefreshTokenCreatedDate = timeTwoDaysAgo;
            DateTimeOffset timeSixtyMinsAgo = currentTime.AddSeconds(-3601);
            accessToken.AccessTokenCreatedDate = timeSixtyMinsAgo;

            IAuthorizationService authorizationService = new AuthorizationService(config, bungieApiService, emissaryDao);

            string actual = authorizationService.GetAccessToken(discordId);
            Assert.IsNull(actual);
            Mock.Get(bungieApiService).Verify(m => m.RefreshAccessToken(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void AuthorizeUser_AuthCodeIsValid_ShouldSendRequestToBungieApiAndUpdateDatabaseAndReturnTrue()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();

            IAuthorizationService authorizationService = new AuthorizationService(config, bungieApiService, emissaryDao);

            Assert.Fail();
        }

        [TestMethod]
        public void AuthorizeUser_AuthCodeInvalid_ShouldSendRequestToBungieApiButThenReturnFalse()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void AuthorizeUser_UserIsNotRegistered_Should()
        {
            Assert.Fail();
        }






    }
}