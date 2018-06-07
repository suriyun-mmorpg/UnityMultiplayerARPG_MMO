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
        [SerializeField]
        private string dbPath = "./mmorpgtemplate.sqlite3";
        private SqliteConnection connection;

        private void Awake()
        {
            connection = new SqliteConnection(GetConnectionString());
            connection.Open();
            Init();
        }

        private void OnDestroy()
        {
            if (connection != null)
                connection.Close();
        }

        private async void Init()
        {
            await ExecuteNonQuery("BEGIN");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterattribute (
              id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              characterId TEXT NOT NULL,
              attributeId TEXT NOT NULL,
              amount INTEGER NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterbuff (
              id TEXT NOT NULL PRIMARY KEY,
              characterId TEXT NOT NULL,
              type INTEGER NOT NULL,
              dataId TEXT NOT NULL,
              level INTEGER NOT NULL,
              buffRemainsDuration REAL NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterhotkey (
              id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              characterId TEXT NOT NULL,
              hotkeyId TEXT NOT NULL,
              type INTEGER NOT NULL,
              dataId TEXT NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterinventory (
              id TEXT NOT NULL PRIMARY KEY,
              inventoryType INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              itemId TEXT NOT NULL,
              level INTEGER NOT NULL,
              amount INTEGER NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterquest (
              id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              characterId TEXT NOT NULL,
              questId TEXT NOT NULL,
              isComplete INTEGER NOT NULL,
              killedMonsters TEXT NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characters (
              id TEXT NOT NULL PRIMARY KEY,
              userId TEXT NOT NULL,
              databaseId TEXT NOT NULL,
              characterName TEXT NOT NULL UNIQUE,
              level INTEGER NOT NULL,
              exp INTEGER NOT NULL,
              currentHp INTEGER NOT NULL,
              currentMp INTEGER NOT NULL,
              currentStamina INTEGER NOT NULL,
              currentFood INTEGER NOT NULL,
              currentWater INTEGER NOT NULL,
              statPoint INTEGER NOT NULL,
              skillPoint INTEGER NOT NULL,
              gold INTEGER NOT NULL,
              currentMapName TEXT NOT NULL,
              currentPositionX REAL NOT NULL,
              currentPositionY REAL NOT NULL,
              currentPositionZ REAL NOT NULL,
              respawnMapName TEXT NOT NULL,
              respawnPositionX REAL NOT NULL,
              respawnPositionY REAL NOT NULL,
              respawnPositionZ REAL NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterskill (
              id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              characterId TEXT NOT NULL,
              skillId TEXT NOT NULL,
              level INTEGER NOT NULL,
              coolDownRemainsDuration REAL NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS userlogin (
              id TEXT NOT NULL PRIMARY KEY,
              username TEXT NOT NULL UNIQUE,
              password TEXT NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery("END");
        }

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

        public async Task<int> ExecuteNonQuery(string sql, params SqliteParameter[] args)
        {
            var numRows = 0;
            using (var cmd = new SqliteCommand(sql, connection))
            {
                foreach (var arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                numRows = await cmd.ExecuteNonQueryAsync();
            }
            return numRows;
        }

        public async Task<object> ExecuteScalar(string sql, params SqliteParameter[] args)
        {
            object result;
            using (var cmd = new SqliteCommand(sql, connection))
            {
                foreach (var arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                result = await cmd.ExecuteScalarAsync();
            }
            return result;
        }

        public async Task<SQLiteRowsReader> ExecuteReader(string sql, params SqliteParameter[] args)
        {
            SQLiteRowsReader result = new SQLiteRowsReader();
            using (var cmd = new SqliteCommand(sql, connection))
            {
                foreach (var arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                var dataReader = await cmd.ExecuteReaderAsync();
                result.Init(dataReader);
                dataReader.Close();
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
