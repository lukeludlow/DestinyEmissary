using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EmissaryCore
{
    public class EmissaryDao : IEmissaryDao, IDisposable
    {
        private EmissaryDbContext dbContext;

        public EmissaryDao(EmissaryDbContext context)
        {
            this.dbContext = context;
        }

        public EmissaryUser GetUserByDiscordId(ulong discordId)
        {
            return dbContext.Users.Find(discordId);
        }

        public void AddOrUpdateUser(EmissaryUser user)
        {
            EmissaryUser existingUser = dbContext.Users.Where(u => u.DiscordId == user.DiscordId).FirstOrDefault();
            if (existingUser == null) {
                dbContext.Users.Add(user);
            } else {
                dbContext.Entry(existingUser).CurrentValues.SetValues(user);
            }
            dbContext.SaveChanges();
        }

        public void RemoveUser(ulong discordId)
        {
            EmissaryUser user = dbContext.Users.Find(discordId);
            if (user != null) {
                dbContext.Users.Remove(user);
                dbContext.SaveChanges();
            }
        }

        public IList<Loadout> GetAllLoadoutsForUser(ulong discordId)
        {
            IList<Loadout> loadouts = dbContext.Loadouts.Where(l => l.DiscordId == discordId).ToList();
            return loadouts;
        }

        public Loadout GetLoadout(ulong discordId, long destinyCharacterId, string loadoutName)
        {
            Loadout foundLoadout = dbContext.Loadouts.Find(discordId, destinyCharacterId, loadoutName);
            return foundLoadout;
        }

        public void AddOrUpdateLoadout(Loadout loadout)
        {
            Loadout existingLoadout = dbContext.Loadouts.Where(l => l.LoadoutName == loadout.LoadoutName && l.DiscordId == loadout.DiscordId && l.DestinyCharacterId == loadout.DestinyCharacterId).AsQueryable().FirstOrDefault();
            if (existingLoadout == null) {
                dbContext.Loadouts.Add(loadout);
            } else {
                dbContext.Entry(existingLoadout).CurrentValues.SetValues(loadout);
            }
            dbContext.SaveChanges();
        }

        public void RemoveLoadout(ulong discordId, long destinyCharacterId, string loadoutName)
        {
            Loadout foundLoadout = dbContext.Loadouts.Find(discordId, destinyCharacterId, loadoutName);
            if (foundLoadout != null) {
                dbContext.Loadouts.Remove(foundLoadout);
                dbContext.SaveChanges();
            }
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