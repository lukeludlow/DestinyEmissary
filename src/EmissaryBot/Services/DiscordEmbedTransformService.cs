using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace EmissaryCore
{
    public class DiscordEmbedTransformService
    {

        public static Embed EmissaryResultToDiscordEmbed(Optional<CommandInfo> commandInfo, ICommandContext context, EmissaryResult result)
        {
            EmbedBuilder eb = new EmbedBuilder();
            // eb.WithAuthor(author.Username, author.GetAvatarUrl());
            eb.WithThumbnailUrl(context.User.GetAvatarUrl());
            eb.WithColor(Color.Blue);
            switch (commandInfo.Value.Name) {
                case "list":
                    eb.WithTitle($"{context.User.Username}'s saved loadouts");
                    IList<Loadout> loadouts = JsonConvert.DeserializeObject<List<Loadout>>(result.Message);
                    StringBuilder sb = new StringBuilder();
                    foreach (Loadout loadout in loadouts) {
                        sb.Append(LoadoutToDescription(loadout));
                    }
                    eb.WithDescription(sb.ToString());
                    break;
                case "current":
                    eb.WithTitle($"{context.User.Username}'s currently equipped loadout");
                    eb.WithDescription(LoadoutToDescription(JsonConvert.DeserializeObject<Loadout>(result.Message)));
                    break;
                case "save":
                    eb.WithTitle($"successfully saved loadout");
                    eb.WithDescription(LoadoutToDescription(JsonConvert.DeserializeObject<Loadout>(result.Message)));
                    break;
                case "equip":
                    eb.WithTitle($"successfully equipped loadout");
                    eb.WithDescription(LoadoutToDescription(JsonConvert.DeserializeObject<Loadout>(result.Message)));
                    break;
                default:
                    eb.WithTitle($"success");
                    eb.WithDescription("");
                    break;
            }
            return eb.Build();
        }

        private static string LoadoutToDescription(Loadout loadout)
        {
            string kineticWeapon = loadout.Items.Where(item => item.Categories.Contains("Kinetic Weapon")).Select(item => item.Name).FirstOrDefault();
            string energyWeapon = loadout.Items.Where(item => item.Categories.Contains("Energy Weapon")).Select(item => item.Name).FirstOrDefault();
            string powerWeapon = loadout.Items.Where(item => item.Categories.Contains("Power Weapon")).Select(item => item.Name).FirstOrDefault();
            string helmet = loadout.Items.Where(item => item.Categories.Contains("Helmets")).Select(item => item.Name).FirstOrDefault();
            string arms = loadout.Items.Where(item => item.Categories.Contains("Arms")).Select(item => item.Name).FirstOrDefault();
            string chest = loadout.Items.Where(item => item.Categories.Contains("Chest")).Select(item => item.Name).FirstOrDefault();
            string legs = loadout.Items.Where(item => item.Categories.Contains("Legs")).Select(item => item.Name).FirstOrDefault();
            string classItem = loadout.Items.Where(item => item.Categories.Contains("Class Items")).Select(item => item.Name).FirstOrDefault();
            StringBuilder sb = new StringBuilder();
            sb.Append($"\n**{loadout.LoadoutName}**\n");
            sb.Append($"weapons:\n");
            sb.Append($"- {kineticWeapon}\n");
            sb.Append($"- {energyWeapon}\n");
            sb.Append($"- {powerWeapon}\n");
            sb.Append($"armor:\n");
            sb.Append($"- {helmet}\n");
            sb.Append($"- {arms}\n");
            sb.Append($"- {chest}\n");
            sb.Append($"- {legs}\n");
            sb.Append($"- {classItem}\n");
            return sb.ToString();
        }
    }
}
