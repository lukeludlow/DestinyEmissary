using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EmissaryCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace EmissaryBot
{
    public class PostExecutionService
    {
        private readonly DiscordSocketClient discordClient;
        private readonly LogService logService;
        private readonly IConfiguration config;

        public PostExecutionService(DiscordSocketClient discordClient, LogService logService, IConfiguration config)
        {
            this.discordClient = discordClient;
            this.logService = logService;
            this.config = config;
        }

        public async Task HandlePostExecution(Optional<CommandInfo> commandInfo, ICommandContext context, IResult result)
        {
            switch (result) {
                case EmissaryResult emissaryResult:
                    if (emissaryResult.Success) {
                        await HandleSuccessEmissaryResult(commandInfo, context, emissaryResult);
                    } else {
                        await HandleErrorEmissaryResult(context, emissaryResult);
                    }
                    break;
                default:
                    break;
            }
        }

        private async Task HandleSuccessEmissaryResult(Optional<CommandInfo> commandInfo, ICommandContext context, EmissaryResult result)
        {
            Embed embedMessage = DiscordEmbedTransformService.EmissaryResultToDiscordEmbed(commandInfo, context, result);
            await context.Channel.SendMessageAsync(embed: embedMessage);
            // if (successMessage.Length > 2000) {
            // successMessage = Truncate(successMessage, 2000);
            // }
            // await logService.LogAsync(new LogMessage(LogSeverity.Info, "PostExecutionService", successMessage));
        }

        // private string Truncate(string value, int maxChars)
        // {
        //     return value.Length <= maxChars ? value : value.Substring(0, maxChars - 4) + "\n...";
        // }

        private async Task HandleErrorEmissaryResult(ICommandContext context, EmissaryResult result)
        {
            string errorMessage = $"{result.ErrorMessage}";
            await context.Channel.SendMessageAsync(errorMessage);
            await logService.LogAsync(new LogMessage(LogSeverity.Error, "PostExecutionService", $"{errorMessage}"));
        }



    }
}