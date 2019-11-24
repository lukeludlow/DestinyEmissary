using System;

namespace EmissaryApi
{
    /// <summary>
    ///     
	/// https://bungie-net.github.io/multi/operation_get_Destiny2-GetProfile.html#operation_get_Destiny2-GetProfile
	/// </summary>
    public class DestinyProfileRequest
    {

        public Int64 destinyMembershipId;
        public int membershipType;

        public DestinyProfileRequest()
        {
        }

        public string BuildRequestURL()
        {
            throw new NotImplementedException();
        }

    }
}