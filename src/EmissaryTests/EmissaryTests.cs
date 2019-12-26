using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryCore;
using EmissaryCore.Common;
using System.Collections.Generic;
using System.IO;
using Moq;

namespace EmissaryTests
{
    // random side note. if i run the tests in iterm and then can't see what i'm typing, 
    // then enter the command `stty sane`
    /// <summary>
    ///    
    /// </summary>
    [TestClass]
    public class EmissaryTests
    {

        [TestMethod]
        public void CurrentlyEquipped_MultipleCharacters_ShouldReturnEquippedItemsForMostRecentlyPlayedCharacter()
        {
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 221313820847636491;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            EmissaryUser user = new EmissaryUser();
            user.DiscordId = discordId;
            user.DestinyProfileId = destinyProfileId;
            user.DestinyMembershipType = destinyMembershipType;
            Mock.Get(userDao).Setup(ud => ud.GetUserByDiscordId(discordId)).Returns(user);

            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter warlock = new DestinyCharacter(2305843009470834170, DateTimeOffset.Parse("2019-12-13T02:31:43Z"), destinyProfileId, BungieMembershipType.Steam);
            // titan is the most recently played character
            DestinyCharacter titan = new DestinyCharacter(2305843009504575107, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            DestinyCharacter hunter = new DestinyCharacter(2305843009557154440, DateTimeOffset.Parse("2019-11-25T03:34:56Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(warlock.CharacterId, warlock);
            charactersResponse.Characters.Add(titan.CharacterId, titan);
            charactersResponse.Characters.Add(hunter.CharacterId, hunter);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.Is<ProfileCharactersRequest>(r => r.DestinyProfileId == destinyProfileId && r.MembershipType == destinyMembershipType))).Returns(charactersResponse);

            DestinyGenericItem[] equippedItems = new DestinyGenericItem[] { new DestinyGenericItem(69, 420) };
            CharacterEquipmentResponse equipmentResponse = new CharacterEquipmentResponse(equippedItems);
            Mock.Get(bungieApiService).Setup(m => m.GetCharacterEquipment(It.Is<CharacterEquipmentRequest>(r => r.DestinyCharacterId == titan.CharacterId && r.DestinyProfileId == destinyProfileId && r.DestinyMembershipType == destinyMembershipType))).Returns(equipmentResponse);

            // IBungieApiService bungieApiService = Mock.Of<IBungieApiService>(x =>
            // x.GetProfileCharacters(charactersRequest) == charactersResponse &&
            // x.GetCharacterEquipment(equipmentRequest) == equipmentResponse
            // );

            uint izanagiHash = 3211806999;
            ManifestItemDefinition izanagiItemDefinition = new ManifestItemDefinition("Izanagi's Burden", new uint[] { 2, 1, 10 });
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(izanagiHash)).Returns(izanagiItemDefinition);

            IEmissary emissary = new Emissary(bungieApiService, manifestDao, userDao, loadoutDao, dbContext);

            Loadout actual = emissary.CurrentlyEquipped(discordId);

            Assert.AreEqual(discordId, actual.DiscordId);
            Assert.AreEqual(titan.CharacterId, actual.DestinyCharacterId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(actual.LoadoutName));
            Assert.AreEqual(1, actual.Items.Count);
            Assert.AreEqual("Izanagi's Burden", actual.Items[0].Name);
            Assert.AreEqual(3, actual.Items[0].Categories.Count);
            Assert.IsTrue(actual.Items[0].Categories.Contains("Weapon"));
            Assert.IsTrue(actual.Items[0].Categories.Contains("Kinetic Weapon"));
            Assert.IsTrue(actual.Items[0].Categories.Contains("Sniper Rifle"));
        }

        [TestMethod]
        public void CurrentlyEquipped_UserNotRegisteredYet_ShouldThrowUserNotFoundException()
        {
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
            IEmissary emissary = new Emissary(bungieApiService, manifestDao, userDao, loadoutDao, dbContext);
            ulong discordId = 221313820847636491;
            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Throws(new UserNotFoundException());
            Assert.ThrowsException<UserNotFoundException>(() => emissary.CurrentlyEquipped(discordId));
        }

        // [TestMethod]
        // public void CurrentlyEquipped_NotASavedLoadout_ShouldReturnLoadoutWithProperValuesAndNoName()
        // {
        //     IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
        //     IManifestDao manifestDao = Mock.Of<IManifestDao>();
        //     IUserDao userDao = Mock.Of<IUserDao>();
        //     ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
        //     EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
        //     IEmissary emissary = new Emissary(bungieApiService, manifestDao, userDao, loadoutDao, dbContext);

        //     ulong discordId = 221313820847636491;
        //     // int destinyMembershipType = BungieMembershipType.Steam;
        //     // long destinyProfileId = 4611686018467260757;
        //     long destinyCharacterId = 2305843009504575107;
        //     Loadout actual = emissary.CurrentlyEquipped(discordId);
        //     Assert.AreEqual(discordId, actual.DiscordId);
        //     Assert.AreEqual(destinyCharacterId, actual.DestinyCharacterId);
        //     Assert.AreEqual("no name", actual.LoadoutName);
        //     Assert.AreEqual(17, actual.Items.Length);
        // }






        // [TestMethod]
        // public void GetMostRecentlyUsedCharacter_PersonalAccount_ShouldReturnTitanId()
        // {
        //     Mock<IBungieApiService> mock = new Mock<IBungieApiService>();
        //     string requestString = "https://www.bungie.net/Platform/Destiny2/3/Profile/4611686018467260757/?components=200";
        //     string responseString = ReadFile("get-characters-personal.json");
        //     mock.Setup(m => m.Get(requestString)).Returns(responseString);
        //     IEmissary emissary = new Emissary(mock.Object, new ManifestAccessor());
        //     long membershipId = 4611686018467260757;
        //     long expectedCharacterId = 2305843009504575107;
        //     long characterId = emissary.GetMostRecentlyPlayedCharacter(membershipId);
        //     Assert.AreEqual(expectedCharacterId, characterId);
        // }



        // [TestMethod]
        // public void GetCurrentlyEquipped_PersonalAccount_ShouldReturnKineticIzanagi()
        // {

        //     Mock<IBungieApiService> mock = new Mock<IBungieApiService>();
        //     string charactersRequest = "https://www.bungie.net/Platform/Destiny2/3/Profile/4611686018467260757/?components=200";
        //     string charactersResponse = ReadFile("get-characters-personal.json");
        //     string characterInventoryRequest = "https://www.bungie.net/Platform/Destiny2/3/Profile/4611686018467260757/?components=205";
        //     string characterInventoryResponse = ReadFile("get-character-equipment-personal.json");
        //     mock.Setup(m => m.Get(charactersRequest)).Returns(charactersResponse);
        //     mock.Setup(m => m.Get(characterInventoryRequest)).Returns(characterInventoryResponse);

        //     IEmissary emissary = new Emissary(mock.Object, new ManifestAccessor());
        //     long membershipId = 4611686018467260757;

        //     Loadout currentlyEquipped = emissary.GetCurrentlyEquipped(membershipId);

        //     Assert.AreEqual("Izanagi's Burden", currentlyEquipped.KineticWeapon.Name);
        // }




        // [TestMethod]
        // public void GetCharacterEquipmentAsItemHashes_PersonalAccount_ShouldReturnListContainingItemHashForIzanagi()
        // {
        //     Mock<IBungieApiService> mock = new Mock<IBungieApiService>();
        //     string requestString = "https://www.bungie.net/Platform/Destiny2/3/Profile/4611686018467260757/?components=205";
        //     string responseString = ReadFile("get-character-equipment-personal.json");
        //     mock.Setup(m => m.Get(requestString)).Returns(responseString);
        //     IEmissary emissary = new Emissary(mock.Object, new ManifestAccessor());
        //     long membershipId = 4611686018467260757;
        //     long characterId = 2305843009504575107;
        //     uint expectedItemHash = 0;  // TODO ?
        //     List<uint> actual = emissary.GetCharacterEquipmentAsItemHashes(membershipId, characterId);
        //     Assert.IsTrue(actual.Contains(expectedItemHash));  // TODO what should i assert? not sure
        // }

        // [TestMethod]
        // public void GetCharacterEquipmentNames_PersonalAccount_x()
        // {
        //     Mock<IBungieApiService> mock = new Mock<IBungieApiService>();
        //     string requestString = "https://www.bungie.net/Platform/Destiny2/3/Profile/4611686018467260757/?components=205";
        //     string responseString = ReadFile("get-character-equipment-personal.json");
        //     mock.Setup(m => m.Get(requestString)).Returns(responseString);
        //     IEmissary emissary = new Emissary(mock.Object, new ManifestAccessor());
        //     long membershipId = 4611686018467260757;
        //     long characterId = 2305843009504575107;
        //     List<string> actual = emissary.GetCharacterEquipmentNames(membershipId, characterId);
        //     Assert.Fail();  // TODO what should i assert? not sure
        // }


        // [TestMethod]
        // public void TrySearchDestinyPlayer_PersonalAccount_ShouldReturnTrueAndProperMembershipId()
        // {

        //     Mock<IBungieApiService> mock = new Mock<IBungieApiService>();
        //     string requestString = "https://www.bungie.net/platform/Destiny2/SearchDestinyPlayer/-1/pimpdaddy/";
        //     string responseString = ReadFile("search-destiny-player-personal.json");
        //     mock.Setup(m => m.Get(requestString)).Returns(responseString);
        //     IEmissary emissary = new Emissary(mock.Object, new ManifestAccessor());
        //     long expectedMembershipId = 4611686018467260757;
        //     bool success = emissary.TrySearchDestinyPlayer("pimpdaddy", out long membershipId);
        //     Assert.IsTrue(success);
        //     Assert.AreEqual(expectedMembershipId, membershipId);
        // }


    }
}
