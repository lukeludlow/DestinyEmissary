using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Emissary
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;
        private readonly IConfigurationRoot config;
        private readonly IServiceProvider serviceProvider;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IConfigurationRoot configuration, IServiceProvider services)
        {
            discordClient = client;
            commandService = commands;
            config = configuration;
            serviceProvider = services;
            discordClient.MessageReceived += OnMessageReceivedAsync;
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

    }
}