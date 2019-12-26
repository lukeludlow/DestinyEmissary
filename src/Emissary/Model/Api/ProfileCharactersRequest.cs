using System;
using System.Net.Http;

namespace EmissaryCore
{
    public class ProfileCharactersRequest
    {
        public int MembershipType { get; set; }
        public long DestinyProfileId { get; set; }

        public ProfileCharactersRequest(int membershipType, long destinyProfileId)
        {
            this.MembershipType = membershipType;
            this.DestinyProfileId = destinyProfileId;
        }
    }
}