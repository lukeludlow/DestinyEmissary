using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryCore;
using System.Collections.Generic;
using Moq;
using Microsoft.Extensions.Configuration;

namespace EmissaryTests.Core
{
    [TestClass]
    public class DeleteLoadoutTests
    {

        [TestMethod]
        public void DeleteLoadout_LoadoutDoesNotExist_ShouldReturnErrorResult()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 69;
            string loadoutName = "last wish raid";
            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(2305843009504575107, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), 420, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(It.IsAny<ulong>())).Returns(new EmissaryUser());
            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(It.IsAny<ulong>(), It.IsAny<long>(), It.IsAny<string>())).Returns(value: null);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.DeleteLoadout(discordId, loadoutName);

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("not found"));
            Mock.Get(loadoutDao).Verify(m => m.RemoveLoadout(It.IsAny<ulong>(), It.IsAny<long>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void DeleteLoadout_LoadoutDoesExist_ShouldReturnSuccessAndRemoveFromDatabase()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 69;
            string loadoutName = "last wish raid";
            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(2305843009504575107, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), 420, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);
            DestinyItem izanagiItem = new DestinyItem(6917529135183883487, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, 3211806999, new List<uint>() { 2, 1, 10 }, "Exotic");
            Loadout savedLoadout = new Loadout(discordId, titan.CharacterId, loadoutName, new List<DestinyItem>() { izanagiItem });

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(new EmissaryUser());
            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(discordId, titan.CharacterId, loadoutName)).Returns(savedLoadout);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.DeleteLoadout(discordId, loadoutName);

            Assert.IsTrue(result.Success);
            Mock.Get(loadoutDao).Verify(m => m.RemoveLoadout(discordId, titan.CharacterId, loadoutName), Times.Once());
        }

        [TestMethod]
        public void DeleteLoadout_LoadoutNameDoesntMatchExactly_ShouldReturnErrorResultAndNotChangeDatabase()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 69;
            string loadoutName = "last wish raid";
            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(2305843009504575107, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), 420, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);
            DestinyItem izanagiItem = new DestinyItem(6917529135183883487, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, 3211806999, new List<uint>() { 2, 1, 10 }, "Exotic");
            Loadout savedLoadout = new Loadout(discordId, titan.CharacterId, loadoutName, new List<DestinyItem>() { izanagiItem });

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(new EmissaryUser());
            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(discordId, titan.CharacterId, loadoutName)).Returns(savedLoadout);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.DeleteLoadout(discordId, "Last Wish Raid");

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("not found"));
            Mock.Get(loadoutDao).Verify(m => m.RemoveLoadout(It.IsAny<ulong>(), It.IsAny<long>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void DeleteLoadout_LoadoutNameMatchesWhenWhitespaceIsTrimmed_ShouldSucceed()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 69;
            string loadoutName = "last wish raid";
            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(2305843009504575107, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), 420, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);
            DestinyItem izanagiItem = new DestinyItem(6917529135183883487, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, 3211806999, new List<uint>() { 2, 1, 10 }, "Exotic");
            Loadout savedLoadout = new Loadout(discordId, titan.CharacterId, loadoutName, new List<DestinyItem>() { izanagiItem });

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(new EmissaryUser());
            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(discordId, titan.CharacterId, loadoutName)).Returns(savedLoadout);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.DeleteLoadout(discordId, "   \t   last wish raid   \n  ");

            Assert.IsTrue(result.Success);
            Mock.Get(loadoutDao).Verify(m => m.RemoveLoadout(discordId, titan.CharacterId, loadoutName), Times.Once());
        }

    }
}
