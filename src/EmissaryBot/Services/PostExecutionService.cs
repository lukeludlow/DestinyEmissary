using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EmissaryCore;
using Microsoft.Extensions.Configuration;

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
                        await HandleSuccessEmissaryResult(context, emissaryResult);
                    } else {
                        await HandleErrorEmissaryResult(context, emissaryResult);
                    }
                    break;
                default:
                    break;
            }
        }

        private async Task HandleSuccessEmissaryResult(ICommandContext context, EmissaryResult result)
        {
            string successMessage = $"success{(string.IsNullOrWhiteSpace(result.Message) ? "" : ":\n{emissaryResult.Message}")}";
            await context.Channel.SendMessageAsync(successMessage);
            await logService.LogAsync(new LogMessage(LogSeverity.Info, "PostExecutionService", successMessage));
        }

        private async Task HandleErrorEmissaryResult(ICommandContext context, EmissaryResult result)
        {
            if (result.ErrorCode == EmissaryErrorCodes.UserNotFound) {
                await TellUserToRegisterOrReauthorize(context);
                await logService.LogAsync(new LogMessage(LogSeverity.Error, "PostExecutionService", $"telling user to register or reauthorize using link {GetNewOAuthLinkForUser(context.User)}"));
            } else {
                string errorMessage = $"an unexpected error occurred. {result.ErrorMessage}";
                await context.Channel.SendMessageAsync(errorMessage);
                await logService.LogAsync(new LogMessage(LogSeverity.Error, "PostExecutionService", errorMessage));
            }
        }

        private async Task TellUserToRegisterOrReauthorize(ICommandContext context)
        {
            string channelMessage = "i need access to your bungie account to do this. please check your DMs for instructions";
            string registrationLink = GetNewOAuthLinkForUser(context.User);
            string userMessage = $"use this link to authorize access to your bungie account:\n{registrationLink}\n";
            userMessage += "Destiny Emissary will not have access to your email, password, or any other personal information.";
            await context.Channel.SendMessageAsync(channelMessage);
            await Discord.UserExtensions.SendMessageAsync(context.User, userMessage);
        }

        private string GetNewOAuthLinkForUser(IUser user)
        {
            return $"https://www.bungie.net/en/oauth/authorize?client_id={config["Bungie:ClientId"]}&response_type=code&state={user.Id}";
        }

    }
}