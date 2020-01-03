using System.ComponentModel.DataAnnotations;

namespace EmissaryCore
{
    public class EmissaryUser
    {
        [Key]
        public ulong DiscordId { get; set; }
        public long DestinyProfileId { get; set; }
        public int DestinyMembershipType { get; set; }

        public EmissaryUser(ulong discordId, long destinyProfileId, int destinyMembershipType)
        {
            this.DiscordId = discordId;
            this.DestinyProfileId = destinyProfileId;
            this.DestinyMembershipType = destinyMembershipType;
        }

        public EmissaryUser()
        {
        }
    }
}