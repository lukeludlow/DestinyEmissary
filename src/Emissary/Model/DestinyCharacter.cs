using System;
using Newtonsoft.Json;

namespace Emissary
{
    public class DestinyCharacter
    {
        [JsonProperty("characterId")]
        public long CharacterId { get; set; }
        [JsonProperty("dateLastPlayed")]
        public DateTimeOffset DateLastPlayed { get; set; }
        [JsonProperty("membershipId")]
        public long MembershipId { get; set; }
        [JsonProperty("membershipType")]
        public int MembershipType { get; set; }
    }
}