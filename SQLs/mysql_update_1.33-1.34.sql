START TRANSACTION;

ALTER TABLE `characterattribute` DROP `id`;
ALTER TABLE `characterattribute` DROP `idx`;
ALTER TABLE `characterattribute` ADD PRIMARY KEY( `characterId`, `dataId`);
ALTER TABLE `characterbuff` DROP `id`;
ALTER TABLE `characterbuff` ADD PRIMARY KEY( `characterId`, `type`, `dataId`);
ALTER TABLE `characterhotkey` DROP `id`;
ALTER TABLE `characterhotkey` ADD PRIMARY KEY( `characterId`, `hotkeyId`);
ALTER TABLE `characteritem` DROP `id`;
ALTER TABLE `characteritem` ADD PRIMARY KEY( `idx`, `inventoryType`, `characterId`);
ALTER TABLE `characterquest` DROP `id`;
ALTER TABLE `characterquest` DROP `idx`;
ALTER TABLE `characterquest` ADD PRIMARY KEY( `characterId`, `dataId`);
ALTER TABLE `characterskill` DROP `id`;
ALTER TABLE `characterskill` DROP `idx`;
ALTER TABLE `characterskill` DROP `coolDownRemainsDuration`;
ALTER TABLE `characterskill` ADD PRIMARY KEY( `characterId`, `dataId`);

CREATE TABLE `characterskillusage` (
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `type` tinyint(4) NOT NULL DEFAULT '0',
  `dataId` int(11) NOT NULL DEFAULT '0',
  `coolDownRemainsDuration` float NOT NULL DEFAULT '0',
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

CREATE TABLE `guildskill` (
  `guildId` int(11) NOT NULL,
  `dataId` int(11) NOT NULL,
  `level` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

ALTER TABLE `characterskillusage` ADD PRIMARY KEY (`characterId`,`type`,`dataId`);
ALTER TABLE `guildskill` ADD PRIMARY KEY (`guildId`,`dataId`);

COMMIT;