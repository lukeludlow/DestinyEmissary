using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Emissary
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;
        private readonly LogService logService;
        private readonly IConfigurationRoot config;
        private readonly IServiceProvider serviceProvider;

        public CommandHandler(DiscordSocketClient client, CommandService commands, LogService logger, IConfigurationRoot configuration, IServiceProvider services)
        {
            discordClient = client;
            commandService = commands;
            logService = logger;
            config = configuration;
            serviceProvider = services;
            discordClient.MessageReceived += OnMessageReceivedAsync;
            commandService.CommandExecuted += OnCommandExecutedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;
            // ensure the message is from a user or a bot
            if (message == null) return;
            // ignore self when checking commands
            if (message.Author.Id == discordClient.CurrentUser.Id) return;

            int argPos = 0;
            if (message.HasCharPrefix('$', ref argPos) || message.HasMentionPrefix(discordClient.CurrentUser, ref argPos)) {
                SocketCommandContext context = new SocketCommandContext(discordClient, message);
                await commandService.ExecuteAsync(context, argPos, serviceProvider);
            }
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> commandInfo, ICommandContext context, IResult result)
        {
            string commandName = commandInfo.IsSpecified ? commandInfo.Value.Name : "unknown command";
            LogMessage logMessage = new LogMessage(LogSeverity.Info, "CommandExecution", $"{commandName} was executed at {DateTime.UtcNow}");
            await logService.LogAsync(logMessage);
        }

    }
}