using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EmissaryCore
{
    public class UserDao : IUserDao, IDisposable
    {
        private EmissaryDbContext dbContext;

        public UserDao(EmissaryDbContext context)
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