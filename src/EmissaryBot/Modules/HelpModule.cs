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
        public async Task Help()
        {
            List<CommandInfo> commands = commandService.Commands.ToList();
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle("available commands:");
            foreach (CommandInfo command in commands) {
                string commandSummary = command.Summary ?? "no description available";
                eb.AddField(command.Name, commandSummary);
            }
            await Context.Channel.SendMessageAsync(embed: eb.Build());
        }

    }
}