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
    public class ListLoadoutsTests
    {


        [TestMethod]
        public void ListLoadouts_UserHasOneLoadout_ShouldReturnSuccessWithLoadoutMessageString()
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

            long titanCharacterId = 2305843009504575107;
            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden",
                    new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash,
                    new List<uint>() { 2, 1, 10 }, "Exotic");
            Loadout titanLoadout = new Loadout(discordId, titanCharacterId, "raid",
                    new List<DestinyItem>() { izanagiItem });

            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(titanCharacterId, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);

            IList<Loadout> allLoadoutsForUser = new List<Loadout>() { titanLoadout };

            Mock.Get(emissaryDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);
            Mock.Get(emissaryDao).Setup(m => m.GetAllLoadoutsForUser(discordId)).Returns(allLoadoutsForUser);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);
            EmissaryResult result = emissary.ListLoadouts(discordId);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(JsonConvert.SerializeObject(allLoadoutsForUser), result.Message);
        }

        [TestMethod]
        public void ListLoadouts_UserHasLoadoutsOnDifferentCharacters_ShouldReturnLoadoutsForCurrentCharacterOnly()
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

            long titanCharacterId = 2305843009504575107;
            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden",
                    new List<string>() { "Kinetic Weapon", "Weapon", "Sniper Rifle" }, izanagiHash,
                    new List<uint>() { 2, 1, 10 }, "Exotic");
            uint suddenDeathHash = 1879212552;
            long suddenDeathInstanceId = 6917529043814140192;
            DestinyItem suddenDeathItem = new DestinyItem(suddenDeathInstanceId, "A Sudden Death",
                    new List<string>() { "Energy Weapon", "Weapon", "Shotgun" }, suddenDeathHash,
                    new List<uint>() { 3, 1, 11 }, "Exotic");

            Loadout titanLoadout = new Loadout(discordId, titanCharacterId, "raid",
                    new List<DestinyItem>() { izanagiItem });
            Loadout titanLoadout2 = new Loadout(discordId, titanCharacterId, "raid2", new List<DestinyItem>() { suddenDeathItem });
            Loadout titanLoadout3 = new Loadout(discordId, titanCharacterId, "raid3", new List<DestinyItem>() { izanagiItem, suddenDeathItem });
            Loadout warlockLoadout = new Loadout(discordId, 69, "raid", new List<DestinyItem>() { izanagiItem });
            Loadout hunterLoadout = new Loadout(discordId, 420, "raid", new List<DestinyItem>() { izanagiItem });
            IList<Loadout> allLoadoutsForUser = new List<Loadout>() { titanLoadout, titanLoadout2, titanLoadout3, warlockLoadout, hunterLoadout };
            // need this for verification
            IList<Loadout> titanLoadouts = new List<Loadout>() { titanLoadout, titanLoadout2, titanLoadout3 };

            // assemble so that titan is the most recently played character
            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(titanCharacterId, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);

            Mock.Get(emissaryDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);
            Mock.Get(emissaryDao).Setup(m => m.GetAllLoadoutsForUser(discordId)).Returns(allLoadoutsForUser);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);
            EmissaryResult result = emissary.ListLoadouts(discordId);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(JsonConvert.SerializeObject(titanLoadouts), result.Message);
        }

        [TestMethod]
        public void ListLoadouts_UserIsNotRegistered_ShouldReturnError()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();
            IAuthorizationService authorizationService = Mock.Of<IAuthorizationService>();


            ulong discordId = 69;

            Mock.Get(emissaryDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(value: null);

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);
            EmissaryResult result = emissary.ListLoadouts(discordId);

            Assert.IsFalse(result.Success);
            // Assert.IsTrue(result.ErrorMessage.Contains("user not found"));
        }

        [TestMethod]
        public void ListLoadouts_UserHasNoLoadouts_ShouldReturnSuccessButZeroLoadoutsEmptyList()
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

            IList<Loadout> allLoadoutsForUser = new List<Loadout>() { };

            // assemble so that titan is the most recently played character
            long titanCharacterId = 2305843009504575107;
            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(titanCharacterId, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);

            Mock.Get(emissaryDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);
            Mock.Get(emissaryDao).Setup(m => m.GetAllLoadoutsForUser(discordId)).Returns(allLoadoutsForUser);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);
            EmissaryResult result = emissary.ListLoadouts(discordId);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(JsonConvert.SerializeObject(allLoadoutsForUser), result.Message);
        }







    }
}
