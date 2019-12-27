using Newtonsoft.Json;

namespace EmissaryCore
{
    public class DestinyMembership
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("membershipId")]
        public long DestinyProfileId { get; set; }
        [JsonProperty("membershipType")]
        public int MembershipType { get; set; }
        [JsonProperty("crossSaveOverride")]
        public int CrossSaveOverride { get; set; }

        public DestinyMembership(string displayName, long destinyProfileId, int membershipType, int crossSaveOverride)
        {
            this.DisplayName = displayName;
            this.DestinyProfileId = destinyProfileId;
            this.MembershipType = membershipType;
            this.CrossSaveOverride = crossSaveOverride;
        }

        public DestinyMembership()
        {
        }
    }
}