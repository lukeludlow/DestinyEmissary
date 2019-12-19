using System;
using System.Collections.Generic;
using System.Linq;

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

        public void AddUser(EmissaryUser user)
        {
            context.Users.Add(user);
        }

        public void RemoveUser(ulong discordId)
        {
            EmissaryUser user = context.Users.Find(discordId);
            context.Users.Remove(user);
        }

        public void Save()
        {
            context.SaveChanges();
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