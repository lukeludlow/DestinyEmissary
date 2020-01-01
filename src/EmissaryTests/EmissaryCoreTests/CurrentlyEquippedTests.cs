using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmissaryCore;
using System.Collections.Generic;
using System.IO;
using Moq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Newtonsoft.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace EmissaryTests.Core
{
    [TestClass]
    public class CurrentlyEquippedTests
    {

        [TestMethod]
        public void CurrentlyEquipped_MultipleCharacters_ShouldReturnEquippedItemsForMostRecentlyPlayedCharacter()
        {
            IConfiguration config = Mock.Of<IConfiguration>();

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

            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyGenericItem[] equippedItems = new DestinyGenericItem[] { new DestinyGenericItem(izanagiHash, izanagiInstanceId) };
            CharacterEquipmentResponse equipmentResponse = new CharacterEquipmentResponse(equippedItems);
            Mock.Get(bungieApiService).Setup(m => m.GetCharacterEquipment(It.Is<CharacterEquipmentRequest>(r => r.DestinyCharacterId == titan.CharacterId && r.DestinyProfileId == destinyProfileId && r.DestinyMembershipType == destinyMembershipType))).Returns(equipmentResponse);

            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden",
                    new List<string>() { "Kinetic Weapon", "Weapon", "Sniper Rifle" }, izanagiHash,
                    new List<uint>() { 2, 1, 10 }, "Exotic");
            IList<DestinyItem> expectedLoadoutItems = new List<DestinyItem>() { izanagiItem };
            Loadout expectedLoadout = new Loadout(discordId, titan.CharacterId, "unsaved loadout", expectedLoadoutItems);

            // IBungieApiService bungieApiService = Mock.Of<IBungieApiService>(x =>
            // x.GetProfileCharacters(charactersRequest) == charactersResponse &&
            // x.GetCharacterEquipment(equipmentRequest) == equipmentResponse
            // );

            ManifestItemDefinition izanagiItemDefinition = new ManifestItemDefinition("Izanagi's Burden", "Exotic", new uint[] { 2, 1, 10 });
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(izanagiHash)).Returns(izanagiItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(1)).Returns(new ManifestItemCategoryDefinition("Weapon"));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(2)).Returns(new ManifestItemCategoryDefinition("Kinetic Weapon"));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(10)).Returns(new ManifestItemCategoryDefinition("Sniper Rifle"));

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);

            EmissaryResult result = emissary.CurrentlyEquipped(discordId);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(JsonConvert.SerializeObject(expectedLoadout), result.Message);
        }

        [TestMethod]
        public void CurrentlyEquipped_UserNotRegisteredYet_ShouldEmitRequestAuthorizationEvent()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            bool eventEmitted = false;
            emissary.RequestAuthorizationEvent += (discordId) => eventEmitted = true;
            ulong discordId = 221313820847636491;
            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(value: null);
            EmissaryResult result = emissary.CurrentlyEquipped(discordId);
            Assert.IsTrue(eventEmitted);
        }

        [TestMethod]
        public void CurrentlyEquipped_LotsOfItems_ShouldReturnLoadoutWithAllItems()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 221313820847636491;
            long destinyProfileId = 4611686018467260757;
            // long destinyCharacterId = 2305843009504575107;
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

            // item hashes
            uint perfectParadoxHash = 3393519051;
            uint suddenDeathHash = 1879212552;
            uint apexPredatorHash = 2545083870;
            uint maskOfRullHash = 2913992254;
            uint reverieDawnGauntletsHash = 2336820707;
            uint plateOfTranscendenceHash = 3939809874;
            uint peacekeepersHash = 3539357319;
            uint markOfTheGreatHuntHash = 16387641;
            uint starMapShellHash = 1558857470;
            uint soloStandSparrowHash = 3538153284;
            uint safePassageShipHash = 4063523619;
            uint sentinelSubclassHash = 3382391785;
            uint clanBannerHash = 2873099163;
            uint prismaticInfernoEmblemHash = 4159550313;
            uint finisherHash = 152583919;
            uint emoteHash = 3183180185;
            uint lanternOfOsirisHash = 3360014173;

            // item instance IDs
            long perfectParadox = 6917529138356180356;
            long suddenDeath = 6917529043814140192;
            long apexPredator = 6917529137866710642;
            long maskOfRull = 6917529110566559001;
            long reverieDawnGauntlets = 6917529138010460936;
            long plateOfTranscendence = 6917529109687230597;
            long peacekeepers = 6917529122999918127;
            long markOfTheGreatHunt = 6917529128966008940;
            long starMapShell = 6917529134911753611;
            long soloStandSparrow = 6917529096117947574;
            long safePassageShip = 6917529128292261186;
            long sentinelSubclass = 6917529102011422104;
            long clanBanner = 6917529137830892799;
            long prismaticInfernoEmblem = 6917529105645094388;
            long finisher = 6917529137848551327;
            long emote = 6917529101999517414;
            long lanternOfOsiris = 6917529137968184629;

            DestinyGenericItem[] equippedItems = new DestinyGenericItem[] {
                new DestinyGenericItem(perfectParadoxHash, perfectParadox),
                new DestinyGenericItem(suddenDeathHash, suddenDeath),
                new DestinyGenericItem(apexPredatorHash, apexPredator),
                new DestinyGenericItem(maskOfRullHash, maskOfRull),
                new DestinyGenericItem(reverieDawnGauntletsHash, reverieDawnGauntlets),
                new DestinyGenericItem(plateOfTranscendenceHash, plateOfTranscendence),
                new DestinyGenericItem(peacekeepersHash, peacekeepers),
                new DestinyGenericItem(markOfTheGreatHuntHash, markOfTheGreatHunt),
                new DestinyGenericItem(starMapShellHash, starMapShell),
                new DestinyGenericItem(soloStandSparrowHash, soloStandSparrow),
                new DestinyGenericItem(safePassageShipHash, safePassageShip),
                new DestinyGenericItem(sentinelSubclassHash, sentinelSubclass),
                new DestinyGenericItem(clanBannerHash, clanBanner),
                new DestinyGenericItem(prismaticInfernoEmblemHash, prismaticInfernoEmblem),
                new DestinyGenericItem(finisherHash, finisher),
                new DestinyGenericItem(emoteHash, emote),
                new DestinyGenericItem(lanternOfOsirisHash, lanternOfOsiris)
            };
            CharacterEquipmentResponse equipmentResponse = new CharacterEquipmentResponse(equippedItems);
            Mock.Get(bungieApiService).Setup(m => m.GetCharacterEquipment(It.Is<CharacterEquipmentRequest>(r => r.DestinyCharacterId == titan.CharacterId && r.DestinyProfileId == destinyProfileId && r.DestinyMembershipType == destinyMembershipType))).Returns(equipmentResponse);

            // IBungieApiService bungieApiService = Mock.Of<IBungieApiService>(x =>
            // x.GetProfileCharacters(charactersRequest) == charactersResponse &&
            // x.GetCharacterEquipment(equipmentRequest) == equipmentResponse
            // );

            ManifestItemDefinition perfectParadoxItemDefinition = new ManifestItemDefinition("Perfect Paradox", "Legendary", new List<uint>() { 1, 2 });
            ManifestItemDefinition suddenDeathItemDefinition = new ManifestItemDefinition("A Sudden Death", "Legendary", new List<uint>() { 1, 3 });
            ManifestItemDefinition apexPredatorItemDefinition = new ManifestItemDefinition("Apex Predator", "Legendary", new List<uint>() { 1, 4 });
            ManifestItemDefinition maskOfRullItemDefinition = new ManifestItemDefinition("Mask of Rull", "Legendary", new List<uint>() { 20 });
            ManifestItemDefinition reverieDawnGauntletsItemDefinition = new ManifestItemDefinition("Reverie Dawn Gauntlets", "Legendary", new List<uint>() { 20 });
            ManifestItemDefinition plateOfTranscendenceItemDefinition = new ManifestItemDefinition("Plate of Transcendence", "Legendary", new List<uint>() { 20 });
            ManifestItemDefinition peacekeepersItemDefinition = new ManifestItemDefinition("Peacekeepers", "Exotic", new List<uint>() { 20 });
            ManifestItemDefinition markOfTheGreatHuntItemDefinition = new ManifestItemDefinition("Mark of the Great Hunt", "Legendary", new List<uint>() { 20 });
            ManifestItemDefinition starMapShellItemDefinition = new ManifestItemDefinition("Star Map Shell", "Exotic", new List<uint>());
            ManifestItemDefinition soloStandSparrowItemDefinition = new ManifestItemDefinition("Solo Stand", "Legendary", new List<uint>());
            ManifestItemDefinition safePassageItemDefinition = new ManifestItemDefinition("Safe Passage", "Legendary", new List<uint>());
            ManifestItemDefinition sentinelItemDefinition = new ManifestItemDefinition("Sentinel", "Common", new List<uint>());
            ManifestItemDefinition clanBannerItemDefinition = new ManifestItemDefinition("Clan Banner", "Legendary", new List<uint>());
            ManifestItemDefinition prismaticInfernoItemDefinition = new ManifestItemDefinition("Prismatic Inferno", "Legendary", new List<uint>());
            ManifestItemDefinition finishersItemDefinition = new ManifestItemDefinition("Finishers", "Common", new List<uint>());
            ManifestItemDefinition emotesItemDefinition = new ManifestItemDefinition("Emotes", "Common", new List<uint>());
            ManifestItemDefinition lanternOfOsirisItemDefinition = new ManifestItemDefinition("The Lantern of Osiris", "Legendary", new List<uint>());
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(perfectParadoxHash)).Returns(perfectParadoxItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(suddenDeathHash)).Returns(suddenDeathItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(apexPredatorHash)).Returns(apexPredatorItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(maskOfRullHash)).Returns(maskOfRullItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(reverieDawnGauntletsHash)).Returns(reverieDawnGauntletsItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(plateOfTranscendenceHash)).Returns(plateOfTranscendenceItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(peacekeepersHash)).Returns(peacekeepersItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(markOfTheGreatHuntHash)).Returns(markOfTheGreatHuntItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(starMapShellHash)).Returns(starMapShellItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(soloStandSparrowHash)).Returns(soloStandSparrowItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(safePassageShipHash)).Returns(safePassageItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(sentinelSubclassHash)).Returns(sentinelItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(clanBannerHash)).Returns(clanBannerItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(prismaticInfernoEmblemHash)).Returns(prismaticInfernoItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(finisherHash)).Returns(finishersItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(emoteHash)).Returns(emotesItemDefinition);
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(lanternOfOsirisHash)).Returns(lanternOfOsirisItemDefinition);

            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(1)).Returns(new ManifestItemCategoryDefinition("Weapon"));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(2)).Returns(new ManifestItemCategoryDefinition("Kinetic Weapon"));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(3)).Returns(new ManifestItemCategoryDefinition("Energy Weapon"));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(4)).Returns(new ManifestItemCategoryDefinition("Power Weapon"));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(20)).Returns(new ManifestItemCategoryDefinition("Armor"));

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);

            EmissaryResult result = emissary.CurrentlyEquipped(discordId);
            Assert.IsTrue(result.Success);
            Loadout loadoutResult = JsonConvert.DeserializeObject<Loadout>(result.Message);
            Assert.AreEqual(discordId, loadoutResult.DiscordId);
            Assert.AreEqual(titan.CharacterId, loadoutResult.DestinyCharacterId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(loadoutResult.LoadoutName));
            // Assert.AreEqual(17, loadoutResult.Items.Count);
            // it should only return 8 items, because now we only pay attention to weapons and armor
            Assert.AreEqual(8, loadoutResult.Items.Count);
            Assert.AreEqual("Perfect Paradox", loadoutResult.Items[0].Name);
            Assert.AreEqual("A Sudden Death", loadoutResult.Items[1].Name);
        }

        [TestMethod]
        public void CurrentlyEquipped_CurrentGearMatchesASavedLoadout_ShouldReturnLoadoutWithSavedName()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 69;
            long destinyCharacterId = 420;
            string loadoutName = "last wish raid";
            DestinyItem destinyItem1 = new DestinyItem(6969, "dummy item 1", new List<string>() { "Weapon", "Kinetic Weapon" }, 420420, new List<uint>() { 1, 2 }, "Legendary");
            DestinyItem destinyItem2 = new DestinyItem(9696, "dummy item 2", new List<string>() { "Armor", "Helmet" }, 240240, new List<uint>() { 3, 4 }, "Legendary");
            List<DestinyItem> loadoutItems = new List<DestinyItem>() { destinyItem1, destinyItem2 };
            Loadout savedLoadout = new Loadout(discordId, destinyCharacterId, loadoutName, loadoutItems);

            long destinyProfileId = 42069;
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, BungieMembershipType.Steam, "access.token");

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);
            Mock.Get(loadoutDao).Setup(m => m.GetAllLoadoutsForUser(discordId)).Returns(new List<Loadout>() { savedLoadout });
            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(discordId, destinyCharacterId, loadoutName)).Returns(savedLoadout);

            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter character = new DestinyCharacter(destinyCharacterId, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(character.CharacterId, character);

            CharacterEquipmentResponse equipmentResponse = new CharacterEquipmentResponse();
            DestinyGenericItem genericItem1 = new DestinyGenericItem(destinyItem1.ItemHash, destinyItem1.ItemInstanceId);
            DestinyGenericItem genericItem2 = new DestinyGenericItem(destinyItem2.ItemHash, destinyItem2.ItemInstanceId);
            equipmentResponse.Items = new List<DestinyGenericItem>() { genericItem1, genericItem2 };

            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);
            Mock.Get(bungieApiService).Setup(m => m.GetCharacterEquipment(It.IsAny<CharacterEquipmentRequest>())).Returns(equipmentResponse);

            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(destinyItem1.ItemHash)).Returns(new ManifestItemDefinition(destinyItem1.Name, destinyItem1.TierTypeName, destinyItem1.CategoryHashes));
            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(destinyItem2.ItemHash)).Returns(new ManifestItemDefinition(destinyItem2.Name, destinyItem2.TierTypeName, destinyItem2.CategoryHashes));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(1)).Returns(new ManifestItemCategoryDefinition("Weapon"));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(2)).Returns(new ManifestItemCategoryDefinition("Kinetic Weapon"));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(3)).Returns(new ManifestItemCategoryDefinition("Armor"));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(4)).Returns(new ManifestItemCategoryDefinition("Helmet"));

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);

            EmissaryResult result = emissary.CurrentlyEquipped(discordId);

            Loadout actualLoadout = JsonConvert.DeserializeObject<Loadout>(result.Message);

            Assert.AreEqual("last wish raid", actualLoadout.LoadoutName);
        }

        [TestMethod]
        public void CurrentlyEquipped_CurrentGearDoesNotMatchAnySavedLoadouts_ShouldReturnLoadoutNameUnsavedLoadout()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 69;
            long destinyCharacterId = 420;
            string loadoutName = "last wish raid";
            DestinyItem destinyItem1 = new DestinyItem(6969, "dummy item 1", new List<string>() { "Weapon", "Kinetic Weapon" }, 420420, new List<uint>() { 1, 2 }, "Legendary");
            DestinyItem destinyItem2 = new DestinyItem(9696, "dummy item 2", new List<string>() { "Armor", "Helmet" }, 240240, new List<uint>() { 3, 4 }, "Legendary");
            List<DestinyItem> loadoutItems = new List<DestinyItem>() { destinyItem1, destinyItem2 };
            Loadout savedLoadout = new Loadout(discordId, destinyCharacterId, loadoutName, loadoutItems);

            long destinyProfileId = 42069;
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, BungieMembershipType.Steam, "access.token");

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);
            Mock.Get(loadoutDao).Setup(m => m.GetAllLoadoutsForUser(discordId)).Returns(new List<Loadout>() { savedLoadout });
            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(discordId, destinyCharacterId, loadoutName)).Returns(savedLoadout);

            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter character = new DestinyCharacter(destinyCharacterId, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(character.CharacterId, character);

            CharacterEquipmentResponse equipmentResponse = new CharacterEquipmentResponse();
            DestinyGenericItem genericItem1 = new DestinyGenericItem(destinyItem1.ItemHash, destinyItem1.ItemInstanceId);
            equipmentResponse.Items = new List<DestinyGenericItem>() { genericItem1 };

            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);
            Mock.Get(bungieApiService).Setup(m => m.GetCharacterEquipment(It.IsAny<CharacterEquipmentRequest>())).Returns(equipmentResponse);

            Mock.Get(manifestDao).Setup(m => m.GetItemDefinition(destinyItem1.ItemHash)).Returns(new ManifestItemDefinition(destinyItem1.Name, destinyItem1.TierTypeName, destinyItem1.CategoryHashes));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(1)).Returns(new ManifestItemCategoryDefinition("Weapon"));
            Mock.Get(manifestDao).Setup(m => m.GetItemCategoryDefinition(2)).Returns(new ManifestItemCategoryDefinition("Kinetic Weapon"));

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);

            EmissaryResult result = emissary.CurrentlyEquipped(discordId);

            Loadout actualLoadout = JsonConvert.DeserializeObject<Loadout>(result.Message);

            Assert.AreEqual("unsaved loadout", actualLoadout.LoadoutName);
        }


    }
}
