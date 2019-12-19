using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emissary;
using System.Linq;
using System;
using System.IO;

namespace EmissaryTests
{
    [TestClass]
    public class UserDaoTests
    {

        [TestMethod]
        public void AddUser_ExampleUser_ShouldWriteToDatabase()
        {
            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryUsersDbContext> options = new DbContextOptionsBuilder<EmissaryUsersDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryUsersDbContext context = new EmissaryUsersDbContext(options)) {
                    // create the schema in the database
                    bool databaseCreated = context.Database.EnsureCreated();
                    Assert.IsTrue(databaseCreated);
                }
                using (EmissaryUsersDbContext context = new EmissaryUsersDbContext(options)) {
                    UserDao userRepository = new UserDao(context);
                    userRepository.AddUser(new EmissaryUser(69, 420));
                    EmissaryUser user = userRepository.GetUserByDiscordId(69);
                    context.SaveChanges();
                }
                using (EmissaryUsersDbContext context = new EmissaryUsersDbContext(options)) {
                    Assert.AreEqual(1, context.Users.Count());
                    Assert.AreEqual((ulong)69, context.Users.Single().DiscordID);
                    Assert.AreEqual((long)420, context.Users.Single().BungieID);
                }
            }
        }

        [TestMethod]
        public void Save_AddUserThenSave_ShouldWriteToDatabase()
        {
            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryUsersDbContext> options = new DbContextOptionsBuilder<EmissaryUsersDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryUsersDbContext context = new EmissaryUsersDbContext(options)) {
                    context.Database.EnsureCreated();
                    UserDao userRepository = new UserDao(context);
                    userRepository.AddUser(new EmissaryUser(69, 420));
                    userRepository.Save();
                }
                using (EmissaryUsersDbContext context = new EmissaryUsersDbContext(options)) {
                    Assert.AreEqual(1, context.Users.Count());
                    Assert.AreEqual((ulong)69, context.Users.Single().DiscordID);
                    Assert.AreEqual((long)420, context.Users.Single().BungieID);
                }
            }
        }

    }
}