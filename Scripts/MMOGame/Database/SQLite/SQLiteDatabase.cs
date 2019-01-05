using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
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
            // Json file read
            string configFilePath = "./config/sqliteConfig.json";
            Dictionary<string, object> jsonConfig = new Dictionary<string, object>();
            Debug.Log("[SQLiteDatabase] Reading config file from " + configFilePath);
            if (File.Exists(configFilePath))
            {
                Debug.Log("[SQLiteDatabase] Found config file");
                string dataAsJson = File.ReadAllText(configFilePath);
                jsonConfig = Json.Deserialize(dataAsJson) as Dictionary<string, object>;
            }

            ConfigReader.ReadConfigs(jsonConfig, "sqliteDbPath", out dbPath, dbPath);

            connection = NewConnection();
            connection.Open();
            Init();
        }

        public override void Destroy()
        {
            connection.Close();
        }

        private void Init()
        {
            ExecuteNonQuery("BEGIN");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterattribute (
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              dataId INTEGER NOT NULL,
              amount INTEGER NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterbuff (
              id TEXT NOT NULL PRIMARY KEY,
              characterId TEXT NOT NULL,
              type INTEGER NOT NULL,
              dataId INTEGER NOT NULL,
              level INTEGER NOT NULL,
              buffRemainsDuration REAL NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterhotkey (
              id TEXT NOT NULL PRIMARY KEY,
              characterId TEXT NOT NULL,
              hotkeyId TEXT NOT NULL,
              type INTEGER NOT NULL,
              dataId INTEGER NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characteritem (
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              inventoryType INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              dataId INTERGER NOT NULL,
              level INTEGER NOT NULL,
              amount INTEGER NOT NULL,
              durability REAL NOT NULL DEFAULT 0,
              exp INTEGER NOT NULL DEFAULT 0,
              lockRemainsDuration REAL NOT NULL DEFAULT 0,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterquest (
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              dataId INTEGER NOT NULL,
              isComplete INTEGER NOT NULL,
              killedMonsters TEXT NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characters (
              id TEXT NOT NULL PRIMARY KEY,
              userId TEXT NOT NULL,
              dataId INGETER NOT NULL DEFAULT 0,
              entityId INGETER NOT NULL DEFAULT 0,
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
              partyId INTEGER NOT NULL DEFAULT 0,
              guildId INTEGER NOT NULL DEFAULT 0,
              guildRole INTEGER NOT NULL DEFAULT 0,
              sharedGuildExp INTEGER NOT NULL DEFAULT 0,
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

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterskill (
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              dataId INTEGER NOT NULL,
              level INTEGER NOT NULL,
              coolDownRemainsDuration REAL NOT NULL,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterskillusage (
              id TEXT NOT NULL PRIMARY KEY,
              characterId TEXT NOT NULL,
              type INTEGER NOT NULL DEFAULT 0,
              dataId INTEGER NOT NULL DEFAULT 0,
              coolDownRemainsDuration REAL NOT NULL DEFAULT 0,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS charactersummon (
              id TEXT NOT NULL PRIMARY KEY,
              characterId TEXT NOT NULL,
              type INTEGER NOT NULL DEFAULT 0,
              dataId INTEGER NOT NULL DEFAULT 0,
              summonRemainsDuration REAL NOT NULL DEFAULT 0,
              level INTEGER NOT NULL DEFAULT 0,
              exp INTEGER NOT NULL DEFAULT 0,
              currentHp INTEGER NOT NULL DEFAULT 0,
              currentMp INTEGER NOT NULL DEFAULT 0,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS userlogin (
              id TEXT NOT NULL PRIMARY KEY,
              username TEXT NOT NULL UNIQUE,
              password TEXT NOT NULL,
              cash INTEGER NOT NULL DEFAULT 0,
              userLevel INTEGER NOT NULL DEFAULT 0,
              email TEXT NOT NULL,
              authType INTEGER NOT NULL,
              accessToken TEXT,
              createAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS buildings (
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

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS guild (
              id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              guildName TEXT NOT NULL,
              leaderId TEXT NOT NULL,
              level INTEGER NOT NULL DEFAULT 1,
              exp INTEGER NOT NULL DEFAULT 0,
              skillPoint INTEGER NOT NULL DEFAULT 0,
              guildMessage TEXT NOT NULL DEFAULT ''
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS guildrole (
              guildId INTEGER NOT NULL,
              guildRole INTEGER NOT NULL,
              name TEXT NOT NULL,
              canInvite INTEGER NOT NULL,
              canKick INTEGER NOT NULL,
              shareExpPercentage INTEGER NOT NULL
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS guildskill (
              guildId INTEGER NOT NULL,
              dataId INTEGER NOT NULL,
              level INTEGER NOT NULL
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS party (
              id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              shareExp INTEGER NOT NULL,
              shareItem INTEGER NOT NULL,
              leaderId TEXT NOT NULL
            )");

            ExecuteNonQuery("END");

            // Update data
            if (!IsColumnExist("characteritem", "durability"))
                ExecuteNonQuery("ALTER TABLE characteritem ADD durability REAL NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characteritem", "exp"))
                ExecuteNonQuery("ALTER TABLE characteritem ADD exp INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characteritem", "lockRemainsDuration"))
                ExecuteNonQuery("ALTER TABLE characteritem ADD lockRemainsDuration REAL NOT NULL DEFAULT 0;");

            if (!IsColumnExist("userlogin", "cash"))
                ExecuteNonQuery("ALTER TABLE userlogin ADD cash INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("userlogin", "userLevel"))
                ExecuteNonQuery("ALTER TABLE userlogin ADD userLevel INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "partyId"))
                ExecuteNonQuery("ALTER TABLE characters ADD partyId INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "guildId"))
                ExecuteNonQuery("ALTER TABLE characters ADD guildId INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "guildRole"))
                ExecuteNonQuery("ALTER TABLE characters ADD guildRole INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "sharedGuildExp"))
                ExecuteNonQuery("ALTER TABLE characters ADD sharedGuildExp INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "entityId"))
                ExecuteNonQuery("ALTER TABLE characters ADD entityId INTEGER NOT NULL DEFAULT 0;");

            this.InvokeInstanceDevExtMethods("Init");
        }

        private bool IsColumnExist(string tableName, string findingColumn)
        {
            using (SqliteCommand cmd = new SqliteCommand("PRAGMA table_info(" + tableName + ");", connection))
            {
                DataTable table = new DataTable();

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
            string path = dbPath;
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

        public int ExecuteNonQuery(string sql, params SqliteParameter[] args)
        {
            int numRows = 0;
            using (SqliteCommand cmd = new SqliteCommand(sql, connection))
            {
                foreach (SqliteParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                numRows = cmd.ExecuteNonQuery();
            }
            return numRows;
        }

        public object ExecuteScalar(string sql, params SqliteParameter[] args)
        {
            object result;
            using (SqliteCommand cmd = new SqliteCommand(sql, connection))
            {
                foreach (SqliteParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                result = cmd.ExecuteScalar();
            }
            return result;
        }

        public SQLiteRowsReader ExecuteReader(string sql, params SqliteParameter[] args)
        {
            SQLiteRowsReader result = new SQLiteRowsReader();
            using (SqliteCommand cmd = new SqliteCommand(sql, connection))
            {
                foreach (SqliteParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                SqliteDataReader dataReader = cmd.ExecuteReader();
                result.Init(dataReader);
                dataReader.Close();
            }
            return result;
        }

        public override string ValidateUserLogin(string username, string password)
        {
            string id = string.Empty;
            SQLiteRowsReader reader = ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                new SqliteParameter("@username", username),
                new SqliteParameter("@password", GenericUtils.GetMD5(password)),
                new SqliteParameter("@authType", AUTH_TYPE_NORMAL));

            if (reader.Read())
                id = reader.GetString("id");

            return id;
        }

        public override bool ValidateAccessToken(string userId, string accessToken)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE id=@id AND accessToken=@accessToken",
                new SqliteParameter("@id", userId),
                new SqliteParameter("@accessToken", accessToken));
            return (result != null ? (long)result : 0) > 0;
        }

        public override byte GetUserLevel(string userId)
        {
            byte userLevel = (byte)0;
            SQLiteRowsReader reader = ExecuteReader("SELECT userLevel FROM userlogin WHERE id=@id LIMIT 1",
                new SqliteParameter("@id", userId));
            if (reader.Read())
                userLevel = (byte)reader.GetSByte("userLevel");
            return userLevel;
        }

        public override int GetCash(string userId)
        {
            int cash = 0;
            SQLiteRowsReader reader = ExecuteReader("SELECT cash FROM userlogin WHERE id=@id LIMIT 1",
                new SqliteParameter("@id", userId));
            if (reader.Read())
                cash = reader.GetInt32("cash");
            return cash;
        }

        public override int IncreaseCash(string userId, int amount)
        {
            int cash = GetCash(userId);
            cash += amount;
            ExecuteNonQuery("UPDATE userlogin SET cash=@cash WHERE id=@id",
                new SqliteParameter("@id", userId),
                new SqliteParameter("@cash", cash));
            return cash;
        }

        public override int DecreaseCash(string userId, int amount)
        {
            int cash = GetCash(userId);
            cash -= amount;
            ExecuteNonQuery("UPDATE userlogin SET cash=@cash WHERE id=@id",
                new SqliteParameter("@id", userId),
                new SqliteParameter("@cash", cash));
            return cash;
        }

        public override void UpdateAccessToken(string userId, string accessToken)
        {
            ExecuteNonQuery("UPDATE userlogin SET accessToken=@accessToken WHERE id=@id",
                new SqliteParameter("@id", userId),
                new SqliteParameter("@accessToken", accessToken));
        }

        public override void CreateUserLogin(string username, string password)
        {
            ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                new SqliteParameter("@id", GenericUtils.GetUniqueId()),
                new SqliteParameter("@username", username),
                new SqliteParameter("@password", GenericUtils.GetMD5(password)),
                new SqliteParameter("@email", ""),
                new SqliteParameter("@authType", AUTH_TYPE_NORMAL));
        }

        public override long FindUsername(string username)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE username LIKE @username",
                new SqliteParameter("@username", username));
            return result != null ? (long)result : 0;
        }

        public override string FacebookLogin(string fbId, string accessToken)
        {
            string url = "https://graph.facebook.com/" + fbId + "?access_token=" + accessToken + "&fields=id,name,email";
            WebClient webClient = new WebClient();
            string json = webClient.DownloadString(url);
            json = json.Replace(@"\u0040", "@");

            string id = string.Empty;
            Dictionary<string, object> dict = Json.Deserialize(json) as Dictionary<string, object>;
            if (dict.ContainsKey("id") && dict.ContainsKey("email"))
            {
                string email = (string)dict["email"];
                SQLiteRowsReader reader = ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                    new SqliteParameter("@username", "fb_" + fbId),
                    new SqliteParameter("@password", GenericUtils.GetMD5(fbId)),
                    new SqliteParameter("@authType", AUTH_TYPE_FACEBOOK));

                if (reader.Read())
                    id = reader.GetString("id");
                else
                {
                    ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                        new SqliteParameter("@id", GenericUtils.GetUniqueId()),
                        new SqliteParameter("@username", "fb_" + fbId),
                        new SqliteParameter("@password", GenericUtils.GetMD5(fbId)),
                        new SqliteParameter("@email", email),
                        new SqliteParameter("@authType", AUTH_TYPE_FACEBOOK));

                    // Read last entry
                    reader = ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                        new SqliteParameter("@username", "fb_" + fbId),
                        new SqliteParameter("@password", GenericUtils.GetMD5(fbId)),
                        new SqliteParameter("@authType", AUTH_TYPE_FACEBOOK));

                    if (reader.Read())
                        id = reader.GetString("id");
                }
            }
            return id;
        }

        public override string GooglePlayLogin(string idToken)
        {
            string url = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=" + idToken;
            WebClient webClient = new WebClient();
            string json = webClient.DownloadString(url);

            string id = string.Empty;
            Dictionary<string, object> dict = Json.Deserialize(json) as Dictionary<string, object>;
            if (dict.ContainsKey("sub") && dict.ContainsKey("email"))
            {
                string gId = (string)dict["sub"];
                string email = (string)dict["email"];
                SQLiteRowsReader reader = ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                    new SqliteParameter("@username", "g_" + gId),
                    new SqliteParameter("@password", GenericUtils.GetMD5(gId)),
                    new SqliteParameter("@authType", AUTH_TYPE_GOOGLE_PLAY));

                if (reader.Read())
                    id = reader.GetString("id");
                else
                {
                    ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                        new SqliteParameter("@id", GenericUtils.GetUniqueId()),
                        new SqliteParameter("@username", "g_" + gId),
                        new SqliteParameter("@password", GenericUtils.GetMD5(gId)),
                        new SqliteParameter("@email", email),
                        new SqliteParameter("@authType", AUTH_TYPE_GOOGLE_PLAY));

                    // Read last entry
                    reader = ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
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
