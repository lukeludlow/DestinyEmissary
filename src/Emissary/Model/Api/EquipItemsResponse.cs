using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace EmissaryCore
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class EquipItemsResponse
    {
        [JsonProperty("Response.equipResults")]
        public EquipItemResult[] EquipResults { get; set; }
    }

    public class EquipItemResult
    {
        [JsonProperty("itemInstanceId")]
        public long ItemInstanceId { get; set; }
        [JsonProperty("equipStatus")]
        public int EquipStatus { get; set; }
    }
}