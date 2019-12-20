using System;
using Newtonsoft.Json;

namespace Emissary
{
    public class Loadout
    {
        public string Name { get; set; }
        public ulong DiscordID { get; set; }
        public long BungieID { get; set; }
        public WeaponLoadout Weapons { get; set; }
        public ArmorLoadout Armor { get; set; }
        public ClassLoadout CharacterClass { get; set; }

        public Loadout()
        {
        }

        public Loadout(string name, ulong discordID, long bungieID, WeaponLoadout weapons, ArmorLoadout armor, ClassLoadout characterClass)
        {
            this.Name = name;
            this.DiscordID = discordID;
            this.BungieID = bungieID;
            this.Weapons = weapons;
            this.Armor = armor;
            this.CharacterClass = characterClass;
        }
    }
}