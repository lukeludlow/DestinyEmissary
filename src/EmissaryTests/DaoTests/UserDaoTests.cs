using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryCore;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;

namespace EmissaryTests.Dao
{
    [TestClass]
    public class UserDaoTests
    {

        [TestMethod]
        public void AddOrUpdateUser_ExampleUser_ShouldWriteToDatabase()
        {
            ulong discordId = 221313820847636491;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            string accessToken = "access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    // create the schema in the database
                    // dbContext.Database.OpenConnection();
                    dbContext.Database.EnsureCreated();
                    // dbContext.Database.CloseConnection();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    UserDao userDao = new UserDao(dbContext);
                    userDao.AddOrUpdateUser(user);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    Assert.AreEqual(1, dbContext.Users.Count());
                    EmissaryUser foundUser = dbContext.Users.Find(discordId);
                    Assert.AreEqual(discordId, foundUser.DiscordId);
                    Assert.AreEqual(destinyProfileId, foundUser.DestinyProfileId);
                    Assert.AreEqual(destinyMembershipType, foundUser.DestinyMembershipType);
                    Assert.AreEqual(accessToken, foundUser.BungieAccessToken);
                }
            }
        }

        [TestMethod]
        public void AddOrUpdateUser_UpdateExistingUser_ShouldSucceedAndWriteToDatabase()
        {
            ulong discordId = 221313820847636491;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            string accessToken = "access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    Assert.AreEqual(1, dbContext.Users.Count());
                    EmissaryUser foundUser = dbContext.Users.Find(discordId);
                    Assert.AreEqual(discordId, foundUser.DiscordId);
                    Assert.AreEqual(destinyProfileId, foundUser.DestinyProfileId);
                    Assert.AreEqual(destinyMembershipType, foundUser.DestinyMembershipType);
                    Assert.AreEqual(accessToken, foundUser.BungieAccessToken);

                    UserDao userDao = new UserDao(dbContext);
                    user.DestinyProfileId = 69;
                    user.BungieAccessToken = "new-token";
                    userDao.AddOrUpdateUser(user);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    Assert.AreEqual(1, dbContext.Users.Count());
                    EmissaryUser foundUser = dbContext.Users.Find(discordId);
                    Assert.AreEqual(discordId, foundUser.DiscordId);
                    Assert.AreEqual((long)69, foundUser.DestinyProfileId);
                    Assert.AreEqual(destinyMembershipType, foundUser.DestinyMembershipType);
                    Assert.AreEqual("new-token", foundUser.BungieAccessToken);
                }
            }
        }

        [TestMethod]
        public void AddOrUpdateUser_UserWithSameValuesButDifferentDiscordId_ShouldKeepBoth()
        {
            ulong discordId = 221313820847636491;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            string accessToken = "access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);
            EmissaryUser user2 = new EmissaryUser(69, destinyProfileId, destinyMembershipType, accessToken);

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    Assert.AreEqual(1, dbContext.Users.Count());
                    UserDao userDao = new UserDao(dbContext);
                    userDao.AddOrUpdateUser(user2);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    Assert.AreEqual(2, dbContext.Users.Count());
                    EmissaryUser foundUser = dbContext.Users.Find(user.DiscordId);
                    Assert.IsNotNull(foundUser);
                    EmissaryUser foundUser2 = dbContext.Users.Find(user2.DiscordId);
                    Assert.IsNotNull(foundUser2);
                }
            }
        }


        [TestMethod]
        public void GetUserByDiscordId_UserDoesExistAndThereAreLotsOfOtherUsersToo_ShouldReturnUser()
        {
            EmissaryUser user1 = new EmissaryUser(69, 69, 3, "access-token");
            EmissaryUser user2 = new EmissaryUser(420, 69, 3, "access-token");
            EmissaryUser user3 = new EmissaryUser(42069, 69, 3, "access-token");
            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                    dbContext.Users.Add(user1);
                    dbContext.Users.Add(user2);
                    dbContext.Users.Add(user3);
                    dbContext.SaveChanges();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    UserDao userDao = new UserDao(dbContext);
                    EmissaryUser foundUser = userDao.GetUserByDiscordId(69);
                    Assert.AreEqual((ulong)69, foundUser.DiscordId);
                    Assert.AreEqual((long)69, foundUser.DestinyProfileId);
                }
            }
        }

        [TestMethod]
        public void GetUserByDiscordId_UserExistsButThenIsRemoved_ShouldReturnUserThenReturnNull()
        {
            ulong discordId = 221313820847636491;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            string accessToken = "access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    UserDao userDao = new UserDao(dbContext);
                    EmissaryUser foundUser = userDao.GetUserByDiscordId(discordId);
                    Assert.AreEqual(discordId, foundUser.DiscordId);
                    Assert.AreEqual(destinyProfileId, foundUser.DestinyProfileId);
                    dbContext.Users.Remove(foundUser);
                    dbContext.SaveChanges();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    UserDao userDao = new UserDao(dbContext);
                    EmissaryUser foundUser = userDao.GetUserByDiscordId(discordId);
                    Assert.IsNull(foundUser);
                }
            }
        }

        [TestMethod]
        public void GetUserByDiscordId_UserHasKeyButAllValuesAreDefaultOrNull_ShouldStillReturnUser()
        {
            ulong discordId = 221313820847636491;
            EmissaryUser user = new EmissaryUser(discordId, default, default, null);

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    UserDao userDao = new UserDao(dbContext);
                    EmissaryUser foundUser = userDao.GetUserByDiscordId(discordId);
                    Assert.AreEqual(discordId, foundUser.DiscordId);
                    Assert.AreEqual(0, foundUser.DestinyProfileId);
                    Assert.AreEqual(0, foundUser.DestinyMembershipType);
                    Assert.AreEqual(null, foundUser.BungieAccessToken);
                }
            }
        }

        [TestMethod]
        public void RemoveUser_UserDoesNotExist_ShouldDoNothing()
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
                    UserDao userDao = new UserDao(dbContext);
                    Assert.AreEqual(0, dbContext.Users.Count());
                    // this shouldn't throw and shouldn't change the database
                    userDao.RemoveUser(69);
                    Assert.AreEqual(0, dbContext.Users.Count());
                }
            }
        }

        [TestMethod]
        public void RemoveUser_UserExists_ShouldRemoveTheSpecifiedUserAndUpdateDatabase()
        {
            ulong discordId = 221313820847636491;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            string accessToken = "access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);
            EmissaryUser user2 = new EmissaryUser(69, destinyProfileId, destinyMembershipType, accessToken);
            EmissaryUser user3 = new EmissaryUser(420, destinyProfileId, destinyMembershipType, accessToken);

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                    dbContext.Users.Add(user);
                    dbContext.Users.Add(user2);
                    dbContext.Users.Add(user3);
                    dbContext.SaveChanges();
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    Assert.AreEqual(3, dbContext.Users.Count());
                    UserDao userDao = new UserDao(dbContext);
                    userDao.RemoveUser(discordId);
                    Assert.AreEqual(2, dbContext.Users.Count());
                    Assert.IsNull(dbContext.Users.Find(discordId));
                }
            }
        }

        [TestMethod]
        public void Dispose_NormalUseCase_UserDaoObjectShouldBeSuccessfullyDisposed()
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
                    using (UserDao userDao = new UserDao(dbContext)) {
                        userDao.AddOrUpdateUser(new EmissaryUser(69, 420, 3, ""));
                    }
                    // this is a kinda useful test but i'm mainly doing it for code coverage
                    using (UserDao userDao = new UserDao(dbContext)) {
                        IDisposable disposable = userDao as IDisposable;
                        disposable.Dispose();
                    }
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    Assert.AreEqual(1, dbContext.Users.Count());
                    Assert.AreEqual((ulong)69, dbContext.Users.Single().DiscordId);
                    Assert.AreEqual((long)420, dbContext.Users.Single().DestinyProfileId);
                }
            }
        }



    }
}