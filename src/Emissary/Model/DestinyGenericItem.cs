using Newtonsoft.Json;

namespace EmissaryCore
{
    public class DestinyGenericItem
    {
        [JsonProperty("itemHash")]
        public uint ItemHash { get; set; }
        [JsonProperty("itemInstanceId")]
        public long ItemInstanceId { get; set; }
    }
}