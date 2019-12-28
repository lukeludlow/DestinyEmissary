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
        public async Task<RuntimeResult> CurrentlyEquipped() =>
                await emissaryService.CurrentlyEquipped(Context.User.Id);

        [Command("list")]
        public async Task<RuntimeResult> ListLoadouts() =>
                await emissaryService.ListLoadouts(Context.User.Id);

        [Command("equip")]
        public async Task<RuntimeResult> EquipLoadout(string loadoutName) =>
                await emissaryService.EquipLoadout(Context.User.Id, loadoutName);

        [Command("save")]
        public async Task<RuntimeResult> SaveLoadout(string loadoutName) =>
                await emissaryService.SaveCurrentlyEquippedAsLoadout(Context.User.Id, loadoutName);

    }
}