using System;
using System.Linq;

namespace EmissaryCore
{
    public class AccessTokenDao : IAccessTokenDao, IDisposable
    {
        private readonly EmissaryDbContext dbContext;

        public AccessTokenDao(EmissaryDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void AddOrUpdateAccessToken(BungieAccessToken accessToken)
        {
            BungieAccessToken existingToken = dbContext.AccessTokens.Where(token => token.DiscordId == accessToken.DiscordId).FirstOrDefault();
            if (existingToken == null) {
                dbContext.AccessTokens.Add(accessToken);
            } else {
                dbContext.Entry(existingToken).CurrentValues.SetValues(accessToken);
            }
            dbContext.SaveChanges();
        }

        public BungieAccessToken GetAccessTokenByDiscordId(ulong discordId)
        {
            return dbContext.AccessTokens.Find(discordId);
        }



        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed) {
                if (disposing) {
                    dbContext.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}