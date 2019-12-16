using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Emissary
{
    public class StartupService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;
        private readonly IConfigurationRoot config;

        public StartupService(IServiceProvider services, DiscordSocketClient client, CommandService commands, IConfigurationRoot configuration)
        {
            serviceProvider = services;
            discordClient = client;
            commandService = commands;
            config = configuration;
        }

        public async Task StartAsync()
        {
            string discordToken = config["Discord:BotToken"];
            await discordClient.LoginAsync(TokenType.Bot, discordToken);
            await discordClient.StartAsync();
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);  
        }
        
    }
}