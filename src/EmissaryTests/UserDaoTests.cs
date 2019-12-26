using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryCore;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;

namespace EmissaryTests
{
    [TestClass]
    public class UserDaoTests
    {

        // [TestMethod]
        // public void AddOrUpdateUser_ExampleUser_ShouldWriteToDatabase()
        // {
        //     using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
        //         connection.Open();
        //         DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
        //             .UseSqlite(connection)
        //             .Options;
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             // create the schema in the database
        //             dbContext.Database.EnsureCreated();
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             UserDao userDao = new UserDao(dbContext);
        //             userDao.AddOrUpdateUser(new EmissaryUser(69, 420));
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Assert.AreEqual(1, dbContext.Users.Count());
        //             Assert.AreEqual((ulong)69, dbContext.Users.Single().DiscordID);
        //             Assert.AreEqual((long)420, dbContext.Users.Single().BungieID);
        //         }
        //     }
        // }

        // [TestMethod]
        // public void AddOrUpdateUser_UserAlreadyExists_ShouldSucceed()
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
        //             UserDao userDao = new UserDao(dbContext);
        //             userDao.AddOrUpdateUser(new EmissaryUser(69, 420));
        //             // this shouldn't throw
        //             userDao.AddOrUpdateUser(new EmissaryUser(69, 420));
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Assert.AreEqual(1, dbContext.Users.Count());
        //             Assert.AreEqual((ulong)69, dbContext.Users.Single().DiscordID);
        //             Assert.AreEqual((long)420, dbContext.Users.Single().BungieID);
        //         }
        //     }
        // }

        // [TestMethod]
        // public void AddOrUpdateUser_UpdateExistingUser_ShouldWriteToDatabase()
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
        //             UserDao userDao = new UserDao(dbContext);
        //             userDao.AddOrUpdateUser(new EmissaryUser(69, 420));
        //             userDao.AddOrUpdateUser(new EmissaryUser(69, -421));
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Assert.AreEqual(1, dbContext.Users.Count());
        //             Assert.AreEqual((ulong)69, dbContext.Users.Single().DiscordID);
        //             Assert.AreEqual((long)-421, dbContext.Users.Single().BungieID);
        //         }
        //     }
        // }

        // [TestMethod]
        // public void GetUsers_NoUsers_ShouldReturnEmptyList()
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
        //             UserDao userDao = new UserDao(dbContext);
        //             IList<EmissaryUser> users = userDao.GetUsers();
        //             Assert.AreEqual(0, users.Count());
        //         }
        //     }
        // }

        // [TestMethod]
        // public void GetUsers_OneUser_ShouldReturnListWithOneEntry()
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
        //             UserDao userDao = new UserDao(dbContext);
        //             userDao.AddOrUpdateUser(new EmissaryUser(69, 420));
        //             IList<EmissaryUser> users = userDao.GetUsers();
        //             Assert.AreEqual(1, users.Count());
        //             Assert.AreEqual((ulong)69, users[0].DiscordID);
        //             Assert.AreEqual((long)420, users[0].BungieID);
        //         }
        //     }
        // }

        // [TestMethod]
        // public void GetUsers_MultipleUsers_ShouldReturnListOfUsersInNoParticularOrder()
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
        //             UserDao userDao = new UserDao(dbContext);
        //             // adding them in reverse order of keys, so i can make sure that the list comes out properly sorted
        //             userDao.AddOrUpdateUser(new EmissaryUser(420, 420420));
        //             userDao.AddOrUpdateUser(new EmissaryUser(69, 42069));
        //             userDao.AddOrUpdateUser(new EmissaryUser(1, 420));
        //             IList<EmissaryUser> users = userDao.GetUsers();
        //             Assert.AreEqual(3, users.Count());
        //             Assert.IsTrue(users.Any(u => u.DiscordID == 1 && u.BungieID == 420));
        //             Assert.IsTrue(users.Any(u => u.DiscordID == 69 && u.BungieID == 42069));
        //             Assert.IsTrue(users.Any(u => u.DiscordID == 420 && u.BungieID == 420420));
        //         }
        //     }
        // }

        // [TestMethod]
        // public void GetUserByDiscordId_UserDoesNotExist_ShouldReturnNull()
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
        //             UserDao userDao = new UserDao(dbContext);
        //             EmissaryUser foundUser = userDao.GetUserByDiscordId(69);
        //             Assert.IsNull(foundUser);
        //         }
        //     }
        // }

        // [TestMethod]
        // public void GetUserByDiscordId_UserDoesExistAndThereAreLotsOfOtherUsersToo_ShouldReturnUser()
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
        //             UserDao userDao = new UserDao(dbContext);
        //             dbContext.Users.Add(new EmissaryUser(69, 420));
        //             dbContext.Users.Add(new EmissaryUser(68, 419));
        //             dbContext.Users.Add(new EmissaryUser(70, 421));
        //             dbContext.SaveChanges();
        //             EmissaryUser foundUser = userDao.GetUserByDiscordId(69);
        //             Assert.AreEqual((ulong)69, foundUser.DiscordID);
        //             Assert.AreEqual((long)420, foundUser.BungieID);
        //         }
        //     }
        // }

        // [TestMethod]
        // public void GetUserByDiscordId_UserExistsButThenIsRemoved_ShouldReturnUserThenReturnNull()
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
        //             UserDao userDao = new UserDao(dbContext);
        //             userDao.AddOrUpdateUser(new EmissaryUser(69, 420));
        //             EmissaryUser foundUser = userDao.GetUserByDiscordId(69);
        //             Assert.AreEqual((ulong)69, foundUser.DiscordID);
        //             Assert.AreEqual((long)420, foundUser.BungieID);
        //             dbContext.Users.Remove(foundUser);
        //             dbContext.SaveChanges();
        //             foundUser = userDao.GetUserByDiscordId(69);
        //             Assert.IsNull(foundUser);
        //         }
        //     }
        // }

        // [TestMethod]
        // public void RemoveUser_UserDoesNotExist_ShouldDoNothing()
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
        //             UserDao userDao = new UserDao(dbContext);
        //             Assert.AreEqual(0, dbContext.Users.Count());
        //             // this shouldn't throw and shouldn't change the database
        //             userDao.RemoveUser(69);
        //             Assert.AreEqual(0, dbContext.Users.Count());
        //         }
        //     }
        // }

        // [TestMethod]
        // public void RemoveUser_UserExists_ShouldRemoveTheSpecifiedUserAndUpdateDatabase()
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
        //             UserDao userDao = new UserDao(dbContext);
        //             dbContext.Users.Add(new EmissaryUser(69, 420));
        //             dbContext.Users.Add(new EmissaryUser(1, 1));
        //             dbContext.Users.Add(new EmissaryUser(2, 2));
        //             dbContext.SaveChanges();
        //             Assert.AreEqual(3, dbContext.Users.Count());
        //             userDao.RemoveUser(69);
        //             Assert.AreEqual(2, dbContext.Users.Count());
        //             Assert.IsNull(dbContext.Users.Find((ulong)69));
        //         }
        //     }
        // }

        // [TestMethod]
        // public void Dispose_NormalUseCase_UserDaoObjectShouldBeSuccessfullyDisposed()
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
        //             using (UserDao userDao = new UserDao(dbContext)) {
        //                 userDao.AddOrUpdateUser(new EmissaryUser(69, 420));
        //             }
        //             // this is a kinda useful test but i'm mainly doing it for code coverage
        //             using (UserDao userDao = new UserDao(dbContext)) {
        //                 IDisposable disposable = userDao as IDisposable;
        //                 disposable.Dispose();
        //             }
        //         }
        //         using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
        //             Assert.AreEqual(1, dbContext.Users.Count());
        //             Assert.AreEqual((ulong)69, dbContext.Users.Single().DiscordID);
        //             Assert.AreEqual((long)420, dbContext.Users.Single().BungieID);
        //         }
        //     }
        // }



    }
}