using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
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

        public EmissaryService(IConfiguration config, IServiceProvider services)
        {
            this.emissary = Emissary.Startup(config);
            this.config = config;
            this.services = services;
        }

        public Task<RuntimeResult> CurrentlyEquipped(ulong discordId)
        {
            return Task.Run(() => (RuntimeResult)emissary.CurrentlyEquipped(discordId));
        }

        public Task<RuntimeResult> ListLoadouts(ulong discordId)
        {
            return Task.Run(() => (RuntimeResult)emissary.ListLoadouts(discordId));
        }

        public Task<RuntimeResult> EquipLoadout(ulong discordId, string loadoutName)
        {
            return Task.Run(() => (RuntimeResult)emissary.EquipLoadout(discordId, loadoutName));
        }

        public Task<RuntimeResult> SaveCurrentlyEquippedAsLoadout(ulong discordId, string loadoutName)
        {
            return Task.Run(() => (RuntimeResult)emissary.SaveCurrentlyEquippedAsLoadout(discordId, loadoutName));
        }

        public Task<RuntimeResult> RegisterOrReauthorize(string discordId, string authCode)
        {
            return Task.Run(() => (RuntimeResult)emissary.RegisterOrReauthorize(ulong.Parse(discordId), authCode));
        }



        // TODO create an event/delegate for sending an oauth link to the user, give it to the emissary instance

    }
}