using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryCore;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EmissaryTests
{
    [TestClass]
    public class LoadoutDaoTests
    {

        [TestMethod]
        public void AddOrUpdateLoadout_ExampleLoadout_ShouldWriteToDatabase()
        {

            ulong discordId = 221313820847636491;
            long destinyCharacterId = 2305843009504575107;
            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            string loadoutName = "crucible";
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 });
            Loadout loadout = new Loadout(discordId, destinyCharacterId, loadoutName, new List<DestinyItem>() { izanagiItem });

            // string testDbPath = "/Users/luke/code/DestinyEmissary/src/data/emissary-test.db";
            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    loadoutDao.AddOrUpdateLoadout(loadout);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    Assert.AreEqual(1, dbContext.Loadouts.Count());
                    Loadout foundLoadout = dbContext.Loadouts.Where(l => l.LoadoutName == loadoutName && l.DiscordId == discordId).AsQueryable().FirstOrDefault();
                    Assert.IsNotNull(foundLoadout);
                }
            }
        }

        [TestMethod]
        public void AddOrUpdateLoadout_LoadoutAlreadyExistsUpdateItWithNewValue_ShouldWriteToDatabase()
        {

            ulong discordId = 221313820847636491;
            long destinyCharacterId = 2305843009504575107;
            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            string loadoutName = "crucible";
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 });
            Loadout loadout = new Loadout(discordId, destinyCharacterId, loadoutName, new List<DestinyItem>() { izanagiItem });

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    loadoutDao.AddOrUpdateLoadout(loadout);
                    Assert.AreEqual(1, dbContext.Loadouts.Count());
                }
                // now let's update the loadout contents and assert again
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    loadout.Items.Remove(loadout.Items.Single(item => item.Name == "Izanagi's Burden"));
                    loadoutDao.AddOrUpdateLoadout(loadout);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    Assert.AreEqual(1, dbContext.Loadouts.Count());
                    Loadout foundLoadout = dbContext.Loadouts.Where(l => l.LoadoutName == loadoutName && l.DiscordId == discordId).AsQueryable().FirstOrDefault();
                    Assert.AreEqual(0, foundLoadout.Items.Count);
                }
            }
        }

        [TestMethod]
        public void AddOrUpdateLoadout_SameDiscordIdAndSameNameButDifferentDestinyCharacter_ShouldWriteBothToDatabase()
        {
            ulong discordId = 221313820847636491;
            long destinyCharacterId = 2305843009504575107;
            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            string loadoutName = "crucible";
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 });
            Loadout loadout = new Loadout(discordId, destinyCharacterId, loadoutName, new List<DestinyItem>() { izanagiItem });
            Loadout loadout2 = new Loadout(discordId, 69, loadoutName, new List<DestinyItem>() { });

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    loadoutDao.AddOrUpdateLoadout(loadout);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    Assert.AreEqual(1, dbContext.Loadouts.Count());
                    Loadout foundLoadout = dbContext.Loadouts.Where(l => l.LoadoutName == loadoutName && l.DiscordId == discordId && l.DestinyCharacterId == destinyCharacterId).AsQueryable().FirstOrDefault();
                    Assert.AreEqual(loadout.DestinyCharacterId, foundLoadout.DestinyCharacterId);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    loadoutDao.AddOrUpdateLoadout(loadout2);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    Assert.AreEqual(2, dbContext.Loadouts.Count());
                    Loadout foundLoadout = dbContext.Loadouts.Where(l => l.LoadoutName == loadoutName && l.DiscordId == discordId && l.DestinyCharacterId == 69).AsQueryable().FirstOrDefault();
                    Assert.AreEqual(69, foundLoadout.DestinyCharacterId);
                }
            }
        }

        [TestMethod]
        public void GetLoadout_UserHasNoLoadouts_ShouldReturnNull()
        {
            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    Loadout foundLoadout = loadoutDao.GetLoadout(69, 420, "crucible");
                    Assert.IsNull(foundLoadout);
                }
            }
        }

        [TestMethod]
        public void GetLoadout_SameDiscordIdAndSameNameButDifferentCharacters_ShouldReturnNull()
        {
            ulong discordId = 221313820847636491;
            long titanCharacterId = 2305843009504575107;

            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden",
                    new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash,
                    new List<uint>() { 2, 1, 10 });
            Loadout titanLoadout = new Loadout(discordId, titanCharacterId, "raid",
                    new List<DestinyItem>() { izanagiItem });

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    dbContext.Loadouts.Add(titanLoadout);
                    dbContext.SaveChanges();
                    Assert.AreEqual(1, dbContext.Loadouts.Count());
                    Loadout foundLoadout = loadoutDao.GetLoadout(discordId, 69, "raid");
                    Assert.IsNull(foundLoadout);
                }
            }
        }

        [TestMethod]
        public void GetLoadout_LoadoutExistsAndThenLoadoutValueIsChanged_ShouldReturnLoadoutThenUpdatedLoadout()
        {
            ulong discordId = 221313820847636491;
            long titanCharacterId = 2305843009504575107;

            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden",
                    new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash,
                    new List<uint>() { 2, 1, 10 });
            Loadout titanLoadout = new Loadout(discordId, titanCharacterId, "raid",
                    new List<DestinyItem>() { izanagiItem });

            uint suddenDeathHash = 1879212552;
            long suddenDeathInstanceId = 6917529043814140192;
            DestinyItem suddenDeathItem = new DestinyItem(suddenDeathInstanceId, "A Sudden Death",
                    new List<string>() { "Energy Weapon", "Weapon", "Shotgun" }, suddenDeathHash,
                    new List<uint>() { 3, 1, 11 });

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    dbContext.Loadouts.Add(titanLoadout);
                    dbContext.SaveChanges();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    Assert.AreEqual(1, dbContext.Loadouts.Count());
                    Loadout foundLoadout = loadoutDao.GetLoadout(discordId, titanCharacterId, "raid");
                    Assert.AreEqual(1, foundLoadout.Items.Count);
                    Assert.AreEqual(izanagiInstanceId, foundLoadout.Items[0].ItemInstanceId);
                    titanLoadout.Items.Add(suddenDeathItem);
                    loadoutDao.AddOrUpdateLoadout(titanLoadout);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    Assert.AreEqual(1, dbContext.Loadouts.Count());
                    Loadout foundLoadout = loadoutDao.GetLoadout(discordId, titanCharacterId, "raid");
                    Assert.AreEqual(2, foundLoadout.Items.Count);
                    Assert.AreEqual(izanagiInstanceId, foundLoadout.Items[0].ItemInstanceId);
                    Assert.AreEqual(suddenDeathInstanceId, foundLoadout.Items[1].ItemInstanceId);
                }
            }
        }

        [TestMethod]
        public void GetAllLoadoutsForUser_NoLoadouts_ShouldReturnEmptyList()
        {
            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    IList<Loadout> loadouts = loadoutDao.GetAllLoadoutsForUser(69);
                    Assert.AreEqual(0, loadouts.Count);
                }
            }
        }

        [TestMethod]
        public void GetAllLoadoutsForUser_MultipleLoadoutsOnMultipleCharacters_ShouldReturnLoadoutsForAllCharacters()
        {
            ulong discordId = 221313820847636491;

            long titanCharacterId = 2305843009504575107;
            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden",
                    new List<string>() { "Kinetic Weapon", "Weapon", "Sniper Rifle" }, izanagiHash,
                    new List<uint>() { 2, 1, 10 });
            uint suddenDeathHash = 1879212552;
            long suddenDeathInstanceId = 6917529043814140192;
            DestinyItem suddenDeathItem = new DestinyItem(suddenDeathInstanceId, "A Sudden Death",
                    new List<string>() { "Energy Weapon", "Weapon", "Shotgun" }, suddenDeathHash,
                    new List<uint>() { 3, 1, 11 });

            Loadout titanLoadout = new Loadout(discordId, titanCharacterId, "raid",
                    new List<DestinyItem>() { izanagiItem });
            Loadout titanLoadout2 = new Loadout(discordId, titanCharacterId, "raid2", new List<DestinyItem>() { suddenDeathItem });
            Loadout titanLoadout3 = new Loadout(discordId, titanCharacterId, "raid3", new List<DestinyItem>() { izanagiItem, suddenDeathItem });
            Loadout warlockLoadout = new Loadout(discordId, 69, "raid", new List<DestinyItem>() { izanagiItem });
            Loadout hunterLoadout = new Loadout(discordId, 420, "raid", new List<DestinyItem>() { izanagiItem });

            IList<Loadout> allLoadoutsForUser = new List<Loadout>() { titanLoadout, titanLoadout2, titanLoadout3, warlockLoadout, hunterLoadout };

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                    dbContext.Loadouts.Add(titanLoadout);
                    dbContext.Loadouts.Add(titanLoadout2);
                    dbContext.Loadouts.Add(titanLoadout3);
                    dbContext.Loadouts.Add(warlockLoadout);
                    dbContext.Loadouts.Add(hunterLoadout);
                    dbContext.SaveChanges();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    IList<Loadout> loadoutsForUser = loadoutDao.GetAllLoadoutsForUser(discordId);
                    Assert.AreEqual(5, loadoutsForUser.Count);
                }
            }
        }

        [TestMethod]
        public void RemoveLoadout_LoadoutDoesNotExist_ShouldDoNothing()
        {
            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {

                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    Assert.AreEqual(0, dbContext.Loadouts.Count());
                    loadoutDao.RemoveLoadout(69, 420, "crucible");
                    Assert.AreEqual(0, dbContext.Loadouts.Count());
                }
            }
        }

        [TestMethod]
        public void RemoveLoadout_LoadoutDoesExist_ShouldRemoveAndUpdateDatabase()
        {
            ulong discordId = 221313820847636491;
            long titanCharacterId = 2305843009504575107;

            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden",
                    new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash,
                    new List<uint>() { 2, 1, 10 });
            Loadout titanLoadout = new Loadout(discordId, titanCharacterId, "raid",
                    new List<DestinyItem>() { izanagiItem });

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                    dbContext.Loadouts.Add(titanLoadout);
                    dbContext.SaveChanges();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    LoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    Assert.AreEqual(1, dbContext.Loadouts.Count());
                    loadoutDao.RemoveLoadout(discordId, titanCharacterId, "raid");
                    Assert.AreEqual(0, dbContext.Loadouts.Count());
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    Assert.AreEqual(0, dbContext.Loadouts.Count());
                }
            }
        }

        // this is a kinda useful test but i'm mainly doing it for code coverage
        [TestMethod]
        public void Dispose_NormalUseCase_LoadoutDaoObjectShouldBeSuccessfullyDisposed()
        {
            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    using (LoadoutDao loadoutDao = new LoadoutDao(dbContext)) {
                        loadoutDao.GetAllLoadoutsForUser(69);
                    }
                    using (LoadoutDao loadoutDao = new LoadoutDao(dbContext)) {
                        IDisposable disposable = loadoutDao as IDisposable;
                        disposable.Dispose();
                    }
                }
            }
        }

    }
}