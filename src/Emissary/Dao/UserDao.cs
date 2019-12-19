using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Emissary
{
    public class UserDao : IUserDao, IDisposable
    {
        private EmissaryUsersDbContext context;

        public UserDao(EmissaryUsersDbContext context)
        {
            this.context = context;
        }

        public IList<EmissaryUser> GetUsers()
        {
            return context.Users.ToList();
        }

        public EmissaryUser GetUserByDiscordId(ulong discordId)
        {
            return context.Users.Find(discordId);
        }

        public void AddOrUpdateUser(EmissaryUser user)
        {
            EmissaryUser existingUser = context.Users.Where(u => u.DiscordID == user.DiscordID).AsQueryable().FirstOrDefault();
            if (existingUser == null) {
                context.Users.Add(user);
            } else {
                context.Entry(existingUser).CurrentValues.SetValues(user);
            }
            context.SaveChanges();
        }

        public void RemoveUser(ulong discordId)
        {
            EmissaryUser user = context.Users.Find(discordId);
            if (user != null) {
                context.Users.Remove(user);
                context.SaveChanges();
            }
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed) {
                if (disposing) {
                    context.Dispose();
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