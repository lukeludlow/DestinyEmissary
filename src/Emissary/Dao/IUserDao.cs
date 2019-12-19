using System;
using System.Collections.Generic;

namespace Emissary
{
    public interface IUserDao : IDisposable
    {
        IList<EmissaryUser> GetUsers();
        EmissaryUser GetUserByDiscordId(ulong discordId);
        void AddUser(EmissaryUser emissaryUser); 
        void RemoveUser(ulong discordId);
        void Save();
    }
}