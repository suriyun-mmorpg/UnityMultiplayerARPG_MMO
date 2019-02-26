START TRANSACTION;

ALTER TABLE `guild` ADD `gold` INT NOT NULL DEFAULT '0' AFTER `guildMessage`;
ALTER TABLE `userlogin` ADD `gold` INT NOT NULL DEFAULT '0' AFTER `password`;
ALTER TABLE `characteritem` ADD `ammo` INT NOT NULL DEFAULT '0' AFTER `lockRemainsDuration`;

CREATE TABLE `storageitem` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `idx` int(11) NOT NULL,
  `storageType` tinyint(4) NOT NULL DEFAULT '0',
  `storageOwnerId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL DEFAULT '0',
  `level` int(11) NOT NULL DEFAULT '1',
  `amount` int(11) NOT NULL DEFAULT '0',
  `durability` float NOT NULL DEFAULT '0',
  `exp` int(11) NOT NULL DEFAULT '0',
  `lockRemainsDuration` float NOT NULL DEFAULT '0',
  `ammo` int(11) NOT NULL DEFAULT '0',
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

ALTER TABLE `storageitem`
  ADD PRIMARY KEY (`id`);

COMMIT;