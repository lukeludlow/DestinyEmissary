using System;
using System.Collections.Generic;
using System.IO;
using Emissary.Model;
using Microsoft.Data.Sqlite;

namespace Emissary
{

    // CREATE TABLE "UserIds" (
    // 	"discordId"	INTEGER NOT NULL,
    // 	"bungieId"	INTEGER NOT NULL,
    // 	PRIMARY KEY("discordId")
    // );

    // INSERT INTO UserIds (discordId, bungieId) 
    // VALUES (-9002058216007139317, 4611686018467260757);

    public class UserInfoDao : IUserInfoDao
    {
        // TODO create config files for dev and prod that properly specify the db file path.
        // then get it from the ConfigurationManager or something
        private string userInfoDatabasePath;

        public UserInfoDao(string userInfoDatabasePath)
        {
            this.userInfoDatabasePath = userInfoDatabasePath;
        }

        public UserInfoDao()
        {
            this.userInfoDatabasePath = GetUserInfoDatabasePath();
        }

        public long GetUserBungieId(ulong discordId)
        {
            long convertedDiscordId = MapULongToLong(discordId);
            string commandText = $"SELECT bungieId FROM UserIds WHERE discordId = {convertedDiscordId};";
            long bungieId = ExecuteQueryGetBungieId(commandText, userInfoDatabasePath);
            return bungieId;
        }

        public List<Loadout> GetAllLoadouts(ulong discordId)
        {
            throw new NotImplementedException();
        }

        public Loadout GetLoadout(ulong discordId, string loadoutName)
        {
            throw new NotImplementedException();
        }

        public bool AddUser(ulong discordId, long bungieId)
        {
            throw new NotImplementedException();
        }

        public bool AddLoadout(ulong discordId, string loadoutName, Loadout loadout)
        {
            throw new NotImplementedException();
        }

        public bool RemoveLoadout(ulong discordId, string loadoutName)
        {
            throw new NotImplementedException();
        }


        private long ExecuteQueryGetBungieId(string commandText, string databasePath)
        {
            try {
                long queryResult;
                if (!File.Exists(databasePath)) {
                    throw new EmissaryDataAccessException("database file does not exist");
                }
                using (SqliteConnection db = new SqliteConnection($"Filename={databasePath}")) {
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand(commandText, db);
                    SqliteDataReader reader = selectCommand.ExecuteReader();
                    if (reader.HasRows) {
                        reader.Read();
                        queryResult = reader.GetInt64(0);
                        db.Close();
                    } else {
                        db.Close();
                        throw new UserDoesNotExistException("user does not exist in the database");
                    }
                }
                return queryResult;
            } catch (SqliteException e) {
                throw new EmissaryDataAccessException(e.Message);
            }
        }


        // need these two methods so that i can convert the discord ulong to a long.
        // because sqlite does support 8byte int (long), but it's a signed 8byte int, so i 
        // need to convert the unsigned long back and forth just to be safe. 
        // similarly, the sqlite reader class only offers a GetInt64 method.
        public long MapULongToLong(ulong ulongValue)
        {
            return unchecked((long)ulongValue + long.MinValue);
        }

        public ulong MapLongToULong(long longValue)
        {
            return unchecked((ulong)(longValue - long.MinValue));
        }


        private string GetUserInfoDatabasePath()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            string dataDirectory = Path.Combine(solutionDirectory, "data");
            string localManifestFileName = "user-info.db";
            string localManifestFile = Path.Combine(dataDirectory, localManifestFileName);
            return localManifestFile;
        }

    }
}