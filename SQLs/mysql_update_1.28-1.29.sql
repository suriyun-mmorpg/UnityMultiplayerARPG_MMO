START TRANSACTION;

ALTER TABLE `characters` ADD `partyId` INT NOT NULL DEFAULT '0' AFTER `gold`, ADD `guildId` INT NOT NULL DEFAULT '0' AFTER `partyId`;

CREATE TABLE `party` (
  `id` int(11) NOT NULL,
  `shareExp` tinyint(1) NOT NULL,
  `shareItem` tinyint(1) NOT NULL,
  `leaderId` varchar(50) COLLATE utf8_unicode_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

ALTER TABLE `party` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

COMMIT;