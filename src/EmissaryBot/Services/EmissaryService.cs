using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
        private readonly DiscordSocketClient discordClient;
        private readonly IConfiguration config;
        private readonly IServiceProvider services;

        public EmissaryService(DiscordSocketClient discordClient, IConfiguration config, IServiceProvider services)
        {
            this.discordClient = discordClient;
            this.emissary = Emissary.Startup(config);
            this.emissary.RequestAuthorizationEvent += RequestAuthorizationCallback;
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

        public Task<RuntimeResult> DeleteLoadout(ulong discordId, string loadoutName)
        {
            return Task.Run(() => (RuntimeResult)emissary.DeleteLoadout(discordId, loadoutName));
        }

        public Task<RuntimeResult> RegisterOrReauthorize(string discordId, string authCode)
        {
            return Task.Run(() => (RuntimeResult)emissary.RegisterOrReauthorize(ulong.Parse(discordId), authCode));
        }


        public async void RequestAuthorizationCallback(ulong discordId)
        {
            IUser user = discordClient.GetUser(discordId);
            await SendDirectMessageWithAuthorizationLink(user);
        }

        private async Task SendDirectMessageWithAuthorizationLink(IUser user)
        {
            string registrationLink = GetNewOAuthLinkForUser(user);
            string userMessage = $"use this link to authorize access to your bungie account:\n{registrationLink}\n";
            userMessage += "Destiny Emissary **won't** have access to your email, password, or any other personal information.";
            await Discord.UserExtensions.SendMessageAsync(user, userMessage);
        }

        private string GetNewOAuthLinkForUser(IUser user)
        {
            return $"https://www.bungie.net/en/oauth/authorize?client_id={config["Bungie:ClientId"]}&response_type=code&state={user.Id}";
        }

    }
}