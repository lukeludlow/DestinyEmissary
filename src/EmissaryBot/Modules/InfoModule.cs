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
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle("available commands");
            string embedDescription = "";
            foreach (CommandInfo command in commands) {
                if (command.Name == "help") {
                    continue;
                }
                embedDescription += $"\n**{command.Name}**";
                if (command.Parameters != null && command.Parameters.Count > 0) {
                    embedDescription += $" [{command.Parameters[0].Name}]";
                }
                embedDescription += "\n";
                embedDescription += $"{command.Summary}\n";
            }
            eb.WithDescription(embedDescription);
            await Context.Channel.SendMessageAsync(embed: eb.Build());
        }

        // TODO not sure if i want to do this
        [Command("about")]
        [Summary("get more information about destiny emissary")]
        public async Task About()
        {
            string aboutMessage = "Destiny Emissary is a service that saves weapon and armor loadouts for you.\n";
            aboutMessage += "you can quickly switch tabs back to discord and tell destiny emissary to equip what you need. this means less clicking through your inventory! destiny emissary handles the details of remembering your loadouts and equipping all your gear, including any gear that is stored in the vault.\n";
            aboutMessage += "for example, when you're loading into a crucible match, you can quickly switch tabs back to discord and tell emissary to equip one of your PvP loadouts. this means you don't have to memorize what armor you need for which activity and you don't have to spend time clicking through your inventory (or dragging and dropping in DIM).\n";
            aboutMessage += "\nif you're curious, you can check out the emissary source code here: github.com/lukeludlow/DestinyEmissary";
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle("about destiny emissary");
            eb.WithDescription(aboutMessage);
            eb.WithAuthor(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl());
            await Context.Channel.SendMessageAsync(embed: eb.Build());
        }

    }
}