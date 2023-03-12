#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using System.Collections.Generic;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private void FillCharacterAttributes(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterAttributes(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Attributes.Count; ++i)
                {
                    CreateCharacterAttribute(connection, transaction, insertedIds, characterData.Id, characterData.Attributes[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing attributes of character: " + characterData.Id);
                LogException(LogTag, ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterBuffs(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterBuffs(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Buffs.Count; ++i)
                {
                    CreateCharacterBuff(connection, transaction, insertedIds, characterData.Id, characterData.Buffs[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing buffs of character: " + characterData.Id);
                LogException(LogTag, ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterHotkeys(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterHotkeys(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Hotkeys.Count; ++i)
                {
                    CreateCharacterHotkey(connection, transaction, insertedIds, characterData.Id, characterData.Hotkeys[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing hotkeys of character: " + characterData.Id);
                LogException(LogTag, ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterItems(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterItems(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.SelectableWeaponSets.Count; ++i)
                {
                    CreateCharacterEquipWeapons(connection, transaction, insertedIds, i, characterData.Id, characterData.SelectableWeaponSets[i]);
                }
                for (i = 0; i < characterData.EquipItems.Count; ++i)
                {
                    CreateCharacterEquipItem(connection, transaction, insertedIds, i, characterData.Id, characterData.EquipItems[i]);
                }
                for (i = 0; i < characterData.NonEquipItems.Count; ++i)
                {
                    CreateCharacterNonEquipItem(connection, transaction, insertedIds, i, characterData.Id, characterData.NonEquipItems[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing items of character: " + characterData.Id);
                LogException(LogTag, ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterQuests(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterQuests(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Quests.Count; ++i)
                {
                    CreateCharacterQuest(connection, transaction, insertedIds, characterData.Id, characterData.Quests[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing quests of character: " + characterData.Id);
                LogException(LogTag, ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterCurrencies(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterCurrencies(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Currencies.Count; ++i)
                {
                    CreateCharacterCurrency(connection, transaction, insertedIds, characterData.Id, characterData.Currencies[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing currencies of character: " + characterData.Id);
                LogException(LogTag, ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterSkills(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterSkills(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Skills.Count; ++i)
                {
                    CreateCharacterSkill(connection, transaction, insertedIds, characterData.Id, characterData.Skills[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing skills of character: " + characterData.Id);
                LogException(LogTag, ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterSkillUsages(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterSkillUsages(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.SkillUsages.Count; ++i)
                {
                    CreateCharacterSkillUsage(connection, transaction, insertedIds, characterData.Id, characterData.SkillUsages[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing skill usages of character: " + characterData.Id);
                LogException(LogTag, ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterSummons(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterSummons(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Summons.Count; ++i)
                {
                    CreateCharacterSummon(connection, transaction, insertedIds, i, characterData.Id, characterData.Summons[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing skill usages of character: " + characterData.Id);
                LogException(LogTag, ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterRelatesData(IPlayerCharacterData characterData)
        {
            FillCharacterAttributes(characterData);
            FillCharacterCurrencies(characterData);
            FillCharacterBuffs(characterData);
            FillCharacterHotkeys(characterData);
            FillCharacterItems(characterData);
            FillCharacterQuests(characterData);
            FillCharacterSkills(characterData);
            FillCharacterSkillUsages(characterData);
            FillCharacterSummons(characterData);
        }

        public override void CreateCharacter(string userId, IPlayerCharacterData character)
        {
            ExecuteNonQuerySync("INSERT INTO characters " +
                "(id, userId, dataId, entityId, factionId, characterName, level, exp, currentHp, currentMp, currentStamina, currentFood, currentWater, equipWeaponSet, statPoint, skillPoint, gold, currentMapName, currentPositionX, currentPositionY, currentPositionZ, currentRotationX, currentRotationY, currentRotationZ, respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ, mountDataId, iconDataId, frameDataId, titleDataId) VALUES " +
                "(@id, @userId, @dataId, @entityId, @factionId, @characterName, @level, @exp, @currentHp, @currentMp, @currentStamina, @currentFood, @currentWater, @equipWeaponSet, @statPoint, @skillPoint, @gold, @currentMapName, @currentPositionX, @currentPositionY, @currentPositionZ, @currentRotationX, @currentRotationY, @currentRotationZ, @respawnMapName, @respawnPositionX, @respawnPositionY, @respawnPositionZ, @mountDataId, @iconDataId, @frameDataId, @titleDataId)",
                new MySqlParameter("@id", character.Id),
                new MySqlParameter("@userId", userId),
                new MySqlParameter("@dataId", character.DataId),
                new MySqlParameter("@entityId", character.EntityId),
                new MySqlParameter("@factionId", character.FactionId),
                new MySqlParameter("@characterName", character.CharacterName),
                new MySqlParameter("@level", character.Level),
                new MySqlParameter("@exp", character.Exp),
                new MySqlParameter("@currentHp", character.CurrentHp),
                new MySqlParameter("@currentMp", character.CurrentMp),
                new MySqlParameter("@currentStamina", character.CurrentStamina),
                new MySqlParameter("@currentFood", character.CurrentFood),
                new MySqlParameter("@currentWater", character.CurrentWater),
                new MySqlParameter("@equipWeaponSet", character.EquipWeaponSet),
                new MySqlParameter("@statPoint", character.StatPoint),
                new MySqlParameter("@skillPoint", character.SkillPoint),
                new MySqlParameter("@gold", character.Gold),
                new MySqlParameter("@currentMapName", character.CurrentMapName),
                new MySqlParameter("@currentPositionX", character.CurrentPositionX),
                new MySqlParameter("@currentPositionY", character.CurrentPositionY),
                new MySqlParameter("@currentPositionZ", character.CurrentPositionZ),
                new MySqlParameter("@currentRotationX", character.CurrentRotationX),
                new MySqlParameter("@currentRotationY", character.CurrentRotationY),
                new MySqlParameter("@currentRotationZ", character.CurrentRotationZ),
                new MySqlParameter("@respawnMapName", character.RespawnMapName),
                new MySqlParameter("@respawnPositionX", character.RespawnPositionX),
                new MySqlParameter("@respawnPositionY", character.RespawnPositionY),
                new MySqlParameter("@respawnPositionZ", character.RespawnPositionZ),
                new MySqlParameter("@mountDataId", character.MountDataId),
                new MySqlParameter("@iconDataId", character.IconDataId),
                new MySqlParameter("@frameDataId", character.FrameDataId),
                new MySqlParameter("@titleDataId", character.TitleDataId));
            FillCharacterRelatesData(character);
            this.InvokeInstanceDevExtMethods("CreateCharacter", userId, character);
        }

        private bool ReadCharacter(MySqlDataReader reader, out PlayerCharacterData result)
        {
            if (reader.Read())
            {
                result = new PlayerCharacterData();
                result.Id = reader.GetString(0);
                result.UserId = reader.GetString(1);
                result.DataId = reader.GetInt32(2);
                result.EntityId = reader.GetInt32(3);
                result.FactionId = reader.GetInt32(4);
                result.CharacterName = reader.GetString(5);
                result.Level = reader.GetInt32(6);
                result.Exp = reader.GetInt32(7);
                result.CurrentHp = reader.GetInt32(8);
                result.CurrentMp = reader.GetInt32(9);
                result.CurrentStamina = reader.GetInt32(10);
                result.CurrentFood = reader.GetInt32(11);
                result.CurrentWater = reader.GetInt32(12);
                result.EquipWeaponSet = reader.GetByte(13);
                result.StatPoint = reader.GetFloat(14);
                result.SkillPoint = reader.GetFloat(15);
                result.Gold = reader.GetInt32(16);
                result.PartyId = reader.GetInt32(17);
                result.GuildId = reader.GetInt32(18);
                result.GuildRole = reader.GetByte(19);
                result.SharedGuildExp = reader.GetInt32(20);
                result.CurrentMapName = reader.GetString(21);
                result.CurrentPositionX = reader.GetFloat(22);
                result.CurrentPositionY = reader.GetFloat(23);
                result.CurrentPositionZ = reader.GetFloat(24);
                result.CurrentRotationX = reader.GetFloat(25);
                result.CurrentRotationY = reader.GetFloat(26);
                result.CurrentRotationZ = reader.GetFloat(27);
                result.RespawnMapName = reader.GetString(28);
                result.RespawnPositionX = reader.GetFloat(29);
                result.RespawnPositionY = reader.GetFloat(30);
                result.RespawnPositionZ = reader.GetFloat(31);
                result.MountDataId = reader.GetInt32(32);
                result.IconDataId = reader.GetInt32(33);
                result.FrameDataId = reader.GetInt32(34);
                result.TitleDataId = reader.GetInt32(35);
                result.LastDeadTime = reader.GetInt64(36);
                result.UnmuteTime = reader.GetInt64(37);
                result.LastUpdate = ((System.DateTimeOffset)reader.GetDateTime(38)).ToUnixTimeSeconds();
                return true;
            }
            result = null;
            return false;
        }

        public override PlayerCharacterData ReadCharacter(
            string id,
            bool withEquipWeapons = true,
            bool withAttributes = true,
            bool withSkills = true,
            bool withSkillUsages = true,
            bool withBuffs = true,
            bool withEquipItems = true,
            bool withNonEquipItems = true,
            bool withSummons = true,
            bool withHotkeys = true,
            bool withQuests = true,
            bool withCurrencies = true)
        {
            PlayerCharacterData result = null;
            ExecuteReaderSync((reader) =>
            {
                ReadCharacter(reader, out result);
            }, "SELECT " +
                "id, userId, dataId, entityId, factionId, characterName, level, exp, " +
                "currentHp, currentMp, currentStamina, currentFood, currentWater, " +
                "equipWeaponSet, statPoint, skillPoint, gold, partyId, guildId, guildRole, sharedGuildExp, " +
                "currentMapName, currentPositionX, currentPositionY, currentPositionZ, currentRotationX, currentRotationY, currentRotationZ," +
                "respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ," +
                "mountDataId, iconDataId, frameDataId, titleDataId, lastDeadTime, unmuteTime, updateAt FROM characters WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", id));
            // Found character, then read its relates data
            if (result != null)
            {
                List<EquipWeapons> selectableWeaponSets = new List<EquipWeapons>();
                List<CharacterAttribute> attributes = new List<CharacterAttribute>();
                List<CharacterSkill> skills = new List<CharacterSkill>();
                List<CharacterSkillUsage> skillUsages = new List<CharacterSkillUsage>();
                List<CharacterBuff> buffs = new List<CharacterBuff>();
                List<CharacterItem> equipItems = new List<CharacterItem>();
                List<CharacterItem> nonEquipItems = new List<CharacterItem>();
                List<CharacterSummon> summons = new List<CharacterSummon>();
                List<CharacterHotkey> hotkeys = new List<CharacterHotkey>();
                List<CharacterQuest> quests = new List<CharacterQuest>();
                List<CharacterCurrency> currencies = new List<CharacterCurrency>();
                // Read data
                if (withEquipWeapons)
                    ReadCharacterEquipWeapons(id, selectableWeaponSets);
                if (withAttributes)
                    ReadCharacterAttributes(id, attributes);
                if (withSkills)
                    ReadCharacterSkills(id, skills);
                if (withSkillUsages)
                    ReadCharacterSkillUsages(id, skillUsages);
                if (withBuffs)
                    ReadCharacterBuffs(id, buffs);
                if (withEquipItems)
                    ReadCharacterEquipItems(id, equipItems);
                if (withNonEquipItems)
                    ReadCharacterNonEquipItems(id, nonEquipItems);
                if (withSummons)
                    ReadCharacterSummons(id, summons);
                if (withHotkeys)
                    ReadCharacterHotkeys(id, hotkeys);
                if (withQuests)
                    ReadCharacterQuests(id, quests);
                if (withCurrencies)
                    ReadCharacterCurrencies(id, currencies);
                // Assign read data
                if (withEquipWeapons)
                    result.SelectableWeaponSets = selectableWeaponSets;
                if (withAttributes)
                    result.Attributes = attributes;
                if (withSkills)
                    result.Skills = skills;
                if (withSkillUsages)
                    result.SkillUsages = skillUsages;
                if (withBuffs)
                    result.Buffs = buffs;
                if (withEquipItems)
                    result.EquipItems = equipItems;
                if (withNonEquipItems)
                    result.NonEquipItems = nonEquipItems;
                if (withSummons)
                    result.Summons = summons;
                if (withHotkeys)
                    result.Hotkeys = hotkeys;
                if (withQuests)
                    result.Quests = quests;
                if (withCurrencies)
                    result.Currencies = currencies;
                // Invoke dev extension methods
                this.InvokeInstanceDevExtMethods("ReadCharacter",
                    result,
                    withEquipWeapons,
                    withAttributes,
                    withSkills,
                    withSkillUsages,
                    withBuffs,
                    withEquipItems,
                    withNonEquipItems,
                    withSummons,
                    withHotkeys,
                    withQuests,
                    withCurrencies);
            }
            return result;
        }

        public override List<PlayerCharacterData> ReadCharacters(string userId)
        {
            List<PlayerCharacterData> result = new List<PlayerCharacterData>();
            List<string> characterIds = new List<string>();
            ExecuteReaderSync((reader) =>
            {
                while (reader.Read())
                {
                    characterIds.Add(reader.GetString(0));
                }
            }, "SELECT id FROM characters WHERE userId=@userId ORDER BY updateAt DESC", new MySqlParameter("@userId", userId));
            foreach (string characterId in characterIds)
            {
                result.Add(ReadCharacter(characterId, true, true, true, false, false, true, false, false, false, false));
            }
            return result;
        }

        public override void UpdateCharacter(IPlayerCharacterData character)
        {
            ExecuteNonQuerySync("UPDATE characters SET " +
                "dataId=@dataId, " +
                "entityId=@entityId, " +
                "factionId=@factionId, " +
                "characterName=@characterName, " +
                "level=@level, " +
                "exp=@exp, " +
                "currentHp=@currentHp, " +
                "currentMp=@currentMp, " +
                "currentStamina=@currentStamina, " +
                "currentFood=@currentFood, " +
                "currentWater=@currentWater, " +
                "equipWeaponSet=@equipWeaponSet, " +
                "statPoint=@statPoint, " +
                "skillPoint=@skillPoint, " +
                "gold=@gold, " +
                "currentMapName=@currentMapName, " +
                "currentPositionX=@currentPositionX, " +
                "currentPositionY=@currentPositionY, " +
                "currentPositionZ=@currentPositionZ, " +
                "currentRotationX=@currentRotationX, " +
                "currentRotationY=@currentRotationY, " +
                "currentRotationZ=@currentRotationZ, " +
                "respawnMapName=@respawnMapName, " +
                "respawnPositionX=@respawnPositionX, " +
                "respawnPositionY=@respawnPositionY, " +
                "respawnPositionZ=@respawnPositionZ, " +
                "mountDataId=@mountDataId, " +
                "iconDataId=@iconDataId, " +
                "frameDataId=@frameDataId, " +
                "titleDataId=@titleDataId, " +
                "lastDeadTime=@lastDeadTime, " +
                "unmuteTime=@unmuteTime " +
                "WHERE id=@id",
                new MySqlParameter("@dataId", character.DataId),
                new MySqlParameter("@entityId", character.EntityId),
                new MySqlParameter("@factionId", character.FactionId),
                new MySqlParameter("@characterName", character.CharacterName),
                new MySqlParameter("@level", character.Level),
                new MySqlParameter("@exp", character.Exp),
                new MySqlParameter("@currentHp", character.CurrentHp),
                new MySqlParameter("@currentMp", character.CurrentMp),
                new MySqlParameter("@currentStamina", character.CurrentStamina),
                new MySqlParameter("@currentFood", character.CurrentFood),
                new MySqlParameter("@currentWater", character.CurrentWater),
                new MySqlParameter("@equipWeaponSet", character.EquipWeaponSet),
                new MySqlParameter("@statPoint", character.StatPoint),
                new MySqlParameter("@skillPoint", character.SkillPoint),
                new MySqlParameter("@gold", character.Gold),
                new MySqlParameter("@currentMapName", character.CurrentMapName),
                new MySqlParameter("@currentPositionX", character.CurrentPositionX),
                new MySqlParameter("@currentPositionY", character.CurrentPositionY),
                new MySqlParameter("@currentPositionZ", character.CurrentPositionZ),
                new MySqlParameter("@currentRotationX", character.CurrentRotationX),
                new MySqlParameter("@currentRotationY", character.CurrentRotationY),
                new MySqlParameter("@currentRotationZ", character.CurrentRotationZ),
                new MySqlParameter("@respawnMapName", character.RespawnMapName),
                new MySqlParameter("@respawnPositionX", character.RespawnPositionX),
                new MySqlParameter("@respawnPositionY", character.RespawnPositionY),
                new MySqlParameter("@respawnPositionZ", character.RespawnPositionZ),
                new MySqlParameter("@mountDataId", character.MountDataId),
                new MySqlParameter("@iconDataId", character.IconDataId),
                new MySqlParameter("@frameDataId", character.FrameDataId),
                new MySqlParameter("@titleDataId", character.TitleDataId),
                new MySqlParameter("@lastDeadTime", character.LastDeadTime),
                new MySqlParameter("@unmuteTime", character.UnmuteTime),
                new MySqlParameter("@id", character.Id));
            FillCharacterRelatesData(character);
            this.InvokeInstanceDevExtMethods("UpdateCharacter", character);
        }

        public override void DeleteCharacter(string userId, string id)
        {
            object result = ExecuteScalarSync("SELECT COUNT(*) FROM characters WHERE id=@id AND userId=@userId",
                new MySqlParameter("@id", id),
                new MySqlParameter("@userId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                MySqlConnection connection = NewConnection();
                OpenConnectionSync(connection);
                MySqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    ExecuteNonQuerySync(connection, transaction, "DELETE FROM characters WHERE id=@characterId", new MySqlParameter("@characterId", id));
                    ExecuteNonQuerySync(connection, transaction, "DELETE FROM friend WHERE characterId1 LIKE @characterId OR characterId2 LIKE @characterId", new MySqlParameter("@characterId", id));
                    DeleteCharacterAttributes(connection, transaction, id);
                    DeleteCharacterCurrencies(connection, transaction, id);
                    DeleteCharacterBuffs(connection, transaction, id);
                    DeleteCharacterHotkeys(connection, transaction, id);
                    DeleteCharacterItems(connection, transaction, id);
                    DeleteCharacterQuests(connection, transaction, id);
                    DeleteCharacterSkills(connection, transaction, id);
                    DeleteCharacterSkillUsages(connection, transaction, id);
                    DeleteCharacterSummons(connection, transaction, id);
                    transaction.Commit();
                }
                catch (System.Exception ex)
                {
                    LogError(LogTag, "Transaction, Error occurs while deleting character: " + id);
                    LogException(LogTag, ex);
                    transaction.Rollback();
                }
                transaction.Dispose();
                connection.Close();
                this.InvokeInstanceDevExtMethods("DeleteCharacter", userId, id);
            }
        }

        public override long FindCharacterName(string characterName)
        {
            object result = ExecuteScalarSync("SELECT COUNT(*) FROM characters WHERE characterName LIKE @characterName",
                new MySqlParameter("@characterName", characterName));
            return result != null ? (long)result : 0;
        }

        public override string GetIdByCharacterName(string characterName)
        {
            object result = ExecuteScalarSync("SELECT id FROM characters WHERE characterName LIKE @characterName LIMIT 1",
                new MySqlParameter("@characterName", characterName));
            return result != null ? (string)result : string.Empty;
        }

        public override string GetUserIdByCharacterName(string characterName)
        {
            object result = ExecuteScalarSync("SELECT userId FROM characters WHERE characterName LIKE @characterName LIMIT 1",
                new MySqlParameter("@characterName", characterName));
            return result != null ? (string)result : string.Empty;
        }

        public override List<SocialCharacterData> FindCharacters(string finderId, string characterName, int skip, int limit)
        {
            string excludeIdsQuery = "(id!='" + finderId + "'";
            // Exclude friend, requested characters
            ExecuteReaderSync((reader) =>
            {
                while (reader.Read())
                {
                    excludeIdsQuery += " AND id!='" + reader.GetString(0) + "'";
                }
            }, "SELECT characterId2 FROM friend WHERE characterId1='" + finderId + "'");
            excludeIdsQuery += ")";
            List<SocialCharacterData> result = new List<SocialCharacterData>();
            ExecuteReaderSync((reader) =>
            {
                SocialCharacterData socialCharacterData;
                while (reader.Read())
                {
                    // Get some required data, other data will be set at server side
                    socialCharacterData = new SocialCharacterData();
                    socialCharacterData.id = reader.GetString(0);
                    socialCharacterData.dataId = reader.GetInt32(1);
                    socialCharacterData.characterName = reader.GetString(2);
                    socialCharacterData.level = reader.GetInt32(3);
                    result.Add(socialCharacterData);
                }
            }, "SELECT id, dataId, characterName, level FROM characters WHERE characterName LIKE @characterName AND " + excludeIdsQuery + " ORDER BY RAND() LIMIT " + skip + ", " + limit,
                new MySqlParameter("@characterName", "%" + characterName + "%"));
            return result;
        }

        public override void CreateFriend(string id1, string id2, byte state)
        {
            DeleteFriend(id1, id2);
            ExecuteNonQuerySync("INSERT INTO friend " +
                "(characterId1, characterId2, state) VALUES " +
                "(@characterId1, @characterId2, @state)",
                new MySqlParameter("@characterId1", id1),
                new MySqlParameter("@characterId2", id2),
                new MySqlParameter("@state", state));
        }

        public override void DeleteFriend(string id1, string id2)
        {
            ExecuteNonQuerySync("DELETE FROM friend WHERE " +
               "characterId1 LIKE @characterId1 AND " +
               "characterId2 LIKE @characterId2",
               new MySqlParameter("@characterId1", id1),
               new MySqlParameter("@characterId2", id2));
        }

        public override List<SocialCharacterData> ReadFriends(string id, bool readById2, byte state, int skip, int limit)
        {
            List<SocialCharacterData> result = new List<SocialCharacterData>();
            List<string> characterIds = new List<string>();
            if (readById2)
            {
                ExecuteReaderSync((reader) =>
                {
                    while (reader.Read())
                    {
                        characterIds.Add(reader.GetString(0));
                    }
                }, "SELECT characterId1 FROM friend WHERE characterId2=@id AND state=" + state + " LIMIT " + skip + ", " + limit,
                    new MySqlParameter("@id", id));
            }
            else
            {
                ExecuteReaderSync((reader) =>
                {
                    while (reader.Read())
                    {
                        characterIds.Add(reader.GetString(0));
                    }
                }, "SELECT characterId2 FROM friend WHERE characterId1=@id AND state=" + state + " LIMIT " + skip + ", " + limit,
                    new MySqlParameter("@id", id));
            }
            SocialCharacterData socialCharacterData;
            foreach (string characterId in characterIds)
            {
                ExecuteReaderSync((reader) =>
                {
                    while (reader.Read())
                    {
                        // Get some required data, other data will be set at server side
                        socialCharacterData = new SocialCharacterData();
                        socialCharacterData.id = reader.GetString(0);
                        socialCharacterData.dataId = reader.GetInt32(1);
                        socialCharacterData.characterName = reader.GetString(2);
                        socialCharacterData.level = reader.GetInt32(3);
                        result.Add(socialCharacterData);
                    }
                }, "SELECT id, dataId, characterName, level FROM characters WHERE BINARY id = @id",
                    new MySqlParameter("@id", characterId));
            }
            return result;
        }

        public override int GetFriendRequestNotification(string characterId)
        {
            object result = ExecuteScalarSync("SELECT COUNT(*) FROM friend WHERE characterId2=@characterId AND state=1",
                new MySqlParameter("@characterId", characterId));
            return (int)(long)result;
        }
    }
}
#endif