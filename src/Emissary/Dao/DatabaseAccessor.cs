using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace EmissaryCore
{
    public class DatabaseAccessor : IDatabaseAccessor
    {

        public string ExecuteCommand(string commandText, string databasePath)
        {
            try {
                string queryResult = "";
                if (!File.Exists(databasePath)) {
                    throw new DataAccessException($"database file not found. path: {databasePath}");
                }
                using (SqliteConnection db = new SqliteConnection($"Filename={databasePath}")) {
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand(commandText, db);
                    SqliteDataReader reader = selectCommand.ExecuteReader();
                    if (reader.HasRows) {
                        reader.Read();
                        queryResult = reader.GetString(0);
                        db.Close();
                    } else {
                        queryResult = "";
                        db.Close();
                        // throw new DataAccessException("0 rows returned");
                    }
                }
                return queryResult;
            } catch (Exception e) {
                throw new DataAccessException(e.Message);
            }
        }

    }
}