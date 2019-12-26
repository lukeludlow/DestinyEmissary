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

        public IList<EmissaryUser> GetUsers()
        {
            // return dbContext.Users.ToList();
            throw new NotImplementedException();
        }

        public EmissaryUser GetUserByDiscordId(ulong discordId)
        {
            // return dbContext.Users.Find(discordId);
            throw new NotImplementedException();
        }

        public void AddOrUpdateUser(EmissaryUser user)
        {
            // EmissaryUser existingUser = dbContext.Users.Where(u => u.DiscordID == user.DiscordID).AsQueryable().FirstOrDefault();
            // if (existingUser == null) {
            //     dbContext.Users.Add(user);
            // } else {
            //     dbContext.Entry(existingUser).CurrentValues.SetValues(user);
            // }
            // dbContext.SaveChanges();
            throw new NotImplementedException();
        }

        public void RemoveUser(ulong discordId)
        {
            // EmissaryUser user = dbContext.Users.Find(discordId);
            // if (user != null) {
            //     dbContext.Users.Remove(user);
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