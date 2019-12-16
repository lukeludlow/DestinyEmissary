using System.Threading.Tasks;
using Discord.Commands;

namespace Emissary
{
    public class LoadoutModule : ModuleBase<SocketCommandContext>
    {

        [Command("loadout")]
        public Task LoadoutAsync()
        {
            return ReplyAsync("i guess you want your loadout idk");
        }

        [Command("list")]
        public async Task ListLoadoutsAsync()
        {
            await ReplyAsync("list loadouts");
        }

    }
}