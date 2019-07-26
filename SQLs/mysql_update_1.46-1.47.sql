START TRANSACTION;

ALTER TABLE `characterhotkey` DROP `dataId`;
ALTER TABLE `characterhotkey` ADD `relateId` VARCHAR(50) COLLATE utf8_unicode_ci NOT NULL AFTER `type`;

COMMIT;