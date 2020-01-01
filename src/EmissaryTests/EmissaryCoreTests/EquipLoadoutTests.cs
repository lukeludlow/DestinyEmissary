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
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 }, "Exotic");
            Loadout loadout = new Loadout(discordId, destinyCharacterId, "raid", new List<DestinyItem>() { izanagiItem });

            EquipItemsResponse response = new EquipItemsResponse(new List<EquipItemResult>() { new EquipItemResult(izanagiInstanceId, BungiePlatformErrorCodes.Success) });
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
            Mock.Get(bungieApiService).VerifyNoOtherCalls();
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
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 }, "Exotic");
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

        [TestMethod]
        public void EquipLoadout_UserIsNotRegistered_ShouldEmitRequestAuthorizationEvent()
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
            bool eventEmitted = false;
            emissary.RequestAuthorizationEvent += (discordId) => eventEmitted = true;
            EmissaryResult result = emissary.EquipLoadout(discordId, "raid");
            Assert.IsFalse(result.Success);
            Assert.IsTrue(eventEmitted);
        }

        [TestMethod]
        public void EquipLoadout_AccessTokenExpired_ShouldEmitRequestAuthorizationEvent()
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

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(It.Is<ulong>(u => u == discordId))).Returns(user);


            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(2305843009504575107, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            long destinyCharacterId = 2305843009504575107;
            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 }, "Exotic");
            Loadout loadout = new Loadout(discordId, destinyCharacterId, "raid", new List<DestinyItem>() { izanagiItem });

            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(discordId, destinyCharacterId, "last wish raid")).Returns(loadout);

            Mock.Get(bungieApiService).Setup(m => m.EquipItems(It.IsAny<EquipItemsRequest>())).Throws(new BungieApiException("Unauthorized: Access is denied due to invalid credentials."));

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            bool eventEmitted = false;
            emissary.RequestAuthorizationEvent += (discordId) => eventEmitted = true;
            EmissaryResult result = emissary.EquipLoadout(discordId, "last wish raid");
            Assert.IsFalse(result.Success);
            Assert.IsTrue(eventEmitted);
        }

        [TestMethod]
        public void EquipLoadout_ExoticWeaponAndArmorCurrentlyEquipped_ShouldPutExoticsAtEndOfEquipRequestList()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 221313820847636491;

            long izanagiInstanceId = 6917529135183883487;
            long recluseInstanceId = 6917529123204409619;
            long wendigoInstanceId = 6917529112673221040;
            long maskOfRullInstanceId = 6917529110566559001;
            long reverieDawnInstanceId = 6917529138010460936;
            long plateOfTranscendenceInstanceId = 6917529109687230597;
            long peacekeepersInstanceId = 6917529122999918127;
            long markOfTheGreatHuntInstanceId = 6917529128966008940;

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(new EmissaryUser());

            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { }, 69, new List<uint>() { }, "Exotic");
            DestinyItem recluseItem = new DestinyItem(recluseInstanceId, "The Recluse", new List<string>() { }, 69, new List<uint>() { }, "Legendary");
            DestinyItem wendigoItem = new DestinyItem(wendigoInstanceId, "Wendigo GL3", new List<string>() { }, 69, new List<uint>() { }, "Legendary");
            DestinyItem maskOfRullItem = new DestinyItem(maskOfRullInstanceId, "Mask of Rull", new List<string>() { }, 69, new List<uint>() { }, "Legendary");
            DestinyItem reverieDawnItem = new DestinyItem(reverieDawnInstanceId, "Reverie Dawn Gauntlets", new List<string>() { }, 69, new List<uint>() { }, "Legendary");
            DestinyItem plateOfTranscendenceItem = new DestinyItem(plateOfTranscendenceInstanceId, "Plate of Transcendence", new List<string>() { }, 69, new List<uint>() { }, "Legendary");
            DestinyItem peacekeepersItem = new DestinyItem(peacekeepersInstanceId, "Peacekeepers", new List<string>() { }, 69, new List<uint>() { }, "Exotic");
            DestinyItem markOfTheGreatHuntItem = new DestinyItem(markOfTheGreatHuntInstanceId, "Mark of the Great Hunt", new List<string>() { }, 69, new List<uint>() { }, "Legendary");

            IList<DestinyItem> loadoutItems = new List<DestinyItem>() { izanagiItem, recluseItem, wendigoItem, maskOfRullItem, reverieDawnItem, plateOfTranscendenceItem, peacekeepersItem, markOfTheGreatHuntItem };

            Loadout savedLoadout = new Loadout(discordId, 420, "last wish raid", loadoutItems);

            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(discordId, It.IsAny<long>(), "last wish raid")).Returns(savedLoadout);

            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(2305843009504575107, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), 69, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            IList<long> originalItemInstanceIdsOrder = new List<long>() { izanagiInstanceId, recluseInstanceId, wendigoInstanceId, maskOfRullInstanceId, reverieDawnInstanceId, plateOfTranscendenceInstanceId, peacekeepersInstanceId, markOfTheGreatHuntInstanceId };

            IList<long> expectedItemInstanceIdsOrder = new List<long>() { recluseInstanceId, wendigoInstanceId, maskOfRullInstanceId, reverieDawnInstanceId, plateOfTranscendenceInstanceId, markOfTheGreatHuntInstanceId, izanagiInstanceId, peacekeepersInstanceId };

            EquipItemResult izanagiEquipResult = new EquipItemResult(izanagiInstanceId, BungiePlatformErrorCodes.Success);
            EquipItemResult recluseEquipResult = new EquipItemResult(recluseInstanceId, BungiePlatformErrorCodes.Success);
            EquipItemResult wendigoEquipResult = new EquipItemResult(wendigoInstanceId, BungiePlatformErrorCodes.Success);
            EquipItemResult maskOfRullEquipResult = new EquipItemResult(maskOfRullInstanceId, BungiePlatformErrorCodes.Success);
            EquipItemResult reverieDawnEquipResult = new EquipItemResult(reverieDawnInstanceId, BungiePlatformErrorCodes.Success);
            EquipItemResult plateOfTranscendenceEquipResult = new EquipItemResult(plateOfTranscendenceInstanceId, BungiePlatformErrorCodes.Success);
            EquipItemResult peacekeepersEquipResult = new EquipItemResult(peacekeepersInstanceId, BungiePlatformErrorCodes.Success);
            EquipItemResult markOfTheGreatHuntEquipResult = new EquipItemResult(markOfTheGreatHuntInstanceId, BungiePlatformErrorCodes.Success);
            EquipItemsResponse equipResponse = new EquipItemsResponse(new List<EquipItemResult>() { izanagiEquipResult, recluseEquipResult, wendigoEquipResult, maskOfRullEquipResult, reverieDawnEquipResult, plateOfTranscendenceEquipResult, peacekeepersEquipResult, markOfTheGreatHuntEquipResult });
            Mock.Get(bungieApiService).Setup(m => m.EquipItems(It.IsAny<EquipItemsRequest>())).Returns(equipResponse);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.EquipLoadout(discordId, "last wish raid");
            Assert.IsTrue(result.Success);
            Mock.Get(bungieApiService).Verify(m => m.EquipItems(It.Is<EquipItemsRequest>(r => r.ItemInstanceIds.SequenceEqual(expectedItemInstanceIdsOrder))), Times.Once());
        }




        [TestMethod]
        public void EquipLoadout_LoadoutNameHasExtraWhitespace_ShouldTrimNameAndThenEquipLoadout()
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
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 }, "Exotic");
            Loadout loadout = new Loadout(discordId, destinyCharacterId, "last wish raid", new List<DestinyItem>() { izanagiItem });

            EquipItemsResponse response = new EquipItemsResponse(new List<EquipItemResult>() { new EquipItemResult(izanagiHash, BungiePlatformErrorCodes.Success) });
            Mock.Get(bungieApiService).Setup(m => m.EquipItems(It.IsAny<EquipItemsRequest>())).Returns(response);

            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(2305843009504575107, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(It.IsAny<ulong>())).Returns(user);
            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(It.IsAny<ulong>(), It.IsAny<long>(), It.IsAny<string>())).Returns(loadout);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);

            EmissaryResult result = emissary.EquipLoadout(discordId, "    \t  last wish raid  \t \n  ");
            Assert.IsTrue(result.Success);
            Mock.Get(loadoutDao).Verify(m => m.GetLoadout(discordId, destinyCharacterId, "last wish raid"));
            // Mock.Get(bungieApiService).Verify(m =>
            //     m.EquipItems(It.Is<EquipItemsRequest>(r =>
            //         r.DestinyCharacterId == destinyCharacterId &&
            //         r.MembershipType == destinyMembershipType &&
            //         r.AccessToken == accessToken &&
            //         r.ItemInstanceIds.Count == loadout.Items.Count &&
            //         r.ItemInstanceIds[0] == izanagiInstanceId)), Times.Once());
        }

        [TestMethod]
        public void EquipLoadout_ItemHasBeenDismantled_ShouldReturnErrorResult()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            long wendigoInstanceId = 6917529112673221040;
            DestinyItem wendigoItem = new DestinyItem(wendigoInstanceId, "Wendigo GL3", new List<string>() { }, 69, new List<uint>() { }, "Legendary");
            EquipItemResult wendigoEquipResult = new EquipItemResult(wendigoInstanceId, BungiePlatformErrorCodes.DestinyItemNotFound);

            EquipItemsResponse equipResponse = new EquipItemsResponse(new List<EquipItemResult>() { wendigoEquipResult });
            Mock.Get(bungieApiService).Setup(m => m.EquipItems(It.IsAny<EquipItemsRequest>())).Returns(equipResponse);

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(It.IsAny<ulong>())).Returns(new EmissaryUser());


            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(2305843009504575107, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), 420, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(It.IsAny<ulong>(), It.IsAny<long>(), It.IsAny<string>())).Returns(new Loadout(69, 420, "raid", new List<DestinyItem>() { wendigoItem }));

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);

            EmissaryResult result = emissary.EquipLoadout(69, "raid");
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("dismantled"));
        }



    }
}
