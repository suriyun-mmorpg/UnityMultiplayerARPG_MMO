#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private void FillCharacterRelatesData(IPlayerCharacterData characterData)
        {
            // Delete all character then add all of them
            string characterId = characterData.Id;
            SqliteTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterAttributes(transaction, characterId);
                DeleteCharacterCurrencies(transaction, characterId);
                DeleteCharacterBuffs(transaction, characterId);
                DeleteCharacterHotkeys(transaction, characterId);
                DeleteCharacterItems(transaction, characterId);
                DeleteCharacterQuests(transaction, characterId);
                DeleteCharacterSkills(transaction, characterId);
                DeleteCharacterSkillUsages(transaction, characterId);
                DeleteCharacterSummons(transaction, characterId);

                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                insertedIds.Clear();
                for (i = 0; i < characterData.SelectableWeaponSets.Count; ++i)
                {
                    CreateCharacterEquipWeapons(transaction, insertedIds, i, characterData.Id, characterData.SelectableWeaponSets[i]);
                }
                for (i = 0; i < characterData.EquipItems.Count; ++i)
                {
                    CreateCharacterEquipItem(transaction, insertedIds, i, characterData.Id, characterData.EquipItems[i]);
                }
                for (i = 0; i < characterData.NonEquipItems.Count; ++i)
                {
                    CreateCharacterNonEquipItem(transaction, insertedIds, i, characterData.Id, characterData.NonEquipItems[i]);
                }

                insertedIds.Clear();
                for (i = 0; i < characterData.Attributes.Count; ++i)
                {
                    CreateCharacterAttribute(transaction, insertedIds, i, characterData.Id, characterData.Attributes[i]);
                }

                insertedIds.Clear();
                for (i = 0; i < characterData.Currencies.Count; ++i)
                {
                    CreateCharacterCurrency(transaction, insertedIds, i, characterData.Id, characterData.Currencies[i]);
                }

                insertedIds.Clear();
                for (i = 0; i < characterData.Skills.Count; ++i)
                {
                    CreateCharacterSkill(transaction, insertedIds, i, characterData.Id, characterData.Skills[i]);
                }

                insertedIds.Clear();
                for (i = 0; i < characterData.SkillUsages.Count; ++i)
                {
                    CreateCharacterSkillUsage(transaction, insertedIds, characterData.Id, characterData.SkillUsages[i]);
                }

                insertedIds.Clear();
                for (i = 0; i < characterData.Summons.Count; ++i)
                {
                    CreateCharacterSummon(transaction, insertedIds, i, characterData.Id, characterData.Summons[i]);
                }

                insertedIds.Clear();
                for (i = 0; i < characterData.Quests.Count; ++i)
                {
                    CreateCharacterQuest(transaction, insertedIds, i, characterData.Id, characterData.Quests[i]);
                }

                insertedIds.Clear();
                for (i = 0; i < characterData.Buffs.Count; ++i)
                {
                    CreateCharacterBuff(transaction, insertedIds, characterData.Id, characterData.Buffs[i]);
                }

                insertedIds.Clear();
                for (i = 0; i < characterData.Hotkeys.Count; ++i)
                {
                    CreateCharacterHotkey(transaction, insertedIds, characterData.Id, characterData.Hotkeys[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while filling character relates data");
                LogException(LogTag, ex);
                transaction.Rollback();
            }
            transaction.Dispose();
        }

        public override void CreateCharacter(string userId, IPlayerCharacterData character)
        {
            ExecuteNonQuery("INSERT INTO characters " +
                "(id, userId, dataId, entityId, factionId, characterName, level, exp, currentHp, currentMp, currentStamina, currentFood, currentWater, equipWeaponSet, statPoint, skillPoint, gold, currentMapName, currentPositionX, currentPositionY, currentPositionZ, currentRotationX, currentRotationY, currentRotationZ, respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ, mountDataId, iconDataId, frameDataId, titleDataId) VALUES " +
                "(@id, @userId, @dataId, @entityId, @factionId, @characterName, @level, @exp, @currentHp, @currentMp, @currentStamina, @currentFood, @currentWater, @equipWeaponSet, @statPoint, @skillPoint, @gold, @currentMapName, @currentPositionX, @currentPositionY, @currentPositionZ, @currentRotationX, @currentRotationY, @currentRotationZ, @respawnMapName, @respawnPositionX, @respawnPositionY, @respawnPositionZ, @mountDataId, @iconDataId, @frameDataId, @titleDataId)",
                new SqliteParameter("@id", character.Id),
                new SqliteParameter("@userId", userId),
                new SqliteParameter("@dataId", character.DataId),
                new SqliteParameter("@entityId", character.EntityId),
                new SqliteParameter("@factionId", character.FactionId),
                new SqliteParameter("@characterName", character.CharacterName),
                new SqliteParameter("@level", character.Level),
                new SqliteParameter("@exp", character.Exp),
                new SqliteParameter("@currentHp", character.CurrentHp),
                new SqliteParameter("@currentMp", character.CurrentMp),
                new SqliteParameter("@currentStamina", character.CurrentStamina),
                new SqliteParameter("@currentFood", character.CurrentFood),
                new SqliteParameter("@currentWater", character.CurrentWater),
                new SqliteParameter("@equipWeaponSet", character.EquipWeaponSet),
                new SqliteParameter("@statPoint", character.StatPoint),
                new SqliteParameter("@skillPoint", character.SkillPoint),
                new SqliteParameter("@gold", character.Gold),
                new SqliteParameter("@currentMapName", character.CurrentMapName),
                new SqliteParameter("@currentPositionX", character.CurrentPositionX),
                new SqliteParameter("@currentPositionY", character.CurrentPositionY),
                new SqliteParameter("@currentPositionZ", character.CurrentPositionZ),
                new SqliteParameter("@currentRotationX", character.CurrentRotationX),
                new SqliteParameter("@currentRotationY", character.CurrentRotationY),
                new SqliteParameter("@currentRotationZ", character.CurrentRotationZ),
                new SqliteParameter("@respawnMapName", character.RespawnMapName),
                new SqliteParameter("@respawnPositionX", character.RespawnPositionX),
                new SqliteParameter("@respawnPositionY", character.RespawnPositionY),
                new SqliteParameter("@respawnPositionZ", character.RespawnPositionZ),
                new SqliteParameter("@mountDataId", character.MountDataId),
                new SqliteParameter("@iconDataId", character.IconDataId),
                new SqliteParameter("@frameDataId", character.FrameDataId),
                new SqliteParameter("@titleDataId", character.TitleDataId));
            FillCharacterRelatesData(character);
            this.InvokeInstanceDevExtMethods("CreateCharacter", userId, character);
        }

        private bool ReadCharacter(SqliteDataReader reader, out PlayerCharacterData result)
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
            ExecuteReader((reader) =>
            {
                ReadCharacter(reader, out result);
            }, "SELECT " +
                "id, userId, dataId, entityId, factionId, characterName, level, exp, " +
                "currentHp, currentMp, currentStamina, currentFood, currentWater, " +
                "equipWeaponSet, statPoint, skillPoint, gold, partyId, guildId, guildRole, sharedGuildExp, " +
                "currentMapName, currentPositionX, currentPositionY, currentPositionZ, currentRotationX, currentRotationY, currentRotationZ," +
                "respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ," +
                "mountDataId, iconDataId, frameDataId, titleDataId, lastDeadTime, unmuteTime, updateAt FROM characters WHERE id=@id LIMIT 1",
                new SqliteParameter("@id", id));
            // Found character, then read its relates data
            if (result != null)
            {
                if (withEquipWeapons)
                    result.SelectableWeaponSets = ReadCharacterEquipWeapons(id);
                if (withAttributes)
                    result.Attributes = ReadCharacterAttributes(id);
                if (withSkills)
                    result.Skills = ReadCharacterSkills(id);
                if (withSkillUsages)
                    result.SkillUsages = ReadCharacterSkillUsages(id);
                if (withBuffs)
                    result.Buffs = ReadCharacterBuffs(id);
                if (withEquipItems)
                    result.EquipItems = ReadCharacterEquipItems(id);
                if (withNonEquipItems)
                    result.NonEquipItems = ReadCharacterNonEquipItems(id);
                if (withSummons)
                    result.Summons = ReadCharacterSummons(id);
                if (withHotkeys)
                    result.Hotkeys = ReadCharacterHotkeys(id);
                if (withQuests)
                    result.Quests = ReadCharacterQuests(id);
                if (withCurrencies)
                    result.Currencies = ReadCharacterCurrencies(id);
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
            ExecuteReader((reader) =>
            {
                while (reader.Read())
                {
                    characterIds.Add(reader.GetString(0));
                }
            }, "SELECT id FROM characters WHERE userId=@userId ORDER BY updateAt DESC", new SqliteParameter("@userId", userId));
            foreach (string characterId in characterIds)
            {
                result.Add(ReadCharacter(characterId, true, true, true, false, false, true, false, false, false, false));
            }
            return result;
        }

        public override void UpdateCharacter(IPlayerCharacterData character)
        {
            ExecuteNonQuery("UPDATE characters SET " +
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
                new SqliteParameter("@dataId", character.DataId),
                new SqliteParameter("@entityId", character.EntityId),
                new SqliteParameter("@factionId", character.FactionId),
                new SqliteParameter("@characterName", character.CharacterName),
                new SqliteParameter("@level", character.Level),
                new SqliteParameter("@exp", character.Exp),
                new SqliteParameter("@currentHp", character.CurrentHp),
                new SqliteParameter("@currentMp", character.CurrentMp),
                new SqliteParameter("@currentStamina", character.CurrentStamina),
                new SqliteParameter("@currentFood", character.CurrentFood),
                new SqliteParameter("@currentWater", character.CurrentWater),
                new SqliteParameter("@equipWeaponSet", character.EquipWeaponSet),
                new SqliteParameter("@statPoint", character.StatPoint),
                new SqliteParameter("@skillPoint", character.SkillPoint),
                new SqliteParameter("@gold", character.Gold),
                new SqliteParameter("@currentMapName", character.CurrentMapName),
                new SqliteParameter("@currentPositionX", character.CurrentPositionX),
                new SqliteParameter("@currentPositionY", character.CurrentPositionY),
                new SqliteParameter("@currentPositionZ", character.CurrentPositionZ),
                new SqliteParameter("@currentRotationX", character.CurrentRotationX),
                new SqliteParameter("@currentRotationY", character.CurrentRotationY),
                new SqliteParameter("@currentRotationZ", character.CurrentRotationZ),
                new SqliteParameter("@respawnMapName", character.RespawnMapName),
                new SqliteParameter("@respawnPositionX", character.RespawnPositionX),
                new SqliteParameter("@respawnPositionY", character.RespawnPositionY),
                new SqliteParameter("@respawnPositionZ", character.RespawnPositionZ),
                new SqliteParameter("@mountDataId", character.MountDataId),
                new SqliteParameter("@iconDataId", character.IconDataId),
                new SqliteParameter("@frameDataId", character.FrameDataId),
                new SqliteParameter("@titleDataId", character.TitleDataId),
                new SqliteParameter("@lastDeadTime", character.LastDeadTime),
                new SqliteParameter("@unmuteTime", character.UnmuteTime),
                new SqliteParameter("@id", character.Id));
            FillCharacterRelatesData(character);
            this.InvokeInstanceDevExtMethods("UpdateCharacter", character);
        }

        public override void DeleteCharacter(string userId, string id)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM characters WHERE id=@id AND userId=@userId",
                new SqliteParameter("@id", id),
                new SqliteParameter("@userId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                SqliteTransaction transaction = connection.BeginTransaction();
                try
                {
                    ExecuteNonQuery(transaction, "DELETE FROM characters WHERE id=@characterId", new SqliteParameter("@characterId", id));
                    ExecuteNonQuery(transaction, "DELETE FROM friend WHERE characterId1 LIKE @characterId OR characterId2 LIKE @characterId", new SqliteParameter("@characterId", id));
                    DeleteCharacterAttributes(transaction, id);
                    DeleteCharacterCurrencies(transaction, id);
                    DeleteCharacterBuffs(transaction, id);
                    DeleteCharacterHotkeys(transaction, id);
                    DeleteCharacterItems(transaction, id);
                    DeleteCharacterQuests(transaction, id);
                    DeleteCharacterSkills(transaction, id);
                    DeleteCharacterSkillUsages(transaction, id);
                    DeleteCharacterSummons(transaction, id);
                    transaction.Commit();
                }
                catch (System.Exception ex)
                {
                    LogError(LogTag, "Transaction, Error occurs while deleting character: " + id);
                    LogException(LogTag, ex);
                    transaction.Rollback();
                }
                transaction.Dispose();
                this.InvokeInstanceDevExtMethods("DeleteCharacter", userId, id);
            }
        }

        public override long FindCharacterName(string characterName)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM characters WHERE characterName LIKE @characterName",
                new SqliteParameter("@characterName", characterName));
            return result != null ? (long)result : 0;
        }

        public override string GetIdByCharacterName(string characterName)
        {
            object result = ExecuteScalar("SELECT id FROM characters WHERE characterName LIKE @characterName LIMIT 1",
                new SqliteParameter("@characterName", characterName));
            return result != null ? (string)result : string.Empty;
        }

        public override string GetUserIdByCharacterName(string characterName)
        {
            object result = ExecuteScalar("SELECT userId FROM characters WHERE characterName LIKE @characterName LIMIT 1",
                new SqliteParameter("@characterName", characterName));
            return result != null ? (string)result : string.Empty;
        }

        public override List<SocialCharacterData> FindCharacters(string finderId, string characterName, int skip, int limit)
        {
            string excludeIdsQuery = "(id!='" + finderId + "'";
            // Exclude friend, requested characters
            ExecuteReader((reader) =>
            {
                while (reader.Read())
                {
                    excludeIdsQuery += " AND id!='" + reader.GetString(0) + "'";
                }
            }, "SELECT characterId2 FROM friend WHERE characterId1='" + finderId + "'");
            excludeIdsQuery += ")";
            List<SocialCharacterData> result = new List<SocialCharacterData>();
            ExecuteReader((reader) =>
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
            }, "SELECT id, dataId, characterName, level FROM characters WHERE characterName LIKE @characterName AND " + excludeIdsQuery + " LIMIT " + skip + ", " + limit,
                new SqliteParameter("@characterName", "%" + characterName + "%"));
            return result;
        }

        public override void CreateFriend(string id1, string id2, byte state)
        {
            DeleteFriend(id1, id2);
            ExecuteNonQuery("INSERT INTO friend " +
                "(characterId1, characterId2, state) VALUES " +
                "(@characterId1, @characterId2, @state)",
                new SqliteParameter("@characterId1", id1),
                new SqliteParameter("@characterId2", id2),
                new SqliteParameter("@state", (int)state));
        }

        public override void DeleteFriend(string id1, string id2)
        {
            ExecuteNonQuery("DELETE FROM friend WHERE " +
                "characterId1 LIKE @characterId1 AND " +
                "characterId2 LIKE @characterId2",
                new SqliteParameter("@characterId1", id1),
                new SqliteParameter("@characterId2", id2));
        }

        public override List<SocialCharacterData> ReadFriends(string id, bool readById2, byte state, int skip, int limit)
        {
            List<SocialCharacterData> result = new List<SocialCharacterData>();
            List<string> characterIds = new List<string>();
            if (readById2)
            {
                ExecuteReader((reader) =>
                {
                    while (reader.Read())
                    {
                        characterIds.Add(reader.GetString(0));
                    }
                }, "SELECT characterId1 FROM friend WHERE characterId2=@id AND state=" + state + " LIMIT " + skip + ", " + limit,
                    new SqliteParameter("@id", id));
            }
            else
            {
                ExecuteReader((reader) =>
                {
                    while (reader.Read())
                    {
                        characterIds.Add(reader.GetString(0));
                    }
                }, "SELECT characterId2 FROM friend WHERE characterId1=@id AND state=" + state + " LIMIT " + skip + ", " + limit,
                    new SqliteParameter("@id", id));
            }
            SocialCharacterData socialCharacterData;
            foreach (string characterId in characterIds)
            {
                ExecuteReader((reader) =>
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
                }, "SELECT id, dataId, characterName, level FROM characters WHERE id LIKE @id",
                    new SqliteParameter("@id", characterId));
            }
            return result;
        }

        public override int GetFriendRequestNotification(string characterId)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM friend WHERE characterId2=@characterId AND state=1",
                new SqliteParameter("@characterId", characterId));
            return (int)(long)result;
        }
    }
}
#endif