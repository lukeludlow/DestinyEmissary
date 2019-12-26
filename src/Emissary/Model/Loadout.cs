namespace EmissaryCore
{
    public class Loadout
    {
        public ulong DiscordId { get; set; }
        public long DestinyCharacterId { get; set; }
        public string LoadoutName { get; set; }
        public DestinyItem[] Items { get; set; }
    }
}