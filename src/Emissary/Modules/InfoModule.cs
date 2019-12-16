using System.Threading.Tasks;
using Discord.Commands;

namespace Emissary.Modules
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("echo")]
        public Task EchoAsync([Remainder] string echo)
        {
            return ReplyAsync(echo);
        }
    }
}