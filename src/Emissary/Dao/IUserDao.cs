using System;
using System.Collections.Generic;

namespace EmissaryCore
{
    public interface IUserDao : IDisposable
    {
        EmissaryUser GetUserByDiscordId(ulong discordId);
        void AddOrUpdateUser(EmissaryUser emissaryUser); 
        void RemoveUser(ulong discordId);
    }
}