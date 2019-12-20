using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emissary;
using Moq;
using System;
using System.IO;
using System.Collections.Generic;

namespace EmissaryTests
{
    // [TestClass]
    // public class UserInfoDaoTests
    // {

        // [TestMethod]
        // public void GetUserBungieId_PersonalAccount_ShouldReturnCorrectId()
        // {
        //     ulong discordId = 221313820847636491;  // personal account discord id
        //     long convertedDiscordId = -9002058216007139317;
        //     long expectedBungieId = 4611686018467260757;  // personal account bungie id
        //     string commandText = $"SELECT bungieId FROM UserIds WHERE discordId = {convertedDiscordId}";
        //     // string commandResponse = "4611686018467260757";
        //     IUserInfoDao userInfoDao = new UserInfoDao();

        //     long actualBungieId = userInfoDao.GetUserBungieId(discordId);

        //     Assert.AreEqual(expectedBungieId, actualBungieId);
        // }

        // [TestMethod]
        // public void GetUserBungieId_UserDoesNotExist_ShouldThrowUserDoesNotExistException()
        // {
        //     ulong discordIdThatDoesNotExist = 1;
        //     IUserInfoDao userInfoDao = new UserInfoDao();
        //     Assert.ThrowsException<UserDoesNotExistException>(() => userInfoDao.GetUserBungieId(discordIdThatDoesNotExist));
        // }

        // [TestMethod]
        // public void GetUserBungieId_InvalidDatabasePath_ShouldThrowDataAccessException()
        // {
        //     ulong discordId = 221313820847636491;
        //     string invalidDatabasePath = Path.Combine(Directory.GetCurrentDirectory(), "invalid.db");
        //     IUserInfoDao userInfoDao = new UserInfoDao(invalidDatabasePath);
        //     Assert.ThrowsException<DataAccessException>(() => userInfoDao.GetUserBungieId(discordId));
        // }


        // [TestMethod]
        // public void GetAllLoadouts_NoLoadoutsFound_ShouldReturnEmptyList()
        // {
        //     IUserInfoDao userInfoDao = new UserInfoDao();
        //     ulong dummyDiscordId = 69;
        //     long dummyBungieId = 420;
        //     // setup database (add the data needed for this test)
        //     // the user has registered, it's just that they haven't saved any loadouts yet
        //     userInfoDao.AddUser(69, 420);
        //     List<Loadout> actual = userInfoDao.GetAllLoadouts(dummyDiscordId);
        //     Assert.AreEqual(0, actual);
        //     // cleanup database (set back to original state)
            
        // }

        // [TestMethod]
        // public void GetAllLoadouts_UserDoesNotExistAtAll_ShouldThrowUserDoesNotExistException()
        // {
        //     Assert.Fail();
        // }

    // }
}