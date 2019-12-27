using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace EmissaryCore
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EquipItemsRequest
    {
        public string AccessToken { get; set; }
        [JsonProperty("membershipType")]
        public int MembershipType { get; set; }
        [JsonProperty("characterId")]
        public long DestinyCharacterId { get; set; }
        [JsonProperty("itemIds")]
        public IList<long> ItemInstanceIds { get; set; }

        public EquipItemsRequest(string accessToken, int membershipType, long destinyCharacterId, IList<long> itemInstanceIds)
        {
            this.AccessToken = accessToken;
            this.MembershipType = membershipType;
            this.DestinyCharacterId = destinyCharacterId;
            this.ItemInstanceIds = itemInstanceIds;
        }
    }
}