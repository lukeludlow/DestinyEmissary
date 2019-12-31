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
    public class EquipLoadoutTests
    {


        [TestMethod]
        public void EquipLoadout_UserIsRegisteredAndAuthorizedAndLoadoutExists_ShouldSendRequestToBungieApi()
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
            string accessToken = "access.token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            long destinyCharacterId = 2305843009504575107;
            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 });
            Loadout loadout = new Loadout(discordId, destinyCharacterId, "raid", new List<DestinyItem>() { izanagiItem });

            EquipItemsResponse response = new EquipItemsResponse(new List<EquipItemResult>() { new EquipItemResult(izanagiHash, BungiePlatformErrorCodes.Success) });
            Mock.Get(bungieApiService).Setup(m => m.EquipItems(It.IsAny<EquipItemsRequest>())).Returns(response);

            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(2305843009504575107, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(It.IsAny<ulong>())).Returns(user);
            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(It.IsAny<ulong>(), It.IsAny<long>(), It.IsAny<string>())).Returns(loadout);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);

            EmissaryResult result = emissary.EquipLoadout(discordId, "raid");
            Assert.IsTrue(result.Success);
            Mock.Get(bungieApiService).Verify(m =>
                m.EquipItems(It.Is<EquipItemsRequest>(r =>
                    r.DestinyCharacterId == destinyCharacterId &&
                    r.MembershipType == destinyMembershipType &&
                    r.AccessToken == accessToken &&
                    r.ItemInstanceIds.Count == loadout.Items.Count &&
                    r.ItemInstanceIds[0] == izanagiInstanceId)), Times.Once());
        }

        [TestMethod]
        public void EquipLoadout_UserIsNotRegistered_ShouldReturnErrorResultAndNotCallBungieApi()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 221313820847636491;
            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(It.Is<ulong>(u => u == discordId))).Returns(value: null);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.EquipLoadout(discordId, "raid");
            Assert.IsFalse(result.Success);
            Mock.Get(bungieApiService).Verify(m => m.GetOAuthAccessToken(It.IsAny<OAuthRequest>()), Times.Never());
            Mock.Get(bungieApiService).Verify(m => m.EquipItems(It.IsAny<EquipItemsRequest>()), Times.Never());
            // Mock.Get(bungieApiService).VerifyNoOtherCalls();
        }

        [TestMethod]
        public void EquipLoadout_UserIsRegisteredButAccessTokenExpired_ShouldReturnErrorResult()
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
            string accessToken = "expired-access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);

            long destinyCharacterId = 2305843009504575107;
            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 });
            Loadout loadout = new Loadout(discordId, destinyCharacterId, "raid", new List<DestinyItem>() { izanagiItem });

            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(discordId, destinyCharacterId, "raid")).Returns(loadout);

            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(2305843009504575107, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            Mock.Get(bungieApiService).Setup(m => m.EquipItems(It.IsAny<EquipItemsRequest>())).Throws(new BungieApiException("Unauthorized: Access is denied due to invalid credentials."));

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.EquipLoadout(discordId, "raid");

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("Unauthorized"));
        }
        // TODO this should refresh the token and try again
        // EquipLoadout_AccessTokenExpiredButCanBeRefreshed_ShouldSendRequestToBungieApiToReauthorizeThenProceedToEquipLoadout




    }
}
