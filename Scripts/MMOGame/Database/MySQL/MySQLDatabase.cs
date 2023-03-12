#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.IO;
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase : BaseDatabase
    {
        public static readonly string LogTag = nameof(MySQLDatabase);

        [SerializeField]
        private string address = "127.0.0.1";
        [SerializeField]
        private int port = 3306;
        [SerializeField]
        private string username = "root";
        [SerializeField]
        private string password = "";
        [SerializeField]
        private string dbName = "mmorpgtemplate";

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public override void Initialize()
        {
            // Json file read
            bool configFileFound = false;
            string configFolder = "./config";
            string configFilePath = configFolder + "/mySqlConfig.json";
            MySQLConfig config = new MySQLConfig()
            {
                mySqlAddress = address,
                mySqlPort = port,
                mySqlUsername = username,
                mySqlPassword = password,
                mySqlDbName = dbName,
            };
            LogInformation(LogTag, "Reading config file from " + configFilePath);
            if (File.Exists(configFilePath))
            {
                LogInformation(LogTag, "Found config file");
                string dataAsJson = File.ReadAllText(configFilePath);
                MySQLConfig replacingConfig = JsonConvert.DeserializeObject<MySQLConfig>(dataAsJson);
                if (!string.IsNullOrWhiteSpace(replacingConfig.mySqlAddress))
                    config.mySqlAddress = replacingConfig.mySqlAddress;
                if (replacingConfig.mySqlPort.HasValue)
                    config.mySqlPort = replacingConfig.mySqlPort.Value;
                if (!string.IsNullOrWhiteSpace(replacingConfig.mySqlUsername))
                    config.mySqlUsername = replacingConfig.mySqlUsername;
                if (!string.IsNullOrWhiteSpace(replacingConfig.mySqlPassword))
                    config.mySqlPassword = replacingConfig.mySqlPassword;
                if (!string.IsNullOrWhiteSpace(replacingConfig.mySqlDbName))
                    config.mySqlDbName = replacingConfig.mySqlDbName;
                configFileFound = true;
            }

            address = config.mySqlAddress;
            port = config.mySqlPort.Value;
            username = config.mySqlUsername;
            password = config.mySqlPassword;
            dbName = config.mySqlDbName;

            if (!configFileFound)
            {
                // Write config file
                LogInformation(LogTag, "Not found config file, creating a new one");
                if (!Directory.Exists(configFolder))
                    Directory.CreateDirectory(configFolder);
                File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config));
            }

            Migration();
            this.InvokeInstanceDevExtMethods("Init");
        }

        private void Migration()
        {
            // 1.57b
            string migrationId = "1.57b";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                // Migrate data
                try
                {
                    // Avoid exception which occuring when `dataId` field not found
                    foreach (BuildingEntity prefab in GameInstance.BuildingEntities.Values)
                    {
                        ExecuteNonQuerySync("UPDATE buildings SET entityId=@entityId, dataId=0 WHERE dataId=@dataId",
                            new MySqlParameter("entityId", prefab.EntityId),
                            new MySqlParameter("dataId", prefab.name.GenerateHashId()));
                    }
                }
                catch { }
                // Migrate fields
                try
                {
                    // Avoid exception which occuring when `dataId` field not found
                    ExecuteNonQuerySync("ALTER TABLE buildings DROP dataId;");
                }
                catch { }
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.58";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `characterbuff` CHANGE `type` `type` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `characterhotkey` CHANGE `type` `type` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `characteritem` CHANGE `inventoryType` `inventoryType` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `characterskillusage` CHANGE `type` `type` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `charactersummon` CHANGE `type` `type` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `storageitem` CHANGE `storageType` `storageType` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `userlogin` CHANGE `authType` `authType` TINYINT(3) UNSIGNED NOT NULL DEFAULT '1';");
                ExecuteNonQuerySync("ALTER TABLE `userlogin` CHANGE `userLevel` `userLevel` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `characters` ADD `currentRotationX` FLOAT NOT NULL DEFAULT '0' AFTER `currentPositionZ`;");
                ExecuteNonQuerySync("ALTER TABLE `characters` ADD `currentRotationY` FLOAT NOT NULL DEFAULT '0' AFTER `currentRotationX`;");
                ExecuteNonQuerySync("ALTER TABLE `characters` ADD `currentRotationZ` FLOAT NOT NULL DEFAULT '0' AFTER `currentRotationY`;");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.60c";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `characterquest` ADD `completedTasks` TEXT NOT NULL AFTER `killedMonsters`;");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.61";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("CREATE TABLE `charactercurrency` ("
                    + "`id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`idx` int(11) NOT NULL,"
                    + "`characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`dataId` int(11) NOT NULL DEFAULT '0',"
                    + "`amount` int(11) NOT NULL DEFAULT '0',"
                    + "`createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,"
                    + "`updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,"
                    + "PRIMARY KEY(`id`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                ExecuteNonQuerySync("CREATE TABLE `mail` ("
                    + "`id` bigint(20) NOT NULL AUTO_INCREMENT,"
                    + "`eventId` varchar(50) NULL DEFAULT NULL,"
                    + "`senderId` varchar(50) NULL DEFAULT NULL,"
                    + "`senderName` varchar(32) NULL DEFAULT NULL,"
                    + "`receiverId` varchar(50) NOT NULL,"
                    + "`title` varchar(160) NOT NULL,"
                    + "`content` text NOT NULL,"
                    + "`gold` int(11) NOT NULL,"
                    + "`currencies` TEXT NOT NULL,"
                    + "`items` text NOT NULL,"
                    + "`isRead` tinyint(1) NOT NULL DEFAULT FALSE,"
                    + "`readTimestamp` timestamp NULL DEFAULT NULL,"
                    + "`isDelete` tinyint(1) NOT NULL DEFAULT FALSE,"
                    + "`deleteTimestamp` timestamp NULL DEFAULT NULL,"
                    + "`sentTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,"
                    + "PRIMARY KEY(`id`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE utf8_unicode_ci;");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.61b";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `mail` ADD `isClaim` tinyint(1) NOT NULL DEFAULT 0 AFTER `readTimestamp`, ADD `claimTimestamp` timestamp NULL DEFAULT NULL AFTER `isClaim`;");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.62e";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `characterattribute` ADD INDEX(`idx`);");
                ExecuteNonQuerySync("ALTER TABLE `characterattribute` ADD INDEX(`characterId`);");
                ExecuteNonQuerySync("ALTER TABLE `characterbuff` ADD INDEX(`characterId`);");
                ExecuteNonQuerySync("ALTER TABLE `charactercurrency` ADD INDEX(`idx`);");
                ExecuteNonQuerySync("ALTER TABLE `charactercurrency` ADD INDEX(`characterId`);");
                ExecuteNonQuerySync("ALTER TABLE `characterhotkey` ADD INDEX(`characterId`);");
                ExecuteNonQuerySync("ALTER TABLE `characterhotkey` ADD INDEX(`hotkeyId`);");
                ExecuteNonQuerySync("ALTER TABLE `characteritem` ADD INDEX(`idx`);");
                ExecuteNonQuerySync("ALTER TABLE `characteritem` ADD INDEX(`inventoryType`);");
                ExecuteNonQuerySync("ALTER TABLE `characteritem` ADD INDEX(`characterId`);");
                ExecuteNonQuerySync("ALTER TABLE `characterquest` ADD INDEX(`idx`);");
                ExecuteNonQuerySync("ALTER TABLE `characterquest` ADD INDEX(`characterId`);");
                ExecuteNonQuerySync("ALTER TABLE `characters` ADD INDEX(`userId`);");
                ExecuteNonQuerySync("ALTER TABLE `characters` ADD INDEX(`factionId`);");
                ExecuteNonQuerySync("ALTER TABLE `characters` ADD INDEX(`partyId`);");
                ExecuteNonQuerySync("ALTER TABLE `characters` ADD INDEX(`guildId`);");
                ExecuteNonQuerySync("ALTER TABLE `characterskill` ADD INDEX(`idx`);");
                ExecuteNonQuerySync("ALTER TABLE `characterskill` ADD INDEX(`characterId`);");
                ExecuteNonQuerySync("ALTER TABLE `characterskillusage` ADD INDEX(`characterId`);");
                ExecuteNonQuerySync("ALTER TABLE `charactersummon` ADD INDEX(`characterId`);");
                ExecuteNonQuerySync("ALTER TABLE `friend` ADD INDEX(`characterId1`);");
                ExecuteNonQuerySync("ALTER TABLE `friend` ADD INDEX(`characterId2`);");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD INDEX(`leaderId`);");
                ExecuteNonQuerySync("ALTER TABLE `mail` ADD INDEX(`eventId`);");
                ExecuteNonQuerySync("ALTER TABLE `mail` ADD INDEX(`senderId`);");
                ExecuteNonQuerySync("ALTER TABLE `mail` ADD INDEX(`senderName`);");
                ExecuteNonQuerySync("ALTER TABLE `mail` ADD INDEX(`receiverId`);");
                ExecuteNonQuerySync("ALTER TABLE `mail` ADD INDEX(`isRead`);");
                ExecuteNonQuerySync("ALTER TABLE `mail` ADD INDEX(`isClaim`);");
                ExecuteNonQuerySync("ALTER TABLE `mail` ADD INDEX(`isDelete`);");
                ExecuteNonQuerySync("ALTER TABLE `party` ADD INDEX(`leaderId`);");
                ExecuteNonQuerySync("ALTER TABLE `storageitem` ADD INDEX(`idx`);");
                ExecuteNonQuerySync("ALTER TABLE `storageitem` ADD INDEX(`storageType`);");
                ExecuteNonQuerySync("ALTER TABLE `storageitem` ADD INDEX(`storageOwnerId`);");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.63b";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `characters` ADD `lastDeadTime` INT NOT NULL DEFAULT '0' AFTER `mountDataId`;");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.65d";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `characters` CHANGE `statPoint` `statPoint` FLOAT NOT NULL DEFAULT '0', CHANGE `skillPoint` `skillPoint` FLOAT NOT NULL DEFAULT '0';");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.67";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD `guildMessage2` VARCHAR(160) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL AFTER `guildMessage`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD `score` INT(11) NOT NULL DEFAULT '0' AFTER `gold`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD `optionId1` INT(11) NOT NULL DEFAULT '0' AFTER `score`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD `optionId2` INT(11) NOT NULL DEFAULT '0' AFTER `optionId1`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD `optionId3` INT(11) NOT NULL DEFAULT '0' AFTER `optionId2`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD `optionId4` INT(11) NOT NULL DEFAULT '0' AFTER `optionId3`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD `optionId5` INT(11) NOT NULL DEFAULT '0' AFTER `optionId4`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD `autoAcceptRequests` TINYINT(1) NOT NULL DEFAULT '0' AFTER `optionId5`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD `rank` INT(11) NOT NULL DEFAULT '0' AFTER `autoAcceptRequests`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD `currentMembers` INT(11) NOT NULL DEFAULT '0' AFTER `rank`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD `maxMembers` INT(11) NOT NULL DEFAULT '0' AFTER `currentMembers`;");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.67b";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `mail` CHANGE `gold` `gold` INT(11) NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `mail` ADD `cash` INT(11) NOT NULL DEFAULT '0' AFTER `gold`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` DROP `optionId1`, DROP `optionId2`, DROP `optionId3`, DROP `optionId4`, DROP `optionId5`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD `options` TEXT CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL AFTER `score`;");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.69";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `characterquest` ADD `isTracking` TINYINT(1) NOT NULL DEFAULT '0' AFTER `isComplete`;");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.70";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `characters` CHANGE `lastDeadTime` `lastDeadTime` BIGINT NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `characters` ADD `unmuteTime` BIGINT NOT NULL DEFAULT '0' AFTER `lastDeadTime`;");
                ExecuteNonQuerySync("ALTER TABLE `characteritem` ADD `expireTime` BIGINT NOT NULL DEFAULT '0' AFTER `lockRemainsDuration`;");
                ExecuteNonQuerySync("ALTER TABLE `characteritem` ADD `randomSeed` TINYINT UNSIGNED NOT NULL DEFAULT '0' AFTER `expireTime`;");
                ExecuteNonQuerySync("ALTER TABLE `storageitem` ADD `expireTime` BIGINT NOT NULL DEFAULT '0' AFTER `lockRemainsDuration`;");
                ExecuteNonQuerySync("ALTER TABLE `storageitem` ADD `randomSeed` TINYINT UNSIGNED NOT NULL DEFAULT '0' AFTER `expireTime`;");
                ExecuteNonQuerySync("ALTER TABLE `userlogin` ADD `unbanTime` BIGINT NOT NULL DEFAULT '0' AFTER `userLevel`;");
                ExecuteNonQuerySync("ALTER TABLE `userlogin` CHANGE `password` `password` VARCHAR(72) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL;");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.71";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `buildings` ADD `extraData` TEXT CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL AFTER `creatorName`;");
                ExecuteNonQuerySync("CREATE TABLE `summonbuffs` (" +
                    "`id` varchar(50) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL," +
                    "`characterId` varchar(50) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL," +
                    "`buffId` varchar(50) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL," +
                    "`type` tinyint UNSIGNED NOT NULL DEFAULT '0'," +
                    "`dataId` int NOT NULL DEFAULT '0'," +
                    "`level` int NOT NULL DEFAULT '1'," +
                    "`buffRemainsDuration` float NOT NULL DEFAULT '0'," +
                    "`createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP," +
                    "`updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP" +
                    ") ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                ExecuteNonQuerySync("ALTER TABLE `summonbuffs` ADD PRIMARY KEY (`id`);");
                ExecuteNonQuerySync("ALTER TABLE `summonbuffs` ADD KEY (`characterId`);");
                ExecuteNonQuerySync("ALTER TABLE `summonbuffs` ADD KEY (`buffId`);");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.71b";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `userlogin` ADD `isEmailVerified` tinyint(1) NOT NULL DEFAULT '0' AFTER `email`;");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.72d";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `characteritem` CHANGE `randomSeed` `randomSeed` INT NOT NULL DEFAULT '0';");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.73";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `storageitem` CHANGE `randomSeed` `randomSeed` INT NOT NULL DEFAULT '0';");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.76";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `friend` ADD `state` tinyint(1) NOT NULL DEFAULT '0' AFTER `characterId2`;");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.77";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("CREATE TABLE `statistic` (`userCount` INT NOT NULL DEFAULT '0' ) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.78";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `characters` ADD `iconDataId` INT NOT NULL DEFAULT '0' AFTER `mountDataId`, ADD `frameDataId` INT NOT NULL DEFAULT '0' AFTER `iconDataId`, ADD `titleDataId` INT NOT NULL DEFAULT '0' AFTER `frameDataId`;");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.78b";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `storageitem` CHANGE `randomSeed` `randomSeed` INT(11) NOT NULL DEFAULT '0';");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }
            migrationId = "1.79";
            if (!HasMigrationId(migrationId))
            {
                LogInformation(LogTag, $"Migrating up to {migrationId}");
                ExecuteNonQuerySync("ALTER TABLE `characterattribute` DROP `idx`;");
                ExecuteNonQuerySync("ALTER TABLE `charactercurrency` DROP `idx`;");
                ExecuteNonQuerySync("ALTER TABLE `characterquest` DROP `idx`;");
                ExecuteNonQuerySync("ALTER TABLE `characterskill` DROP `idx`;");
                ExecuteNonQuerySync("ALTER TABLE `statistic` ADD `id` INT NOT NULL FIRST, ADD PRIMARY KEY(`id`);");
                // Insert migrate history
                InsertMigrationId(migrationId);
                LogInformation(LogTag, $"Migrated to {migrationId}");
            }

        }
        private bool HasMigrationId(string migrationId)
        {
            object result = ExecuteScalarSync("SELECT COUNT(*) FROM __migrations WHERE migrationId=@migrationId", new MySqlParameter("@migrationId", migrationId));
            long count = result != null ? (long)result : 0;
            return count > 0;
        }
        public void InsertMigrationId(string migrationId)
        {
            ExecuteNonQuerySync("INSERT INTO __migrations (migrationId) VALUES (@migrationId)", new MySqlParameter("@migrationId", migrationId));
        }
        public string GetConnectionString()
        {
            string connectionString = "Server=" + address + ";" +
            "Port=" + port + ";" +
            "Uid=" + username + ";" +
                (string.IsNullOrEmpty(password) ? "" : "Pwd=\"" + password + "\";") +
                "Database=" + dbName + ";" +
                "SSL Mode=None;";
            return connectionString;
        }

        public MySqlConnection NewConnection()
        {
            return new MySqlConnection(GetConnectionString());
        }

        private async UniTask OpenConnection(MySqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();
            }
            catch (MySqlException ex)
            {
                LogException(LogTag, ex);
            }
        }

        private void OpenConnectionSync(MySqlConnection connection)
        {
            try
            {
                connection.Open();
            }
            catch (MySqlException ex)
            {
                LogException(LogTag, ex);
            }
        }

        public async UniTask<long> ExecuteInsertData(string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            long result = await ExecuteInsertData(connection, null, sql, args);
            await connection.CloseAsync();
            return result;
        }

        public async UniTask<long> ExecuteInsertData(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                await OpenConnection(connection);
                createLocalConnection = true;
            }
            long result = 0;
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (MySqlParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                    result = cmd.LastInsertedId;
                }
                catch (MySqlException ex)
                {
                    LogException(LogTag, ex);
                }
            }
            if (createLocalConnection)
                await connection.CloseAsync();
            return result;
        }

        public long ExecuteInsertDataSync(string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            long result = ExecuteInsertDataSync(connection, null, sql, args);
            connection.Close();
            return result;
        }

        public long ExecuteInsertDataSync(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                OpenConnectionSync(connection);
                createLocalConnection = true;
            }
            long result = 0;
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (MySqlParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                try
                {
                    cmd.ExecuteNonQuery();
                    result = cmd.LastInsertedId;
                }
                catch (MySqlException ex)
                {
                    LogException(LogTag, ex);
                }
            }
            if (createLocalConnection)
                connection.Close();
            return result;
        }

        public async UniTask<int> ExecuteNonQuery(string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            int result = await ExecuteNonQuery(connection, null, sql, args);
            await connection.CloseAsync();
            return result;
        }

        public async UniTask<int> ExecuteNonQuery(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                await OpenConnection(connection);
                createLocalConnection = true;
            }
            int numRows = 0;
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (MySqlParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                try
                {
                    numRows = await cmd.ExecuteNonQueryAsync();
                }
                catch (MySqlException ex)
                {
                    LogException(LogTag, ex);
                }
            }
            if (createLocalConnection)
                await connection.CloseAsync();
            return numRows;
        }

        public int ExecuteNonQuerySync(string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            int result = ExecuteNonQuerySync(connection, null, sql, args);
            connection.Close();
            return result;
        }

        public int ExecuteNonQuerySync(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                OpenConnectionSync(connection);
                createLocalConnection = true;
            }
            int numRows = 0;
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (MySqlParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                try
                {
                    numRows = cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    LogException(LogTag, ex);
                }
            }
            if (createLocalConnection)
                connection.Close();
            return numRows;
        }

        public async UniTask<object> ExecuteScalar(string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            object result = await ExecuteScalar(connection, null, sql, args);
            await connection.CloseAsync();
            return result;
        }

        public async UniTask<object> ExecuteScalar(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                await OpenConnection(connection);
                createLocalConnection = true;
            }
            object result = null;
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (MySqlParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                try
                {
                    result = await cmd.ExecuteScalarAsync();
                }
                catch (MySqlException ex)
                {
                    LogException(LogTag, ex);
                }
            }
            if (createLocalConnection)
                await connection.CloseAsync();
            return result;
        }

        public object ExecuteScalarSync(string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            object result = ExecuteScalarSync(connection, null, sql, args);
            connection.Close();
            return result;
        }

        public object ExecuteScalarSync(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                OpenConnectionSync(connection);
                createLocalConnection = true;
            }
            object result = null;
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (MySqlParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                try
                {
                    result = cmd.ExecuteScalar();
                }
                catch (MySqlException ex)
                {
                    LogException(LogTag, ex);
                }
            }
            if (createLocalConnection)
                connection.Close();
            return result;
        }

        public async UniTask ExecuteReader(Action<MySqlDataReader> onRead, string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            await ExecuteReader(connection, null, onRead, sql, args);
            await connection.CloseAsync();
        }

        public async UniTask ExecuteReader(MySqlConnection connection, MySqlTransaction transaction, Action<MySqlDataReader> onRead, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                await OpenConnection(connection);
                createLocalConnection = true;
            }
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (MySqlParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                try
                {
                    MySqlDataReader dataReader = await cmd.ExecuteReaderAsync();
                    if (onRead != null) onRead.Invoke(dataReader);
                    dataReader.Close();
                }
                catch (MySqlException ex)
                {
                    LogException(LogTag, ex);
                }
            }
            if (createLocalConnection)
                await connection.CloseAsync();
        }

        public void ExecuteReaderSync(Action<MySqlDataReader> onRead, string sql, params MySqlParameter[] args)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            ExecuteReaderSync(connection, null, onRead, sql, args);
            connection.Close();
        }

        public void ExecuteReaderSync(MySqlConnection connection, MySqlTransaction transaction, Action<MySqlDataReader> onRead, string sql, params MySqlParameter[] args)
        {
            bool createLocalConnection = false;
            if (connection == null)
            {
                connection = NewConnection();
                transaction = null;
                OpenConnectionSync(connection);
                createLocalConnection = true;
            }
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                if (transaction != null)
                    cmd.Transaction = transaction;
                foreach (MySqlParameter arg in args)
                {
                    cmd.Parameters.Add(arg);
                }
                try
                {
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    if (onRead != null) onRead.Invoke(dataReader);
                    dataReader.Close();
                }
                catch (MySqlException ex)
                {
                    LogException(LogTag, ex);
                }
            }
            if (createLocalConnection)
                connection.Close();
        }

        public override string ValidateUserLogin(string username, string password)
        {
            string id = string.Empty;
            ExecuteReaderSync((reader) =>
            {
                if (reader.Read())
                {
                    id = reader.GetString(0);
                    string hashedPassword = reader.GetString(1);
                    if (!password.PasswordVerify(hashedPassword))
                        id = string.Empty;
                }
            }, "SELECT id, password FROM userlogin WHERE username=@username AND authType=@authType LIMIT 1",
                new MySqlParameter("@username", username),
                new MySqlParameter("@authType", AUTH_TYPE_NORMAL));

            return id;
        }

        public override bool ValidateAccessToken(string userId, string accessToken)
        {
            object result = ExecuteScalarSync("SELECT COUNT(*) FROM userlogin WHERE id=@id AND accessToken=@accessToken",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@accessToken", accessToken));
            return (result != null ? (long)result : 0) > 0;
        }

        public override byte GetUserLevel(string userId)
        {
            byte userLevel = 0;
            ExecuteReaderSync((reader) =>
            {
                if (reader.Read())
                    userLevel = reader.GetByte(0);
            }, "SELECT userLevel FROM userlogin WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId));
            return userLevel;
        }

        public override int GetGold(string userId)
        {
            int gold = 0;
            ExecuteReaderSync((reader) =>
            {
                if (reader.Read())
                    gold = reader.GetInt32(0);
            }, "SELECT gold FROM userlogin WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId));
            return gold;
        }

        public override void UpdateGold(string userId, int gold)
        {
            ExecuteNonQuerySync("UPDATE userlogin SET gold=@gold WHERE id=@id",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@gold", gold));
        }

        public override int GetCash(string userId)
        {
            int cash = 0;
            ExecuteReaderSync((reader) =>
            {
                if (reader.Read())
                    cash = reader.GetInt32(0);
            }, "SELECT cash FROM userlogin WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId));
            return cash;
        }

        public override void UpdateCash(string userId, int cash)
        {
            ExecuteNonQuerySync("UPDATE userlogin SET cash=@cash WHERE id=@id",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@cash", cash));
        }

        public override void UpdateAccessToken(string userId, string accessToken)
        {
            ExecuteNonQuerySync("UPDATE userlogin SET accessToken=@accessToken WHERE id=@id",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@accessToken", accessToken));
        }

        public override void CreateUserLogin(string username, string password, string email)
        {
            ExecuteNonQuerySync("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                new MySqlParameter("@id", GenericUtils.GetUniqueId()),
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", password.PasswordHash()),
                new MySqlParameter("@email", email),
                new MySqlParameter("@authType", AUTH_TYPE_NORMAL));
        }

        public override long FindUsername(string username)
        {
            object result = ExecuteScalarSync("SELECT COUNT(*) FROM userlogin WHERE username LIKE @username",
                new MySqlParameter("@username", username));
            return result != null ? (long)result : 0;
        }

        public override long GetUserUnbanTime(string userId)
        {
            long unbanTime = 0;
            ExecuteReaderSync((reader) =>
            {
                if (reader.Read())
                {
                    unbanTime = reader.GetInt64(0);
                }
            }, "SELECT unbanTime FROM userlogin WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId));
            return unbanTime;
        }

        public override void SetUserUnbanTimeByCharacterName(string characterName, long unbanTime)
        {
            string userId = string.Empty;
            ExecuteReaderSync((reader) =>
            {
                if (reader.Read())
                {
                    userId = reader.GetString(0);
                }
            }, "SELECT userId FROM characters WHERE characterName LIKE @characterName LIMIT 1",
                new MySqlParameter("@characterName", characterName));
            if (string.IsNullOrEmpty(userId))
                return;
            ExecuteNonQuerySync("UPDATE userlogin SET unbanTime=@unbanTime WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", userId),
                new MySqlParameter("@unbanTime", unbanTime));
        }

        public override void SetCharacterUnmuteTimeByName(string characterName, long unmuteTime)
        {
            ExecuteNonQuerySync("UPDATE characters SET unmuteTime=@unmuteTime WHERE characterName LIKE @characterName LIMIT 1",
                new MySqlParameter("@characterName", characterName),
                new MySqlParameter("@unmuteTime", unmuteTime));
        }

        public override bool ValidateEmailVerification(string userId)
        {
            object result = ExecuteScalarSync("SELECT COUNT(*) FROM userlogin WHERE id=@userId AND isEmailVerified=1",
                new MySqlParameter("@userId", userId));
            return (result != null ? (long)result : 0) > 0;
        }

        public override long FindEmail(string email)
        {
            object result = ExecuteScalarSync("SELECT COUNT(*) FROM userlogin WHERE email LIKE @email",
                new MySqlParameter("@email", email));
            return result != null ? (long)result : 0;
        }

        public override void UpdateUserCount(int userCount)
        {
            object result = ExecuteScalarSync("SELECT COUNT(*) FROM statistic WHERE 1");
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                ExecuteNonQuerySync("UPDATE statistic SET userCount=@userCount;",
                    new MySqlParameter("@userCount", userCount));
            }
            else
            {
                ExecuteNonQuerySync("INSERT INTO statistic (userCount) VALUES(@userCount);",
                    new MySqlParameter("@userCount", userCount));
            }
        }
#endif
    }
}
