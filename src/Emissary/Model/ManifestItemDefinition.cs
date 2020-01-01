using System.Collections.Generic;
using Newtonsoft.Json;

namespace EmissaryCore
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class ManifestItemDefinition
    {
        [JsonProperty("displayProperties.name")]
        public string DisplayName { get; set; }
        [JsonProperty("inventory.tierTypeName")]
        public string TierTypeName { get; set; }
        [JsonProperty("itemCategoryHashes")]
        public IList<uint> ItemCategoryHashes { get; set; }

        public ManifestItemDefinition(string displayName, string tierTypeName, IList<uint> itemCategoryHashes)
        {
            this.DisplayName = displayName;
            this.TierTypeName = tierTypeName;
            this.ItemCategoryHashes = itemCategoryHashes;
        }

        public ManifestItemDefinition()
        {
            this.DisplayName = "";
            this.TierTypeName = "";
            this.ItemCategoryHashes = new List<uint>();
        }
    }
}