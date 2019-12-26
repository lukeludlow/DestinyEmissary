using Newtonsoft.Json;

namespace EmissaryCore
{
    public class DestinyMembership
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("membershipId")]
        public long DestinyMembershipId { get; set; }
        [JsonProperty("membershipType")]
        public int MembershipType { get; set; }
        [JsonProperty("crossSaveOverride")]
        public int CrossSaveOverride { get; set; }
    }
}