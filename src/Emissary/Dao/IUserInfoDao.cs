using System.Collections.Generic;
using Emissary.Model;

namespace Emissary
{
    public interface IUserInfoDao
    {
        long GetUserBungieId(ulong discordId);
        List<Loadout> GetAllLoadouts(ulong discordId);
        Loadout GetLoadout(ulong discordId, string loadoutName);
        bool AddUser(ulong discordId, long bungieId);
        bool AddLoadout(ulong discordId, string loadoutName, Loadout loadout);
        bool RemoveLoadout(ulong discordId, string loadoutName);
    }
}