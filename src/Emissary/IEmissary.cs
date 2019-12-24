using System;
using System.Collections.Generic;

namespace Emissary.Common
{
    /// <summary>
    /// this is the main interface that allows the discord bot to call the actual Destiny Emissary logic. each discord
    /// module/service will call one of these methods on the given Emissary implementation.
    /// </summary>
    public interface IEmissary
    {
        Loadout CurrentlyEquipped(ulong discordId);
        IList<Loadout> ListLoadouts(ulong discordId);
        string EquipLoadout(ulong discordId, string loadoutName);
        string SaveLoadout(ulong discordId, string loadoutName);
        string DeleteLoadout(ulong discordId, string loadoutName);
        string Authorize(string discordIdAuthState, string authCode);
    }
}
