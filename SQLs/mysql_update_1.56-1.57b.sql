START TRANSACTION;

CREATE TABLE `__migrations` (
  `migrationId` VARCHAR(50) NOT NULL , PRIMARY KEY (`migrationId`)
) ENGINE = InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

ALTER TABLE `buildings` ADD `remainsLifeTime` FLOAT NOT NULL DEFAULT '0' AFTER `currentHp`;

COMMIT;