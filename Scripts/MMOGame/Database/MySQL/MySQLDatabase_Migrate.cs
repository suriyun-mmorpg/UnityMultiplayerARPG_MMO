#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        [DevExtMethods("Init")]
        public void Migrate()
        {
            DoMigration("1.57b", () =>
            {
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
            });
            DoMigration("1.58", () =>
            {
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
            });
            DoMigration("1.60c", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `characterquest` ADD `completedTasks` TEXT NOT NULL AFTER `killedMonsters`;");
            });
            DoMigration("1.61", () =>
            {
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
            });
            DoMigration("1.61b", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `mail` ADD `isClaim` tinyint(1) NOT NULL DEFAULT 0 AFTER `readTimestamp`, ADD `claimTimestamp` timestamp NULL DEFAULT NULL AFTER `isClaim`;");
            });
            DoMigration("1.62e", () =>
            {
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
            });
            DoMigration("1.63b", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `characters` ADD `lastDeadTime` INT NOT NULL DEFAULT '0' AFTER `mountDataId`;");
            });
            DoMigration("1.65d", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `characters` CHANGE `statPoint` `statPoint` FLOAT NOT NULL DEFAULT '0', CHANGE `skillPoint` `skillPoint` FLOAT NOT NULL DEFAULT '0';");
            });
            DoMigration("1.67", () =>
            {
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
            });
            DoMigration("1.67b", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `mail` CHANGE `gold` `gold` INT(11) NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `mail` ADD `cash` INT(11) NOT NULL DEFAULT '0' AFTER `gold`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` DROP `optionId1`, DROP `optionId2`, DROP `optionId3`, DROP `optionId4`, DROP `optionId5`;");
                ExecuteNonQuerySync("ALTER TABLE `guild` ADD `options` TEXT CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL AFTER `score`;");
            });
            DoMigration("1.69", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `characterquest` ADD `isTracking` TINYINT(1) NOT NULL DEFAULT '0' AFTER `isComplete`;");
            });
            DoMigration("1.70", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `characters` CHANGE `lastDeadTime` `lastDeadTime` BIGINT NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `characters` ADD `unmuteTime` BIGINT NOT NULL DEFAULT '0' AFTER `lastDeadTime`;");
                ExecuteNonQuerySync("ALTER TABLE `characteritem` ADD `expireTime` BIGINT NOT NULL DEFAULT '0' AFTER `lockRemainsDuration`;");
                ExecuteNonQuerySync("ALTER TABLE `characteritem` ADD `randomSeed` TINYINT UNSIGNED NOT NULL DEFAULT '0' AFTER `expireTime`;");
                ExecuteNonQuerySync("ALTER TABLE `storageitem` ADD `expireTime` BIGINT NOT NULL DEFAULT '0' AFTER `lockRemainsDuration`;");
                ExecuteNonQuerySync("ALTER TABLE `storageitem` ADD `randomSeed` TINYINT UNSIGNED NOT NULL DEFAULT '0' AFTER `expireTime`;");
                ExecuteNonQuerySync("ALTER TABLE `userlogin` ADD `unbanTime` BIGINT NOT NULL DEFAULT '0' AFTER `userLevel`;");
                ExecuteNonQuerySync("ALTER TABLE `userlogin` CHANGE `password` `password` VARCHAR(72) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL;");
            });
            DoMigration("1.71", () =>
            {
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
            });
            DoMigration("1.71b", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `userlogin` ADD `isEmailVerified` tinyint(1) NOT NULL DEFAULT '0' AFTER `email`;");
            });
            DoMigration("1.72d", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `characteritem` CHANGE `randomSeed` `randomSeed` INT NOT NULL DEFAULT '0';");
            });
            DoMigration("1.73", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `storageitem` CHANGE `randomSeed` `randomSeed` INT NOT NULL DEFAULT '0';");
            });
            DoMigration("1.76", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `friend` ADD `state` tinyint(1) NOT NULL DEFAULT '0' AFTER `characterId2`;");
            });
            DoMigration("1.77", () =>
            {
                ExecuteNonQuerySync("CREATE TABLE `statistic` (`userCount` INT NOT NULL DEFAULT '0' ) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci");
            });
            DoMigration("1.78", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `characters` ADD `iconDataId` INT NOT NULL DEFAULT '0' AFTER `mountDataId`, ADD `frameDataId` INT NOT NULL DEFAULT '0' AFTER `iconDataId`, ADD `titleDataId` INT NOT NULL DEFAULT '0' AFTER `frameDataId`;");
            });
            DoMigration("1.78b", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `storageitem` CHANGE `randomSeed` `randomSeed` INT(11) NOT NULL DEFAULT '0';");
            });
            DoMigration("1.79", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `characterattribute` DROP `idx`;");
                ExecuteNonQuerySync("ALTER TABLE `charactercurrency` DROP `idx`;");
                ExecuteNonQuerySync("ALTER TABLE `characterquest` DROP `idx`;");
                ExecuteNonQuerySync("ALTER TABLE `characterskill` DROP `idx`;");
                ExecuteNonQuerySync("ALTER TABLE `statistic` ADD `id` INT NOT NULL FIRST, ADD PRIMARY KEY(`id`);");
            });
            DoMigration("1.82", () =>
            {
                ExecuteNonQuerySync("ALTER TABLE `guildrole` ADD `canUseStorage` BOOLEAN NOT NULL AFTER `canKick`;");
                ExecuteNonQuerySync("ALTER TABLE `guildrole` CHANGE `canInvite` `canInvite` TINYINT(1) NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `guildrole` CHANGE `canKick` `canKick` TINYINT(1) NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `guildrole` CHANGE `canUseStorage` `canUseStorage` TINYINT(1) NOT NULL DEFAULT '0';");
                ExecuteNonQuerySync("ALTER TABLE `guildrole` CHANGE `shareExpPercentage` `shareExpPercentage` INT(11) NOT NULL DEFAULT '0';");
            });
        }

        private bool DoMigration(string migrationId, System.Action migrateAction)
        {
            if (HasMigrationId(migrationId))
                return false;
            LogInformation(LogTag, $"Migrating up to {migrationId}");
            migrateAction.Invoke();
            InsertMigrationId(migrationId);
            LogInformation(LogTag, $"Migrated to {migrationId}");
            return true;
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
    }
}
#endif