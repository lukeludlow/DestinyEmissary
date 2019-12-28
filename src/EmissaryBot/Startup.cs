using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using System.Net.Http;
using System.Net;
using EmissaryCore;

namespace EmissaryBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }

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
            Configuration = BuildConfiguration();

            IServiceCollection services = new ServiceCollection();
            ConfigureServices(services);

            IServiceProvider provider = services.BuildServiceProvider();
            provider.GetRequiredService<LogService>();  
            provider.GetRequiredService<CommandHandler>();  
            provider.GetRequiredService<AuthorizationRedirectService>();
            provider.GetRequiredService<EmissaryService>();
            provider.GetRequiredService<PostExecutionService>();

            // start the startup service which actually runs the bot
            await provider.GetRequiredService<StartupService>().StartAsync();  
            // block this task until the program is closed
            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient())
                    .AddSingleton(new CommandService())
                    .AddSingleton(new HttpListener())  // TODO config httplistener for authorization redirect service
                    .AddSingleton<LogService>()
                    .AddSingleton<CommandHandler>()
                    .AddSingleton<AuthorizationRedirectService>()
                    .AddSingleton<StartupService>()
                    .AddSingleton<PostExecutionService>()
                    .AddSingleton<EmissaryService>()
                    .AddSingleton(Configuration);
        }

        private IConfiguration BuildConfiguration()
        {
            // TODO detect where the config.json file is
            // we are in "production" (running from the console)
            // we are in "development" (running unit tests)
            IConfigurationBuilder builder = new ConfigurationBuilder()
                          .SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                          .AddEnvironmentVariables();
            return builder.Build();
        }

    }
}