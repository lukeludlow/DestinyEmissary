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
    public class SaveLoadoutTests
    {


        [TestMethod]
        public void SaveLoadout_NewLoadoutForRegisteredUser_ShouldSucceedAndWriteToDatabase()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();
            IAuthorizationService authorizationService = Mock.Of<IAuthorizationService>();


            ulong discordId = 221313820847636491;
            long destinyCharacterId = 2305843009504575107;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType);

            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 }, "Exotic");
            Loadout loadoutToSave = new Loadout(discordId, destinyCharacterId, "crucible", new List<DestinyItem>() { izanagiItem });

            Mock.Get(emissaryDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);
            Mock.Get(emissaryDao).Setup(m => m.GetAllLoadoutsForUser(discordId)).Returns(new List<Loadout>());

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);
            EmissaryResult result = emissary.SaveLoadout(discordId, loadoutToSave, "crucible");
            Assert.IsTrue(result.Success);
            Mock.Get(emissaryDao)
                .Verify(m =>
                    m.AddOrUpdateLoadout(It.Is<Loadout>(l =>
                        l.DiscordId == loadoutToSave.DiscordId &&
                        l.DestinyCharacterId == loadoutToSave.DestinyCharacterId &&
                        l.LoadoutName == loadoutToSave.LoadoutName)), Times.Once());
        }

        [TestMethod]
        public void SaveLoadout_OverwriteExistingLoadout_ShouldSucceedAndWriteToDatabase()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            // EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
            // IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();
            IAuthorizationService authorizationService = Mock.Of<IAuthorizationService>();

            ulong discordId = 221313820847636491;
            long destinyCharacterId = 2305843009504575107;
            // long destinyProfileId = 4611686018467260757;
            // int destinyMembershipType = BungieMembershipType.Steam;
            // EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, "");

            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 }, "Exotic");
            Loadout loadoutToSave = new Loadout(discordId, destinyCharacterId, "crucible", new List<DestinyItem>() { izanagiItem });

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                    IList<Loadout> existingLoadoutsForUser = dbContext.Loadouts.Where(l => l.DiscordId == discordId).ToList();
                    Assert.AreEqual(0, existingLoadoutsForUser.Count);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    IEmissaryDao emissaryDao = new EmissaryDao(dbContext);
                    IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);
                    emissary.SaveLoadout(discordId, loadoutToSave, "crucible");
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    IList<Loadout> existingLoadoutsForUser = dbContext.Loadouts.Where(l => l.DiscordId == discordId).ToList();
                    Assert.AreEqual(1, existingLoadoutsForUser.Count);
                    Assert.AreEqual(1, existingLoadoutsForUser[0].Items.Count);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    IEmissaryDao emissaryDao = new EmissaryDao(dbContext);
                    IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);
                    loadoutToSave.Items.Remove(loadoutToSave.Items.Single(item => item.Name == "Izanagi's Burden"));
                    EmissaryResult result = emissary.SaveLoadout(discordId, loadoutToSave, "crucible");
                    Assert.IsTrue(result.Success);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    IList<Loadout> existingLoadoutsForUser = dbContext.Loadouts.Where(l => l.DiscordId == discordId).ToList();
                    Assert.AreEqual(1, existingLoadoutsForUser.Count);
                    Assert.AreEqual(0, existingLoadoutsForUser[0].Items.Count);
                }
            }
        }

        [TestMethod]
        public void SaveLoadout_CantAccessDatabase_ShouldReturnError()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IAccessTokenDao accessTokenDao = Mock.Of<IAccessTokenDao>();
            IAuthorizationService authorizationService = Mock.Of<IAuthorizationService>();
            Loadout loadoutToSave = new Loadout(69, 420, "crucible", new List<DestinyItem>() { });
            // using Mode=ReadWrite will fail because it can't create the database.
            // the default is Mode=ReadWriteCreate which creates the database if it doesn't exist.
            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
                    IManifestDao manifestDao = Mock.Of<IManifestDao>();
                    // IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();
                    IEmissaryDao emissaryDao = new EmissaryDao(dbContext);
                    // EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
                    // Mock.Get(emissaryDao).Setup(m => m.GetAllLoadoutsForUser(69)).Returns(new List<Loadout>());
                    IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);
                    // connection.Close();
                    // dbContext.Database.CloseConnection();
                    EmissaryResult result = emissary.SaveLoadout(69, loadoutToSave, "crucible");
                    Assert.IsFalse(result.Success);
                    Assert.IsTrue(result.ErrorMessage.Contains("no such table: Loadouts"));
                }
            }
        }

        // TODO does this test make sense? do i need it?
        // [TestMethod]
        // public void SaveLoadout_UserHasNotRegisteredYet_ShouldThrowException()
        // {
        //     Assert.Fail();
        // }

        [TestMethod]
        public void SaveLoadout_AlreadyReachedMaxLoadoutLimit_ShouldReturnError()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();
            IAuthorizationService authorizationService = Mock.Of<IAuthorizationService>();
            int maxLoadoutsLimit = 25;



            ulong discordId = 69;
            long characterId = 420;
            Loadout loadoutToSave = new Loadout(discordId, characterId, "loadout 26", new List<DestinyItem>());

            IList<Loadout> savedLoadouts = new List<Loadout>(new Loadout[maxLoadoutsLimit]);
            for (int i = 0; i < maxLoadoutsLimit; i++) {
                savedLoadouts[i] = new Loadout(discordId, characterId, $"loadout {i + 1}", new List<DestinyItem>());
            }
            Mock.Get(emissaryDao).Setup(m => m.GetAllLoadoutsForUser(discordId)).Returns(savedLoadouts);

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);
            EmissaryResult result = emissary.SaveLoadout(discordId, loadoutToSave, "loadout 26");

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("limit"));
        }

        [TestMethod]
        public void SaveCurrentlyEquippedAsLoadout_CurrentlyEquipped_ShouldCallSaveLoadoutAndHaveSameBehavior()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();
            IAuthorizationService authorizationService = Mock.Of<IAuthorizationService>();


            ulong discordId = 69;
            long destinyCharacterId = 420;
            string loadoutName = "last wish raid";
            DestinyItem destinyItem1 = new DestinyItem(6969, "dummy item 1", new List<string>() { "Weapon", "Kinetic Weapon" }, 420420, new List<uint>() { 1, 2 }, "Legendary");
            DestinyItem destinyItem2 = new DestinyItem(9696, "dummy item 2", new List<string>() { "Armor", "Helmet" }, 240240, new List<uint>() { 3, 4 }, "Legendary");
            // List<DestinyItem> loadoutItems = new List<DestinyItem>() { destinyItem1, destinyItem2 };
            // Loadout currentlyEquippedLoadout = new Loadout(discordId, destinyCharacterId, loadoutName, loadoutItems);

            long destinyProfileId = 42069;
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, BungieMembershipType.Steam);

            Mock.Get(emissaryDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);
            Mock.Get(emissaryDao).Setup(m => m.GetAllLoadoutsForUser(discordId)).Returns(new List<Loadout>() { });
            Mock.Get(emissaryDao).Setup(m => m.GetLoadout(discordId, destinyCharacterId, loadoutName)).Returns(value: null);

            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter character = new DestinyCharacter(destinyCharacterId, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(character.CharacterId, character);

            CharacterEquipmentResponse equipmentResponse = new CharacterEquipmentResponse();
            DestinyGenericItem genericItem1 = new DestinyGenericItem(destinyItem1.ItemHash, destinyItem1.ItemInstanceId);
            DestinyGenericItem genericItem2 = new DestinyGenericItem(destinyItem2.ItemHash, destinyItem2.ItemInstanceId);
            equipmentResponse.Items = new List<DestinyGenericItem>() { genericItem1, genericItem2 };

            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);
            Mock.Get(bungieApiService).Setup(m => m.GetCharacterEquipment(It.IsAny<CharacterEquipmentRequest>())).Returns(equipmentResponse);

            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(destinyItem1.ItemHash)).Returns(new ManifestItemDefinition(destinyItem1.Name, destinyItem1.TierTypeName, destinyItem1.CategoryHashes));
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(destinyItem2.ItemHash)).Returns(new ManifestItemDefinition(destinyItem2.Name, destinyItem2.TierTypeName, destinyItem2.CategoryHashes));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(1)).Returns(new ManifestItemCategoryDefinition("Weapon"));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(2)).Returns(new ManifestItemCategoryDefinition("Kinetic Weapon"));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(3)).Returns(new ManifestItemCategoryDefinition("Armor"));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(4)).Returns(new ManifestItemCategoryDefinition("Helmet"));

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);

            EmissaryResult result = emissary.SaveCurrentlyEquippedAsLoadout(discordId, loadoutName);
            Assert.IsTrue(result.Success);
            Loadout actualSavedLoadout = JsonConvert.DeserializeObject<Loadout>(result.Message);
            Assert.AreEqual(loadoutName, actualSavedLoadout.LoadoutName);
            Assert.AreEqual(2, actualSavedLoadout.Items.Count);
        }

        [TestMethod]
        public void SaveCurrentlyEquippedAsLoadout_CurrentlyEquippedFailsBecauseUserIsNotRegistered_ShouldReturnSameErrorAsCurrentlyEquipped()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
            IEmissaryDao emissaryDao = Mock.Of<IEmissaryDao>();
            IAuthorizationService authorizationService = Mock.Of<IAuthorizationService>();


            ulong discordId = 69;

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, emissaryDao, authorizationService);
            bool eventEmitted = false;
            emissary.RequestAuthorizationEvent += (discordId) => eventEmitted = true;
            EmissaryResult result = emissary.SaveCurrentlyEquippedAsLoadout(discordId, "raid");

            Assert.IsFalse(result.Success);
            Assert.IsTrue(eventEmitted);
            Assert.IsTrue(result.ErrorMessage.Contains("need access"));
        }


    }
}
