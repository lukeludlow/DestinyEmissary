using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Emissary
{
    public class EmissaryUser
    {
        [Key]
        public ulong DiscordID { get; set; }
        public long BungieID { get; set; }
        // bungie membership type

        public EmissaryUser()
        {
        }

        public EmissaryUser(ulong discordId, long bungieId)
        {
            this.DiscordID = discordId;
            this.BungieID = bungieId;
        }
    }
}