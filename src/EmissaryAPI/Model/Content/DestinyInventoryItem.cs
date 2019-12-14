using System;
using Newtonsoft.Json;

namespace EmissaryApi.Model
{
    /// <summary>
    /// mobile manifest entity!
    /// https://bungie-net.github.io/multi/schema_Destiny-Definitions-DestinyInventoryItemDefinition.html#schema_Destiny-Definitions-DestinyInventoryItemDefinition
    /// </summary>
    public class DestinyInventoryItem
    {

        [JsonProperty("displayProperties")]
        public DestinyDisplayPropertiesDefinition DisplayProperties { get; set; }

        [JsonProperty("itemTypeDisplayName")]
        public string ItemTypeDisplayName { get; set; }

        // hash maps to DestinyItemCategoryDefinition 
        // https://bungie-net.github.io/multi/schema_Destiny-Definitions-DestinyItemCategoryDefinition.html#schema_Destiny-Definitions-DestinyItemCategoryDefinition
        [JsonProperty("itemCategoryHashes")]
        public UInt32[] ItemCategoryHashes { get; set; }



    }

    /// <summary>
	/// https://bungie-net.github.io/multi/schema_Destiny-Definitions-Common-DestinyDisplayPropertiesDefinition.html#schema_Destiny-Definitions-Common-DestinyDisplayPropertiesDefinition
	/// </summary>
    public class DestinyDisplayPropertiesDefinition
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }


}