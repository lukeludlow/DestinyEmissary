using System;
using System.Collections.Generic;

namespace EmissaryCore
{
    /// <summary>
    /// this is the main interface that allows the discord bot to call the actual Destiny Emissary logic. 
    /// each discord module/service will call one of these methods on the given Emissary implementation.
    /// </summary>
    public interface IEmissary
    {
        EmissaryResult CurrentlyEquipped(ulong discordId);
        EmissaryResult ListLoadouts(ulong discordId);
        EmissaryResult EquipLoadout(ulong discordId, string loadoutName);
        EmissaryResult SaveCurrentlyEquippedAsLoadout(ulong discordId, string loadoutName);
        EmissaryResult SaveLoadout(ulong discordId, Loadout loadout, string loadoutName);
        string DeleteLoadout(ulong discordId, string loadoutName);
        EmissaryResult RegisterOrReauthorize(ulong discordId, string authCode);
        event Action<ulong> RequestAuthorizationEvent;
    }
}
