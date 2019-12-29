using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace EmissaryCore
{
    public class DiscordMessageTransformService
    {
        public static Embed LoadoutsListToDiscordEmbed(string discordName, IList<Loadout> loadouts)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle($"{discordName}'s saved loadouts");
            foreach (Loadout loadout in loadouts) {
                eb.AddField(loadout.LoadoutName, LoadoutToString(loadout));
            }
            return eb.Build();
        }

        public static string LoadoutToString(Loadout loadout)
        {
            string kineticWeapon = loadout.Items.Where(item => item.Categories.Contains("Kinetic Weapon"))
                                                .Select(item => item.Name).FirstOrDefault();
            string energyWeapon = loadout.Items.Where(item => item.Categories.Contains("Energy Weapon"))
                                                .Select(item => item.Name).FirstOrDefault();
            string powerWeapon = loadout.Items.Where(item => item.Categories.Contains("Power Weapon"))
                                                .Select(item => item.Name).FirstOrDefault();
            string helmet = loadout.Items.Where(item => item.Categories.Contains("Helmets"))
                                                .Select(item => item.Name).FirstOrDefault();
            string arms = loadout.Items.Where(item => item.Categories.Contains("Arms"))
                                                .Select(item => item.Name).FirstOrDefault();
            string chest = loadout.Items.Where(item => item.Categories.Contains("Chest"))
                                                .Select(item => item.Name).FirstOrDefault();
            string legs = loadout.Items.Where(item => item.Categories.Contains("Legs"))
                                                .Select(item => item.Name).FirstOrDefault();
            string classItem = loadout.Items.Where(item => item.Categories.Contains("Class Items"))
                                                .Select(item => item.Name).FirstOrDefault();
            string s = "";
            s += $"Kinetic Weapon:  {kineticWeapon}\n";
            s += $"Energy Weapon:   {energyWeapon}\n";
            s += $"Power Weapon:    {powerWeapon}\n";
            s += $"Helmet:          {helmet}\n";
            s += $"Gauntlets:       {arms}\n";
            s += $"Chest:           {chest}\n";
            s += $"Legs:            {legs}\n";
            s += $"Class Item:      {classItem}\n";
            return s;
        }
    }
}