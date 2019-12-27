using System;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EmissaryCore
{
    public class EmissaryDbContext : DbContext
    {
        public DbSet<EmissaryUser> Users { get; set; }
        public DbSet<Loadout> Loadouts { get; set; }

        public EmissaryDbContext()
        {
            // for this constructor, the OnConfiguring method will be called to configure the database.
        }

        public EmissaryDbContext(DbContextOptions<EmissaryDbContext> options)
            : base(options)
        {
        }

        // private string GetDbPath()
        // {
        //     string solutionDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
        //     string dataDirectory = Path.Combine(solutionDirectory, "data");
        //     string localDbFileName = "users-test.db";
        //     string localDbFilePath = Path.Combine(dataDirectory, localDbFileName);
        //     return localDbFilePath;
        // }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Loadout>()
                        .HasKey(l => new { l.DiscordId, l.DestinyCharacterId, l.LoadoutName });
            modelBuilder.Entity<Loadout>()
                        .Property(l => l.Items)
                        .HasConversion(
                            v => JsonConvert.SerializeObject(v),
                            v => JsonConvert.DeserializeObject<List<DestinyItem>>(v)
                        );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // TODO figure out what the current path of the actual database is and set it here
            // i.e. prod is ./src/data/emissary.db, dev is ../../../src/data/emissary.db
            if (!optionsBuilder.IsConfigured) {
                // "DataSource={databasePath}  e.g. full path of src/data/user-info.db
                optionsBuilder.UseSqlite("idk");
            }
        }
    }
}