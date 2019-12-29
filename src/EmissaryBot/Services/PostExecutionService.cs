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
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithThumbnailUrl(context.User.GetAvatarUrl());
            eb.WithColor(Color.Blue);
            switch (commandInfo.Value.Name) {
                case "list":
                    eb.WithTitle($"{context.User.Username}'s saved loadouts");
                    eb.WithDescription(result.Message);
                    break;
                case "current":
                    eb.WithTitle($"{context.User.Username}'s currently equipped items");
                    eb.WithDescription(result.Message);
                    break;
                case "save":
                    eb.WithTitle($"successfully saved loadout");
                    eb.WithDescription(result.Message);
                    break;
                case "equip":
                    eb.WithTitle($"successfully equipped loadout");
                    eb.WithDescription(result.Message);
                    break;
                default:
                    eb.WithTitle($"success");
                    eb.WithDescription("");
                    break;
            }
            Embed embedMessage = eb.Build();
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
            if (result.ErrorCode == EmissaryErrorCodes.UserNotFound) {
                await TellUserToRegisterOrReauthorize(context);
                await logService.LogAsync(new LogMessage(LogSeverity.Error, "PostExecutionService", $"telling user to register or reauthorize using link {GetNewOAuthLinkForUser(context.User)}"));
            } else {
                string errorMessage = $"an unexpected error occurred. {result.ErrorMessage}";
                await context.Channel.SendMessageAsync(errorMessage);
                await logService.LogAsync(new LogMessage(LogSeverity.Error, "PostExecutionService", errorMessage));
            }
        }

        public void TellUserToRegisterOrReauthorizeCallback(ulong discordId)
        {
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