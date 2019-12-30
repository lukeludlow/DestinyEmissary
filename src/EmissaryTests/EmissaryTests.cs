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
            IConfiguration config = Mock.Of<IConfiguration>();
            // TODO do i need this?
            // Mock.Get(config).Setup(m => m["Bungie:ApiToken"]).Returns("");

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
                    new List<uint>() { 2, 1, 10 });
            IList<DestinyItem> expectedLoadoutItems = new List<DestinyItem>() { izanagiItem };
            Loadout expectedLoadout = new Loadout(discordId, titan.CharacterId, "currently equipped", expectedLoadoutItems);

            // IBungieApiService bungieApiService = Mock.Of<IBungieApiService>(x =>
            // x.GetProfileCharacters(charactersRequest) == charactersResponse &&
            // x.GetCharacterEquipment(equipmentRequest) == equipmentResponse
            // );

            ManifestItemDefinition izanagiItemDefinition = new ManifestItemDefinition("Izanagi's Burden", new uint[] { 2, 1, 10 });
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

            ManifestItemDefinition perfectParadoxItemDefinition = new ManifestItemDefinition("Perfect Paradox", new List<uint>(){1, 2});
            ManifestItemDefinition suddenDeathItemDefinition = new ManifestItemDefinition("A Sudden Death", new List<uint>(){1, 3});
            ManifestItemDefinition apexPredatorItemDefinition = new ManifestItemDefinition("Apex Predator", new List<uint>(){1, 4});
            ManifestItemDefinition maskOfRullItemDefinition = new ManifestItemDefinition("Mask of Rull", new List<uint>(){20});
            ManifestItemDefinition reverieDawnGauntletsItemDefinition = new ManifestItemDefinition("Reverie Dawn Gauntlets", new List<uint>(){20});
            ManifestItemDefinition plateOfTranscendenceItemDefinition = new ManifestItemDefinition("Plate of Transcendence", new List<uint>(){20});
            ManifestItemDefinition peacekeepersItemDefinition = new ManifestItemDefinition("Peacekeepers", new List<uint>(){20});
            ManifestItemDefinition markOfTheGreatHuntItemDefinition = new ManifestItemDefinition("Mark of the Great Hunt", new List<uint>(){20});
            ManifestItemDefinition starMapShellItemDefinition = new ManifestItemDefinition("Star Map Shell", new List<uint>());
            ManifestItemDefinition soloStandSparrowItemDefinition = new ManifestItemDefinition("Solo Stand", new List<uint>());
            ManifestItemDefinition safePassageItemDefinition = new ManifestItemDefinition("Safe Passage", new List<uint>());
            ManifestItemDefinition sentinelItemDefinition = new ManifestItemDefinition("Sentinel", new List<uint>());
            ManifestItemDefinition clanBannerItemDefinition = new ManifestItemDefinition("Clan Banner", new List<uint>());
            ManifestItemDefinition prismaticInfernoItemDefinition = new ManifestItemDefinition("Prismatic Inferno", new List<uint>());
            ManifestItemDefinition finishersItemDefinition = new ManifestItemDefinition("Finishers", new List<uint>());
            ManifestItemDefinition emotesItemDefinition = new ManifestItemDefinition("Emotes", new List<uint>());
            ManifestItemDefinition lanternOfOsirisItemDefinition = new ManifestItemDefinition("The Lantern of Osiris", new List<uint>());
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
        public void SaveLoadout_NewLoadoutForRegisteredUser_ShouldSucceedAndWriteToDatabase()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 221313820847636491;
            long destinyCharacterId = 2305843009504575107;
            long destinyProfileId = 4611686018467260757;
            int destinyMembershipType = BungieMembershipType.Steam;
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, "");
            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);

            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 });
            Loadout loadoutToSave = new Loadout(discordId, destinyCharacterId, "crucible", new List<DestinyItem>() { izanagiItem });

            IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.SaveLoadout(discordId, loadoutToSave, "crucible");
            Assert.IsTrue(result.Success);
            Mock.Get(loadoutDao)
                .Verify(m =>
                    m.AddOrUpdateLoadout(It.Is<Loadout>(l =>
                        l.DiscordId == loadoutToSave.DiscordId &&
                        l.DestinyCharacterId == loadoutToSave.DestinyCharacterId &&
                        l.LoadoutName == loadoutToSave.LoadoutName)), Times.Once());
        }

        [TestMethod]
        public void SaveLoadout_OverwriteExistingLoadout_ShouldSucceedAndWriteToDatabase()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();

            ulong discordId = 221313820847636491;
            long destinyCharacterId = 2305843009504575107;
            // long destinyProfileId = 4611686018467260757;
            // int destinyMembershipType = BungieMembershipType.Steam;
            // EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, "");

            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 });
            Loadout loadoutToSave = new Loadout(discordId, destinyCharacterId, "crucible", new List<DestinyItem>() { izanagiItem });

            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    dbContext.Database.EnsureCreated();
                    IList<Loadout> existingLoadoutsForUser = dbContext.Loadouts.Where(l => l.DiscordId == discordId).ToList();
                    Assert.AreEqual(0, existingLoadoutsForUser.Count);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    IUserDao userDao = new UserDao(dbContext);
                    ILoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
                    emissary.SaveLoadout(discordId, loadoutToSave, "crucible");
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    IList<Loadout> existingLoadoutsForUser = dbContext.Loadouts.Where(l => l.DiscordId == discordId).ToList();
                    Assert.AreEqual(1, existingLoadoutsForUser.Count);
                    Assert.AreEqual(1, existingLoadoutsForUser[0].Items.Count);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    IUserDao userDao = new UserDao(dbContext);
                    ILoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    IEmissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
                    loadoutToSave.Items.Remove(loadoutToSave.Items.Single(item => item.Name == "Izanagi's Burden"));
                    EmissaryResult result = emissary.SaveLoadout(discordId, loadoutToSave, "crucible");
                    Assert.IsTrue(result.Success);
                }
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    IList<Loadout> existingLoadoutsForUser = dbContext.Loadouts.Where(l => l.DiscordId == discordId).ToList();
                    Assert.AreEqual(1, existingLoadoutsForUser.Count);
                    Assert.AreEqual(0, existingLoadoutsForUser[0].Items.Count);
                }
            }
        }

        [TestMethod]
        public void SaveLoadout_CantAccessDatabase_ShouldReturnError()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            Loadout loadoutToSave = new Loadout(69, 420, "crucible", new List<DestinyItem>() { });
            // using Mode=ReadWrite will fail because it can't create the database.
            // the default is Mode=ReadWriteCreate which creates the database if it doesn't exist.
            using (SqliteConnection connection = new SqliteConnection("DataSource=:memory:")) {
                connection.Open();
                DbContextOptions<EmissaryDbContext> options = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(connection)
                    .Options;
                using (EmissaryDbContext dbContext = new EmissaryDbContext(options)) {
                    IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
                    IManifestDao manifestDao = Mock.Of<IManifestDao>();
                    IUserDao userDao = Mock.Of<IUserDao>();
                    // ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
                    ILoadoutDao loadoutDao = new LoadoutDao(dbContext);
                    // EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();
                    Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
                    // connection.Close();
                    // dbContext.Database.CloseConnection();
                    EmissaryResult result = emissary.SaveLoadout(69, loadoutToSave, "crucible");
                    Assert.IsFalse(result.Success);
                    Assert.IsTrue(result.ErrorMessage.Contains("no such table: Loadouts"));
                }
            }
        }

        // TODO does this test make sense? do i need it?
        // [TestMethod]
        // public void SaveLoadout_UserHasNotRegisteredYet_ShouldThrowException()
        // {
        //     Assert.Fail();
        // }



        [TestMethod]
        public void EquipLoadout_UserIsRegisteredAndAuthorizedAndLoadoutExists_ShouldSendRequestToBungieApi()
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
            string accessToken = "access.token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            long destinyCharacterId = 2305843009504575107;
            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 });
            Loadout loadout = new Loadout(discordId, destinyCharacterId, "raid", new List<DestinyItem>() { izanagiItem });

            EquipItemsResponse response = new EquipItemsResponse(new List<EquipItemResult>() { new EquipItemResult(izanagiHash, BungiePlatformErrorCodes.Success) });
            Mock.Get(bungieApiService).Setup(m => m.EquipItems(It.IsAny<EquipItemsRequest>())).Returns(response);

            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(2305843009504575107, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(It.IsAny<ulong>())).Returns(user);
            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(It.IsAny<ulong>(), It.IsAny<long>(), It.IsAny<string>())).Returns(loadout);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);

            EmissaryResult result = emissary.EquipLoadout(discordId, "raid");
            Assert.IsTrue(result.Success);
            Mock.Get(bungieApiService).Verify(m =>
                m.EquipItems(It.Is<EquipItemsRequest>(r =>
                    r.DestinyCharacterId == destinyCharacterId &&
                    r.MembershipType == destinyMembershipType &&
                    r.AccessToken == accessToken &&
                    r.ItemInstanceIds.Count == loadout.Items.Count &&
                    r.ItemInstanceIds[0] == izanagiInstanceId)), Times.Once());
        }

        [TestMethod]
        public void EquipLoadout_UserIsNotRegistered_ShouldReturnErrorResultAndNotCallBungieApi()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 221313820847636491;
            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(It.Is<ulong>(u => u == discordId))).Returns(value: null);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.EquipLoadout(discordId, "raid");
            Assert.IsFalse(result.Success);
            Mock.Get(bungieApiService).Verify(m => m.GetOAuthAccessToken(It.IsAny<OAuthRequest>()), Times.Never());
            Mock.Get(bungieApiService).Verify(m => m.EquipItems(It.IsAny<EquipItemsRequest>()), Times.Never());
            // Mock.Get(bungieApiService).VerifyNoOtherCalls();
        }

        [TestMethod]
        public void EquipLoadout_UserIsRegisteredButAccessTokenExpired_ShouldReturnErrorResult()
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
            string accessToken = "expired-access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);

            long destinyCharacterId = 2305843009504575107;
            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden", new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash, new List<uint>() { 2, 1, 10 });
            Loadout loadout = new Loadout(discordId, destinyCharacterId, "raid", new List<DestinyItem>() { izanagiItem });

            Mock.Get(loadoutDao).Setup(m => m.GetLoadout(discordId, destinyCharacterId, "raid")).Returns(loadout);

            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(2305843009504575107, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            Mock.Get(bungieApiService).Setup(m => m.EquipItems(It.IsAny<EquipItemsRequest>())).Throws(new BungieApiException("Unauthorized: Access is denied due to invalid credentials."));

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.EquipLoadout(discordId, "raid");

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("Unauthorized"));
        }
        // TODO this should refresh the token and try again
        // EquipLoadout_AccessTokenExpiredButCanBeRefreshed_ShouldSendRequestToBungieApiToReauthorizeThenProceedToEquipLoadout


        [TestMethod]
        public void RegisterOrReauthorize_NewUser_ShouldRequestNewAccessTokenAndWriteToDatabase()
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
            string accessToken = "access-token";
            EmissaryUser expectedUser = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            string authCode = "auth-code";

            OAuthResponse authResponse = new OAuthResponse();
            authResponse.AccessToken = accessToken;

            Mock.Get(bungieApiService)
                .Setup(m =>
                    m.GetOAuthAccessToken(It.Is<OAuthRequest>(r =>
                        r.AuthCode == authCode)))
                .Returns(authResponse);

            UserMembershipsResponse membershipsResponse = new UserMembershipsResponse();
            membershipsResponse.DestinyMemberships = new List<DestinyMembership>();
            membershipsResponse.DestinyMemberships.Add(new DestinyMembership("pimpdaddy", destinyProfileId, destinyMembershipType, BungieMembershipType.Steam));

            Mock.Get(bungieApiService)
                .Setup(m =>
                    m.GetMembershipsForUser(It.Is<UserMembershipsRequest>(r =>
                        r.AccessToken == accessToken)))
                .Returns(membershipsResponse);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.RegisterOrReauthorize(discordId, authCode);
            Assert.IsTrue(result.Success);

            Mock.Get(bungieApiService).Verify(m => m.GetOAuthAccessToken(It.IsAny<OAuthRequest>()), Times.Once());
            Mock.Get(bungieApiService).Verify(m => m.GetMembershipsForUser(It.IsAny<UserMembershipsRequest>()), Times.Once());

            Mock.Get(userDao)
                .Verify(m =>
                    m.AddOrUpdateUser(It.Is<EmissaryUser>(u =>
                        u.DiscordId == discordId &&
                        u.DestinyProfileId == destinyProfileId &&
                        u.DestinyMembershipType == destinyMembershipType &&
                        u.BungieAccessToken == accessToken)), Times.Once());
        }

        [TestMethod]
        public void RegisterOrReauthorize_UserAlreadyExists_ShouldRefreshAccessTokenAndWriteToDatabase()
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
            string expiredAccessToken = "expired-access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, expiredAccessToken);

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);

            string authCode = "auth-code";

            OAuthResponse oauthResponse = new OAuthResponse();
            string newAccessToken = "new-access-token";
            oauthResponse.AccessToken = newAccessToken;

            Mock.Get(bungieApiService)
                .Setup(m =>
                    m.GetOAuthAccessToken(It.Is<OAuthRequest>(r =>
                        r.AuthCode == authCode)))
                .Returns(oauthResponse);


            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.RegisterOrReauthorize(discordId, authCode);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Message.ToLower().Contains("reauthorized"));

            Mock.Get(userDao).Verify(m => m.GetUserByDiscordId(discordId), Times.Once());

            Mock.Get(userDao).Verify(m =>
                m.AddOrUpdateUser(It.Is<EmissaryUser>(r =>
                    r.DiscordId == discordId &&
                    r.BungieAccessToken == newAccessToken)), Times.Once());
        }

        [TestMethod]
        public void RegisterOrReauthorize_ExistingUserButInvalidAuthCode_ShouldReturnErrorResultAndNotChangeDatabase()
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
            string accessToken = "access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);

            string authCode = "auth-code";

            OAuthResponse authResponse = new OAuthResponse();
            authResponse.AccessToken = null;
            authResponse.ErrorMessage = "AuthorizationCodeInvalid";

            Mock.Get(bungieApiService)
                .Setup(m =>
                    m.GetOAuthAccessToken(It.Is<OAuthRequest>(r =>
                        r.AuthCode == authCode)))
                .Returns(authResponse);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.RegisterOrReauthorize(discordId, authCode);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("AuthorizationCodeInvalid"));

            Mock.Get(bungieApiService).Verify(m => m.GetOAuthAccessToken(It.IsAny<OAuthRequest>()), Times.Once());
            Mock.Get(bungieApiService).VerifyNoOtherCalls();

            Mock.Get(userDao).Verify(m => m.GetUserByDiscordId(discordId), Times.Once());
            // it should not try to update the user in the database
            Mock.Get(userDao).VerifyNoOtherCalls();
        }

        [TestMethod]
        public void ListLoadouts_UserHasOneLoadout_ShouldReturnSuccessWithLoadoutMessageString()
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
            string accessToken = "access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            long titanCharacterId = 2305843009504575107;
            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden",
                    new List<string>() { "Weapon", "Kinetic Weapon", "Sniper Rifle" }, izanagiHash,
                    new List<uint>() { 2, 1, 10 });
            Loadout titanLoadout = new Loadout(discordId, titanCharacterId, "raid",
                    new List<DestinyItem>() { izanagiItem });

            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(titanCharacterId, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);

            IList<Loadout> allLoadoutsForUser = new List<Loadout>() { titanLoadout };

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);
            Mock.Get(loadoutDao).Setup(m => m.GetAllLoadoutsForUser(discordId)).Returns(allLoadoutsForUser);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.ListLoadouts(discordId);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(JsonConvert.SerializeObject(allLoadoutsForUser), result.Message);
        }

        [TestMethod]
        public void ListLoadouts_UserHasLoadoutsOnDifferentCharacters_ShouldReturnLoadoutsForCurrentCharacterOnly()
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
            string accessToken = "access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            long titanCharacterId = 2305843009504575107;
            uint izanagiHash = 3211806999;
            long izanagiInstanceId = 6917529135183883487;
            DestinyItem izanagiItem = new DestinyItem(izanagiInstanceId, "Izanagi's Burden",
                    new List<string>() { "Kinetic Weapon", "Weapon", "Sniper Rifle" }, izanagiHash,
                    new List<uint>() { 2, 1, 10 });
            uint suddenDeathHash = 1879212552;
            long suddenDeathInstanceId = 6917529043814140192;
            DestinyItem suddenDeathItem = new DestinyItem(suddenDeathInstanceId, "A Sudden Death",
                    new List<string>() { "Energy Weapon", "Weapon", "Shotgun" }, suddenDeathHash,
                    new List<uint>() { 3, 1, 11 });

            Loadout titanLoadout = new Loadout(discordId, titanCharacterId, "raid",
                    new List<DestinyItem>() { izanagiItem });
            Loadout titanLoadout2 = new Loadout(discordId, titanCharacterId, "raid2", new List<DestinyItem>() { suddenDeathItem });
            Loadout titanLoadout3 = new Loadout(discordId, titanCharacterId, "raid3", new List<DestinyItem>() { izanagiItem, suddenDeathItem });
            Loadout warlockLoadout = new Loadout(discordId, 69, "raid", new List<DestinyItem>() { izanagiItem });
            Loadout hunterLoadout = new Loadout(discordId, 420, "raid", new List<DestinyItem>() { izanagiItem });
            IList<Loadout> allLoadoutsForUser = new List<Loadout>() { titanLoadout, titanLoadout2, titanLoadout3, warlockLoadout, hunterLoadout };
            // need this for verification
            IList<Loadout> titanLoadouts = new List<Loadout>() { titanLoadout, titanLoadout2, titanLoadout3 };

            // assemble so that titan is the most recently played character
            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(titanCharacterId, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);
            Mock.Get(loadoutDao).Setup(m => m.GetAllLoadoutsForUser(discordId)).Returns(allLoadoutsForUser);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.ListLoadouts(discordId);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(JsonConvert.SerializeObject(titanLoadouts), result.Message);
        }

        [TestMethod]
        public void ListLoadouts_UserIsNotRegistered_ShouldReturnError()
        {
            IConfiguration config = Mock.Of<IConfiguration>();
            IBungieApiService bungieApiService = Mock.Of<IBungieApiService>();
            IManifestDao manifestDao = Mock.Of<IManifestDao>();
            IUserDao userDao = Mock.Of<IUserDao>();
            ILoadoutDao loadoutDao = Mock.Of<ILoadoutDao>();
            EmissaryDbContext dbContext = Mock.Of<EmissaryDbContext>();

            ulong discordId = 69;

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(value: null);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.ListLoadouts(discordId);

            Assert.IsFalse(result.Success);
            // Assert.IsTrue(result.ErrorMessage.Contains("user not found"));
        }

        [TestMethod]
        public void ListLoadouts_UserHasNoLoadouts_ShouldReturnSuccessButZeroLoadoutsEmptyList()
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
            string accessToken = "access-token";
            EmissaryUser user = new EmissaryUser(discordId, destinyProfileId, destinyMembershipType, accessToken);

            IList<Loadout> allLoadoutsForUser = new List<Loadout>() { };

            // assemble so that titan is the most recently played character
            long titanCharacterId = 2305843009504575107;
            ProfileCharactersResponse charactersResponse = new ProfileCharactersResponse();
            DestinyCharacter titan = new DestinyCharacter(titanCharacterId, DateTimeOffset.Parse("2019-12-24T22:40:31Z"), destinyProfileId, BungieMembershipType.Steam);
            charactersResponse.Characters.Add(titan.CharacterId, titan);

            Mock.Get(userDao).Setup(m => m.GetUserByDiscordId(discordId)).Returns(user);
            Mock.Get(loadoutDao).Setup(m => m.GetAllLoadoutsForUser(discordId)).Returns(allLoadoutsForUser);
            Mock.Get(bungieApiService).Setup(m => m.GetProfileCharacters(It.IsAny<ProfileCharactersRequest>())).Returns(charactersResponse);

            Emissary emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            EmissaryResult result = emissary.ListLoadouts(discordId);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(JsonConvert.SerializeObject(allLoadoutsForUser), result.Message);
        }







    }
}
