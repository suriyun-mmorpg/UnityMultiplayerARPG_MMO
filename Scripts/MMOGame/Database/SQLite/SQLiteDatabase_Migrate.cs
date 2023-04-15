#if NET || NETCOREAPP
using Microsoft.Data.Sqlite;
#elif (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
using Mono.Data.Sqlite;
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        [DevExtMethods("Init")]
        public void Migrate()
        {
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

            if (!IsColumnExist("friend", "state"))
                ExecuteNonQuery("ALTER TABLE friend ADD state INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("userlogin", "gold"))
                ExecuteNonQuery("ALTER TABLE userlogin ADD gold INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("userlogin", "cash"))
                ExecuteNonQuery("ALTER TABLE userlogin ADD cash INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("userlogin", "userLevel"))
                ExecuteNonQuery("ALTER TABLE userlogin ADD userLevel INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("userlogin", "unbanTime"))
                ExecuteNonQuery("ALTER TABLE userlogin ADD unbanTime INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("userlogin", "isEmailVerified"))
                ExecuteNonQuery("ALTER TABLE userlogin ADD isEmailVerified INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("buildings", "remainsLifeTime"))
                ExecuteNonQuery("ALTER TABLE buildings ADD remainsLifeTime REAL NOT NULL DEFAULT 0;");

            if (!IsColumnExist("buildings", "isLocked"))
                ExecuteNonQuery("ALTER TABLE buildings ADD isLocked INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("buildings", "lockPassword"))
                ExecuteNonQuery("ALTER TABLE buildings ADD lockPassword TEXT NOT NULL DEFAULT '';");

            if (!IsColumnExist("buildings", "extraData"))
                ExecuteNonQuery("ALTER TABLE buildings ADD extraData TEXT NOT NULL DEFAULT '';");

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

            if (!IsColumnExist("characters", "iconDataId"))
                ExecuteNonQuery("ALTER TABLE characters ADD iconDataId INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "frameDataId"))
                ExecuteNonQuery("ALTER TABLE characters ADD frameDataId INTEGER NOT NULL DEFAULT 0;");

            if (!IsColumnExist("characters", "titleDataId"))
                ExecuteNonQuery("ALTER TABLE characters ADD titleDataId INTEGER NOT NULL DEFAULT 0;");

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

            if (!IsColumnExist("guildrole", "canUseStorage"))
                ExecuteNonQuery("ALTER TABLE guildrole ADD canUseStorage INTEGER NOT NULL DEFAULT 0;");

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

            if (!IsColumnType("characters", "statPoint", "REAL") || !IsColumnType("characters", "skillPoint", "REAL"))
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
        }
    }
}
#endif