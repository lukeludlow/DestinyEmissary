using Newtonsoft.Json;

namespace Emissary.Model
{
    /// <summary>
    /// https://bungie-net.github.io/multi/schema_Destiny-Definitions-Common-DestinyDisplayPropertiesDefinition.html#schema_Destiny-Definitions-Common-DestinyDisplayPropertiesDefinition
    /// </summary>
    public class DestinyDisplayPropertiesDefinition
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}