using System;
using Newtonsoft.Json;
using EmissaryApi.Model;
using System.Collections.Generic;
using System.Linq;

namespace EmissaryApi
{
    public class Emissary
    {

        private IBungieApiService bungieApiService;
        private Manifest manifest;

        public Emissary(IBungieApiService bungieApiProxy, Manifest manifest)
        {
            this.bungieApiService = bungieApiProxy;
            this.manifest = manifest;
        }

        public Loadout GetCurrentlyEquipped(long membershipId)
        {
            Loadout currentlyEquipped = new Loadout();
            long characterId = GetMostRecentlyPlayedCharacter(membershipId);
            List<UInt32> itemHashes = GetCharacterEquipmentAsItemHashes(membershipId, characterId);

            foreach (UInt32 itemHash in itemHashes) {
                string json = manifest.GetDestinyInventoryItemDefinition(itemHash);
                DestinyInventoryItem item = JsonConvert.DeserializeObject<DestinyInventoryItem>(json);
                if (ItemIsKineticWeapon(item)) {
                    currentlyEquipped.KineticWeapon = new Weapon(item.DisplayProperties.Name, "Kinetic Weapon");
                }
            }

            return currentlyEquipped;
        }

        private bool ItemIsKineticWeapon(DestinyInventoryItem item)
        {
            bool itemIsWeapon = false;
            foreach (UInt32 itemCategoryHash in item.ItemCategoryHashes) {
                string json = manifest.GetDestinyItemCategoryDefinition(itemCategoryHash);
                DestinyItemCategory itemCategory = JsonConvert.DeserializeObject<DestinyItemCategory>(json);
                if (itemCategory.DisplayProperties.Name == "Kinetic Weapon") {
                    itemIsWeapon = true;
                    break;
                }
            } 
            return itemIsWeapon;
        }


        public long GetMostRecentlyPlayedCharacter(long membershipId)
        {
            // get-characters-personal.json
            string requestUrl = $"https://www.bungie.net/Platform/Destiny2/3/Profile/{membershipId}/?components=200";
            string json = bungieApiService.Get(requestUrl);
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

        public List<UInt32> GetCharacterEquipmentAsItemHashes(long membershipId, long characterId)
        {        
            // get-character-equipment.json
            string requestUrl = $"https://www.bungie.net/Platform/Destiny2/3/Profile/{membershipId}/?components=205";
            string json = bungieApiService.Get(requestUrl);
            DestinyProfileCharacterEquipmentResponse response = JsonConvert.DeserializeObject<DestinyProfileCharacterEquipmentResponse>(json);
            DestinyInventoryComponent characterInventory = response.Response.CharacterEquipment.Data[characterId];
            List<UInt32> itemHashes = new List<UInt32>();
            foreach (DestinyItemComponent item in characterInventory.Items) {
                itemHashes.Add(item.ItemHash);
            }
            return itemHashes;
        }

        public List<string> GetCharacterEquipmentNames(long membershipId, long characterId)
        {
            List<UInt32> itemHashes = GetCharacterEquipmentAsItemHashes(membershipId, characterId);
            List<string> itemNames = new List<string>();
            foreach (UInt32 itemHash in itemHashes) {
                string json = manifest.GetDestinyInventoryItemDefinition(itemHash);
                DestinyInventoryItem item = JsonConvert.DeserializeObject<DestinyInventoryItem>(json);
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


        public bool TrySearchDestinyPlayer(string displayName, out long membershipId)
        {
            // search-destiny-player.json
            int membershipType = BungieMembershipType.All;
            string requestUrl = $"https://www.bungie.net/platform/Destiny2/SearchDestinyPlayer/{membershipType}/{displayName}/";
            string json = bungieApiService.Get(requestUrl);
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
