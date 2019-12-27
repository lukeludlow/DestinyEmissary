using System.Collections.Generic;

namespace EmissaryCore
{
    public class Loadout
    {
        public ulong DiscordId { get; set; }
        public long DestinyCharacterId { get; set; }
        public string LoadoutName { get; set; }
        public IList<DestinyItem> Items { get; set; }

        public Loadout(ulong discordId, long destinyCharacterId, string loadoutName, IList<DestinyItem> items)
        {
            this.DiscordId = discordId;
            this.DestinyCharacterId = destinyCharacterId;
            this.LoadoutName = loadoutName;
            this.Items = items;
        }

        public Loadout()
        {
            this.DiscordId = default;
            this.DestinyCharacterId = default;
            this.LoadoutName = "";
            this.Items = new List<DestinyItem>();
        }
    }
}