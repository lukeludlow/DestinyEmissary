using System;
using Newtonsoft.Json;
using EmissaryCore.Common;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace EmissaryCore
{
    // note: discord IDs are UInt64 (ulong). bungie IDs are Int64 (long).
    public class Emissary : IEmissary
    {
        private readonly IBungieApiService bungieApiService;
        private readonly IManifestDao manifestAccessor;
        private readonly EmissaryDbContext dbContext;
        private readonly IUserDao userDao;
        private readonly ILoadoutDao loadoutDao;

        public Emissary(IBungieApiService bungieApiService, IManifestDao manifestAccessor, EmissaryDbContext dbContext, IUserDao userDao, ILoadoutDao loadoutDao)
        {
            this.bungieApiService = bungieApiService;
            this.manifestAccessor = manifestAccessor;
            this.dbContext = dbContext;
            this.userDao = userDao;
            this.loadoutDao = loadoutDao;
        }

        public Loadout CurrentlyEquipped(ulong discordId)
        {
            EmissaryUser user = userDao.GetUserByDiscordId(discordId);
            long destinyCharacterId = GetMostRecentlyPlayedCharacterId(user.DestinyMembershipType, user.DestinyProfileId);
            CharacterEquipmentRequest equipmentRequest = new CharacterEquipmentRequest(user.DestinyMembershipType, user.DestinyProfileId, destinyCharacterId);
            CharacterEquipmentResponse equipmentResponse = bungieApiService.GetCharacterEquipment(equipmentRequest);
            Loadout currentlyEquipped = new Loadout();
            currentlyEquipped.DiscordId = user.DiscordId;
            currentlyEquipped.DestinyCharacterId = destinyCharacterId;
            currentlyEquipped.LoadoutName = "currently equipped";
            List<DestinyItem> items = new List<DestinyItem>();
            foreach (DestinyGenericItem genericItem in equipmentResponse.Items) {
                DestinyItem item = new DestinyItem();
                item.ItemHash = genericItem.ItemHash;
                item.ItemInstanceId = genericItem.ItemInstanceId;
                ManifestItemDefinition manifestItemDefinition = manifestAccessor.GetItemDefinition(genericItem.ItemHash);
                item.Name = manifestItemDefinition.DisplayName;
                item.CategoryHashes = manifestItemDefinition.ItemCategoryHashes;
                item.Categories = item.CategoryHashes.Select(hash => manifestAccessor.GetItemCategoryDefinition(hash).CategoryName).ToList();
                items.Add(item);
            }
            currentlyEquipped.Items = items.ToArray();
            return currentlyEquipped;
        }

        public EmissaryResult ListLoadouts(ulong discordId)
        {
            EmissaryUser user = userDao.GetUserByDiscordId(discordId);
            if (user == null) {
                return EmissaryResult.FromError("user not found. please register with destiny emissary to use this service. TODO registration instructions");
            }
            long destinyCharacterId = GetMostRecentlyPlayedCharacterId(user.DestinyMembershipType, user.DestinyProfileId);
            IList<Loadout> loadouts = loadoutDao.GetAllLoadoutsForUser(discordId).Where(l => l.DestinyCharacterId == destinyCharacterId).ToList();
            string loadoutsMessage = JsonConvert.SerializeObject(loadouts);
            return EmissaryResult.FromSuccess(loadoutsMessage);
        }

        public EmissaryResult EquipLoadout(ulong discordId, string loadoutName)
        {
            EmissaryUser user = userDao.GetUserByDiscordId(discordId);
            if (user == null) {
                return EmissaryResult.FromError("user not found. please register with destiny emissary to use this service. TODO registration instructions");
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
                result = EmissaryResult.FromSuccess("");
            } else {
                result = EmissaryResult.FromError("some items could not be equipped. TODO use error codes to further explain");
            }
            return result;
        }

        public EmissaryResult SaveLoadout(ulong discordId, Loadout loadout, string loadoutName)
        {
            loadout.DiscordId = discordId;
            loadout.LoadoutName = loadoutName;
            try {
                loadoutDao.AddOrUpdateLoadout(loadout);
                return EmissaryResult.FromSuccess("");
            } catch (Exception e) {
                return EmissaryResult.FromError(e.Message);
            }
        }

        public string DeleteLoadout(ulong discordId, string loadoutName)
        {
            throw new NotImplementedException();
        }

        public EmissaryResult RegisterOrReauthorize(ulong discordId, string authCode)
        {
            EmissaryUser existingUser = userDao.GetUserByDiscordId(discordId);
            if (existingUser != null) {
                OAuthRequest refreshOAuthRequest = new OAuthRequest(authCode, GetBungieClientId(), GetBungieClientSecret());
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
            OAuthRequest oauthRequest = new OAuthRequest(authCode, GetBungieClientId(), GetBungieClientSecret());
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

        private string GetBungieClientId()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            string dataDirectory = Path.Combine(solutionDirectory, "data");
            string secretsFileName = "secrets.json";
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(dataDirectory)
                .AddJsonFile(secretsFileName);
            IConfiguration config = configBuilder.Build();
            string bungieClientId = config["BungieClientId"];
            return bungieClientId;
        }

        private string GetBungieClientSecret()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            string dataDirectory = Path.Combine(solutionDirectory, "data");
            string secretsFileName = "secrets.json";
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(dataDirectory)
                .AddJsonFile(secretsFileName);
            IConfiguration config = configBuilder.Build();
            string bungieClientSecret = config["BungieClientSecret"];
            return bungieClientSecret;
        }



        // private Loadout GetCurrentlyEquipped(long membershipId)
        // {
        //     Loadout currentlyEquipped = null;
        //     long characterId = GetMostRecentlyPlayedCharacter(membershipId);
        //     List<uint> itemHashes = GetCharacterEquipmentAsItemHashes(membershipId, characterId);

        //     foreach (uint itemHash in itemHashes) {
        //         DestinyItem item = manifestAccessor.LookupItem(itemHash);
        //         if (ItemIsKineticWeapon(item)) {
        //             // currentlyEquipped.KineticWeapon = new Weapon(item.DisplayProperties.Name, "Kinetic Weapon");
        //         }
        //     }

        //     return currentlyEquipped;
        // }

        // private bool ItemIsKineticWeapon(DestinyItem item)
        // {
        //     bool itemIsWeapon = false;
        //     foreach (uint itemCategoryHash in item.ItemCategoryHashes) {
        //         // string json = manifestAccessor.LookupItemCategory(itemCategoryHash);
        //         // DestinyItemCategory itemCategory = JsonConvert.DeserializeObject<DestinyItemCategory>(json);
        //         DestinyItemCategory itemCategory = manifestAccessor.LookupItemCategory(itemCategoryHash);
        //         if (itemCategory.DisplayProperties.Name == "Kinetic Weapon") {
        //             itemIsWeapon = true;
        //             break;
        //         }
        //     }
        //     return itemIsWeapon;
        // }


        // private long GetMostRecentlyPlayedCharacter(long membershipId)
        // {
        //     // get-characters-personal.json
        //     string requestUrl = $"https://www.bungie.net/Platform/Destiny2/3/Profile/{membershipId}/?components=200";
        //     // string json = bungieApiService.Get(requestUrl);
        //     string json = "";
        //     DestinyProfileCharactersResponse response = JsonConvert.DeserializeObject<DestinyProfileCharactersResponse>(json);
        //     List<DestinyCharacterComponent> characters = response.Response.Characters.Data.Values.ToList();
        //     DestinyCharacterComponent mostRecentlyPlayedCharacter = characters[0];
        //     foreach (DestinyCharacterComponent character in response.Response.Characters.Data.Values) {
        //         if (IsMoreRecentTime(character.DateLastPlayed, mostRecentlyPlayedCharacter.DateLastPlayed)) {
        //             mostRecentlyPlayedCharacter = character;
        //         }
        //     }
        //     return mostRecentlyPlayedCharacter.CharacterId;
        // }

        // private List<uint> GetCharacterEquipmentAsItemHashes(long membershipId, long characterId)
        // {
        //     // get-character-equipment.json
        //     string requestUrl = $"https://www.bungie.net/Platform/Destiny2/3/Profile/{membershipId}/?components=205";
        //     // string json = bungieApiService.Get(requestUrl);
        //     string json = "";
        //     DestinyProfileCharacterEquipmentResponse response = JsonConvert.DeserializeObject<DestinyProfileCharacterEquipmentResponse>(json);
        //     DestinyInventoryComponent characterInventory = response.Response.CharacterEquipment.Data[characterId];
        //     List<uint> itemHashes = new List<uint>();
        //     foreach (DestinyItemComponent item in characterInventory.Items) {
        //         itemHashes.Add(item.ItemHash);
        //     }
        //     return itemHashes;
        // }

        // private List<string> GetCharacterEquipmentNames(long membershipId, long characterId)
        // {
        //     List<uint> itemHashes = GetCharacterEquipmentAsItemHashes(membershipId, characterId);
        //     List<string> itemNames = new List<string>();
        //     foreach (uint itemHash in itemHashes) {
        //         // string json = manifestAccessor.LookupItem(itemHash);
        //         // DestinyItem item = JsonConvert.DeserializeObject<DestinyItem>(json);
        //         DestinyItem item = manifestAccessor.LookupItem(itemHash);
        //         itemNames.Add(item.DisplayProperties.Name);
        //     }
        //     return itemNames;
        // }

        // private bool IsMoreRecentTime(DateTimeOffset first, DateTimeOffset second)
        // {
        //     // Return value	        Meaning
        //     // Less than zero	    first is earlier than second.
        //     // Zero	                first is equal to second.
        //     // Greater than zero	first is later than second.
        //     int compare = DateTimeOffset.Compare(first, second);
        //     return (compare > 0);
        // }


        // private bool TrySearchDestinyPlayer(string displayName, out long membershipId)
        // {
        //     // search-destiny-player.json
        //     int membershipType = BungieMembershipType.All;
        //     string requestUrl = $"https://www.bungie.net/platform/Destiny2/SearchDestinyPlayer/{membershipType}/{displayName}/";
        //     // string json = bungieApiService.Get(requestUrl);
        //     string json = "";
        //     SearchDestinyPlayerResponse response = JsonConvert.DeserializeObject<SearchDestinyPlayerResponse>(json);

        //     bool foundPlayer = false;
        //     membershipId = -1;
        //     foreach (UserInfoCard userInfo in response.Response) {
        //         if (userInfo.DisplayName == displayName) {
        //             foundPlayer = true;
        //             membershipId = userInfo.MembershipId;
        //             break;
        //         }
        //     }
        //     return foundPlayer;
        // }

    }
}
