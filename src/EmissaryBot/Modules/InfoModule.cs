using System.Threading.Tasks;
using Discord.Commands;
using System.Linq;
using System.Collections.Generic;
using Discord;

namespace EmissaryBot
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService commandService;

        public HelpModule(CommandService commandService)
        {
            this.commandService = commandService;
        }

        [Command("help")]
        [Summary("see all available commands")]
        public async Task Help()
        {
            List<CommandInfo> commands = commandService.Commands.ToList();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("available commands");
            embed.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());
            string embedDescription = "";
            foreach (CommandInfo command in commands) {
                if (command.Name == "help") {
                    continue;
                }
                string commandDescription = "";
                commandDescription += $"\n**{command.Name}**";
                if (command.Parameters != null && command.Parameters.Count > 0) {
                    commandDescription += $" [{command.Parameters[0].Name}]";
                }
                commandDescription += "\n";
                commandDescription += $"{command.Summary}\n";
                embedDescription += commandDescription;
            }
            embed.WithDescription(embedDescription);
            EmbedFooterBuilder footer = new EmbedFooterBuilder();
            footer.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());
            footer.WithText("github.com/lukeludlow/DestinyEmissary");
            embed.WithFooter(footer);
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

    }
}