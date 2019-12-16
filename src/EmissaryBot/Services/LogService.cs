using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Emissary.Bot
{
    public class LogService
    {
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;

        public LogService(DiscordSocketClient client, CommandService commands)
        {
            discordClient = client;
            commandService = commands;
            discordClient.Log += LogAsync;
            commandService.Log += LogAsync;
        }

        public async Task LogAsync(LogMessage message)
        {
            await Console.Out.WriteLineAsync(message.ToString());
        }

    }
}