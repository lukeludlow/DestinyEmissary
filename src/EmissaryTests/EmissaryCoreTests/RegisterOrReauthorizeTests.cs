using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryCore;
using System.Collections.Generic;
using System.IO;
using Moq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Newtonsoft.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace EmissaryTests.Core
{
    [TestClass]
    public class RegisterOrReauthorizeTests
    {

        [TestMethod]
        public void RegisterOrReauthorize_NewUser_ShouldRequestNewAccessTokenAndWriteToDatabase()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 221313820847636491;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            string accessToken = "access-token";
            EmissaryUser expectedUser = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            string authCode = "auth-code";

            OAuthResponse authResponse = new OAuthResponse();
            authResponse.AccessToken = accessToken;

            Mock.Get(bungieApiService)
                .Setup(m =>
                    m.GetOAuthAccessToken(It.Is<OAuthRequest>(r =>
                        r.AuthCode == authCode)))
                .Returns(authResponse);

            UserMembershipsResponse membershipsResponse = new UserMembershipsResponse();
            membershipsResponse.DestinyMemberships = new List<DestinyMembership>();
            membershipsResponse.DestinyMemberships.Add(new DestinyMembership("pimpdaddy", destinyProfileId, destinyMembershipType, BungieMembershipType.Steam));

            Mock.Get(bungieApiService)
                .Setup(m =>
                    m.GetMembershipsForUser(It.Is<UserMembershipsRequest>(r =>
                        r.AccessToken == accessToken)))
                .Returns(membershipsResponse);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.RegisterOrReauthorize(discordId, authCode);
            Assert.IsTrue(result.Success);

            Mock.Get(bungieApiService).Verify(m => m.GetOAuthAccessToken(It.IsAny<OAuthRequest>()), Times.Once());
            Mock.Get(bungieApiService).Verify(m => m.GetMembershipsForUser(It.IsAny<UserMembershipsRequest>()), Times.Once());

            Mock.Get(userDao)
                .Verify(m =>
                    m.AddOrUpdateUser(It.Is<EmissaryUser>(u =>
                        u.DiscordId == discordId &&
                        u.DestinyProfileId == destinyProfileId &&
                        u.DestinyMembershipType == destinyMembershipType &&
                        u.BungieAccessToken == accessToken)), Times.Once());
        }

        [TestMethod]
        public void RegisterOrReauthorize_UserAlreadyExists_ShouldRefreshAccessTokenAndWriteToDatabase()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 221313820847636491;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            string expiredAccessToken = "expired-access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, expiredAccessToken);

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);

            string authCode = "auth-code";

            OAuthResponse oauthResponse = new OAuthResponse();
            string newAccessToken = "new-access-token";
            oauthResponse.AccessToken = newAccessToken;

            Mock.Get(bungieApiService)
                .Setup(m =>
                    m.GetOAuthAccessToken(It.Is<OAuthRequest>(r =>
                        r.AuthCode == authCode)))
                .Returns(oauthResponse);


            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.RegisterOrReauthorize(discordId, authCode);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Message.ToLower().Contains("reauthorized"));

            Mock.Get(userDao).Verify(m => m.GetUserByDiscordId(discordId), Times.Once());

            Mock.Get(userDao).Verify(m =>
                m.AddOrUpdateUser(It.Is<EmissaryUser>(r =>
                    r.DiscordId == discordId &&
                    r.BungieAccessToken == newAccessToken)), Times.Once());
        }

        [TestMethod]
        public void RegisterOrReauthorize_ExistingUserButInvalidAuthCode_ShouldReturnErrorResultAndNotChangeDatabase()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 221313820847636491;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            string accessToken = "access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);

            string authCode = "auth-code";

            OAuthResponse authResponse = new OAuthResponse();
            authResponse.AccessToken = null;
            authResponse.ErrorMessage = "AuthorizationCodeInvalid";

            Mock.Get(bungieApiService)
                .Setup(m =>
                    m.GetOAuthAccessToken(It.Is<OAuthRequest>(r =>
                        r.AuthCode == authCode)))
                .Returns(authResponse);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.RegisterOrReauthorize(discordId, authCode);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("AuthorizationCodeInvalid"));

            Mock.Get(bungieApiService).Verify(m => m.GetOAuthAccessToken(It.IsAny<OAuthRequest>()), Times.Once());
            Mock.Get(bungieApiService).VerifyNoOtherCalls();

            Mock.Get(userDao).Verify(m => m.GetUserByDiscordId(discordId), Times.Once());
            // it should not try to update the user in the database
            Mock.Get(userDao).VerifyNoOtherCalls();
        }



    }
}
