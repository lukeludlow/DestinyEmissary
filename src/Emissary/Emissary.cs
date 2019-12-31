using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EmissaryCore
{
    // note: discord IDs are UInt64 (ulong). bungie IDs are Int64 (long).
    public class Emissary : IEmissary
    {
        private readonly IConfiguration config;
        private readonly IBungieApiService bungieApiService;
        private readonly IManifestDao manifestDao;
        private readonly EmissaryDbContext dbContext;
        private readonly IUserDao userDao;
        private readonly ILoadoutDao loadoutDao;

        public event Action<ulong> RequestAuthorizationEvent;


        public Emissary(IConfiguration config, IBungieApiService bungieApiService, IManifestDao manifestAccessor, EmissaryDbContext dbContext, IUserDao userDao, ILoadoutDao loadoutDao)
        {
            this.config = config;
            this.bungieApiService = bungieApiService;
            this.manifestDao = manifestAccessor;
            this.dbContext = dbContext;
            this.userDao = userDao;
            this.loadoutDao = loadoutDao;
        }

        public static Emissary Startup(IConfiguration config)
        {
            IBungieApiService bungieApiService = new BungieApiService(config, new HttpClient());
            IManifestDao manifestDao = new ManifestDao(config, new DatabaseAccessor());
            SqliteConnection sqliteConnection = new SqliteConnection($"DataSource={config["Emissary:DatabasePath"]}");
            sqliteConnection.Open();
            DbContextOptions<EmissaryDbContext> dbContextOptions = new DbContextOptionsBuilder<EmissaryDbContext>()
                .UseSqlite(sqliteConnection)
                .Options;
            EmissaryDbContext dbContext = new EmissaryDbContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            IUserDao userDao = new UserDao(dbContext);
            ILoadoutDao loadoutDao = new LoadoutDao(dbContext);
            return new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
        }

        public EmissaryResult CurrentlyEquipped(ulong discordId)
        {
            EmissaryUser user = userDao.GetUserByDiscordId(discordId);
            if (user == null) {
                RequestAuthorizationEvent?.Invoke(discordId);
                return EmissaryResult.FromError("i need access to your bungie account to do this. please check your DMs for instructions");
            }
            long destinyCharacterId = GetMostRecentlyPlayedCharacterId(user.DestinyMembershipType, user.DestinyProfileId);
            CharacterEquipmentRequest equipmentRequest = new CharacterEquipmentRequest(user.DestinyMembershipType, user.DestinyProfileId, destinyCharacterId);
            CharacterEquipmentResponse equipmentResponse = bungieApiService.GetCharacterEquipment(equipmentRequest);
            Loadout currentlyEquipped = new Loadout();
            currentlyEquipped.DiscordId = user.DiscordId;
            currentlyEquipped.DestinyCharacterId = destinyCharacterId;
            currentlyEquipped.LoadoutName = "currently equipped";
            currentlyEquipped.Items = equipmentResponse.Items
                    .Select(genericItem => CreateDestinyItemFromGenericItem(genericItem))
                    .Where(item => ItemIsWeaponOrArmor(item))
                    .ToList();
            return EmissaryResult.FromSuccess(JsonConvert.SerializeObject(currentlyEquipped));
        }

        private bool ItemIsWeaponOrArmor(DestinyItem item)
        {
            return item.Categories.Contains("Weapon") || item.Categories.Contains("Armor");
        }

        private DestinyItem CreateDestinyItemFromGenericItem(DestinyGenericItem genericItem)
        {
            DestinyItem item = new DestinyItem();
            item.ItemHash = genericItem.ItemHash;
            item.ItemInstanceId = genericItem.ItemInstanceId;
            ManifestItemDefinition manifestItemDefinition = manifestDao.GetItemDefinition(genericItem.ItemHash);
            item.Name = manifestItemDefinition.DisplayName;
            item.CategoryHashes = manifestItemDefinition.ItemCategoryHashes;
            item.Categories = item.CategoryHashes
                    .Select(hash => manifestDao.GetItemCategoryDefinition(hash).CategoryName)
                    .ToList();
            return item;
        }

        public EmissaryResult ListLoadouts(ulong discordId)
        {
            EmissaryUser user = userDao.GetUserByDiscordId(discordId);
            if (user == null) {
                RequestAuthorizationEvent?.Invoke(discordId);
                return EmissaryResult.FromError("i need access to your bungie account to do this. please check your DMs for instructions");
            }
            long destinyCharacterId = GetMostRecentlyPlayedCharacterId(user.DestinyMembershipType, user.DestinyProfileId);
            IList<Loadout> loadouts = loadoutDao.GetAllLoadoutsForUser(discordId).Where(l => l.DestinyCharacterId == destinyCharacterId).ToList();
            return EmissaryResult.FromSuccess(JsonConvert.SerializeObject(loadouts));
        }

        public EmissaryResult EquipLoadout(ulong discordId, string loadoutName)
        {
            EmissaryUser user = userDao.GetUserByDiscordId(discordId);
            if (user == null) {
                RequestAuthorizationEvent?.Invoke(discordId);
                return EmissaryResult.FromError("i need access to your bungie account to do this. please check your DMs for instructions");
            }
            long destinyCharacterId = GetMostRecentlyPlayedCharacterId(user.DestinyMembershipType, user.DestinyProfileId);
            Loadout loadout = loadoutDao.GetLoadout(discordId, destinyCharacterId, loadoutName);
            IList<long> itemInstanceIds = loadout.Items.Select(item => item.ItemInstanceId).ToList();
            EquipItemsRequest equipRequest = new EquipItemsRequest(user.BungieAccessToken, user.DestinyMembershipType, destinyCharacterId, itemInstanceIds);
            EquipItemsResponse equipResponse;
            try {
                equipResponse = bungieApiService.EquipItems(equipRequest);
            } catch (BungieApiException e) {
                return EmissaryResult.FromError(e.Message);
            }
            EmissaryResult result;
            if (equipResponse.EquipResults.All(equipResult => equipResult.EquipStatus == BungiePlatformErrorCodes.Success)) {
                result = EmissaryResult.FromSuccess(JsonConvert.SerializeObject(loadout));
            } else {
                result = EmissaryResult.FromError("some items could not be equipped. TODO use error codes to further explain");
            }
            return result;
        }

        // TODO finalize and test this
        public EmissaryResult SaveCurrentlyEquippedAsLoadout(ulong discordId, string loadoutName)
        {
            Loadout loadoutToSave = JsonConvert.DeserializeObject<Loadout>(CurrentlyEquipped(discordId).Message);
            return SaveLoadout(discordId, loadoutToSave, loadoutName);
        }

        // TODO should this be private? idk
        public EmissaryResult SaveLoadout(ulong discordId, Loadout loadout, string loadoutName)
        {
            loadout.DiscordId = discordId;
            loadout.LoadoutName = loadoutName;
            try {
                loadoutDao.AddOrUpdateLoadout(loadout);
                return EmissaryResult.FromSuccess(JsonConvert.SerializeObject(loadout));
            } catch (Exception e) {
                return EmissaryResult.FromError(e.Message);
            }
        }

        public EmissaryResult DeleteLoadout(ulong discordId, string loadoutName)
        {
            throw new NotImplementedException();
        }

        public EmissaryResult RegisterOrReauthorize(ulong discordId, string authCode)
        {
            EmissaryUser existingUser = userDao.GetUserByDiscordId(discordId);
            if (existingUser != null) {
                OAuthRequest refreshOAuthRequest = new OAuthRequest(authCode);
                OAuthResponse refreshOAuthResponse = bungieApiService.GetOAuthAccessToken(refreshOAuthRequest);
                if (string.IsNullOrWhiteSpace(refreshOAuthResponse.AccessToken)) {
                    return EmissaryResult.FromError(refreshOAuthResponse.ErrorMessage);
                }
                existingUser.BungieAccessToken = refreshOAuthResponse.AccessToken;
                userDao.AddOrUpdateUser(existingUser);
                return EmissaryResult.FromSuccess("successfully reauthorized user");
            }
            EmissaryUser newUser = new EmissaryUser();
            newUser.DiscordId = discordId;
            OAuthRequest oauthRequest = new OAuthRequest(authCode);
            OAuthResponse oauthResponse = bungieApiService.GetOAuthAccessToken(oauthRequest);
            if (string.IsNullOrWhiteSpace(oauthResponse.AccessToken)) {
                return EmissaryResult.FromError(oauthResponse.ErrorMessage);
            }
            newUser.BungieAccessToken = oauthResponse.AccessToken;
            UserMembershipsRequest membershipsRequest = new UserMembershipsRequest(oauthResponse.AccessToken);
            UserMembershipsResponse membershipsResponse = bungieApiService.GetMembershipsForUser(membershipsRequest);

            DestinyMembership mainMembership = membershipsResponse.DestinyMemberships[0];
            if (membershipsResponse.DestinyMemberships.Count > 1) {
                int crossSaveOverrideType = membershipsResponse.DestinyMemberships[0].CrossSaveOverride;
                mainMembership = membershipsResponse.DestinyMemberships.Where(membership => membership.MembershipType == crossSaveOverrideType).FirstOrDefault();
            }
            newUser.DestinyMembershipType = mainMembership.MembershipType;
            newUser.DestinyProfileId = mainMembership.DestinyProfileId;

            userDao.AddOrUpdateUser(newUser);
            return EmissaryResult.FromSuccess("");
        }

        private long GetMostRecentlyPlayedCharacterId(int destinyMembershipType, long destinyProfileId)
        {
            ProfileCharactersRequest charactersRequest = new ProfileCharactersRequest(destinyMembershipType, destinyProfileId);
            ProfileCharactersResponse charactersResponse = bungieApiService.GetProfileCharacters(charactersRequest);
            DestinyCharacter mostRecentlyPlayedCharacter = charactersResponse.Characters.Values.Aggregate((c1, c2) => c1.DateLastPlayed > c2.DateLastPlayed ? c1 : c2);
            long mostRecentlyPlayedCharacterId = mostRecentlyPlayedCharacter.CharacterId;
            return mostRecentlyPlayedCharacterId;
        }

    }
}
