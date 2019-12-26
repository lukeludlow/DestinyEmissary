using System;
using Newtonsoft.Json;

namespace EmissaryCore
{
    public class DestinyCharacter
    {
        [JsonProperty("characterId")]
        public long CharacterId { get; set; }
        [JsonProperty("dateLastPlayed")]
        public DateTimeOffset DateLastPlayed { get; set; }
        [JsonProperty("membershipId")]
        public long DestinyProfileId { get; set; }
        [JsonProperty("membershipType")]
        public int DestinyMembershipType { get; set; }

        public DestinyCharacter(long characterId, DateTimeOffset dateLastPlayed, long destinyProfileId, int destinyMembershipType)
        {
            this.CharacterId = characterId;
            this.DateLastPlayed = dateLastPlayed;
            this.DestinyProfileId = destinyProfileId;
            this.DestinyMembershipType = destinyMembershipType;
        }

        public DestinyCharacter()
        {
        }
    }
}