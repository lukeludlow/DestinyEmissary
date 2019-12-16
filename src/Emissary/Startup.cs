using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;

namespace Emissary
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup()
        {
            Configuration = BuildConfiguration();
        }

        public static async Task MainAsync()
        {
            Startup startup = new Startup();
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            IServiceCollection services = new ServiceCollection();
            ConfigureServices(services);

            IServiceProvider provider = services.BuildServiceProvider();
            provider.GetRequiredService<LoggingService>();  // start the logging services
            provider.GetRequiredService<CommandHandler>();  // start the command handler service
            await provider.GetRequiredService<StartupService>().StartAsync();  // start the startup service which actually "runs" the bot
            // block this task until the program is closed
            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient())
                    .AddSingleton(new CommandService())
                    .AddSingleton<CommandHandler>()
                    .AddSingleton<LoggingService>()
                    .AddSingleton<StartupService>()
                    .AddSingleton(Configuration);
        }

        private IConfigurationRoot BuildConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                          .SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                          .AddEnvironmentVariables();
            return builder.Build();
        }

    }
}