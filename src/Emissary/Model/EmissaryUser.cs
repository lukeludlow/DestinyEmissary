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
    }
}