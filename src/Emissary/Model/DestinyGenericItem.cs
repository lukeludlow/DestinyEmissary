using Newtonsoft.Json;

namespace EmissaryCore
{
    public class DestinyGenericItem
    {
        [JsonProperty("itemHash")]
        public uint ItemHash { get; set; }
        [JsonProperty("itemInstanceId")]
        public long ItemInstanceId { get; set; }

        public DestinyGenericItem(uint itemHash, long instanceId)
        {
            this.ItemHash = itemHash;
            this.ItemInstanceId = instanceId;
        }

        public DestinyGenericItem()
        {
        }
    }
}