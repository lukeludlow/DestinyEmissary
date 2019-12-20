using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Emissary
{
    /// <summary>
    /// https://bungie-net.github.io/multi/schema_Destiny-Responses-DestinyProfileResponse.html#schema_Destiny-Responses-DestinyProfileResponse
    /// 'characterEquipment' object property
    /// The character's equipped items, keyed by the Character's Id.
    /// COMPONENT TYPE: CharacterEquipment
    /// </summary>
    public class DestinyProfileCharacterEquipmentResponse
    {

        [JsonProperty("Response")]
        public CharacterEquipmentResponse Response { get; set; }

        [JsonProperty("ErrorCode")]
        public long ErrorCode { get; set; }

        [JsonProperty("ErrorStatus")]
        public string ErrorStatus { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

    }

    public class CharacterEquipmentResponse
    {
        [JsonProperty("characterEquipment")]
        public CharacterEquipment CharacterEquipment { get; set; }
    }

    public class CharacterEquipment
    {
        [JsonProperty("data")]
        public Dictionary<long, DestinyInventoryComponent> Data { get; set; }
    }

    public class DestinyInventoryComponent
    {
        [JsonProperty("items")]
        public DestinyItemComponent[] Items { get; set; }
    }

    /// <summary>
    /// https://bungie-net.github.io/multi/schema_Destiny-Entities-Items-DestinyItemComponent.html#schema_Destiny-Entities-Items-DestinyItemComponent
    /// </summary>
    public class DestinyItemComponent
    {
        /// <summary>
        /// Mapped to Manifest Database Definition: Destiny.Definitions.DestinyInventoryItemDefinition
        /// https://bungie-net.github.io/multi/schema_Destiny-Definitions-DestinyInventoryItemDefinition.html#schema_Destiny-Definitions-DestinyInventoryItemDefinition
        /// </summary>
        [JsonProperty("itemHash")]
        public UInt32 ItemHash { get; set; }

        [JsonProperty("itemInstanceId")]
        public long ItemInstanceId { get; set; }

        [JsonProperty("location")]
        public int Location { get; set; }

        [JsonProperty("bucketHash")]
        public UInt32 BucketHash { get; set; }

        [JsonProperty("transferStatus")]
        public long TransferStatus { get; set; }
    }



}