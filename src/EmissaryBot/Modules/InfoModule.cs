using System.Threading.Tasks;
using Discord.Commands;

namespace EmissaryBot
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