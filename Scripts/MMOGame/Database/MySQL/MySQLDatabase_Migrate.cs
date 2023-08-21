#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using Cysharp.Threading.Tasks;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        delegate UniTask MigrateActionDelegate();

        [DevExtMethods("Init")]
        public async void Migrate()
        {
            await DoMigration("1.57b", async () =>
            {
                // Migrate data
                try
                {
                    // Avoid exception which occuring when `dataId` field not found
                    foreach (BuildingEntity prefab in GameInstance.BuildingEntities.Values)
                    {
                        await ExecuteNonQuery("UPDATE buildings SET entityId=@entityId, dataId=0 WHERE dataId=@dataId",
                            new MySqlParameter("entityId", prefab.EntityId),
                            new MySqlParameter("dataId", prefab.name.GenerateHashId()));
                    }
                }
                catch { }
                // Migrate fields
                try
                {
                    // Avoid exception which occuring when `dataId` field not found
                    await ExecuteNonQuery("ALTER TABLE buildings DROP dataId;");
                }
                catch { }
            });
            await DoMigration("1.58", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `characterbuff` CHANGE `type` `type` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                await ExecuteNonQuery("ALTER TABLE `characterhotkey` CHANGE `type` `type` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                await ExecuteNonQuery("ALTER TABLE `characteritem` CHANGE `inventoryType` `inventoryType` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                await ExecuteNonQuery("ALTER TABLE `characterskillusage` CHANGE `type` `type` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                await ExecuteNonQuery("ALTER TABLE `charactersummon` CHANGE `type` `type` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                await ExecuteNonQuery("ALTER TABLE `storageitem` CHANGE `storageType` `storageType` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                await ExecuteNonQuery("ALTER TABLE `userlogin` CHANGE `authType` `authType` TINYINT(3) UNSIGNED NOT NULL DEFAULT '1';");
                await ExecuteNonQuery("ALTER TABLE `userlogin` CHANGE `userLevel` `userLevel` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0';");
                await ExecuteNonQuery("ALTER TABLE `characters` ADD `currentRotationX` FLOAT NOT NULL DEFAULT '0' AFTER `currentPositionZ`;");
                await ExecuteNonQuery("ALTER TABLE `characters` ADD `currentRotationY` FLOAT NOT NULL DEFAULT '0' AFTER `currentRotationX`;");
                await ExecuteNonQuery("ALTER TABLE `characters` ADD `currentRotationZ` FLOAT NOT NULL DEFAULT '0' AFTER `currentRotationY`;");
            });
            await DoMigration("1.60c", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `characterquest` ADD `completedTasks` TEXT NOT NULL AFTER `killedMonsters`;");
            });
            await DoMigration("1.61", async () =>
            {
                await ExecuteNonQuery("CREATE TABLE `charactercurrency` ("
                    + "`id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`idx` int(11) NOT NULL,"
                    + "`characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`dataId` int(11) NOT NULL DEFAULT '0',"
                    + "`amount` int(11) NOT NULL DEFAULT '0',"
                    + "`createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,"
                    + "`updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,"
                    + "PRIMARY KEY(`id`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                await ExecuteNonQuery("CREATE TABLE `mail` ("
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
            });
            await DoMigration("1.61b", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `mail` ADD `isClaim` tinyint(1) NOT NULL DEFAULT 0 AFTER `readTimestamp`, ADD `claimTimestamp` timestamp NULL DEFAULT NULL AFTER `isClaim`;");
            });
            await DoMigration("1.62e", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `characterattribute` ADD INDEX(`idx`);");
                await ExecuteNonQuery("ALTER TABLE `characterattribute` ADD INDEX(`characterId`);");
                await ExecuteNonQuery("ALTER TABLE `characterbuff` ADD INDEX(`characterId`);");
                await ExecuteNonQuery("ALTER TABLE `charactercurrency` ADD INDEX(`idx`);");
                await ExecuteNonQuery("ALTER TABLE `charactercurrency` ADD INDEX(`characterId`);");
                await ExecuteNonQuery("ALTER TABLE `characterhotkey` ADD INDEX(`characterId`);");
                await ExecuteNonQuery("ALTER TABLE `characterhotkey` ADD INDEX(`hotkeyId`);");
                await ExecuteNonQuery("ALTER TABLE `characteritem` ADD INDEX(`idx`);");
                await ExecuteNonQuery("ALTER TABLE `characteritem` ADD INDEX(`inventoryType`);");
                await ExecuteNonQuery("ALTER TABLE `characteritem` ADD INDEX(`characterId`);");
                await ExecuteNonQuery("ALTER TABLE `characterquest` ADD INDEX(`idx`);");
                await ExecuteNonQuery("ALTER TABLE `characterquest` ADD INDEX(`characterId`);");
                await ExecuteNonQuery("ALTER TABLE `characters` ADD INDEX(`userId`);");
                await ExecuteNonQuery("ALTER TABLE `characters` ADD INDEX(`factionId`);");
                await ExecuteNonQuery("ALTER TABLE `characters` ADD INDEX(`partyId`);");
                await ExecuteNonQuery("ALTER TABLE `characters` ADD INDEX(`guildId`);");
                await ExecuteNonQuery("ALTER TABLE `characterskill` ADD INDEX(`idx`);");
                await ExecuteNonQuery("ALTER TABLE `characterskill` ADD INDEX(`characterId`);");
                await ExecuteNonQuery("ALTER TABLE `characterskillusage` ADD INDEX(`characterId`);");
                await ExecuteNonQuery("ALTER TABLE `charactersummon` ADD INDEX(`characterId`);");
                await ExecuteNonQuery("ALTER TABLE `friend` ADD INDEX(`characterId1`);");
                await ExecuteNonQuery("ALTER TABLE `friend` ADD INDEX(`characterId2`);");
                await ExecuteNonQuery("ALTER TABLE `guild` ADD INDEX(`leaderId`);");
                await ExecuteNonQuery("ALTER TABLE `mail` ADD INDEX(`eventId`);");
                await ExecuteNonQuery("ALTER TABLE `mail` ADD INDEX(`senderId`);");
                await ExecuteNonQuery("ALTER TABLE `mail` ADD INDEX(`senderName`);");
                await ExecuteNonQuery("ALTER TABLE `mail` ADD INDEX(`receiverId`);");
                await ExecuteNonQuery("ALTER TABLE `mail` ADD INDEX(`isRead`);");
                await ExecuteNonQuery("ALTER TABLE `mail` ADD INDEX(`isClaim`);");
                await ExecuteNonQuery("ALTER TABLE `mail` ADD INDEX(`isDelete`);");
                await ExecuteNonQuery("ALTER TABLE `party` ADD INDEX(`leaderId`);");
                await ExecuteNonQuery("ALTER TABLE `storageitem` ADD INDEX(`idx`);");
                await ExecuteNonQuery("ALTER TABLE `storageitem` ADD INDEX(`storageType`);");
                await ExecuteNonQuery("ALTER TABLE `storageitem` ADD INDEX(`storageOwnerId`);");
            });
            await DoMigration("1.63b", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `characters` ADD `lastDeadTime` INT NOT NULL DEFAULT '0' AFTER `mountDataId`;");
            });
            await DoMigration("1.65d", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `characters` CHANGE `statPoint` `statPoint` FLOAT NOT NULL DEFAULT '0', CHANGE `skillPoint` `skillPoint` FLOAT NOT NULL DEFAULT '0';");
            });
            await DoMigration("1.67", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `guild` ADD `guildMessage2` VARCHAR(160) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL AFTER `guildMessage`;");
                await ExecuteNonQuery("ALTER TABLE `guild` ADD `score` INT(11) NOT NULL DEFAULT '0' AFTER `gold`;");
                await ExecuteNonQuery("ALTER TABLE `guild` ADD `optionId1` INT(11) NOT NULL DEFAULT '0' AFTER `score`;");
                await ExecuteNonQuery("ALTER TABLE `guild` ADD `optionId2` INT(11) NOT NULL DEFAULT '0' AFTER `optionId1`;");
                await ExecuteNonQuery("ALTER TABLE `guild` ADD `optionId3` INT(11) NOT NULL DEFAULT '0' AFTER `optionId2`;");
                await ExecuteNonQuery("ALTER TABLE `guild` ADD `optionId4` INT(11) NOT NULL DEFAULT '0' AFTER `optionId3`;");
                await ExecuteNonQuery("ALTER TABLE `guild` ADD `optionId5` INT(11) NOT NULL DEFAULT '0' AFTER `optionId4`;");
                await ExecuteNonQuery("ALTER TABLE `guild` ADD `autoAcceptRequests` TINYINT(1) NOT NULL DEFAULT '0' AFTER `optionId5`;");
                await ExecuteNonQuery("ALTER TABLE `guild` ADD `rank` INT(11) NOT NULL DEFAULT '0' AFTER `autoAcceptRequests`;");
                await ExecuteNonQuery("ALTER TABLE `guild` ADD `currentMembers` INT(11) NOT NULL DEFAULT '0' AFTER `rank`;");
                await ExecuteNonQuery("ALTER TABLE `guild` ADD `maxMembers` INT(11) NOT NULL DEFAULT '0' AFTER `currentMembers`;");
            });
            await DoMigration("1.67b", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `mail` CHANGE `gold` `gold` INT(11) NOT NULL DEFAULT '0';");
                await ExecuteNonQuery("ALTER TABLE `mail` ADD `cash` INT(11) NOT NULL DEFAULT '0' AFTER `gold`;");
                await ExecuteNonQuery("ALTER TABLE `guild` DROP `optionId1`, DROP `optionId2`, DROP `optionId3`, DROP `optionId4`, DROP `optionId5`;");
                await ExecuteNonQuery("ALTER TABLE `guild` ADD `options` TEXT CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL AFTER `score`;");
            });
            await DoMigration("1.69", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `characterquest` ADD `isTracking` TINYINT(1) NOT NULL DEFAULT '0' AFTER `isComplete`;");
            });
            await DoMigration("1.70", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `characters` CHANGE `lastDeadTime` `lastDeadTime` BIGINT NOT NULL DEFAULT '0';");
                await ExecuteNonQuery("ALTER TABLE `characters` ADD `unmuteTime` BIGINT NOT NULL DEFAULT '0' AFTER `lastDeadTime`;");
                await ExecuteNonQuery("ALTER TABLE `characteritem` ADD `expireTime` BIGINT NOT NULL DEFAULT '0' AFTER `lockRemainsDuration`;");
                await ExecuteNonQuery("ALTER TABLE `characteritem` ADD `randomSeed` TINYINT UNSIGNED NOT NULL DEFAULT '0' AFTER `expireTime`;");
                await ExecuteNonQuery("ALTER TABLE `storageitem` ADD `expireTime` BIGINT NOT NULL DEFAULT '0' AFTER `lockRemainsDuration`;");
                await ExecuteNonQuery("ALTER TABLE `storageitem` ADD `randomSeed` TINYINT UNSIGNED NOT NULL DEFAULT '0' AFTER `expireTime`;");
                await ExecuteNonQuery("ALTER TABLE `userlogin` ADD `unbanTime` BIGINT NOT NULL DEFAULT '0' AFTER `userLevel`;");
                await ExecuteNonQuery("ALTER TABLE `userlogin` CHANGE `password` `password` VARCHAR(72) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL;");
            });
            await DoMigration("1.71", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `buildings` ADD `extraData` TEXT CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL AFTER `creatorName`;");
                await ExecuteNonQuery("CREATE TABLE `summonbuffs` (" +
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
                await ExecuteNonQuery("ALTER TABLE `summonbuffs` ADD PRIMARY KEY (`id`);");
                await ExecuteNonQuery("ALTER TABLE `summonbuffs` ADD KEY (`characterId`);");
                await ExecuteNonQuery("ALTER TABLE `summonbuffs` ADD KEY (`buffId`);");
            });
            await DoMigration("1.71b", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `userlogin` ADD `isEmailVerified` tinyint(1) NOT NULL DEFAULT '0' AFTER `email`;");
            });
            await DoMigration("1.72d", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `characteritem` CHANGE `randomSeed` `randomSeed` INT NOT NULL DEFAULT '0';");
            });
            await DoMigration("1.73", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `storageitem` CHANGE `randomSeed` `randomSeed` INT NOT NULL DEFAULT '0';");
            });
            await DoMigration("1.76", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `friend` ADD `state` tinyint(1) NOT NULL DEFAULT '0' AFTER `characterId2`;");
            });
            await DoMigration("1.77", async () =>
            {
                await ExecuteNonQuery("CREATE TABLE `statistic` (`userCount` INT NOT NULL DEFAULT '0' ) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci");
            });
            await DoMigration("1.78", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `characters` ADD `iconDataId` INT NOT NULL DEFAULT '0' AFTER `mountDataId`, ADD `frameDataId` INT NOT NULL DEFAULT '0' AFTER `iconDataId`, ADD `titleDataId` INT NOT NULL DEFAULT '0' AFTER `frameDataId`;");
            });
            await DoMigration("1.78b", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `storageitem` CHANGE `randomSeed` `randomSeed` INT(11) NOT NULL DEFAULT '0';");
            });
            await DoMigration("1.79", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `characterattribute` DROP `idx`;");
                await ExecuteNonQuery("ALTER TABLE `charactercurrency` DROP `idx`;");
                await ExecuteNonQuery("ALTER TABLE `characterquest` DROP `idx`;");
                await ExecuteNonQuery("ALTER TABLE `characterskill` DROP `idx`;");
                await ExecuteNonQuery("ALTER TABLE `statistic` ADD `id` INT NOT NULL FIRST, ADD PRIMARY KEY(`id`);");
            });
            await DoMigration("1.82", async () =>
            {
                await ExecuteNonQuery("ALTER TABLE `guildrole` ADD `canUseStorage` BOOLEAN NOT NULL AFTER `canKick`;");
                await ExecuteNonQuery("ALTER TABLE `guildrole` CHANGE `canInvite` `canInvite` TINYINT(1) NOT NULL DEFAULT '0';");
                await ExecuteNonQuery("ALTER TABLE `guildrole` CHANGE `canKick` `canKick` TINYINT(1) NOT NULL DEFAULT '0';");
                await ExecuteNonQuery("ALTER TABLE `guildrole` CHANGE `canUseStorage` `canUseStorage` TINYINT(1) NOT NULL DEFAULT '0';");
                await ExecuteNonQuery("ALTER TABLE `guildrole` CHANGE `shareExpPercentage` `shareExpPercentage` INT(11) NOT NULL DEFAULT '0';");
            });
            await DoMigration("1.84", async () =>
            {
                // Server custom data
                await ExecuteNonQuery("CREATE TABLE `character_server_boolean` ("
                    + "`id` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`characterId` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`hashedKey` INT(11) NOT NULL,"
                    + "`value` TINYINT(1) NOT NULL DEFAULT '0',"
                    + "PRIMARY KEY (`id`), INDEX (`characterId`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                await ExecuteNonQuery("CREATE TABLE `character_server_int32` ("
                    + "`id` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`characterId` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`hashedKey` INT(11) NOT NULL,"
                    + "`value` INT(11) NOT NULL DEFAULT '0',"
                    + "PRIMARY KEY (`id`), INDEX (`characterId`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                await ExecuteNonQuery("CREATE TABLE `character_server_float32` ("
                    + "`id` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`characterId` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`hashedKey` INT(11) NOT NULL,"
                    + "`value` FLOAT NOT NULL DEFAULT '0',"
                    + "PRIMARY KEY (`id`), INDEX (`characterId`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                // Private custom data
                await ExecuteNonQuery("CREATE TABLE `character_private_boolean` ("
                    + "`id` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`characterId` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`hashedKey` INT(11) NOT NULL,"
                    + "`value` TINYINT(1) NOT NULL DEFAULT '0',"
                    + "PRIMARY KEY (`id`), INDEX (`characterId`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                await ExecuteNonQuery("CREATE TABLE `character_private_int32` ("
                    + "`id` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`characterId` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`hashedKey` INT(11) NOT NULL,"
                    + "`value` INT(11) NOT NULL DEFAULT '0',"
                    + "PRIMARY KEY (`id`), INDEX (`characterId`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                await ExecuteNonQuery("CREATE TABLE `character_private_float32` ("
                    + "`id` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`characterId` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`hashedKey` INT(11) NOT NULL,"
                    + "`value` FLOAT NOT NULL DEFAULT '0',"
                    + "PRIMARY KEY (`id`), INDEX (`characterId`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                // Public custom data
                await ExecuteNonQuery("CREATE TABLE `character_public_boolean` ("
                    + "`id` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`characterId` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`hashedKey` INT(11) NOT NULL,"
                    + "`value` TINYINT(1) NOT NULL DEFAULT '0',"
                    + "PRIMARY KEY (`id`), INDEX (`characterId`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                await ExecuteNonQuery("CREATE TABLE `character_public_int32` ("
                    + "`id` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`characterId` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`hashedKey` INT(11) NOT NULL,"
                    + "`value` INT(11) NOT NULL DEFAULT '0',"
                    + "PRIMARY KEY (`id`), INDEX (`characterId`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                await ExecuteNonQuery("CREATE TABLE `character_public_float32` ("
                    + "`id` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`characterId` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL,"
                    + "`hashedKey` INT(11) NOT NULL,"
                    + "`value` FLOAT NOT NULL DEFAULT '0',"
                    + "PRIMARY KEY (`id`), INDEX (`characterId`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                // PK
                await ExecuteNonQuery("CREATE TABLE `character_pk` ("
                    + "`id` VARCHAR(50) NOT NULL,"
                    + "`isPkOn` BOOLEAN NOT NULL DEFAULT '0',"
                    + "`lastPkOnTime` BIGINT NOT NULL DEFAULT '0',"
                    + "`pkPoint` INT NOT NULL DEFAULT '0',"
                    + "`consecutivePkKills` INT NOT NULL DEFAULT '0',"
                    + "`highestPkPoint` INT NOT NULL DEFAULT '0',"
                    + "`highestConsecutivePkKills` INT NOT NULL DEFAULT '0',"
                    + "PRIMARY KEY (`id`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                // Channel
                await ExecuteNonQuery("ALTER TABLE `buildings` ADD `channel` VARCHAR(50) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL DEFAULT 'default' AFTER `id`;");
            });
            await DoMigration("1.85", async () =>
            {
                // Storage reservation
                await ExecuteNonQuery("CREATE TABLE `storage_reservation` (" +
                    "`storageType` TINYINT(3) UNSIGNED NOT NULL," +
                    "`storageOwnerId` VARCHAR(50) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL," +
                    "`reserverId` VARCHAR(50) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL," +
                    "PRIMARY KEY (`storageType`, `storageOwnerId`), INDEX (`reserverId`)) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;");
                // Add building's is scene object field
                await ExecuteNonQuery("ALTER TABLE `buildings` ADD `isSceneObject` BOOLEAN NOT NULL DEFAULT FALSE AFTER `extraData`;");
            });
        }

        private async UniTask<bool> DoMigration(string migrationId, MigrateActionDelegate migrateAction)
        {
            if (await HasMigrationId(migrationId))
                return false;
            LogInformation(LogTag, $"Migrating up to {migrationId}");
            await migrateAction.Invoke();
            await InsertMigrationId(migrationId);
            LogInformation(LogTag, $"Migrated to {migrationId}");
            return true;
        }

        private async UniTask<bool> HasMigrationId(string migrationId)
        {
            object result = await ExecuteScalar("SELECT COUNT(*) FROM __migrations WHERE migrationId=@migrationId", new MySqlParameter("@migrationId", migrationId));
            long count = result != null ? (long)result : 0;
            return count > 0;
        }

        public async UniTask InsertMigrationId(string migrationId)
        {
            await ExecuteNonQuery("INSERT INTO __migrations (migrationId) VALUES (@migrationId)", new MySqlParameter("@migrationId", migrationId));
        }
    }
}
#endif