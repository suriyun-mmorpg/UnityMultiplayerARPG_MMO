-- phpMyAdmin SQL Dump
-- version 4.7.4
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1:3306
-- Generation Time: Jun 18, 2018 at 06:49 AM
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
-- Table structure for table `characterattribute`
--

CREATE TABLE `characterattribute` (
  `id` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `idx` int(11) NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL,
  `amount` int(11) NOT NULL,
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
  `type` tinyint(4) NOT NULL,
  `dataId` int(11) NOT NULL,
  `level` int(11) NOT NULL,
  `buffRemainsDuration` float NOT NULL,
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
  `type` tinyint(4) NOT NULL,
  `dataId` int(11) NOT NULL,
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
  `inventoryType` tinyint(4) NOT NULL,
  `characterId` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `dataId` int(11) NOT NULL,
  `level` int(11) NOT NULL,
  `amount` int(11) NOT NULL,
  `durability` float NOT NULL,
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
  `dataId` int(11) NOT NULL,
  `isComplete` tinyint(1) NOT NULL,
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
  `dataId` int(11) NOT NULL,
  `characterName` varchar(32) COLLATE utf8_unicode_ci NOT NULL,
  `level` int(11) NOT NULL,
  `exp` int(11) NOT NULL,
  `currentHp` int(11) NOT NULL,
  `currentMp` int(11) NOT NULL,
  `currentStamina` int(11) NOT NULL,
  `currentFood` int(11) NOT NULL,
  `currentWater` int(11) NOT NULL,
  `statPoint` int(11) NOT NULL,
  `skillPoint` int(11) NOT NULL,
  `gold` int(11) NOT NULL,
  `currentMapName` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `currentPositionX` float NOT NULL,
  `currentPositionY` float NOT NULL,
  `currentPositionZ` float NOT NULL,
  `respawnMapName` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `respawnPositionX` float NOT NULL,
  `respawnPositionY` float NOT NULL,
  `respawnPositionZ` float NOT NULL,
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
  `dataId` int(11) NOT NULL,
  `level` int(11) NOT NULL,
  `coolDownRemainsDuration` float NOT NULL,
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
  `accessToken` varchar(36) COLLATE utf8_unicode_ci NOT NULL,
  `createAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updateAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

--
-- Indexes for dumped tables
--

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
