using System.ComponentModel.DataAnnotations;

namespace EmissaryCore
{
    public class EmissaryUser
    {
        [Key]
        public ulong DiscordId { get; set; }
        public long DestinyProfileId { get; set; }
        public int DestinyMembershipType { get; set; }
        public string BungieAccessToken { get; set; }

        public EmissaryUser(ulong discordId, long destinyProfileId, int destinyMembershipType, string bungieAccessToken)
        {
            this.DiscordId = discordId;
            this.DestinyProfileId = destinyProfileId;
            this.DestinyMembershipType = destinyMembershipType;
            this.BungieAccessToken = bungieAccessToken;
        }

        public EmissaryUser()
        {
        }
    }
}