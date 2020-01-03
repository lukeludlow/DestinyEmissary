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
            refreshResponse.RefreshTokenExpiresInSeconds = 7776000;
            Mock.Get(bungieApiService).Setup(m => m.RefreshAccessToken("refresh.token")).Returns(refreshResponse);

            IAuthorizationService authorizationService = new AuthorizationService(config, bungieApiService, emissaryDao);

            string actual = authorizationService.GetAccessToken(discordId);
            Assert.AreEqual("new.access.token", actual);
            Mock.Get(bungieApiService).Verify(m => m.RefreshAccessToken("refresh.token"), Times.Once());
            Mock.Get(emissaryDao).Verify(m => m.AddOrUpdateAccessToken(It.Is<BungieAccessToken>(r => r.AccessToken == "new.access.token" && r.AccessTokenCreatedDate > timeSixtyMinsAgo)), Times.Once());
        }

        [TestMethod]
        public void GetAccessToken_TokenIsExpiredAndRefreshTokenIsAlsoExpired_ShouldNotCallBungieApiAndShouldReturnNull()
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

            Mock.Get(emissaryDao).Setup(m => m.GetAccessTokenByDiscordId(discordId)).Returns(accessToken);

            DateTimeOffset currentTime = DateTimeOffset.UtcNow;
            DateTimeOffset timeOverNinetyDaysAgo = currentTime.AddDays(-91);
            accessToken.RefreshTokenCreatedDate = timeOverNinetyDaysAgo;
            DateTimeOffset timeOverSixtyMinsAgo = currentTime.AddSeconds(-3601);
            accessToken.AccessTokenCreatedDate = timeOverSixtyMinsAgo;

            IAuthorizationService authorizationService = new AuthorizationService(config, bungieApiService, emissaryDao);
            string actual = authorizationService.GetAccessToken(discordId);

            Assert.IsNull(actual);
            Mock.Get(bungieApiService).VerifyNoOtherCalls();
        }

        [TestMethod]
        public void GetAccessToken_TriesToRefreshTokenButApiRequestFails_ShouldCallBungieApiAndShouldReturnNull()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();

            OAuthResponse errorResponse = new OAuthResponse();
            errorResponse.ErrorType = "invalid_request";
            errorResponse.ErrorDescription = "Missing required parameter code";
            Mock.Get(bungieApiService).Setup(m => m.RefreshAccessToken("refresh-token")).Returns(errorResponse);

            IAuthorizationService authorizationService = new AuthorizationService(config, bungieApiService, emissaryDao);
            string actual = authorizationService.GetAccessToken(69);

            Assert.IsNull(actual);
            Mock.Get(bungieApiService).VerifyNoOtherCalls();
        }

        [TestMethod]
        public void AuthorizeUser_AuthCodeIsValid_ShouldSendRequestToBungieApiAndUpdateDatabaseAndReturnTrue()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();

            DateTimeOffset currentTimeBeforeRequest = DateTimeOffset.UtcNow;

            OAuthResponse oauthResponse = new OAuthResponse();
            oauthResponse.AccessToken = "new-access-token";
            oauthResponse.RefreshToken = "refresh-token";
            oauthResponse.AccessTokenExpiresInSeconds = 3600;
            oauthResponse.RefreshTokenExpiresInSeconds = 7776000;

            Mock.Get(bungieApiService).Setup(m => m.GetOAuthAccessToken(It.IsAny<OAuthRequest>())).Returns(oauthResponse);

            IAuthorizationService authorizationService = new AuthorizationService(config, bungieApiService, emissaryDao);

            ulong discordId = 69;
            OAuthResponse actual = authorizationService.AuthorizeUser(discordId, "auth-code");
            Assert.IsFalse(string.IsNullOrWhiteSpace(actual.AccessToken));
            Assert.IsTrue(string.IsNullOrWhiteSpace(actual.ErrorType));
            Mock.Get(bungieApiService).Verify(m => m.GetOAuthAccessToken(It.Is<OAuthRequest>(r => r.AuthCode == "auth-code")), Times.Once());
            Mock.Get(bungieApiService).VerifyNoOtherCalls();
            Mock.Get(emissaryDao).Verify(m => m.AddOrUpdateAccessToken(It.Is<BungieAccessToken>(r => r.DiscordId == discordId && r.AccessToken == "new-access-token" && r.RefreshToken == "refresh-token" && r.AccessTokenExpiresInSeconds == 3600 && r.RefreshTokenExpiresInSeconds == 7776000 && r.AccessTokenCreatedDate > currentTimeBeforeRequest && r.RefreshTokenCreatedDate > currentTimeBeforeRequest)), Times.Once());
            Mock.Get(emissaryDao).VerifyNoOtherCalls();
        }


        [TestMethod]
        public void AuthorizeUser_AuthCodeInvalid_ShouldSendRequestToBungieApiButThenReturnErrorAndNotUpdateDatabase()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();

            OAuthResponse errorOAuthResponse = new OAuthResponse();
            errorOAuthResponse.AccessToken = default;
            errorOAuthResponse.RefreshToken = default;
            errorOAuthResponse.AccessTokenExpiresInSeconds = default;
            errorOAuthResponse.RefreshTokenExpiresInSeconds = default;
            errorOAuthResponse.ErrorType = "server_error";
            errorOAuthResponse.ErrorDescription = "Invalid length for a Base-64 char array or string.";

            Mock.Get(bungieApiService).Setup(m => m.GetOAuthAccessToken(It.IsAny<OAuthRequest>())).Returns(errorOAuthResponse);

            IAuthorizationService authorizationService = new AuthorizationService(config, bungieApiService, emissaryDao);
            OAuthResponse actual = authorizationService.AuthorizeUser(69, "bad-auth-code");

            Assert.IsTrue(string.IsNullOrWhiteSpace(actual.AccessToken));
            Assert.IsFalse(string.IsNullOrWhiteSpace(actual.ErrorType));
            Mock.Get(bungieApiService).Verify(m => m.GetOAuthAccessToken(It.Is<OAuthRequest>(r => r.AuthCode == "bad-auth-code")), Times.Once());
            Mock.Get(bungieApiService).VerifyNoOtherCalls();
            Mock.Get(emissaryDao).VerifyNoOtherCalls();
        }


    }
}