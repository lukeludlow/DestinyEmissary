using System;
using Newtonsoft.Json;

namespace Emissary.Model
{
    /// <summary>
    /// mobile manifest entity!
    /// https://bungie-net.github.io/multi/schema_Destiny-Definitions-DestinyInventoryItemDefinition.html#schema_Destiny-Definitions-DestinyInventoryItemDefinition
    /// </summary>
    public class DestinyItem
    {
        [JsonProperty("displayProperties")]
        public DestinyDisplayPropertiesDefinition DisplayProperties { get; set; }

        // hash maps to DestinyItemCategoryDefinition 
        // https://bungie-net.github.io/multi/schema_Destiny-Definitions-DestinyItemCategoryDefinition.html#schema_Destiny-Definitions-DestinyItemCategoryDefinition
        [JsonProperty("itemCategoryHashes")]
        public uint[] ItemCategoryHashes { get; set; }

        public DestinyItem(DestinyDisplayPropertiesDefinition displayProperties, uint[] itemCategoryHashes)
        {
            this.DisplayProperties = displayProperties;
            this.ItemCategoryHashes = itemCategoryHashes;
        }
    }
}