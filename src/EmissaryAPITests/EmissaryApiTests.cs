using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryApi;
using EmissaryApi.Model;
using System.Collections.Generic;
using System.IO;
using Moq;

namespace EmissaryApiTests
{
    // random side note. if i run the tests in iterm and then can't see what i'm typing, 
    // then enter the command `stty sane`
    /// <summary>
    ///    
    /// </summary>
    [TestClass]
    public class EmissaryApiTests
    {

        [TestMethod]
        public void GetMostRecentlyUsedCharacter_PersonalAccount_ShouldReturnTitanId()
        {
            Mock<IBungieApiService> mock = new Mock<IBungieApiService>();
            string requestString = "https://www.bungie.net/Platform/Destiny2/3/Profile/4611686018467260757/?components=200";
            string responseString = ReadFile("get-characters-personal.json");
            mock.Setup(m => m.Get(requestString)).Returns(responseString);
            Emissary emissary = new Emissary(mock.Object, new Manifest());
            long membershipId = 4611686018467260757;
            long expectedCharacterId = 2305843009504575107;
            long characterId = emissary.GetMostRecentlyPlayedCharacter(membershipId);
            Assert.AreEqual(expectedCharacterId, characterId);
        }

        [TestMethod]
        public void GetCharacterEquipmentAsItemHashes_PersonalAccount_ShouldReturnListContainingItemHashForIzanagi()
        {
            Mock<IBungieApiService> mock = new Mock<IBungieApiService>();
            string requestString = "https://www.bungie.net/Platform/Destiny2/3/Profile/4611686018467260757/?components=205";
            string responseString = ReadFile("get-character-equipment-personal.json");
            mock.Setup(m => m.Get(requestString)).Returns(responseString);
            Emissary emissary = new Emissary(mock.Object, new Manifest());
            long membershipId = 4611686018467260757;
            long characterId = 2305843009504575107;
            UInt32 expectedItemHash = 0;  // TODO ?
            List<UInt32> actual = emissary.GetCharacterEquipmentAsItemHashes(membershipId, characterId);
            Assert.IsTrue(actual.Contains(expectedItemHash));  // TODO what should i assert? not sure
        }

        [TestMethod]
        public void GetCharacterEquipmentNames_PersonalAccount_x()
        {
            Mock<IBungieApiService> mock = new Mock<IBungieApiService>();
            string requestString = "https://www.bungie.net/Platform/Destiny2/3/Profile/4611686018467260757/?components=205";
            string responseString = ReadFile("get-character-equipment-personal.json");
            mock.Setup(m => m.Get(requestString)).Returns(responseString);
            Emissary emissary = new Emissary(mock.Object, new Manifest());
            long membershipId = 4611686018467260757;
            long characterId = 2305843009504575107;
            List<string> actual = emissary.GetCharacterEquipmentNames(membershipId, characterId);
            Assert.Fail();  // TODO what should i assert? not sure
        }


        [TestMethod]
        public void TrySearchDestinyPlayer_PersonalAccount_ShouldReturnTrueAndProperMembershipId()
        {

            Mock<IBungieApiService> mock = new Mock<IBungieApiService>();
            string requestString = "https://www.bungie.net/platform/Destiny2/SearchDestinyPlayer/-1/pimpdaddy/";
            string responseString = ReadFile("search-destiny-player-personal.json");
            mock.Setup(m => m.Get(requestString)).Returns(responseString);
            Emissary emissary = new Emissary(mock.Object, new Manifest());
            long expectedMembershipId = 4611686018467260757;
            bool success = emissary.TrySearchDestinyPlayer("pimpdaddy", out long membershipId);
            Assert.IsTrue(success);
            Assert.AreEqual(expectedMembershipId, membershipId);
        }


        private string ReadFile(string testFileName)
        {
            string testsDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            string jsonResponsesPath = Path.Combine(testsDirectory, "json-responses");
            string filePath = Path.Combine(jsonResponsesPath, testFileName);
            return File.ReadAllText(filePath);
        }

    }
}
