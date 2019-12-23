using System.ComponentModel.DataAnnotations;

namespace Emissary
{
    public class EmissaryUser
    {
        [Key]
        public ulong DiscordId { get; set; }
        public string BungieAccessToken { get; set; }
        public int DestinyMembershipType { get; set; }
        public long DestinyMembershipId { get; set; }
    }
}