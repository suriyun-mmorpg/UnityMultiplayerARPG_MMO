using Mono.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Insthync.MMOG
{
    public partial class SQLiteDatabase : BaseDatabase
    {
        public enum InventoryType : byte
        {
            NonEquipItems,
            EquipItems,
            EquipWeaponRight,
            EquipWeaponLeft,
        }
        public string dbPath = "./mmorpgtemplate.sqlite3";

        public string GetConnectionString()
        {
            if (Application.isMobilePlatform)
            {
                if (dbPath.StartsWith("./"))
                    dbPath = dbPath.Substring(1);
                if (!dbPath.StartsWith("/"))
                    dbPath = "/" + dbPath;
                dbPath = Application.persistentDataPath + dbPath;
            }

            if (!File.Exists(dbPath))
                SqliteConnection.CreateFile(dbPath);
            
            return "URI=file:" + dbPath;
        }

        public void SetupConnection(string dbPath)
        {
            this.dbPath = dbPath;
        }

        public async Task ExecuteNonQuery(string sql, params SqliteParameter[] args)
        {
            using (var connection = new SqliteConnection(GetConnectionString()))
            {
                connection.Open();
                using (var cmd = new SqliteCommand(sql, connection))
                {
                    foreach (var arg in args)
                    {
                        cmd.Parameters.Add(arg);
                    }
                    var task = await cmd.ExecuteNonQueryAsync();
                }
                connection.Close();
            }
        }

        public async Task<object> ExecuteScalar(string sql, params SqliteParameter[] args)
        {
            object result;
            using (var connection = new SqliteConnection(GetConnectionString()))
            {
                connection.Open();
                using (var cmd = new SqliteCommand(sql, connection))
                {
                    foreach (var arg in args)
                    {
                        cmd.Parameters.Add(arg);
                    }
                    result = await cmd.ExecuteScalarAsync();
                }
                connection.Close();
            }
            return result;
        }

        public async Task<SQLiteRowsReader> ExecuteReader(string sql, params SqliteParameter[] args)
        {
            SQLiteRowsReader result = new SQLiteRowsReader();
            using (var connection = new SqliteConnection(GetConnectionString()))
            {
                connection.Open();
                using (var cmd = new SqliteCommand(sql, connection))
                {
                    foreach (var arg in args)
                    {
                        cmd.Parameters.Add(arg);
                    }
                    result.Init(await cmd.ExecuteReaderAsync());
                }
                connection.Close();
            }
            return result;
        }

        public override async Task<string> ValidateUserLogin(string username, string password)
        {
            var id = string.Empty;
            var reader = await ExecuteReader("SELECT id FROM userLogin WHERE username=@username AND password=@password LIMIT 1",
                new SqliteParameter("@username", username),
                new SqliteParameter("@password", password));

            if (reader.Read())
                id = reader.GetString("id");

            return id;
        }

        public override async Task CreateUserLogin(string username, string password)
        {
            await ExecuteNonQuery("INSERT INTO userLogin (id, username, password) VALUES (@id, @username, @password)",
                new SqliteParameter("@id", System.Guid.NewGuid().ToString()),
                new SqliteParameter("@username", username),
                new SqliteParameter("@password", password));
        }

        public override async Task<long> FindUsername(string username)
        {
            var result = await ExecuteScalar("SELECT COUNT(*) FROM userLogin WHERE username LIKE @username", 
                new SqliteParameter("@username", username));
            return result != null ? (long)result : 0;
        }
    }
}
