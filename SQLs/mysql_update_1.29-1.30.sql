ALTER TABLE `characters` ADD `guildRole` INT NOT NULL DEFAULT '0' AFTER `guildId`;

CREATE TABLE `guild` (
  `id` int(11) NOT NULL,
  `guildName` varchar(32) COLLATE utf8_unicode_ci NOT NULL,
  `leaderId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `leaderName` varchar(32) COLLATE utf8_unicode_ci NOT NULL,
  `level` int(11) NOT NULL,
  `exp` int(11) NOT NULL,
  `skillPoint` int(11) NOT NULL,
  `guildMessage` varchar(160) COLLATE utf8_unicode_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

ALTER TABLE `guild` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;