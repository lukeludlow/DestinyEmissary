using Newtonsoft.Json;

namespace EmissaryApi
{
    public class DestinyItemCategory
    {
        [JsonProperty("displayProperties")]
        public DestinyDisplayPropertiesDefinition DisplayProperties { get; set; }
    }
}