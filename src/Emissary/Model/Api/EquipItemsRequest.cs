using System;
using System.Net.Http;

namespace Emissary
{
    public class EquipItemsRequest
    {
        public string AccessToken { get; set; }
        public int MembershipType { get; set; }
        public long DestinyCharacterId { get; set; }
        public long[] ItemInstanceIds { get; set; }

        public EquipItemsRequest(string accessToken, int membershipType, long destinyCharacterId)
        {
            this.AccessToken = accessToken;
            this.MembershipType = membershipType;
            this.DestinyCharacterId = destinyCharacterId;
        }

        public HttpRequestMessage ToHttpRequest()
        {
            throw new NotImplementedException();
        }
    }
}