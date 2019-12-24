using System;
using System.Net.Http;

namespace Emissary
{
    public class CharacterEquipmentRequest
    {
        public int MembershipType { get; set; }
        public long DestinyMembershipId { get; set; }
        public long DestinyCharacterId { get; set; }

        public CharacterEquipmentRequest(int membershipType, long destinyMembershipId, long destinyCharacterId)
        {
            this.MembershipType = membershipType;
            this.DestinyMembershipId = destinyMembershipId;
            this.DestinyCharacterId = destinyCharacterId;
        }

        public HttpRequestMessage ToHttpRequest()
        {
            throw new NotImplementedException();
        }
    }
}