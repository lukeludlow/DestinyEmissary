using Newtonsoft.Json;

namespace Emissary
{
    public class DestinyItemCategory
    {
        [JsonProperty("displayProperties")]
        public DestinyDisplayPropertiesDefinition DisplayProperties { get; set; }
    }
}