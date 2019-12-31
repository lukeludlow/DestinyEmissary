using System.Threading.Tasks;
using Discord.Commands;

namespace EmissaryBot
{
    public class EmissaryModule : ModuleBase<SocketCommandContext>
    {
        private readonly EmissaryService emissaryService;

        public EmissaryModule(EmissaryService emissaryService)
        {
            this.emissaryService = emissaryService;
        }

        [Command("current")]
        [Summary("get your currently equipped items. if your equipped gear matches one of your saved loadouts, then this will tell you what loadout you are currently using.\nexample usage: `$current`")]
        public async Task<RuntimeResult> CurrentlyEquipped() =>
                await emissaryService.CurrentlyEquipped(Context.User.Id);

        [Command("list")]
        [Summary("view all of your saved loadouts.\nexample usage: `$list`")]
        public async Task<RuntimeResult> ListLoadouts() =>
                await emissaryService.ListLoadouts(Context.User.Id);

        [Command("equip")]
        [Summary("equip one of your loadouts.\nexample usage: `$equip raid`")]
        public async Task<RuntimeResult> EquipLoadout(string loadoutName) =>
                await emissaryService.EquipLoadout(Context.User.Id, loadoutName);

        [Command("saveas")]
        [Summary("save your currently equipped gear as a loadout.\nexample usage: `saveas pvp`")]
        public async Task<RuntimeResult> SaveLoadout(string loadoutName) =>
                await emissaryService.SaveCurrentlyEquippedAsLoadout(Context.User.Id, loadoutName);


    }
}