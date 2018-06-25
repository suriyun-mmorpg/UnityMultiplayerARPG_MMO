-- phpMyAdmin SQL Dump
-- version 4.7.4
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1:3306
-- Generation Time: Jun 25, 2018 at 06:33 PM
-- Server version: 10.1.28-MariaDB
-- PHP Version: 7.1.11

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `mmorpgtemplate`
--

-- --------------------------------------------------------

--
-- Table structure for table `buildings`
--

CREATE TABLE `buildings` (
  `id` varchar(50) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `parentId` varchar(50) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `dataId` int(11) NOT NULL DEFAULT '0',
  `currentHp` int(11) NOT NULL DEFAULT '0',
  `mapName` varchar(50) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `positionX` float NOT NULL DEFAULT '0',
  `positionY` float NOT NULL DEFAULT '0',
  `positionZ` float NOT NULL DEFAULT '0',
  `rotationX` float NOT NULL DEFAULT '0',
  `rotationY` float NOT NULL DEFAULT '0',
  `rotationZ` float NOT NULL DEFAULT '0',
  `creatorId` varchar(50) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `creatorName` varchar(32) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `characterattribute`
--

CREATE TABLE `characterattribute` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `idx` int(11) NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL DEFAULT '0',
  `amount` int(11) NOT NULL DEFAULT '0',
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characterbuff`
--

CREATE TABLE `characterbuff` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `type` tinyint(4) NOT NULL DEFAULT '0',
  `dataId` int(11) NOT NULL DEFAULT '0',
  `level` int(11) NOT NULL DEFAULT '1',
  `buffRemainsDuration` float NOT NULL DEFAULT '0',
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characterhotkey`
--

CREATE TABLE `characterhotkey` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `hotkeyId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `type` tinyint(4) NOT NULL DEFAULT '0',
  `dataId` int(11) NOT NULL DEFAULT '0',
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characteritem`
--

CREATE TABLE `characteritem` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `idx` int(11) NOT NULL,
  `inventoryType` tinyint(4) NOT NULL DEFAULT '0',
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL DEFAULT '0',
  `level` int(11) NOT NULL DEFAULT '1',
  `amount` int(11) NOT NULL DEFAULT '0',
  `durability` float NOT NULL DEFAULT '0',
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characterquest`
--

CREATE TABLE `characterquest` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `idx` int(11) NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL DEFAULT '0',
  `isComplete` tinyint(1) NOT NULL DEFAULT '0',
  `killedMonsters` text COLLATE utf8_unicode_ci NOT NULL,
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characters`
--

CREATE TABLE `characters` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `userId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL DEFAULT '0',
  `characterName` varchar(32) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `level` int(11) NOT NULL DEFAULT '1',
  `exp` int(11) NOT NULL DEFAULT '0',
  `currentHp` int(11) NOT NULL DEFAULT '0',
  `currentMp` int(11) NOT NULL DEFAULT '0',
  `currentStamina` int(11) NOT NULL DEFAULT '0',
  `currentFood` int(11) NOT NULL DEFAULT '0',
  `currentWater` int(11) NOT NULL DEFAULT '0',
  `statPoint` int(11) NOT NULL DEFAULT '0',
  `skillPoint` int(11) NOT NULL DEFAULT '0',
  `gold` int(11) NOT NULL DEFAULT '0',
  `currentMapName` varchar(50) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `currentPositionX` float NOT NULL DEFAULT '0',
  `currentPositionY` float NOT NULL DEFAULT '0',
  `currentPositionZ` float NOT NULL DEFAULT '0',
  `respawnMapName` varchar(50) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `respawnPositionX` float NOT NULL DEFAULT '0',
  `respawnPositionY` float NOT NULL DEFAULT '0',
  `respawnPositionZ` float NOT NULL DEFAULT '0',
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `characterskill`
--

CREATE TABLE `characterskill` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `idx` int(11) NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL DEFAULT '0',
  `level` int(11) NOT NULL DEFAULT '1',
  `coolDownRemainsDuration` float NOT NULL DEFAULT '0',
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `userlogin`
--

CREATE TABLE `userlogin` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `username` varchar(32) COLLATE utf8_unicode_ci NOT NULL,
  `password` varchar(32) COLLATE utf8_unicode_ci NOT NULL,
  `email` varchar(50) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `authType` tinyint(4) NOT NULL DEFAULT '1',
  `accessToken` varchar(36) COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

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
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `characterbuff`
--
ALTER TABLE `characterbuff`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `characterhotkey`
--
ALTER TABLE `characterhotkey`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `characteritem`
--
ALTER TABLE `characteritem`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `characterquest`
--
ALTER TABLE `characterquest`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `characters`
--
ALTER TABLE `characters`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `characterskill`
--
ALTER TABLE `characterskill`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `userlogin`
--
ALTER TABLE `userlogin`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `username` (`username`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
