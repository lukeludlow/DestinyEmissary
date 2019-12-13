using System;
using System.Collections.Generic;

namespace EmissaryApi.Model
{
    /// <summary>
	/// https://bungie-net.github.io/multi/operation_get_Destiny2-GetProfile.html#operation_get_Destiny2-GetProfile
	/// </summary>
    public class DestinyProfileRequest
    {

        public int membershipType { get; set; }

        public long membershipId { get; set; }

        /// <summary>
        /// A comma separated list of components to return (as strings or numeric values). 
        /// See the DestinyComponentType enum for valid components to request. 
        /// You must request at least one component to receive results.
        /// </summary>
        public List<int> Components { get; set; }

        public DestinyProfileRequest()
        {
        }

        public string ToRequestUrl()
        {
            throw new NotImplementedException();
        }

    }
}