using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace EmissaryCore
{
    public class DiscordMessageTransformService
    {
        public static Embed LoadoutsListToDiscordEmbed(IUser author, IUser user, IList<Loadout> loadouts)
        {
            EmbedBuilder eb = new EmbedBuilder();
            // eb.WithAuthor(author.Username, author.GetAvatarUrl());
            eb.WithThumbnailUrl(user.GetAvatarUrl());
            eb.WithTitle($"{user.Username}'s saved loadouts");
            eb.WithColor(Color.Blue);
            string content = "";
            foreach (Loadout loadout in loadouts) {
                content += LoadoutToString(loadout);
            }
            eb.WithDescription(content);
            return eb.Build();
        }

        public static Embed CurrentlyEquippedToDiscordEmbed(ICommandContext context, EmissaryResult result)
        {
            Loadout currentlyEquipped = JsonConvert.DeserializeObject<Loadout>(result.Message);
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithThumbnailUrl(context.User.GetAvatarUrl());
            eb.WithTitle($"{context.User.Username}'s currently equipped items");
            eb.WithColor(Color.Blue);
            eb.WithDescription(LoadoutToString(currentlyEquipped));
            return eb.Build();
        }

        public static Embed SaveLoadoutToDiscordEmbed(ICommandContext context, EmissaryResult result)
        {
            Loadout savedLoadout = JsonConvert.DeserializeObject<Loadout>(result.Message);
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithThumbnailUrl(context.User.GetAvatarUrl());
            eb.WithTitle($"successfully saved loadout");
            eb.WithColor(Color.Blue);
            eb.WithDescription(LoadoutToString(savedLoadout));
            return eb.Build();
        }

        public static Embed EquipLoadoutToDiscordEmbed(ICommandContext context, EmissaryResult result)
        {
            Loadout equippedLoadout = JsonConvert.DeserializeObject<Loadout>(result.Message);
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithThumbnailUrl(context.User.GetAvatarUrl());
            eb.WithTitle($"successfully equipped loadout");
            eb.WithColor(Color.Blue);
            eb.WithDescription(LoadoutToString(equippedLoadout));
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
            string s = $"\n**{loadout.LoadoutName}**\n";
            s += $"weapons:\n";
            s += $"- {kineticWeapon}\n";
            s += $"- {energyWeapon}\n";
            s += $"- {powerWeapon}\n";
            s += $"armor:\n";
            s += $"- {helmet}\n";
            s += $"- {arms}\n";
            s += $"- {chest}\n";
            s += $"- {legs}\n";
            s += $"- {classItem}\n";
            // s += $"Kinetic:   {kineticWeapon}\n";
            // s += $"Energy:   {energyWeapon}\n";
            // s += $"Power:    {powerWeapon}\n";
            // s += $"Helmet:          {helmet}\n";
            // s += $"Gauntlets:       {arms}\n";
            // s += $"Chest:           {chest}\n";
            // s += $"Legs:            {legs}\n";
            // s += $"Class Item:      {classItem}\n";
            return s;
        }
    }
}