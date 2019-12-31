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
        [Summary("get your currently equipped weapons and armor. if your equipped gear matches one of your saved loadouts, then this will tell you what loadout you are currently using.\nexample usage: `$current`")]
        public async Task<RuntimeResult> CurrentlyEquipped() =>
                await emissaryService.CurrentlyEquipped(Context.User.Id);

        [Command("list")]
        [Summary("view all of your saved loadouts.\nexample usage: `$list`")]
        public async Task<RuntimeResult> ListLoadouts() =>
                await emissaryService.ListLoadouts(Context.User.Id);

        [Command("equip")]
        [Summary("equip one of your loadouts. the loadout name must match the saved name exactly, including spaces and capitalization.\nexample usage: `$equip crucible`")]
        public async Task<RuntimeResult> EquipLoadout([Remainder] string loadoutName) =>
                await emissaryService.EquipLoadout(Context.User.Id, loadoutName);

        [Command("saveas")]
        [Summary("save your currently equipped weapons and armor as a loadout. (note: everything you write after \"saveas\" will be the loadout name.)\nexample usage: `$saveas last wish raid`")]
        public async Task<RuntimeResult> SaveLoadout([Remainder] string loadoutName) =>
                await emissaryService.SaveCurrentlyEquippedAsLoadout(Context.User.Id, loadoutName);

        [Command("delete")]
        [Summary("delete one of your saved loadouts. the loadout name must match the saved name exactly, including spaces and capitalization. (note: this can't alter any of your in-game items, it just makes destiny emissary forget the loadout.)\nexample usage: `$delete last wish raid`")]
        public async Task<RuntimeResult> DeleteLoadout([Remainder] string loadoutName) =>
                await emissaryService.DeleteLoadout(Context.User.Id, loadoutName);


    }
}