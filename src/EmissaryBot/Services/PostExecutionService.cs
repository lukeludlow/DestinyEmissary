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
            if (commandInfo.Value.Name == "list") {
                IList<Loadout> loadouts = JsonConvert.DeserializeObject<List<Loadout>>(result.Message);
                Embed embedMessage = DiscordMessageTransformService.LoadoutsListToDiscordEmbed(context.Client.CurrentUser, context.User, loadouts);
                await context.Channel.SendMessageAsync(embed: embedMessage);
                return;
            } else if (commandInfo.Value.Name == "current") {
                Embed embedMessage = DiscordMessageTransformService.CurrentlyEquippedToDiscordEmbed(context, result);
                await context.Channel.SendMessageAsync(embed: embedMessage);
                return;
            } else if (commandInfo.Value.Name == "save") {
                Embed embedMessage = DiscordMessageTransformService.SaveLoadoutToDiscordEmbed(context, result);
                await context.Channel.SendMessageAsync(embed: embedMessage);
                return;
            } else if (commandInfo.Value.Name == "equip") {
                Embed embedMessage = DiscordMessageTransformService.EquipLoadoutToDiscordEmbed(context, result);
                await context.Channel.SendMessageAsync(embed: embedMessage);
                return;
            }
            string successMessage = $"success{(string.IsNullOrWhiteSpace(result.Message) ? "" : $":\n{result.Message}")}";
            if (successMessage.Length > 2000) {
                successMessage = Truncate(successMessage, 2000);
            }
            await context.Channel.SendMessageAsync(successMessage);
            // await logService.LogAsync(new LogMessage(LogSeverity.Info, "PostExecutionService", successMessage));
        }

        private string Truncate(string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars-4) + "\n...";
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