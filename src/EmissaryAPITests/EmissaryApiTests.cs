using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryApi;
using EmissaryApi.Model;
using System.Collections.Generic;

namespace EmissaryApiTests
{
    /// <summary>
	/// random side note. if i run the tests in iterm and then can't see what i'm typing, 
    /// then enter the command `stty sane`
	/// </summary>
    [TestClass]
    public class EmissaryApiTests
    {


        [TestMethod]
        public void GetMostRecentlyUsedCharacter_PersonalAccount_ShouldReturnTitanId()
        {
            Emissary emissary = new Emissary();
            long membershipId = 4611686018467260757;
            long expectedCharacterId = 2305843009504575107;
            long characterId = emissary.GetMostRecentlyPlayedCharacter(membershipId);
            Assert.AreEqual(expectedCharacterId, characterId);
        }

        [TestMethod]
        public void GetCharacterEquipmentAsItemHashes_PersonalAccount_ShouldReturnListContainingItemHashForIzanagi()
        {
            Emissary emissary = new Emissary();
            long membershipId = 4611686018467260757;
            long characterId = 2305843009504575107;
            UInt32 expectedItemHash = 0;
            List<UInt32> actual = emissary.GetCharacterEquipmentAsItemHashes(membershipId, characterId);
            Assert.IsTrue(actual.Contains(expectedItemHash));
        }


        [TestMethod]
        public void TrySearchDestinyPlayer_PersonalAccount_ShouldReturnTrue()
        {
            Emissary emissary = new Emissary();
            bool success = emissary.TrySearchDestinyPlayer("pimpdaddy", out long membershipId);
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void TrySearchDestinyPlayer_PersonalAccount_ShouldReturnMembershipId()
        {
            Emissary emissary = new Emissary();
            long expectedMembershipId = 4611686018467260757;
            bool success = emissary.TrySearchDestinyPlayer("pimpdaddy", out long membershipId);
            Assert.AreEqual(expectedMembershipId, membershipId);
        }


    }
}
