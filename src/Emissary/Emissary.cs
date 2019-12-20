using System;
using Newtonsoft.Json;
using Emissary.Common;
using System.Collections.Generic;
using System.Linq;

namespace Emissary
{
    // note: discord IDs are UInt64 (ulong). bungie IDs are Int64 (long).
    public class Emissary : IEmissary
    {
        private IBungieApiService bungieApiService;
        private IManifestDao manifestAccessor;

        public Loadout CurrentlyEquipped(ulong discordId)
        {
            throw new NotImplementedException();
        }

        public IList<Loadout> ListLoadouts(ulong discordId)
        {
            throw new NotImplementedException();
        }

        public string EquipLoadout(ulong discordId, string loadoutName)
        {
            throw new NotImplementedException();
        }

        public string SaveLoadout(ulong discordId, string loadoutName)
        {
            throw new NotImplementedException();
        }

        public string DeleteLoadout(ulong discordId, string loadoutName)
        {
            throw new NotImplementedException();
        }

        public string Register(ulong discordId, long bungieId)
        {
            throw new NotImplementedException();
        }


        public Emissary(IBungieApiService bungieApiService, IManifestDao manifestAccessor)
        {
            this.bungieApiService = bungieApiService;
            this.manifestAccessor = manifestAccessor;
        }

        private Loadout GetCurrentlyEquipped(long membershipId)
        {
            Loadout currentlyEquipped = null;
            long characterId = GetMostRecentlyPlayedCharacter(membershipId);
            List<uint> itemHashes = GetCharacterEquipmentAsItemHashes(membershipId, characterId);

            foreach (uint itemHash in itemHashes) {
                DestinyItem item = manifestAccessor.LookupItem(itemHash);
                if (ItemIsKineticWeapon(item)) {
                    // currentlyEquipped.KineticWeapon = new Weapon(item.DisplayProperties.Name, "Kinetic Weapon");
                }
            }

            return currentlyEquipped;
        }

        private bool ItemIsKineticWeapon(DestinyItem item)
        {
            bool itemIsWeapon = false;
            foreach (uint itemCategoryHash in item.ItemCategoryHashes) {
                // string json = manifestAccessor.LookupItemCategory(itemCategoryHash);
                // DestinyItemCategory itemCategory = JsonConvert.DeserializeObject<DestinyItemCategory>(json);
                DestinyItemCategory itemCategory = manifestAccessor.LookupItemCategory(itemCategoryHash);
                if (itemCategory.DisplayProperties.Name == "Kinetic Weapon") {
                    itemIsWeapon = true;
                    break;
                }
            }
            return itemIsWeapon;
        }


        private long GetMostRecentlyPlayedCharacter(long membershipId)
        {
            // get-characters-personal.json
            string requestUrl = $"https://www.bungie.net/Platform/Destiny2/3/Profile/{membershipId}/?components=200";
            // string json = bungieApiService.Get(requestUrl);
            string json = "";
            DestinyProfileCharactersResponse response = JsonConvert.DeserializeObject<DestinyProfileCharactersResponse>(json);
            List<DestinyCharacterComponent> characters = response.Response.Characters.Data.Values.ToList();
            DestinyCharacterComponent mostRecentlyPlayedCharacter = characters[0];
            foreach (DestinyCharacterComponent character in response.Response.Characters.Data.Values) {
                if (IsMoreRecentTime(character.DateLastPlayed, mostRecentlyPlayedCharacter.DateLastPlayed)) {
                    mostRecentlyPlayedCharacter = character;
                }
            }
            return mostRecentlyPlayedCharacter.CharacterId;
        }

        private List<uint> GetCharacterEquipmentAsItemHashes(long membershipId, long characterId)
        {
            // get-character-equipment.json
            string requestUrl = $"https://www.bungie.net/Platform/Destiny2/3/Profile/{membershipId}/?components=205";
            // string json = bungieApiService.Get(requestUrl);
            string json = "";
            DestinyProfileCharacterEquipmentResponse response = JsonConvert.DeserializeObject<DestinyProfileCharacterEquipmentResponse>(json);
            DestinyInventoryComponent characterInventory = response.Response.CharacterEquipment.Data[characterId];
            List<uint> itemHashes = new List<uint>();
            foreach (DestinyItemComponent item in characterInventory.Items) {
                itemHashes.Add(item.ItemHash);
            }
            return itemHashes;
        }

        private List<string> GetCharacterEquipmentNames(long membershipId, long characterId)
        {
            List<uint> itemHashes = GetCharacterEquipmentAsItemHashes(membershipId, characterId);
            List<string> itemNames = new List<string>();
            foreach (uint itemHash in itemHashes) {
                // string json = manifestAccessor.LookupItem(itemHash);
                // DestinyItem item = JsonConvert.DeserializeObject<DestinyItem>(json);
                DestinyItem item = manifestAccessor.LookupItem(itemHash);
                itemNames.Add(item.DisplayProperties.Name);
            }
            return itemNames;
        }

        private bool IsMoreRecentTime(DateTimeOffset first, DateTimeOffset second)
        {
            // Return value	        Meaning
            // Less than zero	    first is earlier than second.
            // Zero	                first is equal to second.
            // Greater than zero	first is later than second.
            int compare = DateTimeOffset.Compare(first, second);
            return (compare > 0);
        }


        private bool TrySearchDestinyPlayer(string displayName, out long membershipId)
        {
            // search-destiny-player.json
            int membershipType = BungieMembershipType.All;
            string requestUrl = $"https://www.bungie.net/platform/Destiny2/SearchDestinyPlayer/{membershipType}/{displayName}/";
            // string json = bungieApiService.Get(requestUrl);
            string json = "";
            SearchDestinyPlayerResponse response = JsonConvert.DeserializeObject<SearchDestinyPlayerResponse>(json);

            bool foundPlayer = false;
            membershipId = -1;
            foreach (UserInfoCard userInfo in response.Response) {
                if (userInfo.DisplayName == displayName) {
                    foundPlayer = true;
                    membershipId = userInfo.MembershipId;
                    break;
                }
            }
            return foundPlayer;
        }

    }
}
