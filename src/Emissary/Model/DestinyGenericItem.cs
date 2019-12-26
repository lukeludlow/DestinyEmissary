using Newtonsoft.Json;

namespace Emissary
{
    public class DestinyGenericItem
    {
        [JsonProperty("itemHash")]
        public uint ItemHash { get; set; }
        [JsonProperty("itemInstanceId")]
        public long ItemInstanceId { get; set; }
    }
}