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
        /// <summary>
        /// get the user's currently equipped items (weapons, armor, and subclass). note that this doesn't return the
        /// name of the currently equipped DestinyEmissary loadout, it just shows the items currently on the destiny
        /// character. 
        /// </summary>
        Loadout CurrentlyEquipped(ulong discordId);

        IList<Loadout> ListLoadouts(ulong discordId);

        string EquipLoadout(ulong discordId, string loadoutName);

        string SaveLoadout(ulong discordId, string loadoutName);

        string DeleteLoadout(ulong discordId, string loadoutName);

        string Register(ulong discordId, long bungieId);
    }
}
