using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Emissary
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private DiscordSocketClient client;

        public async Task MainAsync()
        {
            client = new DiscordSocketClient();
            client.Log += Log;


            IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddUserSecrets<DiscordConfig>()
                    .AddEnvironmentVariables();

            var services = new ServiceCollection()
                    .Configure<DiscordConfig>(Configuration.GetSection(nameof(DiscordConfig)))
                    .AddOptions()
                    .BuildServiceProvider();

            services.GetService<SecretConsumer>();


            string token = "TODO";


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
