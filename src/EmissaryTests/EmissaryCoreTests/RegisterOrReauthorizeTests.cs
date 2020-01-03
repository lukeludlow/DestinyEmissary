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
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();
            IAuthorizationService authorizationService = Mock.Of<IAuthorizationService>();


            ulong discordId = 221313820847636491;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            EmissaryUser expectedUser = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType);

            string authCode = "auth-code";

            OAuthResponse authResponse = new OAuthResponse();
            authResponse.AccessToken = "access-token";

            // Mock.Get(bungieApiService)
            //     .Setup(m =>
            //         m.GetOAuthAccessToken(It.Is<OAuthRequest>(r =>
            //             r.AuthCode == authCode)))
            //     .Returns(authResponse);

            Mock.Get(authorizationService).Setup(m => m.AuthorizeUser(discordId, authCode)).Returns(authResponse);

            UserMembershipsResponse membershipsResponse = new UserMembershipsResponse();
            membershipsResponse.DestinyMemberships = new List<DestinyMembership>();
            membershipsResponse.DestinyMemberships.Add(new DestinyMembership("pimpdaddy", destinyProfileId, destinyMembershipType, BungieMembershipType.Steam));

            Mock.Get(bungieApiService)
                .Setup(m =>
                    m.GetMembershipsForUser(It.Is<UserMembershipsRequest>(r =>
                        r.AccessToken == "access-token")))
                .Returns(membershipsResponse);

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);
            EmissaryResult result = emissary.RegisterOrReauthorize(discordId, authCode);
            Assert.IsTrue(result.Success);

            // Mock.Get(bungieApiService).Verify(m => m.GetOAuthAccessToken(It.IsAny<OAuthRequest>()), Times.Once());
            Mock.Get(authorizationService).Verify(m => m.AuthorizeUser(discordId, authCode), Times.Once());
            Mock.Get(authorizationService).VerifyNoOtherCalls();
            Mock.Get(bungieApiService).Verify(m => m.GetMembershipsForUser(It.IsAny<UserMembershipsRequest>()), Times.Once());

            Mock.Get(emissaryDao)
                .Verify(m =>
                    m.AddOrUpdateUser(It.Is<EmissaryUser>(u =>
                        u.DiscordId == discordId &&
                        u.DestinyProfileId == destinyProfileId &&
                        u.DestinyMembershipType == destinyMembershipType)), Times.Once());
        }

        [TestMethod]
        public void RegisterOrReauthorize_UserAlreadyExists_ShouldRefreshAccessTokenAndWriteToDatabase()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();
            IAuthorizationService authorizationService = Mock.Of<IAuthorizationService>();


            ulong discordId = 221313820847636491;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            string expiredAccessToken = "expired-access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType);

            Mock.Get(emissaryDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);

            string authCode = "auth-code";

            OAuthResponse oauthResponse = new OAuthResponse();
            string newAccessToken = "new-access-token";
            oauthResponse.AccessToken = newAccessToken;

            Mock.Get(bungieApiService)
                .Setup(m =>
                    m.GetOAuthAccessToken(It.Is<OAuthRequest>(r =>
                        r.AuthCode == authCode)))
                .Returns(oauthResponse);

            Mock.Get(authorizationService).Setup(m => m.AuthorizeUser(discordId, authCode)).Returns(oauthResponse);

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);
            EmissaryResult result = emissary.RegisterOrReauthorize(discordId, authCode);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Message.ToLower().Contains("authorized"));

            Mock.Get(authorizationService).Verify(m => m.AuthorizeUser(discordId, authCode), Times.Once());

            Mock.Get(emissaryDao).Verify(m => m.GetUserByDiscordId(discordId), Times.Once());

            Mock.Get(emissaryDao).Verify(m =>
                m.AddOrUpdateUser(It.Is<EmissaryUser>(r =>
                    r.DiscordId == discordId)), Times.Never());

            // Mock.Get(emissaryDao).Verify(m => m.AddOrUpdateAccessToken(It.Is<BungieAccessToken>(r => r.AccessToken == "new-access-token")), Times.Once());
        }

        [TestMethod]
        public void RegisterOrReauthorize_ExistingUserButInvalidAuthCode_ShouldReturnErrorResultAndNotChangeDatabase()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();
            IAuthorizationService authorizationService = Mock.Of<IAuthorizationService>();

            ulong discordId = 221313820847636491;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType);

            Mock.Get(emissaryDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);

            string authCode = "auth-code";

            OAuthResponse authResponse = new OAuthResponse();
            authResponse.AccessToken = null;
            authResponse.ErrorType = "AuthorizationCodeInvalid";

            // Mock.Get(bungieApiService)
            //     .Setup(m =>
            //         m.GetOAuthAccessToken(It.Is<OAuthRequest>(r =>
            //             r.AuthCode == authCode)))
            //     .Returns(authResponse);

            Mock.Get(authorizationService).Setup(m => m.AuthorizeUser(discordId, authCode)).Returns(authResponse);

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);
            EmissaryResult result = emissary.RegisterOrReauthorize(discordId, authCode);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("AuthorizationCodeInvalid"));

            // Mock.Get(bungieApiService).Verify(m => m.GetOAuthAccessToken(It.IsAny<OAuthRequest>()), Times.Once());
            Mock.Get(bungieApiService).VerifyNoOtherCalls();

            Mock.Get(authorizationService).Verify(m => m.AuthorizeUser(discordId, authCode), Times.Once());
            Mock.Get(authorizationService).VerifyNoOtherCalls();

            Mock.Get(emissaryDao).Verify(m => m.GetUserByDiscordId(discordId), Times.Once());
            // it should not try to update the user in the database
            Mock.Get(emissaryDao).VerifyNoOtherCalls();
        }



    }
}
