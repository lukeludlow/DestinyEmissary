using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryApi;

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
        public void TrySearchDestinyPlayer_PersonalAccount_ShouldReturnTrue()
        {
            Emissary emissary = new Emissary();
            bool success = emissary.TrySearchDestinyPlayer("pimpdaddy", out string membershipId);
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void TrySearchDestinyPlayer_PersonalAccount_ShouldReturnMembershipId()
        {
            // this is just hardcoded because i've already looked up my own account and found my own membership id
            string expectedMembershipId = "4611686018467260757";
            Emissary emissary = new Emissary();
            bool success = emissary.TrySearchDestinyPlayer("pimpdaddy", out string membershipId);
            Assert.AreEqual(expectedMembershipId, membershipId);
        }


        // [TestMethod]
        // public void CurrentlyEquipped_PersonalAccount_ShouldReturnPrimaryIzanagi()
        // {
        //     Assert.Fail();
        // }

    }
}
