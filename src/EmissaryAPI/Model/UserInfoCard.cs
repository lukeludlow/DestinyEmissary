using Newtonsoft.Json;

namespace EmissaryApi.Model
{
    /// <summary>
	/// https://bungie-net.github.io/multi/schema_User-UserInfoCard.html#schema_User-UserInfoCard
	/// </summary>
    public class UserInfoCard
    {

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        
        [JsonProperty("membershipId")]
        public string MembershipId { get; set; }

        [JsonProperty("membershipType")]
        public long MembershipType { get; set; }

        [JsonProperty("iconPath")]
        public string IconPath { get; set; }

        [JsonProperty("crossSaveOverride")]
        public long CrossSaveOverride { get; set; }

        [JsonProperty("isPublic")]
        public bool IsPublic { get; set; }

        [JsonProperty("supplementalDisplayName")]
        public string SupplementalDisplayName { get; set; }

        [JsonProperty("applicableMembershipTypes")]
        public int[] ApplicableMembershipTypes { get; set; }

    }
}