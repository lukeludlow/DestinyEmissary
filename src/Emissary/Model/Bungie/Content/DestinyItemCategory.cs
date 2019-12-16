using Newtonsoft.Json;

namespace Emissary.Model
{
    public class DestinyItemCategory
    {
        [JsonProperty("displayProperties")]
        public DestinyDisplayPropertiesDefinition DisplayProperties { get; set; }
    }
}