using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using MiniJSON;

namespace MultiplayerARPG.MMO
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
        [Header("Running In Editor")]
        [SerializeField]
        [Tooltip("You should set this to where you build app to make database path as same as map server")]
        private string editorDbPath = "./mmorpgtemplate.sqlite3";
        private SqliteConnection connection;

        public override void Initialize()
        {
            connection = NewConnection();
            connection.Open();
            Init();
        }

        public override void Destroy()
        {
            connection.Close();
        }

        private async void Init()
        {
            await ExecuteNonQuery("BEGIN");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterattribute (
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              dataId INTEGER NOT NULL,
              amount INTEGER NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterbuff (
              id TEXT NOT NULL PRIMARY KEY,
              characterId TEXT NOT NULL,
              type INTEGER NOT NULL,
              dataId INTEGER NOT NULL,
              level INTEGER NOT NULL,
              buffRemainsDuration REAL NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterhotkey (
              id TEXT NOT NULL PRIMARY KEY,
              characterId TEXT NOT NULL,
              hotkeyId TEXT NOT NULL,
              type INTEGER NOT NULL,
              dataId INTEGER NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characteritem (
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              inventoryType INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              dataId INTERGER NOT NULL,
              level INTEGER NOT NULL,
              amount INTEGER NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterquest (
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              dataId INTEGER NOT NULL,
              isComplete INTEGER NOT NULL,
              killedMonsters TEXT NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characters (
              id TEXT NOT NULL PRIMARY KEY,
              userId TEXT NOT NULL,
              dataId INGETER NOT NULL,
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
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              dataId INTEGER NOT NULL,
              level INTEGER NOT NULL,
              coolDownRemainsDuration REAL NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS userlogin (
              id TEXT NOT NULL PRIMARY KEY,
              username TEXT NOT NULL UNIQUE,
              password TEXT NOT NULL,
              email TEXT NOT NULL,
              authType INTEGER NOT NULL,
              accessToken TEXT,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS buildings (
              id TEXT NOT NULL PRIMARY KEY,
              parentId TEXT NOT NULL,
              dataId INTEGER NOT NULL,
              currentHp INTEGER NOT NULL,
              mapName TEXT NOT NULL,
              positionX REAL NOT NULL,
              positionY REAL NOT NULL,
              positionZ REAL NOT NULL,
              rotationX REAL NOT NULL,
              rotationY REAL NOT NULL,
              rotationZ REAL NOT NULL,
              creatorId TEXT NOT NULL,
              creatorName TEXT NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            await ExecuteNonQuery("END");
            
            this.InvokeInstanceDevExtMethods("Init");

            // Update data
            if (!IsColumnExist("characteritem", "durability"))
                await ExecuteNonQuery("ALTER TABLE characteritem ADD durability REAL NOT NULL DEFAULT 0;");
            if (!IsColumnExist("userlogin", "cash"))
                await ExecuteNonQuery("ALTER TABLE userlogin ADD cash INTEGER NOT NULL DEFAULT 0;");
        }

        private bool IsColumnExist(string tableName, string findingColumn)
        {
            using (var cmd = new SqliteCommand("PRAGMA table_info(" + tableName + ");", connection))
            {
                var table = new DataTable();

                SqliteDataAdapter adp = null;
                try
                {
                    adp = new SqliteDataAdapter(cmd);
                    adp.Fill(table);
                    for (int i = 0; i < table.Rows.Count; ++i)
                    {
                        if (table.Rows[i]["name"].ToString().Equals(findingColumn))
                            return true;
                    }
                }
                catch { }
            }
            return false;
        }

        public string GetConnectionString()
        {
            var path = dbPath;
            if (Application.isMobilePlatform)
            {
                if (path.StartsWith("./"))
                    path = path.Substring(1);
                if (!path.StartsWith("/"))
                    path = "/" + path;
                path = Application.persistentDataPath + path;
            }

            if (Application.isEditor)
                path = editorDbPath;

            if (!File.Exists(path))
                SqliteConnection.CreateFile(path);

            return "URI=file:" + path;
        }

        public SqliteConnection NewConnection()
        {
            return new SqliteConnection(GetConnectionString());
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
            }
            return result;
        }

        public override async Task<string> ValidateUserLogin(string username, string password)
        {
            var id = string.Empty;
            var reader = await ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                new SqliteParameter("@username", username),
                new SqliteParameter("@password", GenericUtils.GetMD5(password)),
                new SqliteParameter("@authType", AUTH_TYPE_NORMAL));

            if (reader.Read())
                id = reader.GetString("id");

            return id;
        }

        public override async Task<bool> ValidateAccessToken(string userId, string accessToken)
        {
            var result = await ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE id=@id AND accessToken=@accessToken",
                new SqliteParameter("@id", userId),
                new SqliteParameter("@accessToken", accessToken));
            return (result != null ? (long)result : 0) > 0;
        }

        public override async Task<int> GetCash(string userId)
        {
            var cash = 0;
            var reader = await ExecuteReader("SELECT cash FROM userlogin WHERE id=@id LIMIT 1",
                new SqliteParameter("@id", userId));
            if (reader.Read())
                cash = reader.GetInt32("cash");
            return cash;
        }

        public override async Task UpdateAccessToken(string userId, string accessToken)
        {
            await ExecuteNonQuery("UPDATE userlogin SET accessToken=@accessToken WHERE id=@id",
                new SqliteParameter("@id", userId),
                new SqliteParameter("@accessToken", accessToken));
        }

        public override async Task CreateUserLogin(string username, string password)
        {
            await ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                new SqliteParameter("@id", GenericUtils.GetUniqueId()),
                new SqliteParameter("@username", username),
                new SqliteParameter("@password", GenericUtils.GetMD5(password)),
                new SqliteParameter("@email", ""),
                new SqliteParameter("@authType", AUTH_TYPE_NORMAL));
        }

        public override async Task<long> FindUsername(string username)
        {
            var result = await ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE username LIKE @username",
                new SqliteParameter("@username", username));
            return result != null ? (long)result : 0;
        }

        public override async Task<string> FacebookLogin(string fbId, string accessToken)
        {
            var url = "https://graph.facebook.com/" + fbId + "?access_token=" + accessToken + "&fields=id,name,email";
            var webClient = new WebClient();
            var json = await webClient.DownloadStringTaskAsync(url);
            json = json.Replace(@"\u0040", "@");

            var id = string.Empty;
            var dict = Json.Deserialize(json) as Dictionary<string, object>;
            if (dict.ContainsKey("id") && dict.ContainsKey("email"))
            {
                var email = (string)dict["email"];
                var reader = await ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                    new SqliteParameter("@username", "fb_" + fbId),
                    new SqliteParameter("@password", GenericUtils.GetMD5(fbId)),
                    new SqliteParameter("@authType", AUTH_TYPE_FACEBOOK));

                if (reader.Read())
                    id = reader.GetString("id");
                else
                {
                    await ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                        new SqliteParameter("@id", GenericUtils.GetUniqueId()),
                        new SqliteParameter("@username", "fb_" + fbId),
                        new SqliteParameter("@password", GenericUtils.GetMD5(fbId)),
                        new SqliteParameter("@email", email),
                        new SqliteParameter("@authType", AUTH_TYPE_FACEBOOK));

                    // Read last entry
                    reader = await ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                        new SqliteParameter("@username", "fb_" + fbId),
                        new SqliteParameter("@password", GenericUtils.GetMD5(fbId)),
                        new SqliteParameter("@authType", AUTH_TYPE_FACEBOOK));

                    if (reader.Read())
                        id = reader.GetString("id");
                }
            }
            return id;
        }

        public override async Task<string> GooglePlayLogin(string idToken)
        {
            var url = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=" + idToken;
            var webClient = new WebClient();
            var json = await webClient.DownloadStringTaskAsync(url);

            var id = string.Empty;
            var dict = Json.Deserialize(json) as Dictionary<string, object>;
            if (dict.ContainsKey("sub") && dict.ContainsKey("email"))
            {
                var gId = (string)dict["sub"];
                var email = (string)dict["email"];
                var reader = await ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                    new SqliteParameter("@username", "g_" + gId),
                    new SqliteParameter("@password", GenericUtils.GetMD5(gId)),
                    new SqliteParameter("@authType", AUTH_TYPE_GOOGLE_PLAY));

                if (reader.Read())
                    id = reader.GetString("id");
                else
                {
                    await ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                        new SqliteParameter("@id", GenericUtils.GetUniqueId()),
                        new SqliteParameter("@username", "g_" + gId),
                        new SqliteParameter("@password", GenericUtils.GetMD5(gId)),
                        new SqliteParameter("@email", email),
                        new SqliteParameter("@authType", AUTH_TYPE_GOOGLE_PLAY));

                    // Read last entry
                    reader = await ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                        new SqliteParameter("@username", "g_" + gId),
                        new SqliteParameter("@password", GenericUtils.GetMD5(gId)),
                        new SqliteParameter("@authType", AUTH_TYPE_GOOGLE_PLAY));

                    if (reader.Read())
                        id = reader.GetString("id");
                }
            }
            return id;
        }
    }
}
