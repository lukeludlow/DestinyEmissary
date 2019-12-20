using System;
using System.Collections.Generic;

namespace Emissary
{
    public interface ILoadoutDao : IDisposable
    {
        IList<Loadout> GetAllLoadoutsForUser(ulong discordId); 
        Loadout GetLoadout(ulong discordId, string loadoutName);
        void AddOrUpdateLoadout(Loadout loadout); 
        void RemoveLoadout(ulong discordId, string loadoutName);
    }
}