using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace EmissaryCore
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class CharacterEquipmentResponse
    {
        [JsonProperty("Response.equipment.data.items")]
        public IList<DestinyGenericItem> Items { get; set; }

        public CharacterEquipmentResponse(DestinyGenericItem[] items)
        {
            this.Items = items;
        }

        public CharacterEquipmentResponse()
        {
            this.Items = new DestinyGenericItem[] { };
        }
    }
}