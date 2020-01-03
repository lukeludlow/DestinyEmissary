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
                    foreach (Loadout loadout in loadouts) {
                        eb.AddField("___", LoadoutToDescription(loadout));
                    }
                    break;
                case "current":
                    eb.WithTitle($"{context.User.Username}'s currently equipped loadout");
                    eb.AddField("___", LoadoutToDescription(JsonConvert.DeserializeObject<Loadout>(result.Message)));
                    break;
                case "save":
                    eb.WithTitle($"successfully saved loadout");
                    eb.AddField("___", LoadoutToDescription(JsonConvert.DeserializeObject<Loadout>(result.Message)));
                    break;
                case "equip":
                    eb.WithTitle($"successfully equipped loadout");
                    eb.AddField("___", LoadoutToDescription(JsonConvert.DeserializeObject<Loadout>(result.Message)));
                    break;
                default:
                    eb.WithTitle($"success");
                    eb.WithDescription(result.Message);
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
            // using the invisible char \u200b tricks discord to not remove the next whitespace
            string spaceUnicodeString = "\u200b\u0020";
            string indentString = string.Concat(Enumerable.Repeat(spaceUnicodeString, 4));
            sb.Append($"\n**{loadout.LoadoutName}**\n");
            sb.Append($"weapons:\n");
            sb.Append($"{indentString}{kineticWeapon}\n");
            sb.Append($"{indentString}{energyWeapon}\n");
            sb.Append($"{indentString}{powerWeapon}\n");
            sb.Append($"armor:\n");
            sb.Append($"{indentString}{helmet}\n");
            sb.Append($"{indentString}{arms}\n");
            sb.Append($"{indentString}{chest}\n");
            sb.Append($"{indentString}{legs}\n");
            sb.Append($"{indentString}{classItem}\n");
            return sb.ToString();
        }
    }
}
