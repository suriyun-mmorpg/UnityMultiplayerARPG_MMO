START TRANSACTION;

ALTER TABLE `characteritem` ADD `exp` INT NOT NULL DEFAULT '0' AFTER `durability`, ADD `lockRemainsDuration` FLOAT NOT NULL DEFAULT '0' AFTER `exp`;
ALTER TABLE `userlogin` ADD `userLevel` TINYINT(4) NOT NULL DEFAULT 0 AFTER `accessToken`;

CREATE TABLE `charactersummon` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `type` tinyint(4) NOT NULL DEFAULT '0',
  `dataId` int(11) NOT NULL DEFAULT '0',
  `summonRemainsDuration` float NOT NULL DEFAULT '0',
  `level` int(11) NOT NULL DEFAULT '0',
  `exp` int(11) NOT NULL DEFAULT '0',
  `currentHp` int(11) NOT NULL DEFAULT '0',
  `currentMp` int(11) NOT NULL DEFAULT '0',
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

ALTER TABLE `charactersummon`
  ADD PRIMARY KEY (`id`);

COMMIT;