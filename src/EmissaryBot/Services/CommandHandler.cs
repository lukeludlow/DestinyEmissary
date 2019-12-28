using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EmissaryCore;
using Microsoft.Extensions.Configuration;

namespace EmissaryBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;
        private readonly LogService logService;
        private readonly PostExecutionService postExecutionService;
        private readonly IConfiguration config;
        private readonly IServiceProvider serviceProvider;

        public CommandHandler(DiscordSocketClient client, CommandService commands, LogService logger, PostExecutionService postExecutionService, IConfiguration config, IServiceProvider services)
        {
            discordClient = client;
            commandService = commands;
            logService = logger;
            this.postExecutionService = postExecutionService;
            this.config = config;
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
            await postExecutionService.HandlePostExecution(commandInfo, context, result);
            // string commandName = commandInfo.IsSpecified ? commandInfo.Value.Name : "unknown command";
            // LogMessage logMessage = new LogMessage(LogSeverity.Info, "CommandExecution", $"{commandName} was executed at {DateTime.UtcNow}");
            // await logService.LogAsync(logMessage);
        }

    }
}