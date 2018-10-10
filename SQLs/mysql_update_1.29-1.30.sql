START TRANSACTION;

ALTER TABLE `characters` ADD `guildRole` INT NOT NULL DEFAULT '0' AFTER `guildId`;
ALTER TABLE `characters` ADD `sharedGuildExp` INT NOT NULL DEFAULT '0' AFTER `guildRole`;

CREATE TABLE `guild` (
  `id` int(11) NOT NULL,
  `guildName` varchar(32) COLLATE utf8_unicode_ci NOT NULL,
  `leaderId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `level` int(11) NOT NULL,
  `exp` int(11) NOT NULL,
  `skillPoint` int(11) NOT NULL,
  `guildMessage` varchar(160) COLLATE utf8_unicode_ci NOT NULL DEFAULT ''
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

CREATE TABLE `guildrole` (
  `guildId` int(11) NOT NULL,
  `guildRole` int(11) NOT NULL,
  `name` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `canInvite` tinyint(1) NOT NULL,
  `canKick` tinyint(1) NOT NULL,
  `shareExpPercentage` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

ALTER TABLE `guild` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `guildrole` ADD PRIMARY KEY (`guildId`,`guildRole`) USING BTREE;

COMMIT;