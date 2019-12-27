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

        // [TestMethod]
        // public void GetLoadout_UserHasLoadoutsButNameDoesntMatch_ShouldReturnNull()
        // {
        //     using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
        //         connection.Open();
        //         DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
        //             .UseSqlite(connection)
        //             .Options;
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             dbContext.Database.EnsureCreated();
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Loadout loadout = new Loadout("name", 69, 420,
        //                 new WeaponLoadout(
        //                     new Weapon("name", "type", "class"),
        //                     new Weapon("name", "type", "class"),
        //                     new Weapon("name", "type", "class")),
        //                 new ArmorLoadout(
        //                     new Armor("name", "helmet", "classtype"),
        //                     new Armor("name", "gauntlets", "classtype"),
        //                     new Armor("name", "chest", "classtype"),
        //                     new Armor("name", "legs", "classtype"),
        //                     new Armor("name", "classitem", "classtype")),
        //                 new ClassLoadout("race", "gender", "subclass", "subclasstree"));
        //             LoadoutDao loadoutDao = new LoadoutDao(dbContext);
        //             dbContext.Loadouts.Add(new LoadoutDbEntity(loadout));
        //             dbContext.SaveChanges();
        //             Loadout loadout2 = loadout;
        //             loadout2.Name = "loadout2";
        //             dbContext.Loadouts.Add(new LoadoutDbEntity(loadout2));
        //             dbContext.SaveChanges();
        //             Assert.AreEqual(2, dbContext.Loadouts.Count());
        //             Loadout foundLoadout = loadoutDao.GetLoadout(69, "crucible");
        //             Assert.IsNull(foundLoadout);
        //         }
        //     }
        // }

        // [TestMethod]
        // public void GetLoadout_LoadoutExistsAndThenLoadoutValueIsChanged_ShouldReturnLoadoutThenUpdatedLoadout()
        // {
        //     using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
        //         connection.Open();
        //         DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
        //             .UseSqlite(connection)
        //             .Options;
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             dbContext.Database.EnsureCreated();
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Loadout loadout = new Loadout("crucible", 69, 420,
        //                 new WeaponLoadout(
        //                     new Weapon("name", "type", "class"),
        //                     new Weapon("name", "type", "class"),
        //                     new Weapon("name", "type", "class")),
        //                 new ArmorLoadout(
        //                     new Armor("name", "helmet", "classtype"),
        //                     new Armor("name", "gauntlets", "classtype"),
        //                     new Armor("name", "chest", "classtype"),
        //                     new Armor("name", "legs", "classtype"),
        //                     new Armor("name", "classitem", "classtype")),
        //                 new ClassLoadout("race", "gender", "subclass", "subclasstree"));
        //             LoadoutDao loadoutDao = new LoadoutDao(dbContext);
        //             dbContext.Loadouts.Add(new LoadoutDbEntity(loadout));
        //             dbContext.SaveChanges();
        //             Loadout foundLoadout = loadoutDao.GetLoadout(69, "crucible");
        //             Assert.AreEqual(JsonConvert.SerializeObject(loadout), JsonConvert.SerializeObject(foundLoadout));
        //             loadout.Weapons.KineticWeapon = new Weapon("izanagi", "kinetic", "sniper");
        //             loadout.CharacterClass = new ClassLoadout("human", "female", "void", "top");
        //             // we need to use our AddOrUpdate method for this rather than directly doing it to the dbcontext
        //             loadoutDao.AddOrUpdateLoadout(loadout);
        //             foundLoadout = loadoutDao.GetLoadout(69, "crucible");
        //             Assert.AreEqual("izanagi", foundLoadout.Weapons.KineticWeapon.Name);
        //         }
        //     }
        // }

        // [TestMethod]
        // public void GetAllLoadoutsForUser_NoLoadouts_ShouldReturnEmptyList()
        // {
        //     using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
        //         connection.Open();
        //         DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
        //             .UseSqlite(connection)
        //             .Options;
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             dbContext.Database.EnsureCreated();
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             // Loadout loadout = new Loadout("name", 69, 420,
        //             //     new WeaponLoadout(
        //             //         new Weapon("name", "type", "class"),
        //             //         new Weapon("name", "type", "class"),
        //             //         new Weapon("name", "type", "class")),
        //             //     new ArmorLoadout(
        //             //         new Armor("name", "helmet", "classtype"),
        //             //         new Armor("name", "gauntlets", "classtype"),
        //             //         new Armor("name", "chest", "classtype"),
        //             //         new Armor("name", "legs", "classtype"),
        //             //         new Armor("name", "classitem", "classtype")),
        //             //     new ClassLoadout("race", "gender", "subclass", "subclasstree"));
        //             LoadoutDao loadoutDao = new LoadoutDao(dbContext);
        //             IList<Loadout> loadouts = loadoutDao.GetAllLoadoutsForUser(69);
        //             Assert.AreEqual(0, loadouts.Count);
        //         }
        //     }
        // }

        // [TestMethod]
        // public void GetAllLoadoutsForUser_MultipleLoadouts_ShouldReturnListOfLoadoutsInNoParticularOrder()
        // {
        //     using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
        //         connection.Open();
        //         DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
        //             .UseSqlite(connection)
        //             .Options;
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             dbContext.Database.EnsureCreated();
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Loadout loadout = new Loadout("loadout1", 69, 420,
        //                 new WeaponLoadout(
        //                     new Weapon("name", "type", "class"),
        //                     new Weapon("name", "type", "class"),
        //                     new Weapon("name", "type", "class")),
        //                 new ArmorLoadout(
        //                     new Armor("name", "helmet", "classtype"),
        //                     new Armor("name", "gauntlets", "classtype"),
        //                     new Armor("name", "chest", "classtype"),
        //                     new Armor("name", "legs", "classtype"),
        //                     new Armor("name", "classitem", "classtype")),
        //                 new ClassLoadout("race", "gender", "subclass", "subclasstree"));
        //             LoadoutDao loadoutDao = new LoadoutDao(dbContext);
        //             dbContext.Loadouts.Add(new LoadoutDbEntity(loadout));
        //             dbContext.SaveChanges();
        //             loadout.Name = "loadout2";
        //             dbContext.Loadouts.Add(new LoadoutDbEntity(loadout));
        //             dbContext.SaveChanges();
        //             loadout.Name = "loadout3";
        //             dbContext.Loadouts.Add(new LoadoutDbEntity(loadout));
        //             dbContext.SaveChanges();
        //             Assert.AreEqual(3, dbContext.Loadouts.Count());
        //             IList<Loadout> loadouts = loadoutDao.GetAllLoadoutsForUser(69);
        //             Assert.AreEqual(3, loadouts.Count);
        //             Assert.IsTrue(loadouts.Any(l => l.DiscordID == 69 && l.Name == "loadout1"));
        //             Assert.IsTrue(loadouts.Any(l => l.DiscordID == 69 && l.Name == "loadout2"));
        //             Assert.IsTrue(loadouts.Any(l => l.DiscordID == 69 && l.Name == "loadout3"));
        //         }
        //     }
        // }

        // [TestMethod]
        // public void RemoveLoadout_LoadoutDoesNotExist_ShouldDoNothing()
        // {
        //     using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
        //         connection.Open();
        //         DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
        //             .UseSqlite(connection)
        //             .Options;
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             dbContext.Database.EnsureCreated();
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             // Loadout loadout = new Loadout("name", 69, 420,
        //             //     new WeaponLoadout(
        //             //         new Weapon("name", "type", "class"),
        //             //         new Weapon("name", "type", "class"),
        //             //         new Weapon("name", "type", "class")),
        //             //     new ArmorLoadout(
        //             //         new Armor("name", "helmet", "classtype"),
        //             //         new Armor("name", "gauntlets", "classtype"),
        //             //         new Armor("name", "chest", "classtype"),
        //             //         new Armor("name", "legs", "classtype"),
        //             //         new Armor("name", "classitem", "classtype")),
        //             //     new ClassLoadout("race", "gender", "subclass", "subclasstree"));
        //             LoadoutDao loadoutDao = new LoadoutDao(dbContext);
        //             Assert.AreEqual(0, dbContext.Loadouts.Count());
        //             loadoutDao.RemoveLoadout(69, "crucible");
        //             Assert.AreEqual(0, dbContext.Loadouts.Count());
        //         }
        //     }
        // }

        // [TestMethod]
        // public void RemoveLoadout_LoadoutDoesExist_ShouldRemoveAndUpdateDatabase()
        // {
        //     using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
        //         connection.Open();
        //         DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
        //             .UseSqlite(connection)
        //             .Options;
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             dbContext.Database.EnsureCreated();
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Loadout loadout = new Loadout("crucible", 69, 420,
        //                 new WeaponLoadout(
        //                     new Weapon("name", "type", "class"),
        //                     new Weapon("name", "type", "class"),
        //                     new Weapon("name", "type", "class")),
        //                 new ArmorLoadout(
        //                     new Armor("name", "helmet", "classtype"),
        //                     new Armor("name", "gauntlets", "classtype"),
        //                     new Armor("name", "chest", "classtype"),
        //                     new Armor("name", "legs", "classtype"),
        //                     new Armor("name", "classitem", "classtype")),
        //                 new ClassLoadout("race", "gender", "subclass", "subclasstree"));
        //             LoadoutDao loadoutDao = new LoadoutDao(dbContext);
        //             dbContext.Loadouts.Add(new LoadoutDbEntity(loadout));
        //             dbContext.SaveChanges();
        //             Assert.AreEqual(1, dbContext.Loadouts.Count());
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             LoadoutDao loadoutDao = new LoadoutDao(dbContext);
        //             Assert.AreEqual(1, dbContext.Loadouts.Count());
        //             loadoutDao.RemoveLoadout(69, "crucible");
        //             Assert.AreEqual(0, dbContext.Loadouts.Count());
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Assert.AreEqual(0, dbContext.Loadouts.Count());
        //         }
        //     }
        // }

        // [TestMethod]
        // public void Dispose_NormalUseCase_LoadoutDaoObjectShouldBeSuccessfullyDisposed()
        // {
        //     using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
        //         connection.Open();
        //         DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
        //             .UseSqlite(connection)
        //             .Options;
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             dbContext.Database.EnsureCreated();
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             using (LoadoutDao loadoutDao = new LoadoutDao(dbContext)) {
        //                 loadoutDao.GetAllLoadoutsForUser(69);
        //             }
        //             // this is a kinda useful test but i'm mainly doing it for code coverage
        //             using (LoadoutDao loadoutDao = new LoadoutDao(dbContext)) {
        //                 IDisposable disposable = loadoutDao as IDisposable;
        //                 disposable.Dispose();
        //             }
        //         }
        //     }
        // }

    }
}