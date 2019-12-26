using System;
using System.Net.Http;

namespace EmissaryCore
{
    public class ProfileCharactersRequest
    {
        public int MembershipType { get; set; }
        public long DestinyMembershipId { get; set; }

        public ProfileCharactersRequest(int membershipType, long destinyMembershipId)
        {
            this.MembershipType = membershipType;
            this.DestinyMembershipId = destinyMembershipId;
        }
    }
}