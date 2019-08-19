START TRANSACTION;

CREATE TABLE `friend` (
  `id` int(11) NOT NULL,
  `characterId1` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId2` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

ALTER TABLE `friend` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

COMMIT;