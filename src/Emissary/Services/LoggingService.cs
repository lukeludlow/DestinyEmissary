using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Emissary
{
    public class LoggingService
    {
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;

        public LoggingService(DiscordSocketClient client, CommandService commands)
        {
            discordClient = client;
            commandService = commands;
            discordClient.Log += OnLogAsync;
            commandService.Log += OnLogAsync;
        }

        private Task OnLogAsync(LogMessage message)
        {
            return Console.Out.WriteLineAsync(message.ToString());
        }

    }
}