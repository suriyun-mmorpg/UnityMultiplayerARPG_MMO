using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using Cysharp.Threading.Tasks;
using LiteNetLibManager;

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
                DeleteCharacterBuffs(transaction, characterId);
                DeleteCharacterHotkeys(transaction, characterId);
                DeleteCharacterItems(transaction, characterId);
                DeleteCharacterQuests(transaction, characterId);
                DeleteCharacterSkills(transaction, characterId);
                DeleteCharacterSkillUsages(transaction, characterId);
                DeleteCharacterSummons(transaction, characterId);

                int i;
                for (i = 0; i < characterData.SelectableWeaponSets.Count; ++i)
                {
                    CreateCharacterEquipWeapons(transaction, (byte)i, characterData.Id, characterData.SelectableWeaponSets[i]);
                }
                for (i = 0; i < characterData.EquipItems.Count; ++i)
                {
                    CreateCharacterEquipItem(transaction, i, characterData.Id, characterData.EquipItems[i]);
                }
                for (i = 0; i < characterData.NonEquipItems.Count; ++i)
                {
                    CreateCharacterNonEquipItem(transaction, i, characterData.Id, characterData.NonEquipItems[i]);
                }
                for (i = 0; i < characterData.Attributes.Count; ++i)
                {
                    CreateCharacterAttribute(transaction, i, characterData.Id, characterData.Attributes[i]);
                }
                for (i = 0; i < characterData.Skills.Count; ++i)
                {
                    CreateCharacterSkill(transaction, i, characterData.Id, characterData.Skills[i]);
                }
                for (i = 0; i < characterData.SkillUsages.Count; ++i)
                {
                    CreateCharacterSkillUsage(transaction, characterData.Id, characterData.SkillUsages[i]);
                }
                for (i = 0; i < characterData.Summons.Count; ++i)
                {
                    CreateCharacterSummon(transaction, i, characterData.Id, characterData.Summons[i]);
                }
                for (i = 0; i < characterData.Quests.Count; ++i)
                {
                    CreateCharacterQuest(transaction, i, characterData.Id, characterData.Quests[i]);
                }
                for (i = 0; i < characterData.Buffs.Count; ++i)
                {
                    CreateCharacterBuff(transaction, characterData.Id, characterData.Buffs[i]);
                }
                for (i = 0; i < characterData.Hotkeys.Count; ++i)
                {
                    CreateCharacterHotkey(transaction, characterData.Id, characterData.Hotkeys[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while filling character relates data");
                Logging.LogException(ToString(), ex);
                transaction.Rollback();
            }
            transaction.Dispose();
        }

        public override async UniTask CreateCharacter(string userId, IPlayerCharacterData characterData)
        {
            await UniTask.Yield();
            ExecuteNonQuery("INSERT INTO characters " +
                "(id, userId, dataId, entityId, factionId, characterName, level, exp, currentHp, currentMp, currentStamina, currentFood, currentWater, equipWeaponSet, statPoint, skillPoint, gold, currentMapName, currentPositionX, currentPositionY, currentPositionZ, respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ, mountDataId) VALUES " +
                "(@id, @userId, @dataId, @entityId, @factionId, @characterName, @level, @exp, @currentHp, @currentMp, @currentStamina, @currentFood, @currentWater, @equipWeaponSet, @statPoint, @skillPoint, @gold, @currentMapName, @currentPositionX, @currentPositionY, @currentPositionZ, @respawnMapName, @respawnPositionX, @respawnPositionY, @respawnPositionZ, @mountDataId)",
                new SqliteParameter("@id", characterData.Id),
                new SqliteParameter("@userId", userId),
                new SqliteParameter("@dataId", characterData.DataId),
                new SqliteParameter("@entityId", characterData.EntityId),
                new SqliteParameter("@factionId", characterData.FactionId),
                new SqliteParameter("@characterName", characterData.CharacterName),
                new SqliteParameter("@level", characterData.Level),
                new SqliteParameter("@exp", characterData.Exp),
                new SqliteParameter("@currentHp", characterData.CurrentHp),
                new SqliteParameter("@currentMp", characterData.CurrentMp),
                new SqliteParameter("@currentStamina", characterData.CurrentStamina),
                new SqliteParameter("@currentFood", characterData.CurrentFood),
                new SqliteParameter("@currentWater", characterData.CurrentWater),
                new SqliteParameter("@equipWeaponSet", characterData.EquipWeaponSet),
                new SqliteParameter("@statPoint", characterData.StatPoint),
                new SqliteParameter("@skillPoint", characterData.SkillPoint),
                new SqliteParameter("@gold", characterData.Gold),
                new SqliteParameter("@currentMapName", characterData.CurrentMapName),
                new SqliteParameter("@currentPositionX", characterData.CurrentPosition.x),
                new SqliteParameter("@currentPositionY", characterData.CurrentPosition.y),
                new SqliteParameter("@currentPositionZ", characterData.CurrentPosition.z),
                new SqliteParameter("@respawnMapName", characterData.RespawnMapName),
                new SqliteParameter("@respawnPositionX", characterData.RespawnPosition.x),
                new SqliteParameter("@respawnPositionY", characterData.RespawnPosition.y),
                new SqliteParameter("@respawnPositionZ", characterData.RespawnPosition.z),
                new SqliteParameter("@mountDataId", characterData.MountDataId));
            FillCharacterRelatesData(characterData);
            this.InvokeInstanceDevExtMethods("CreateCharacter", userId, characterData);
        }

        private bool ReadCharacter(SqliteDataReader reader, out PlayerCharacterData result)
        {
            if (reader.Read())
            {
                result = new PlayerCharacterData();
                result.Id = reader.GetString(0);
                result.DataId = reader.GetInt32(1);
                result.EntityId = reader.GetInt32(2);
                result.FactionId = reader.GetInt32(3);
                result.CharacterName = reader.GetString(4);
                result.Level = reader.GetInt16(5);
                result.Exp = reader.GetInt32(6);
                result.CurrentHp = reader.GetInt32(7);
                result.CurrentMp = reader.GetInt32(8);
                result.CurrentStamina = reader.GetInt32(9);
                result.CurrentFood = reader.GetInt32(10);
                result.CurrentWater = reader.GetInt32(11);
                result.EquipWeaponSet = reader.GetByte(12);
                result.StatPoint = reader.GetInt16(13);
                result.SkillPoint = reader.GetInt16(14);
                result.Gold = reader.GetInt32(15);
                result.PartyId = reader.GetInt32(16);
                result.GuildId = reader.GetInt32(17);
                result.GuildRole = reader.GetByte(18);
                result.SharedGuildExp = reader.GetInt32(19);
                result.CurrentMapName = reader.GetString(20);
                result.CurrentPosition = new Vector3(
                    reader.GetFloat(21),
                    reader.GetFloat(22),
                    reader.GetFloat(23));
                result.RespawnMapName = reader.GetString(24);
                result.RespawnPosition = new Vector3(
                    reader.GetFloat(25),
                    reader.GetFloat(26),
                    reader.GetFloat(27));
                result.MountDataId = reader.GetInt32(28);
                result.LastUpdate = (int)(reader.GetDateTime(29).Ticks / System.TimeSpan.TicksPerMillisecond);
                return true;
            }
            result = null;
            return false;
        }

        public override async UniTask<PlayerCharacterData> ReadCharacter(
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
            bool withQuests = true)
        {
            await UniTask.Yield();
            PlayerCharacterData result = null;
            ExecuteReader((reader) =>
            {
                if (ReadCharacter(reader, out result))
                {
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
                        withQuests);
                }
            }, "SELECT " +
                "id, dataId, entityId, factionId, characterName, level, exp, " +
                "currentHp, currentMp, currentStamina, currentFood, currentWater, " +
                "equipWeaponSet, statPoint, skillPoint, gold, partyId, guildId, guildRole, sharedGuildExp, " +
                "currentMapName, currentPositionX, currentPositionY, currentPositionZ," +
                "respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ," +
                "mountDataId, updateAt FROM characters WHERE id=@id LIMIT 1",
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
            }
            return result;
        }

        public override async UniTask<List<PlayerCharacterData>> ReadCharacters(string userId)
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
                result.Add(await ReadCharacter(characterId, true, true, true, false, false, true, false, false, false, false));
            }
            return result;
        }

        public override async UniTask UpdateCharacter(IPlayerCharacterData character)
        {
            await UniTask.Yield();
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
                "respawnMapName=@respawnMapName, " +
                "respawnPositionX=@respawnPositionX, " +
                "respawnPositionY=@respawnPositionY, " +
                "respawnPositionZ=@respawnPositionZ, " +
                "mountDataId=@mountDataId " +
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
                new SqliteParameter("@currentPositionX", character.CurrentPosition.x),
                new SqliteParameter("@currentPositionY", character.CurrentPosition.y),
                new SqliteParameter("@currentPositionZ", character.CurrentPosition.z),
                new SqliteParameter("@respawnMapName", character.RespawnMapName),
                new SqliteParameter("@respawnPositionX", character.RespawnPosition.x),
                new SqliteParameter("@respawnPositionY", character.RespawnPosition.y),
                new SqliteParameter("@respawnPositionZ", character.RespawnPosition.z),
                new SqliteParameter("@mountDataId", character.MountDataId),
                new SqliteParameter("@id", character.Id));
            FillCharacterRelatesData(character);
            this.InvokeInstanceDevExtMethods("UpdateCharacter", character);
        }

        public override async UniTask DeleteCharacter(string userId, string id)
        {
            await UniTask.Yield();
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
                    DeleteCharacterAttributes(transaction, id);
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
                    Logging.LogError(ToString(), "Transaction, Error occurs while deleting character: " + id);
                    Logging.LogException(ToString(), ex);
                    transaction.Rollback();
                }
                transaction.Dispose();
                this.InvokeInstanceDevExtMethods("DeleteCharacter", userId, id);
            }
        }

        public override async UniTask<long> FindCharacterName(string characterName)
        {
            await UniTask.Yield();
            object result = ExecuteScalar("SELECT COUNT(*) FROM characters WHERE characterName LIKE @characterName",
                new SqliteParameter("@characterName", characterName));
            return result != null ? (long)result : 0;
        }

        public override async UniTask<List<SocialCharacterData>> FindCharacters(string characterName)
        {
            await UniTask.Yield();
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
                    socialCharacterData.level = reader.GetInt16(3);
                    result.Add(socialCharacterData);
                }
            }, "SELECT id, dataId, characterName, level FROM characters WHERE characterName LIKE @characterName LIMIT 0, 20",
                new SqliteParameter("@characterName", "%" + characterName + "%"));
            return result;
        }

        public override async UniTask CreateFriend(string id1, string id2)
        {
            await DeleteFriend(id1, id2);
            ExecuteNonQuery("INSERT INTO friend " +
                "(characterId1, characterId2) VALUES " +
                "(@characterId1, @characterId2)",
                new SqliteParameter("@characterId1", id1),
                new SqliteParameter("@characterId2", id2));
        }

        public override async UniTask DeleteFriend(string id1, string id2)
        {
            await UniTask.Yield();
            ExecuteNonQuery("DELETE FROM friend WHERE " +
                "characterId1 LIKE @characterId1 AND " +
                "characterId2 LIKE @characterId2",
                new SqliteParameter("@characterId1", id1),
                new SqliteParameter("@characterId2", id2));
        }

        public override async UniTask<List<SocialCharacterData>> ReadFriends(string id1)
        {
            await UniTask.Yield();
            List<SocialCharacterData> result = new List<SocialCharacterData>();
            List<string> characterIds = new List<string>();
            ExecuteReader((reader) =>
            {
                while (reader.Read())
                {
                    characterIds.Add(reader.GetString(0));
                }
            }, "SELECT characterId2 FROM friend WHERE characterId1=@id1",
                new SqliteParameter("@id1", id1));
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
                        socialCharacterData.level = reader.GetInt16(3);
                        result.Add(socialCharacterData);
                    }
                }, "SELECT id, dataId, characterName, level FROM characters WHERE id LIKE @id",
                    new SqliteParameter("@id", characterId));
            }
            return result;
        }
    }
}
