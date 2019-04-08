START TRANSACTION;

ALTER TABLE `characteritem` ADD `sockets` TEXT COLLATE utf8_unicode_ci NOT NULL AFTER `ammo`;
ALTER TABLE `storageitem` ADD `sockets` TEXT COLLATE utf8_unicode_ci NOT NULL AFTER `ammo`;

COMMIT;