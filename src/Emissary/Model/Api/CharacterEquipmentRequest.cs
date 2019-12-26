using System;
using System.Net.Http;

namespace EmissaryCore
{
    public class CharacterEquipmentRequest
    {
        public int MembershipType { get; set; }
        public long MembershipId { get; set; }
        public long DestinyCharacterId { get; set; }

        public CharacterEquipmentRequest(int membershipType, long membershipId, long destinyCharacterId)
        {
            this.MembershipType = membershipType;
            this.MembershipId = membershipId;
            this.DestinyCharacterId = destinyCharacterId;
        }
    }
}