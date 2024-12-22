SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";

--
-- Database: `mmorpg_kit`
--

-- --------------------------------------------------------

--
-- Table structure for table `buildings`
--

CREATE TABLE `buildings` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `channel` varchar(50) COLLATE utf8_unicode_ci NOT NULL DEFAULT 'default',
  `parentId` varchar(50) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `entityId` int(11) NOT NULL DEFAULT 0,
  `currentHp` int(11) NOT NULL DEFAULT 0,
  `remainsLifeTime` float NOT NULL DEFAULT 0,
  `mapName` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `positionX` float NOT NULL DEFAULT 0,
  `positionY` float NOT NULL DEFAULT 0,
  `positionZ` float NOT NULL DEFAULT 0,
  `rotationX` float NOT NULL DEFAULT 0,
  `rotationY` float NOT NULL DEFAULT 0,
  `rotationZ` float NOT NULL DEFAULT 0,
  `isLocked` tinyint(1) NOT NULL DEFAULT 0,
  `lockPassword` varchar(6) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `creatorId` varchar(50) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `creatorName` varchar(32) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `extraData` text COLLATE utf8_unicode_ci NOT NULL,
  `isSceneObject` tinyint(1) NOT NULL DEFAULT 0,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characterattribute`
--

CREATE TABLE `characterattribute` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL DEFAULT 0,
  `amount` int(11) NOT NULL DEFAULT 0,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characterbuff`
--

CREATE TABLE `characterbuff` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `type` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `dataId` int(11) NOT NULL DEFAULT 0,
  `level` int(11) NOT NULL DEFAULT 1,
  `buffRemainsDuration` float NOT NULL DEFAULT 0,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `charactercurrency`
--

CREATE TABLE `charactercurrency` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL DEFAULT 0,
  `amount` int(11) NOT NULL DEFAULT 0,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characterhotkey`
--

CREATE TABLE `characterhotkey` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `hotkeyId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `type` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `relateId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characteritem`
--

CREATE TABLE `characteritem` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `idx` int(11) NOT NULL,
  `inventoryType` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL DEFAULT 0,
  `level` int(11) NOT NULL DEFAULT 1,
  `amount` int(11) NOT NULL DEFAULT 0,
  `equipSlotIndex` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `durability` float NOT NULL DEFAULT 0,
  `exp` int(11) NOT NULL DEFAULT 0,
  `lockRemainsDuration` float NOT NULL DEFAULT 0,
  `expireTime` bigint(20) NOT NULL DEFAULT 0,
  `randomSeed` int(11) NOT NULL DEFAULT 0,
  `ammo` int(11) NOT NULL DEFAULT 0,
  `sockets` text COLLATE utf8_unicode_ci NOT NULL,
  `version` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `charactermount`
--

CREATE TABLE `charactermount` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `type` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `sourceId` varchar(50) COLLATE utf8_unicode_ci DEFAULT NULL,
  `mountRemainsDuration` float NOT NULL DEFAULT 0,
  `level` int(11) NOT NULL DEFAULT 0,
  `currentHp` int(11) NOT NULL DEFAULT 0,
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characterquest`
--

CREATE TABLE `characterquest` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL DEFAULT 0,
  `randomTasksIndex` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `isComplete` tinyint(1) NOT NULL DEFAULT 0,
  `completeTime` bigint(20) NOT NULL DEFAULT 0,
  `isTracking` tinyint(1) NOT NULL DEFAULT 0,
  `killedMonsters` text COLLATE utf8_unicode_ci NOT NULL,
  `completedTasks` text COLLATE utf8_unicode_ci NOT NULL,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characters`
--

CREATE TABLE `characters` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `userId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL DEFAULT 0,
  `entityId` int(11) NOT NULL DEFAULT 0,
  `factionId` int(11) NOT NULL DEFAULT 0,
  `characterName` varchar(32) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `level` int(11) NOT NULL DEFAULT 1,
  `exp` int(11) NOT NULL DEFAULT 0,
  `currentHp` int(11) NOT NULL DEFAULT 0,
  `currentMp` int(11) NOT NULL DEFAULT 0,
  `currentStamina` int(11) NOT NULL DEFAULT 0,
  `currentFood` int(11) NOT NULL DEFAULT 0,
  `currentWater` int(11) NOT NULL DEFAULT 0,
  `equipWeaponSet` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `statPoint` float NOT NULL DEFAULT 0,
  `skillPoint` float NOT NULL DEFAULT 0,
  `gold` int(11) NOT NULL DEFAULT 0,
  `partyId` int(11) NOT NULL DEFAULT 0,
  `guildId` int(11) NOT NULL DEFAULT 0,
  `guildRole` int(11) NOT NULL DEFAULT 0,
  `currentChannel` varchar(50) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `currentMapName` varchar(50) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `currentPositionX` float NOT NULL DEFAULT 0,
  `currentPositionY` float NOT NULL DEFAULT 0,
  `currentPositionZ` float NOT NULL DEFAULT 0,
  `currentRotationX` float NOT NULL DEFAULT 0,
  `currentRotationY` float NOT NULL DEFAULT 0,
  `currentRotationZ` float NOT NULL DEFAULT 0,
  `currentSafeArea` varchar(50) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `respawnMapName` varchar(50) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `respawnPositionX` float NOT NULL DEFAULT 0,
  `respawnPositionY` float NOT NULL DEFAULT 0,
  `respawnPositionZ` float NOT NULL DEFAULT 0,
  `iconDataId` int(11) NOT NULL DEFAULT 0,
  `frameDataId` int(11) NOT NULL DEFAULT 0,
  `titleDataId` int(11) NOT NULL DEFAULT 0,
  `reputation` int(11) NOT NULL DEFAULT 0,
  `lastDeadTime` bigint(20) NOT NULL DEFAULT 0,
  `unmuteTime` bigint(20) NOT NULL DEFAULT 0,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characterskill`
--

CREATE TABLE `characterskill` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL DEFAULT 0,
  `level` int(11) NOT NULL DEFAULT 1,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characterskillusage`
--

CREATE TABLE `characterskillusage` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `type` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `dataId` int(11) NOT NULL DEFAULT 0,
  `level` int(11) NOT NULL DEFAULT 1,
  `coolDownRemainsDuration` float NOT NULL DEFAULT 0,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `charactersummon`
--

CREATE TABLE `charactersummon` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `type` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `sourceId` varchar(50) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `dataId` int(11) NOT NULL DEFAULT 0,
  `summonRemainsDuration` float NOT NULL DEFAULT 0,
  `level` int(11) NOT NULL DEFAULT 0,
  `exp` int(11) NOT NULL DEFAULT 0,
  `currentHp` int(11) NOT NULL DEFAULT 0,
  `currentMp` int(11) NOT NULL DEFAULT 0,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `character_pk`
--

CREATE TABLE `character_pk` (
  `id` varchar(255) COLLATE utf8_unicode_ci NOT NULL,
  `isPkOn` tinyint(1) NOT NULL DEFAULT 0,
  `lastPkOnTime` int(11) NOT NULL DEFAULT 0,
  `pkPoint` int(11) NOT NULL DEFAULT 0,
  `consecutivePkKills` int(11) NOT NULL DEFAULT 0,
  `highestPkPoint` int(11) NOT NULL DEFAULT 0,
  `highestConsecutivePkKills` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `character_private_boolean`
--

CREATE TABLE `character_private_boolean` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `hashedKey` int(11) NOT NULL,
  `value` tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `character_private_float32`
--

CREATE TABLE `character_private_float32` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `hashedKey` int(11) NOT NULL,
  `value` float NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `character_private_int32`
--

CREATE TABLE `character_private_int32` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `hashedKey` int(11) NOT NULL,
  `value` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `character_public_boolean`
--

CREATE TABLE `character_public_boolean` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `hashedKey` int(11) NOT NULL,
  `value` tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `character_public_float32`
--

CREATE TABLE `character_public_float32` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `hashedKey` int(11) NOT NULL,
  `value` float NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `character_public_int32`
--

CREATE TABLE `character_public_int32` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `hashedKey` int(11) NOT NULL,
  `value` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `character_server_boolean`
--

CREATE TABLE `character_server_boolean` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `hashedKey` int(11) NOT NULL,
  `value` tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `character_server_float32`
--

CREATE TABLE `character_server_float32` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `hashedKey` int(11) NOT NULL,
  `value` float NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `character_server_int32`
--

CREATE TABLE `character_server_int32` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `hashedKey` int(11) NOT NULL,
  `value` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `friend`
--

CREATE TABLE `friend` (
  `id` int(11) NOT NULL,
  `characterId1` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId2` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `state` tinyint(1) NOT NULL DEFAULT 0,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `guild`
--

CREATE TABLE `guild` (
  `id` int(11) NOT NULL,
  `guildName` varchar(32) COLLATE utf8_unicode_ci NOT NULL,
  `leaderId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `level` int(11) NOT NULL DEFAULT 1,
  `exp` int(11) NOT NULL DEFAULT 0,
  `skillPoint` int(11) NOT NULL DEFAULT 0,
  `guildMessage` varchar(160) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `guildMessage2` varchar(160) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `gold` int(11) NOT NULL DEFAULT 0,
  `score` int(11) NOT NULL DEFAULT 0,
  `options` text COLLATE utf8_unicode_ci NOT NULL,
  `autoAcceptRequests` tinyint(1) NOT NULL DEFAULT 0,
  `rank` int(11) NOT NULL DEFAULT 0,
  `currentMembers` int(11) NOT NULL DEFAULT 0,
  `maxMembers` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `guildrequest`
--

CREATE TABLE `guildrequest` (
  `id` int(11) NOT NULL,
  `guildId` int(11) NOT NULL,
  `requesterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `guildrole`
--

CREATE TABLE `guildrole` (
  `guildId` int(11) NOT NULL,
  `guildRole` int(11) NOT NULL,
  `name` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `canInvite` tinyint(1) NOT NULL DEFAULT 0,
  `canKick` tinyint(1) NOT NULL DEFAULT 0,
  `canUseStorage` tinyint(1) NOT NULL DEFAULT 0,
  `shareExpPercentage` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `guildskill`
--

CREATE TABLE `guildskill` (
  `guildId` int(11) NOT NULL,
  `dataId` int(11) NOT NULL,
  `level` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `mail`
--

CREATE TABLE `mail` (
  `id` bigint(20) NOT NULL,
  `eventId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `senderId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `senderName` varchar(32) COLLATE utf8_unicode_ci NOT NULL,
  `receiverId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `title` varchar(160) COLLATE utf8_unicode_ci NOT NULL,
  `content` text COLLATE utf8_unicode_ci NOT NULL,
  `gold` int(11) NOT NULL DEFAULT 0,
  `cash` int(11) NOT NULL DEFAULT 0,
  `currencies` text COLLATE utf8_unicode_ci NOT NULL,
  `items` text COLLATE utf8_unicode_ci NOT NULL,
  `isRead` tinyint(1) NOT NULL DEFAULT 0,
  `readTimestamp` timestamp NULL DEFAULT NULL,
  `isClaim` tinyint(1) NOT NULL DEFAULT 0,
  `claimTimestamp` timestamp NULL DEFAULT NULL,
  `isDelete` tinyint(1) NOT NULL DEFAULT 0,
  `deleteTimestamp` timestamp NULL DEFAULT NULL,
  `sentTimestamp` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `party`
--

CREATE TABLE `party` (
  `id` int(11) NOT NULL,
  `shareExp` tinyint(1) NOT NULL,
  `shareItem` tinyint(1) NOT NULL,
  `leaderId` varchar(50) COLLATE utf8_unicode_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `statistic`
--

CREATE TABLE `statistic` (
  `id` int(11) NOT NULL,
  `userCount` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `storageitem`
--

CREATE TABLE `storageitem` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `idx` int(11) NOT NULL,
  `storageType` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `storageOwnerId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL DEFAULT 0,
  `level` int(11) NOT NULL DEFAULT 1,
  `amount` int(11) NOT NULL DEFAULT 0,
  `durability` float NOT NULL DEFAULT 0,
  `exp` int(11) NOT NULL DEFAULT 0,
  `lockRemainsDuration` float NOT NULL DEFAULT 0,
  `expireTime` bigint(20) NOT NULL DEFAULT 0,
  `randomSeed` int(11) NOT NULL DEFAULT 0,
  `ammo` int(11) NOT NULL DEFAULT 0,
  `sockets` text COLLATE utf8_unicode_ci NOT NULL,
  `version` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `storage_reservation`
--

CREATE TABLE `storage_reservation` (
  `storageType` tinyint(3) UNSIGNED NOT NULL,
  `storageOwnerId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `reserverId` varchar(50) COLLATE utf8_unicode_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `summonbuffs`
--

CREATE TABLE `summonbuffs` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `buffId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `type` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `dataId` int(11) NOT NULL DEFAULT 0,
  `level` int(11) NOT NULL DEFAULT 1,
  `buffRemainsDuration` float NOT NULL DEFAULT 0,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `userlogin`
--

CREATE TABLE `userlogin` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `username` varchar(32) COLLATE utf8_unicode_ci NOT NULL,
  `password` varchar(72) COLLATE utf8_unicode_ci NOT NULL,
  `gold` int(11) NOT NULL DEFAULT 0,
  `cash` int(11) NOT NULL DEFAULT 0,
  `email` varchar(50) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `isEmailVerified` tinyint(1) NOT NULL DEFAULT 0,
  `authType` tinyint(3) UNSIGNED NOT NULL DEFAULT 1,
  `accessToken` varchar(255) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `userLevel` tinyint(3) UNSIGNED NOT NULL DEFAULT 0,
  `unbanTime` bigint(20) NOT NULL DEFAULT 0,
  `createAt` timestamp NOT NULL DEFAULT current_timestamp(),
  `updateAt` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `__migrations`
--

CREATE TABLE `__migrations` (
  `migrationId` varchar(50) COLLATE utf8_unicode_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

--
-- Dumping data for table `__migrations`
--

INSERT INTO `__migrations` (`migrationId`) VALUES
('1.57b'),
('1.58'),
('1.60c'),
('1.61'),
('1.61b'),
('1.62e'),
('1.63b'),
('1.65d'),
('1.67'),
('1.67b'),
('1.69'),
('1.70'),
('1.71'),
('1.71b'),
('1.72d'),
('1.73'),
('1.76'),
('1.77'),
('1.78'),
('1.78b'),
('1.79'),
('1.82'),
('1.84'),
('1.85'),
('1.88'),
('1.90'),
('1.90b'),
('1.91');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `buildings`
--
ALTER TABLE `buildings`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `characterattribute`
--
ALTER TABLE `characterattribute`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `characterbuff`
--
ALTER TABLE `characterbuff`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `charactercurrency`
--
ALTER TABLE `charactercurrency`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `characterhotkey`
--
ALTER TABLE `characterhotkey`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`),
  ADD KEY `hotkeyId` (`hotkeyId`);

--
-- Indexes for table `characteritem`
--
ALTER TABLE `characteritem`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx` (`idx`),
  ADD KEY `inventoryType` (`inventoryType`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `charactermount`
--
ALTER TABLE `charactermount`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `characterquest`
--
ALTER TABLE `characterquest`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `characters`
--
ALTER TABLE `characters`
  ADD PRIMARY KEY (`id`),
  ADD KEY `userId` (`userId`),
  ADD KEY `factionId` (`factionId`),
  ADD KEY `partyId` (`partyId`),
  ADD KEY `guildId` (`guildId`);

--
-- Indexes for table `characterskill`
--
ALTER TABLE `characterskill`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `characterskillusage`
--
ALTER TABLE `characterskillusage`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `charactersummon`
--
ALTER TABLE `charactersummon`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `character_pk`
--
ALTER TABLE `character_pk`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `character_private_boolean`
--
ALTER TABLE `character_private_boolean`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `character_private_float32`
--
ALTER TABLE `character_private_float32`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `character_private_int32`
--
ALTER TABLE `character_private_int32`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `character_public_boolean`
--
ALTER TABLE `character_public_boolean`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `character_public_float32`
--
ALTER TABLE `character_public_float32`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `character_public_int32`
--
ALTER TABLE `character_public_int32`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `character_server_boolean`
--
ALTER TABLE `character_server_boolean`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `character_server_float32`
--
ALTER TABLE `character_server_float32`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `character_server_int32`
--
ALTER TABLE `character_server_int32`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`);

--
-- Indexes for table `friend`
--
ALTER TABLE `friend`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId1` (`characterId1`),
  ADD KEY `characterId2` (`characterId2`);

--
-- Indexes for table `guild`
--
ALTER TABLE `guild`
  ADD PRIMARY KEY (`id`),
  ADD KEY `leaderId` (`leaderId`);

--
-- Indexes for table `guildrequest`
--
ALTER TABLE `guildrequest`
  ADD PRIMARY KEY (`id`),
  ADD KEY `guildId` (`guildId`),
  ADD KEY `requesterId` (`requesterId`);

--
-- Indexes for table `guildrole`
--
ALTER TABLE `guildrole`
  ADD PRIMARY KEY (`guildId`,`guildRole`) USING BTREE;

--
-- Indexes for table `guildskill`
--
ALTER TABLE `guildskill`
  ADD PRIMARY KEY (`guildId`,`dataId`) USING BTREE;

--
-- Indexes for table `mail`
--
ALTER TABLE `mail`
  ADD PRIMARY KEY (`id`),
  ADD KEY `eventId` (`eventId`),
  ADD KEY `senderId` (`senderId`),
  ADD KEY `senderName` (`senderName`),
  ADD KEY `receiverId` (`receiverId`),
  ADD KEY `isRead` (`isRead`),
  ADD KEY `isClaim` (`isClaim`),
  ADD KEY `isDelete` (`isDelete`);

--
-- Indexes for table `party`
--
ALTER TABLE `party`
  ADD PRIMARY KEY (`id`),
  ADD KEY `leaderId` (`leaderId`);

--
-- Indexes for table `statistic`
--
ALTER TABLE `statistic`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `storageitem`
--
ALTER TABLE `storageitem`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx` (`idx`),
  ADD KEY `storageType` (`storageType`),
  ADD KEY `storageOwnerId` (`storageOwnerId`);

--
-- Indexes for table `storage_reservation`
--
ALTER TABLE `storage_reservation`
  ADD PRIMARY KEY (`storageType`,`storageOwnerId`),
  ADD KEY `reserverId` (`reserverId`);

--
-- Indexes for table `summonbuffs`
--
ALTER TABLE `summonbuffs`
  ADD PRIMARY KEY (`id`),
  ADD KEY `characterId` (`characterId`),
  ADD KEY `buffId` (`buffId`);

--
-- Indexes for table `userlogin`
--
ALTER TABLE `userlogin`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `username` (`username`);

--
-- Indexes for table `__migrations`
--
ALTER TABLE `__migrations`
  ADD PRIMARY KEY (`migrationId`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `friend`
--
ALTER TABLE `friend`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `guild`
--
ALTER TABLE `guild`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `mail`
--
ALTER TABLE `mail`
  MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `party`
--
ALTER TABLE `party`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `statistic`
--
ALTER TABLE `statistic`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
COMMIT;
