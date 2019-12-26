using System.Collections.Generic;
using Newtonsoft.Json;

namespace EmissaryCore
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class ManifestItemDefinition
    {
        [JsonProperty("displayProperties.name")]
        public string DisplayName { get; set; }
        [JsonProperty("itemCategoryHashes")]
        public IList<uint> ItemCategoryHashes { get; set; }

        public ManifestItemDefinition(string displayName, IList<uint> itemCategoryHashes)
        {
            this.DisplayName = displayName;
            this.ItemCategoryHashes = itemCategoryHashes;
        }

        public ManifestItemDefinition()
        {
            this.DisplayName = "";
            this.ItemCategoryHashes = new List<uint>();
        }
    }
}