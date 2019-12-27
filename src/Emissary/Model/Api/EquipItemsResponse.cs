using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace EmissaryCore
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class EquipItemsResponse
    {
        [JsonProperty("Response.equipResults")]
        public IList<EquipItemResult> EquipResults { get; set; }

        public EquipItemsResponse(IList<EquipItemResult> equipResults) 
        {
            this.EquipResults = equipResults;
        }

        public EquipItemsResponse()
        {
            this.EquipResults = new List<EquipItemResult>();
        }
    }

    public class EquipItemResult
    {
        [JsonProperty("itemInstanceId")]
        public long ItemInstanceId { get; set; }
        [JsonProperty("equipStatus")]
        public int EquipStatus { get; set; }

        public EquipItemResult(long itemInstanceId, int equipStatus)
        {
            this.ItemInstanceId = itemInstanceId;
            this.EquipStatus = equipStatus;
        }

        public EquipItemResult()
        {
        }
    }
}