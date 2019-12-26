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
            // LoadoutDbEntity loadoutEntity = dbContext.Loadouts.Find(discordId, loadoutName);
            // Loadout foundLoadout;
            // if (loadoutEntity != null) {
            //     foundLoadout = loadoutEntity.ToLoadout();
            // } else {
            //     foundLoadout = null;
            // }
            // return foundLoadout;
            throw new NotImplementedException();
        }

        public void AddOrUpdateLoadout(Loadout loadout)
        {
            // LoadoutDbEntity loadoutEntity = new LoadoutDbEntity(loadout);
            // LoadoutDbEntity existingLoadout = dbContext.Loadouts.Where(l => l.Name == loadoutEntity.Name && l.DiscordID == loadoutEntity.DiscordID).AsQueryable().FirstOrDefault();
            // if (existingLoadout == null) {
            //     dbContext.Loadouts.Add(loadoutEntity);
            // } else {
            //     dbContext.Entry(existingLoadout).CurrentValues.SetValues(loadoutEntity);
            // }
            // dbContext.SaveChanges();
            throw new NotImplementedException();
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