using System;
using System.Net.Http;
using System.Threading.Tasks;
using EmissaryCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmissaryBot
{
    public class EmissaryService
    {
        private readonly IEmissary emissary;
        private readonly IConfiguration config;
        private readonly IServiceProvider services;

        public EmissaryService(IEmissary emissary, IConfiguration config, IServiceProvider services)
        {
            IBungieApiService bungieApiService = new BungieApiService(config, services.GetRequiredService<HttpClient>());
            IManifestDao manifestDao = new ManifestDao(config, new DatabaseAccessor());

            SqliteConnection sqliteConnection = new SqliteConnection($"DataSource={config["Emissary:DatabasePath"]}");
                DbContextOptions<EmissaryDbContext> dbContextOptions = new DbContextOptionsBuilder<EmissaryDbContext>()
                    .UseSqlite(sqliteConnection)
                    .Options;
            EmissaryDbContext dbContext = new EmissaryDbContext(dbContextOptions);
            IUserDao userDao = new UserDao(dbContext);
            ILoadoutDao loadoutDao = new LoadoutDao(dbContext);

            this.emissary = new Emissary(config, bungieApiService, manifestDao, dbContext, userDao, loadoutDao);
            this.emissary = emissary;
            this.config = config;
            this.services = services;
        }
    }
}