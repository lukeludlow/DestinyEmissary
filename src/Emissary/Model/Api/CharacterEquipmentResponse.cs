using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace EmissaryCore
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class CharacterEquipmentResponse
    {
        [JsonProperty("Response.equipment.data.items")]
        public DestinyGenericItem[] Items { get; set; }
    }
}