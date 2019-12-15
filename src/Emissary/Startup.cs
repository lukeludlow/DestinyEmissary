using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.WebSocket;

namespace Emissary
{
    public class Startup
    {

        private DiscordSocketClient client;
        private IConfigurationRoot config;

        public Startup()
        {
        }

        public static async Task MainAsync()
        {
            Startup startup = new Startup();
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            client = new DiscordSocketClient();
            client.Log += Log;

            IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();
            config = builder.Build();

            string token = config["Discord:BotToken"];

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            // block this task until the program is closed
            await Task.Delay(-1);
        }


        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

    }
}