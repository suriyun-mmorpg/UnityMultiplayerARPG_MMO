#if UNITY_STANDALONE && !CLIENT_BUILD
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using LiteNetLibManager;
using MiniJSON;
using System;
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase : BaseDatabase
    {
        [SerializeField]
        private string dbPath = "./mmorpgtemplate.sqlite3";

        [Header("Running In Editor")]
        [SerializeField]
        [Tooltip("You should set this to where you build app to make database path as same as map server")]
        private string editorDbPath = "./mmorpgtemplate.sqlite3";
#if UNITY_STANDALONE && !CLIENT_BUILD
        private SqliteConnection connection;

        public override void Initialize()
        {
            // Json file read
            string configFilePath = "./config/sqliteConfig.json";
            Dictionary<string, object> jsonConfig = new Dictionary<string, object>();
            Logging.Log("Reading config file from " + configFilePath);
            if (File.Exists(configFilePath))
            {
                Logging.Log("Found config file");
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
            if (connection != null)
                connection.Close();
        }

        private void Init()
        {
            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterattribute (
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              dataId INTEGER NOT NULL,
              amount INTEGER NOT NULL,
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS charactercurrency (
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              dataId INTEGER NOT NULL,
              amount INTEGER NOT NULL,
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterbuff (
              id TEXT NOT NULL PRIMARY KEY,
              characterId TEXT NOT NULL,
              type INTEGER NOT NULL,
              dataId INTEGER NOT NULL,
              level INTEGER NOT NULL,
              buffRemainsDuration REAL NOT NULL,
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterhotkey (
              id TEXT NOT NULL PRIMARY KEY,
              characterId TEXT NOT NULL,
              hotkeyId TEXT NOT NULL,
              type INTEGER NOT NULL,
              relateId TEXT NOT NULL DEFAULT '',
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characteritem (
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              inventoryType INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              dataId INTERGER NOT NULL,
              level INTEGER NOT NULL,
              amount INTEGER NOT NULL,
              equipSlotIndex INTEGER NOT NULL DEFAULT 0,
              durability REAL NOT NULL DEFAULT 0,
              exp INTEGER NOT NULL DEFAULT 0,
              lockRemainsDuration REAL NOT NULL DEFAULT 0,
              expireTime INTEGER NOT NULL DEFAULT 0,
              randomSeed INTEGER NOT NULL DEFAULT 0,
              ammo INTEGER NOT NULL DEFAULT 0,
              sockets TEXT NOT NULL DEFAULT '',
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS storageitem (
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              storageType INTEGER NOT NULL,
              storageOwnerId TEXT NOT NULL,
              dataId INTERGER NOT NULL,
              level INTEGER NOT NULL,
              amount INTEGER NOT NULL,
              durability REAL NOT NULL DEFAULT 0,
              exp INTEGER NOT NULL DEFAULT 0,
              lockRemainsDuration REAL NOT NULL DEFAULT 0,
              expireTime INTEGER NOT NULL DEFAULT 0,
              randomSeed INTEGER NOT NULL DEFAULT 0,
              ammo INTEGER NOT NULL DEFAULT 0,
              sockets TEXT NOT NULL DEFAULT '',
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterquest (
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              dataId INTEGER NOT NULL,
              isComplete INTEGER NOT NULL DEFAULT 0,
              isTracking INTEGER NOT NULL DEFAULT 0,
              killedMonsters TEXT NOT NULL DEFAULT '',
              completedTasks TEXT NOT NULL DEFAULT '',
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characters (
              id TEXT NOT NULL PRIMARY KEY,
              userId TEXT NOT NULL,
              dataId INGETER NOT NULL DEFAULT 0,
              entityId INGETER NOT NULL DEFAULT 0,
              factionId INGETER NOT NULL DEFAULT 0,
              characterName TEXT NOT NULL UNIQUE,
              level INTEGER NOT NULL,
              exp INTEGER NOT NULL,
              currentHp INTEGER NOT NULL,
              currentMp INTEGER NOT NULL,
              currentStamina INTEGER NOT NULL,
              currentFood INTEGER NOT NULL,
              currentWater INTEGER NOT NULL,
              equipWeaponSet INTEGER NOT NULL DEFAULT 0,
              statPoint REAL NOT NULL DEFAULT 0,
              skillPoint REAL NOT NULL DEFAULT 0,
              gold INTEGER NOT NULL,
              partyId INTEGER NOT NULL DEFAULT 0,
              guildId INTEGER NOT NULL DEFAULT 0,
              guildRole INTEGER NOT NULL DEFAULT 0,
              sharedGuildExp INTEGER NOT NULL DEFAULT 0,
              currentMapName TEXT NOT NULL,
              currentPositionX REAL NOT NULL DEFAULT 0,
              currentPositionY REAL NOT NULL DEFAULT 0,
              currentPositionZ REAL NOT NULL DEFAULT 0,
              currentRotationX REAL NOT NULL DEFAULT 0,
              currentRotationY REAL NOT NULL DEFAULT 0,
              currentRotationZ REAL NOT NULL DEFAULT 0,
              respawnMapName TEXT NOT NULL,
              respawnPositionX REAL NOT NULL DEFAULT 0,
              respawnPositionY REAL NOT NULL DEFAULT 0,
              respawnPositionZ REAL NOT NULL DEFAULT 0,
              mountDataId INTEGER NOT NULL DEFAULT 0,
              lastDeadTime INTEGER NOT NULL DEFAULT 0,
              unmuteTime INTEGER NOT NULL DEFAULT 0,
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterskill (
              id TEXT NOT NULL PRIMARY KEY,
              idx INTEGER NOT NULL,
              characterId TEXT NOT NULL,
              dataId INTEGER NOT NULL,
              level INTEGER NOT NULL,
              coolDownRemainsDuration REAL NOT NULL,
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characterskillusage (
              id TEXT NOT NULL PRIMARY KEY,
              characterId TEXT NOT NULL,
              type INTEGER NOT NULL DEFAULT 0,
              dataId INTEGER NOT NULL DEFAULT 0,
              coolDownRemainsDuration REAL NOT NULL DEFAULT 0,
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
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
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS friend (
              id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              characterId1 TEXT NOT NULL,
              characterId2 TEXT NOT NULL,
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS userlogin (
              id TEXT NOT NULL PRIMARY KEY,
              username TEXT NOT NULL UNIQUE,
              password TEXT NOT NULL,
              gold INTEGER NOT NULL DEFAULT 0,
              cash INTEGER NOT NULL DEFAULT 0,
              userLevel INTEGER NOT NULL DEFAULT 0,
              unbanTime INTEGER NOT NULL DEFAULT 0,
              email TEXT NOT NULL,
              authType INTEGER NOT NULL,
              accessToken TEXT,
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS buildings (
              id TEXT NOT NULL PRIMARY KEY,
              parentId TEXT NOT NULL,
              entityId INGETER NOT NULL,
              currentHp INTEGER NOT NULL DEFAULT 0,
              remainsLifeTime REAL NOT NULL DEFAULT 0,
              mapName TEXT NOT NULL,
              positionX REAL NOT NULL,
              positionY REAL NOT NULL,
              positionZ REAL NOT NULL,
              rotationX REAL NOT NULL,
              rotationY REAL NOT NULL,
              rotationZ REAL NOT NULL,
              isLocked INTEGER NOT NULL DEFAULT 0,
              lockPassword TEXT NOT NULL DEFAULT '',
              creatorId TEXT NOT NULL,
              creatorName TEXT NOT NULL,
              createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
              updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS guild (
              id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              guildName TEXT NOT NULL,
              leaderId TEXT NOT NULL,
              level INTEGER NOT NULL DEFAULT 1,
              exp INTEGER NOT NULL DEFAULT 0,
              skillPoint INTEGER NOT NULL DEFAULT 0,
              guildMessage TEXT NOT NULL DEFAULT '',
              guildMessage2 TEXT NOT NULL DEFAULT '',
              gold INTEGER NOT NULL DEFAULT 0,
              score INTEGER NOT NULL DEFAULT 0,
              options TEXT NOT NULL DEFAULT '',
              autoAcceptRequests INTEGER NOT NULL DEFAULT 0,
              rank INTEGER NOT NULL DEFAULT 0,
              currentMembers INTEGER NOT NULL DEFAULT 0,
              maxMembers INTEGER NOT NULL DEFAULT 0
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

            ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS mail (
              id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              eventId TEXT NULL DEFAULT NULL,
              senderId TEXT NULL DEFAULT NULL,
              senderName TEXT NULL DEFAULT NULL,
              receiverId TEXT NOT NULL,
              title TEXT NOT NULL,
              content TEXT NOT NULL,
              gold INTEGER NOT NULL DEFAULT 0,
              cash INTEGER NOT NULL DEFAULT 0,
              currencies TEXT NOT NULL,
              items TEXT NOT NULL,
              isRead INTEGER NOT NULL DEFAULT 0,
              readTimestamp TIMESTAMP NULL DEFAULT NULL,
              isClaim INTEGER NOT NULL DEFAULT 0,
              claimTimestamp TIMESTAMP NULL DEFAULT NULL,
              isDelete INTEGER NOT NULL DEFAULT 0,
              deleteTimestamp TIMESTAMP NULL DEFAULT NULL,
              sentTimestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

            // Migrate data
            if (IsColumnExist("buildings", "dataId"))
            {
                if (!IsColumnExist("buildings", "entityId"))
                    ExecuteNonQuery("ALTER TABLE buildings ADD entityId INTEGER NOT NULL DEFAULT 0;");
                // Avoid error which occuring when `dataId` field not found
                foreach (BuildingEntity prefab in GameInstance.BuildingEntities.Values)
                {
                    ExecuteNonQuery("UPDATE buildings SET entityId=@entityId, dataId=0 WHERE dataId=@dataId",
                        new SqliteParameter("entityId", prefab.EntityId),
                        new SqliteParameter("dataId", prefab.name.GenerateHashId()));
                }
                ExecuteNonQuery("ALTER TABLE buildings DROP dataId;");
            }

            // Migrate fields
            if (!IsColumnExist("characterhotkey", "relateId"))
                ExecuteNonQuery("ALTER TABLE characterhotkey ADD relateId TEXT NOT NULL DEFAULT '';");

            if (!IsColumnExist("characteritem", "equipSlotIndex"))
                ExecuteNonQuery("ALTER TABLE characteritem ADD equipSlotIndex INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characteritem", "durability"))
                ExecuteNonQuery("ALTER TABLE characteritem ADD durability REAL NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characteritem", "exp"))
                ExecuteNonQuery("ALTER TABLE characteritem ADD exp INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characteritem", "lockRemainsDuration"))
                ExecuteNonQuery("ALTER TABLE characteritem ADD lockRemainsDuration REAL NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characteritem", "expireTime"))
                ExecuteNonQuery("ALTER TABLE characteritem ADD expireTime INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characteritem", "randomSeed"))
                ExecuteNonQuery("ALTER TABLE characteritem ADD randomSeed INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characteritem", "ammo"))
                ExecuteNonQuery("ALTER TABLE characteritem ADD ammo INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characteritem", "sockets"))
                ExecuteNonQuery("ALTER TABLE characteritem ADD sockets TEXT NOT NULL DEFAULT '';");

            if (!IsColumnExist("storageitem", "durability"))
                ExecuteNonQuery("ALTER TABLE storageitem ADD durability REAL NOT NULL DEFAULT 0;");

            if (!IsColumnExist("storageitem", "exp"))
                ExecuteNonQuery("ALTER TABLE storageitem ADD exp INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("storageitem", "lockRemainsDuration"))
                ExecuteNonQuery("ALTER TABLE storageitem ADD lockRemainsDuration REAL NOT NULL DEFAULT 0;");

            if (!IsColumnExist("storageitem", "expireTime"))
                ExecuteNonQuery("ALTER TABLE storageitem ADD expireTime INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("storageitem", "randomSeed"))
                ExecuteNonQuery("ALTER TABLE storageitem ADD randomSeed INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("storageitem", "ammo"))
                ExecuteNonQuery("ALTER TABLE storageitem ADD ammo INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("storageitem", "sockets"))
                ExecuteNonQuery("ALTER TABLE storageitem ADD sockets TEXT NOT NULL DEFAULT '';");

            if (!IsColumnExist("characterquest", "isTracking"))
                ExecuteNonQuery("ALTER TABLE characterquest ADD isTracking INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characterquest", "completedTasks"))
                ExecuteNonQuery("ALTER TABLE characterquest ADD completedTasks TEXT NOT NULL DEFAULT '';");

            if (!IsColumnExist("userlogin", "gold"))
                ExecuteNonQuery("ALTER TABLE userlogin ADD gold INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("userlogin", "cash"))
                ExecuteNonQuery("ALTER TABLE userlogin ADD cash INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("userlogin", "userLevel"))
                ExecuteNonQuery("ALTER TABLE userlogin ADD userLevel INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("userlogin", "unbanTime"))
                ExecuteNonQuery("ALTER TABLE userlogin ADD unbanTime INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("buildings", "remainsLifeTime"))
                ExecuteNonQuery("ALTER TABLE buildings ADD remainsLifeTime REAL NOT NULL DEFAULT 0;");

            if (!IsColumnExist("buildings", "isLocked"))
                ExecuteNonQuery("ALTER TABLE buildings ADD isLocked INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("buildings", "lockPassword"))
                ExecuteNonQuery("ALTER TABLE buildings ADD lockPassword TEXT NOT NULL DEFAULT '';");

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

            if (!IsColumnExist("characters", "factionId"))
                ExecuteNonQuery("ALTER TABLE characters ADD factionId INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "equipWeaponSet"))
                ExecuteNonQuery("ALTER TABLE characters ADD equipWeaponSet INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "mountDataId"))
                ExecuteNonQuery("ALTER TABLE characters ADD mountDataId INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "currentRotationX"))
                ExecuteNonQuery("ALTER TABLE characters ADD currentRotationX REAL NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "currentRotationY"))
                ExecuteNonQuery("ALTER TABLE characters ADD currentRotationY REAL NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "currentRotationZ"))
                ExecuteNonQuery("ALTER TABLE characters ADD currentRotationZ REAL NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "lastDeadTime"))
                ExecuteNonQuery("ALTER TABLE characters ADD lastDeadTime INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "unmuteTime"))
                ExecuteNonQuery("ALTER TABLE characters ADD unmuteTime INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("guild", "guildMessage2"))
                ExecuteNonQuery("ALTER TABLE guild ADD guildMessage2 TEXT NOT NULL DEFAULT '';");

            if (!IsColumnExist("guild", "gold"))
                ExecuteNonQuery("ALTER TABLE guild ADD gold INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("guild", "score"))
                ExecuteNonQuery("ALTER TABLE guild ADD score INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("guild", "options"))
                ExecuteNonQuery("ALTER TABLE guild ADD options TEXT NOT NULL DEFAULT '';");

            if (!IsColumnExist("guild", "autoAcceptRequests"))
                ExecuteNonQuery("ALTER TABLE guild ADD autoAcceptRequests INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("guild", "rank"))
                ExecuteNonQuery("ALTER TABLE guild ADD rank INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("guild", "currentMembers"))
                ExecuteNonQuery("ALTER TABLE guild ADD currentMembers INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("guild", "maxMembers"))
                ExecuteNonQuery("ALTER TABLE guild ADD maxMembers INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("mail", "cash"))
                ExecuteNonQuery("ALTER TABLE mail ADD cash INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("mail", "isClaim"))
                ExecuteNonQuery("ALTER TABLE mail ADD isClaim INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("mail", "claimTimestamp"))
                ExecuteNonQuery("ALTER TABLE mail ADD claimTimestamp TIMESTAMP NULL DEFAULT NULL;");

            if (!IsColumnType("mail", "gold", "INTEGER"))
            {
                ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS mail_modifying (
                  id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                  eventId TEXT NULL DEFAULT NULL,
                  senderId TEXT NULL DEFAULT NULL,
                  senderName TEXT NULL DEFAULT NULL,
                  receiverId TEXT NOT NULL,
                  title TEXT NOT NULL,
                  content TEXT NOT NULL,
                  gold INTEGER NOT NULL DEFAULT 0,
                  currencies TEXT NOT NULL,
                  items TEXT NOT NULL,
                  isRead INTEGER NOT NULL DEFAULT 0,
                  readTimestamp TIMESTAMP NULL DEFAULT NULL,
                  isClaim INTEGER NOT NULL DEFAULT 0,
                  claimTimestamp TIMESTAMP NULL DEFAULT NULL,
                  isDelete INTEGER NOT NULL DEFAULT 0,
                  deleteTimestamp TIMESTAMP NULL DEFAULT NULL,
                  sentTimestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
                );");
                ExecuteNonQuery("INSERT INTO mail_modifying SELECT * FROM mail;");
                ExecuteNonQuery("ALTER TABLE mail RENAME TO mail_delete;");
                ExecuteNonQuery("ALTER TABLE mail_modifying RENAME TO mail;");
                ExecuteNonQuery("DROP TABLE mail_delete;");
            }

            if (!IsColumnType("characters", "statPoint", "REAL") ||
                !IsColumnType("characters", "skillPoint", "REAL"))
            {
                ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characters_modifying (
                  id TEXT NOT NULL PRIMARY KEY,
                  userId TEXT NOT NULL,
                  dataId INGETER NOT NULL DEFAULT 0,
                  entityId INGETER NOT NULL DEFAULT 0,
                  factionId INGETER NOT NULL DEFAULT 0,
                  characterName TEXT NOT NULL UNIQUE,
                  level INTEGER NOT NULL,
                  exp INTEGER NOT NULL,
                  currentHp INTEGER NOT NULL,
                  currentMp INTEGER NOT NULL,
                  currentStamina INTEGER NOT NULL,
                  currentFood INTEGER NOT NULL,
                  currentWater INTEGER NOT NULL,
                  equipWeaponSet INTEGER NOT NULL DEFAULT 0,
                  statPoint REAL NOT NULL DEFAULT 0,
                  skillPoint REAL NOT NULL DEFAULT 0,
                  gold INTEGER NOT NULL,
                  partyId INTEGER NOT NULL DEFAULT 0,
                  guildId INTEGER NOT NULL DEFAULT 0,
                  guildRole INTEGER NOT NULL DEFAULT 0,
                  sharedGuildExp INTEGER NOT NULL DEFAULT 0,
                  currentMapName TEXT NOT NULL,
                  currentPositionX REAL NOT NULL DEFAULT 0,
                  currentPositionY REAL NOT NULL DEFAULT 0,
                  currentPositionZ REAL NOT NULL DEFAULT 0,
                  currentRotationX REAL NOT NULL DEFAULT 0,
                  currentRotationY REAL NOT NULL DEFAULT 0,
                  currentRotationZ REAL NOT NULL DEFAULT 0,
                  respawnMapName TEXT NOT NULL,
                  respawnPositionX REAL NOT NULL DEFAULT 0,
                  respawnPositionY REAL NOT NULL DEFAULT 0,
                  respawnPositionZ REAL NOT NULL DEFAULT 0,
                  mountDataId INTEGER NOT NULL DEFAULT 0,
                  lastDeadTime INTEGER NOT NULL DEFAULT 0,
                  unmuteTime INTEGER NOT NULL DEFAULT 0,
                  createAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                  updateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
                );");
                ExecuteNonQuery("INSERT INTO characters_modifying SELECT * FROM characters;");
                ExecuteNonQuery("ALTER TABLE characters RENAME TO characters_delete;");
                ExecuteNonQuery("DROP TABLE characters_delete;");
                ExecuteNonQuery("ALTER TABLE characters_modifying RENAME TO characters;");
            }

            this.InvokeInstanceDevExtMethods("Init");
        }

        private bool IsColumnExist(string tableName, string findingColumn)
        {
            using (SqliteCommand cmd = new SqliteCommand("PRAGMA table_info(" + tableName + ");", connection))
            {
                SqliteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetString(1).Equals(findingColumn))
                        return true;
                }
                reader.Close();
            }
            return false;
        }

        private bool IsColumnType(string tableName, string findingColumn, string type)
        {
            using (SqliteCommand cmd = new SqliteCommand("PRAGMA table_info(" + tableName + ");", connection))
            {
                SqliteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetString(1).Equals(findingColumn))
                        return reader.GetString(2).ToLower().Equals(type.ToLower());
                }
                reader.Close();
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
            return ExecuteNonQuery(null, sql, args);
        }

        public int ExecuteNonQuery(SqliteTransaction transaction, string sql, params SqliteParameter[] args)
        {
            int numRows = 0;
            using (SqliteCommand cmd = new SqliteCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (SqliteParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                try
                {
                    numRows = cmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Logging.LogException(ex);
                }
            }
            return numRows;
        }

        public object ExecuteScalar(string sql, params SqliteParameter[] args)
        {
            return ExecuteScalar(null, sql, args);
        }

        public object ExecuteScalar(SqliteTransaction transaction, string sql, params SqliteParameter[] args)
        {
            object result = null;
            using (SqliteCommand cmd = new SqliteCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (SqliteParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                try
                {
                    result = cmd.ExecuteScalar();
                }
                catch (SqliteException ex)
                {
                    Logging.LogException(ex);
                }
            }
            return result;
        }

        public void ExecuteReader(Action<SqliteDataReader> onRead, string sql, params SqliteParameter[] args)
        {
            ExecuteReader(null, onRead, sql, args);
        }

        public void ExecuteReader(SqliteTransaction transaction, Action<SqliteDataReader> onRead, string sql, params SqliteParameter[] args)
        {
            using (SqliteCommand cmd = new SqliteCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (SqliteParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                try
                {
                    SqliteDataReader dataReader = cmd.ExecuteReader();
                    if (onRead != null) onRead.Invoke(dataReader);
                    dataReader.Close();
                }
                catch (SqliteException ex)
                {
                    Logging.LogException(ex);
                }
            }
        }

        public override async UniTask<string> ValidateUserLogin(string username, string password)
        {
            await UniTask.Yield();
            string id = string.Empty;
            ExecuteReader((reader) =>
            {
                if (reader.Read())
                {
                    id = reader.GetString(0);
                    string hashedPassword = reader.GetString(1);
                    if (!password.PasswordVerify(hashedPassword))
                        id = string.Empty;
                }
            }, "SELECT id, password FROM userlogin WHERE username=@username AND authType=@authType LIMIT 1",
                new SqliteParameter("@username", username),
                new SqliteParameter("@authType", AUTH_TYPE_NORMAL));

            return id;
        }

        public override async UniTask<bool> ValidateAccessToken(string userId, string accessToken)
        {
            await UniTask.Yield();
            object result = ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE id=@id AND accessToken=@accessToken",
                new SqliteParameter("@id", userId),
                new SqliteParameter("@accessToken", accessToken));
            return (result != null ? (long)result : 0) > 0;
        }

        public override async UniTask<byte> GetUserLevel(string userId)
        {
            await UniTask.Yield();
            byte userLevel = 0;
            ExecuteReader((reader) =>
            {
                if (reader.Read())
                    userLevel = (byte)reader.GetInt32(0);
            }, "SELECT userLevel FROM userlogin WHERE id=@id LIMIT 1",
                new SqliteParameter("@id", userId));
            return userLevel;
        }

        public override async UniTask<int> GetGold(string userId)
        {
            await UniTask.Yield();
            int gold = 0;
            ExecuteReader((reader) =>
            {
                if (reader.Read())
                    gold = reader.GetInt32(0);
            }, "SELECT gold FROM userlogin WHERE id=@id LIMIT 1",
                new SqliteParameter("@id", userId));
            return gold;
        }

        public override async UniTask UpdateGold(string userId, int gold)
        {
            await UniTask.Yield();
            ExecuteNonQuery("UPDATE userlogin SET gold=@gold WHERE id=@id",
                new SqliteParameter("@id", userId),
                new SqliteParameter("@gold", gold));
        }

        public override async UniTask<int> GetCash(string userId)
        {
            await UniTask.Yield();
            int cash = 0;
            ExecuteReader((reader) =>
            {
                if (reader.Read())
                    cash = reader.GetInt32(0);
            }, "SELECT cash FROM userlogin WHERE id=@id LIMIT 1",
                new SqliteParameter("@id", userId));
            return cash;
        }

        public override async UniTask UpdateCash(string userId, int cash)
        {
            await UniTask.Yield();
            ExecuteNonQuery("UPDATE userlogin SET cash=@cash WHERE id=@id",
                new SqliteParameter("@id", userId),
                new SqliteParameter("@cash", cash));
        }

        public override async UniTask UpdateAccessToken(string userId, string accessToken)
        {
            await UniTask.Yield();
            ExecuteNonQuery("UPDATE userlogin SET accessToken=@accessToken WHERE id=@id",
                new SqliteParameter("@id", userId),
                new SqliteParameter("@accessToken", accessToken));
        }

        public override async UniTask CreateUserLogin(string username, string password)
        {
            await UniTask.Yield();
            ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                new SqliteParameter("@id", GenericUtils.GetUniqueId()),
                new SqliteParameter("@username", username),
                new SqliteParameter("@password", password.PasswordHash()),
                new SqliteParameter("@email", ""),
                new SqliteParameter("@authType", AUTH_TYPE_NORMAL));
        }

        public override async UniTask<long> FindUsername(string username)
        {
            await UniTask.Yield();
            object result = ExecuteScalar("SELECT COUNT(*) FROM userlogin WHERE username LIKE @username",
                new SqliteParameter("@username", username));
            return result != null ? (long)result : 0;
        }

        public override UniTask<long> GetUserUnbanTime(string userId)
        {
            throw new NotImplementedException();
        }

        public override UniTask SetUserUnbanTimeByCharacterName(string characterName, long unbanTime)
        {
            throw new NotImplementedException();
        }

        public override UniTask SetCharacterUnmuteTimeByCharacterName(string characterName, long unmuteTime)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
