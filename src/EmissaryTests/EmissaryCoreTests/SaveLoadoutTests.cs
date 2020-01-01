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
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 221313820847636491;
            long destinyCharacterId = 2305843009504575107;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, "");

            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 }, "Exotic");
            Loadout loadoutToSave = new Loadout(discordId, destinyCharacterId, "crucible", new List<DestinyItem>() { izanagiItem });

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);
            Mock.Get(loadoutDao).Setup(m => m.GetAllLoadoutsForUser(discordId)).Returns(new List<Loadout>());

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.SaveLoadout(discordId, loadoutToSave, "crucible");
            Assert.IsTrue(result.Success);
            Mock.Get(loadoutDao)
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
                    IUserDao userDao = new UserDao(dbContext);
                    ILoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
                    emissary.SaveLoadout(discordId, loadoutToSave, "crucible");
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    IList<Loadout> existingLoadoutsForUser = dbContext.Loadouts.Where(l => l.DiscordId == discordId).ToList();
                    Assert.AreEqual(1, existingLoadoutsForUser.Count);
                    Assert.AreEqual(1, existingLoadoutsForUser[0].Items.Count);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    IUserDao userDao = new UserDao(dbContext);
                    ILoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
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
                    IUserDao userDao = Mock.Of<IUserDao>();
                    // ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
                    ILoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    // EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
                    // Mock.Get(loadoutDao).Setup(m => m.GetAllLoadoutsForUser(69)).Returns(new List<Loadout>());
                    Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
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
            int maxLoadoutsLimit = 25;

            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 69;
            long characterId = 420;
            Loadout loadoutToSave = new Loadout(discordId, characterId, "loadout 26", new List<DestinyItem>());

            IList<Loadout> savedLoadouts = new List<Loadout>(new Loadout[maxLoadoutsLimit]);
            for (int i = 0; i < maxLoadoutsLimit; i++) {
                savedLoadouts[i] = new Loadout(discordId, characterId, $"loadout {i + 1}", new List<DestinyItem>());
            }
            Mock.Get(loadoutDao).Setup(m => m.GetAllLoadoutsForUser(discordId)).Returns(savedLoadouts);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.SaveLoadout(discordId, loadoutToSave, "loadout 26");

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("limit"));
        }


    }
}
