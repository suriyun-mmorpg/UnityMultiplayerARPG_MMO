CREATE TABLE `guild` (
  `id` int(11) NOT NULL,
  `guildName` varchar(32) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterName` varchar(32) COLLATE utf8_unicode_ci NOT NULL,
  `level` int(11) NOT NULL,
  `exp` int(11) NOT NULL,
  `skillPoint` int(11) NOT NULL,
  `message` varchar(100) COLLATE utf8_unicode_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

ALTER TABLE `guild` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;