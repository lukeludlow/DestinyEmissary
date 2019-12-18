using System;

namespace Emissary
{
    public class UserInfoDao : IUserInfoDao
    {
        private IDatabaseAccessor databaseAccessor;
        private string userInfoDatabasePath;


        public string GetAllLoadouts(ulong discordId)
        {
            throw new NotImplementedException();
        }

        public long GetBungieId(ulong discordId)
        {
            throw new NotImplementedException();
        }

        public string GetLoadout(ulong discordId, string loadoutName)
        {
            throw new NotImplementedException();
        }
    }
}