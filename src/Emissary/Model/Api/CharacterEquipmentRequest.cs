using System;
using System.Net.Http;

namespace EmissaryCore
{
    public class CharacterEquipmentRequest
    {
        public int DestinyMembershipType { get; set; }
        public long DestinyProfileId { get; set; }
        public long DestinyCharacterId { get; set; }

        public CharacterEquipmentRequest(int destinyMembershipType, long destinyProfileId, long destinyCharacterId)
        {
            this.DestinyMembershipType = destinyMembershipType;
            this.DestinyProfileId = destinyProfileId;
            this.DestinyCharacterId = destinyCharacterId;
        }
    }
}