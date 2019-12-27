using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EmissaryCore
{
    public class LoadoutDao : ILoadoutDao, IDisposable
    {
        private EmissaryDbContext dbContext;

        public LoadoutDao(EmissaryDbContext context)
        {
            this.dbContext = context;
        }

        public IList<Loadout> GetAllLoadoutsForUser(ulong discordId)
        {
            // IList<Loadout> loadouts = dbContext.Loadouts.Where(l => l.DiscordID == discordId)
            //                                                 .Select(l => l.ToLoadout())
            //                                                 .ToList();
            // return loadouts;
            throw new NotImplementedException();
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
            // LoadoutDbEntity loadoutEntity = dbContext.Loadouts.Find(discordId, loadoutName);
            // if (loadoutEntity != null) {
            //     dbContext.Loadouts.Remove(loadoutEntity);
            //     dbContext.SaveChanges();
            // }
            throw new NotImplementedException();
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