using System;
using System.Collections.Generic;

namespace EmissaryCore
{
    public interface IEmissaryDao : IDisposable
    {
        EmissaryUser GetUserByDiscordId(ulong discordId);
        void AddOrUpdateUser(EmissaryUser emissaryUser); 
        void RemoveUser(ulong discordId);

        BungieAccessToken GetAccessTokenByDiscordId(ulong discordId);
        void AddOrUpdateAccessToken(BungieAccessToken accessToken);

        IList<Loadout> GetAllLoadoutsForUser(ulong discordId); 
        Loadout GetLoadout(ulong discordId, long destinyCharacterId, string loadoutName);
        void AddOrUpdateLoadout(Loadout loadout); 
        void RemoveLoadout(ulong discordId, long destinyCharacterId, string loadoutName);
    }
}