START TRANSACTION;

ALTER TABLE `characteritem` ADD `isSummoned` TINYINT(1) NOT NULL DEFAULT 0 AFTER `durability`, ADD `currentSummonedHp` INT NOT NULL DEFAULT '0' AFTER `isSummoned`, ADD `currentSummonedMp` INT NOT NULL DEFAULT '0' AFTER `currentSummonedHp`, ADD `currentSummonedExp` INT NOT NULL DEFAULT '0' AFTER `currentSummonedMp`;
ALTER TABLE `characterskillusage` ADD `isSummoned` TINYINT(1) NOT NULL DEFAULT 0 AFTER `coolDownRemainsDuration`, ADD `currentSummonedHp` INT NOT NULL DEFAULT '0' AFTER `isSummoned`, ADD `currentSummonedMp` INT NOT NULL DEFAULT '0' AFTER `currentSummonedHp`;

COMMIT;