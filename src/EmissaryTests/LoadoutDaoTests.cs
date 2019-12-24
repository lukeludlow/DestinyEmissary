using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emissary;
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

        // [TestMethod]
        // public void AddOrUpdateLoadout_ExampleLoadout_ShouldWriteToDatabase()
        // {
        //     // string testDbPath = "/Users/luke/code/DestinyEmissary/src/data/emissary-test.db";
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
        //             loadoutDao.AddOrUpdateLoadout(loadout);
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Assert.AreEqual(1, dbContext.Loadouts.Count());
        //             LoadoutDbEntity foundLoadout = dbContext.Loadouts.Where(l => l.Name == "name" && l.DiscordID == 69).AsQueryable().FirstOrDefault();
        //             Assert.AreEqual("name", foundLoadout.Name);
        //         }
        //     }
        // }

        // [TestMethod]
        // public void AddOrUpdateLoadout_LoadoutAlreadyExistsUpdateItWithNewValue_ShouldWriteToDatabase()
        // {
        //     using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
        //         connection.Open();
        //         DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
        //             .UseSqlite(connection)
        //             .Options;
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             dbContext.Database.EnsureCreated();
        //         }

        //         Loadout loadout = new Loadout("name", 69, 420,
        //                              new WeaponLoadout(
        //                                  new Weapon("name", "type", "class"),
        //                                  new Weapon("name", "type", "class"),
        //                                  new Weapon("name", "type", "class")),
        //                              new ArmorLoadout(
        //                                  new Armor("name", "helmet", "classtype"),
        //                                  new Armor("name", "gauntlets", "classtype"),
        //                                  new Armor("name", "chest", "classtype"),
        //                                  new Armor("name", "legs", "classtype"),
        //                                  new Armor("name", "classitem", "classtype")),
        //                              new ClassLoadout("race", "gender", "subclass", "subclasstree"));

        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             LoadoutDao loadoutDao = new LoadoutDao(dbContext);
        //             loadoutDao.AddOrUpdateLoadout(loadout);
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Assert.AreEqual(1, dbContext.Loadouts.Count());
        //             LoadoutDbEntity foundLoadout = dbContext.Loadouts.Where(l => l.Name == "name" && l.DiscordID == 69).AsQueryable().FirstOrDefault();
        //             Assert.AreEqual("name", foundLoadout.Name);
        //             Assert.AreEqual(JsonConvert.SerializeObject(loadout), foundLoadout.Json);
        //         }
        //         // now let's update the loadout contents and assert again
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             LoadoutDao loadoutDao = new LoadoutDao(dbContext);
        //             loadout.Weapons.KineticWeapon.Name = "newName";
        //             loadout.Armor.Helmet.Type = "helmet2";
        //             loadout.CharacterClass.Race = "human";
        //             loadoutDao.AddOrUpdateLoadout(loadout);
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Assert.AreEqual(1, dbContext.Loadouts.Count());
        //             LoadoutDbEntity foundLoadout = dbContext.Loadouts.Where(l => l.Name == "name" && l.DiscordID == 69).AsQueryable().FirstOrDefault();
        //             Assert.AreEqual("name", foundLoadout.Name);
        //             Assert.AreEqual(JsonConvert.SerializeObject(loadout), foundLoadout.Json);
        //         }
        //     }
        // }

        // [TestMethod]
        // public void AddOrUpdateLoadout_SameDiscordIDButDifferentName_ShouldWriteBothToDatabase()
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
        //             loadoutDao.AddOrUpdateLoadout(loadout);
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Assert.AreEqual(1, dbContext.Loadouts.Count());
        //             LoadoutDbEntity foundLoadout = dbContext.Loadouts.Where(l => l.Name == "name" && l.DiscordID == 69).AsQueryable().FirstOrDefault();
        //             Assert.AreEqual("name", foundLoadout.Name);
        //         }
        //         // now we'll just change the name of the loadout and re-add it. the discord id and json values will be
        //         // the exact same. the same user should be able to have multiple loadouts with the same equipment. 
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Loadout loadout2 = new Loadout("loadout2", 69, 420,
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
        //             loadoutDao.AddOrUpdateLoadout(loadout2);
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Assert.AreEqual(2, dbContext.Loadouts.Count());
        //             LoadoutDbEntity foundLoadout = dbContext.Loadouts.Where(l => l.Name == "loadout2" && l.DiscordID == 69).AsQueryable().FirstOrDefault();
        //             Assert.AreEqual("loadout2", foundLoadout.Name);
        //         }
        //     }
        // }

        // [TestMethod]
        // public void GetLoadout_UserHasNoLoadouts_ShouldReturnNull()
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
        //             Loadout foundLoadout = loadoutDao.GetLoadout(69, "crucible");
        //             Assert.IsNull(foundLoadout);
        //         }
        //     }
        // }

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