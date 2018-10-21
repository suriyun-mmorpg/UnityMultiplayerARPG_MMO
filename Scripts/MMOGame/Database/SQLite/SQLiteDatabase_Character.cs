using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private void FillCharacterRelatesData(IPlayerCharacterData characterData)
        {
            // Delete all character then add all of them
            var characterId = characterData.Id;
            DeleteCharacterAttributes(characterId);
            DeleteCharacterBuffs(characterId);
            DeleteCharacterHotkeys(characterId);
            DeleteCharacterItems(characterId);
            DeleteCharacterQuests(characterId);
            DeleteCharacterSkills(characterId);
            
            CreateCharacterEquipWeapons(characterId, characterData.EquipWeapons);
            var i = 0;
            foreach (var equipItem in characterData.EquipItems)
            {
                CreateCharacterEquipItem(i++, characterId, equipItem);
            }
            i = 0;
            foreach (var nonEquipItem in characterData.NonEquipItems)
            {
                CreateCharacterNonEquipItem(i++, characterId, nonEquipItem);
            }
            i = 0;
            foreach (var attribute in characterData.Attributes)
            {
                CreateCharacterAttribute(i++, characterId, attribute);
            }
            i = 0;
            foreach (var skill in characterData.Skills)
            {
                CreateCharacterSkill(i++, characterId, skill);
            }
            i = 0;
            foreach (var quest in characterData.Quests)
            {
                CreateCharacterQuest(i++, characterId, quest);
            }
            foreach (var buff in characterData.Buffs)
            {
                CreateCharacterBuff(characterId, buff);
            }
            foreach (var hotkey in characterData.Hotkeys)
            {
                CreateCharacterHotkey(characterId, hotkey);
            }
        }

        public override void CreateCharacter(string userId, IPlayerCharacterData characterData)
        {
            ExecuteNonQuery("BEGIN");
            ExecuteNonQuery("INSERT INTO characters " +
                "(id, userId, dataId, entityId, characterName, level, exp, currentHp, currentMp, currentStamina, currentFood, currentWater, statPoint, skillPoint, gold, currentMapName, currentPositionX, currentPositionY, currentPositionZ, respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ) VALUES " +
                "(@id, @userId, @dataId, @entityId, @characterName, @level, @exp, @currentHp, @currentMp, @currentStamina, @currentFood, @currentWater, @statPoint, @skillPoint, @gold, @currentMapName, @currentPositionX, @currentPositionY, @currentPositionZ, @respawnMapName, @respawnPositionX, @respawnPositionY, @respawnPositionZ)",
                new SqliteParameter("@id", characterData.Id),
                new SqliteParameter("@userId", userId),
                new SqliteParameter("@dataId", characterData.DataId),
                new SqliteParameter("@entityId", characterData.EntityId),
                new SqliteParameter("@characterName", characterData.CharacterName),
                new SqliteParameter("@level", characterData.Level),
                new SqliteParameter("@exp", characterData.Exp),
                new SqliteParameter("@currentHp", characterData.CurrentHp),
                new SqliteParameter("@currentMp", characterData.CurrentMp),
                new SqliteParameter("@currentStamina", characterData.CurrentStamina),
                new SqliteParameter("@currentFood", characterData.CurrentFood),
                new SqliteParameter("@currentWater", characterData.CurrentWater),
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
                new SqliteParameter("@respawnPositionZ", characterData.RespawnPosition.z));
            FillCharacterRelatesData(characterData);
            ExecuteNonQuery("END");
            this.InvokeInstanceDevExtMethods("CreateCharacter", userId, characterData);
        }

        private bool ReadCharacter(SQLiteRowsReader reader, out PlayerCharacterData result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new PlayerCharacterData();
                result.Id = reader.GetString("id");
                result.DataId = reader.GetInt32("dataId");
                result.EntityId = reader.GetInt32("entityId");
                result.CharacterName = reader.GetString("characterName");
                result.Level = reader.GetInt16("level");
                result.Exp = reader.GetInt32("exp");
                result.CurrentHp = reader.GetInt32("currentHp");
                result.CurrentMp = reader.GetInt32("currentMp");
                result.CurrentStamina = reader.GetInt32("currentStamina");
                result.CurrentFood = reader.GetInt32("currentFood");
                result.CurrentWater = reader.GetInt32("currentWater");
                result.StatPoint = reader.GetInt16("statPoint");
                result.SkillPoint = reader.GetInt16("skillPoint");
                result.Gold = reader.GetInt32("gold");
                result.PartyId = reader.GetInt32("partyId");
                result.GuildId = reader.GetInt32("guildId");
                result.GuildRole = (byte)reader.GetInt32("guildRole");
                result.SharedGuildExp = reader.GetInt32("sharedGuildExp");
                result.CurrentMapName = reader.GetString("currentMapName");
                result.CurrentPosition = new Vector3(reader.GetFloat("currentPositionX"), reader.GetFloat("currentPositionY"), reader.GetFloat("currentPositionZ"));
                result.RespawnMapName = reader.GetString("respawnMapName");
                result.RespawnPosition = new Vector3(reader.GetFloat("respawnPositionX"), reader.GetFloat("respawnPositionY"), reader.GetFloat("respawnPositionZ"));
                result.LastUpdate = (int)(reader.GetDateTime("updateAt").Ticks / System.TimeSpan.TicksPerMillisecond);
                return true;
            }
            result = null;
            return false;
        }

        public override PlayerCharacterData ReadCharacter(
            string userId,
            string id,
            bool withEquipWeapons = true,
            bool withAttributes = true,
            bool withSkills = true,
            bool withBuffs = true,
            bool withEquipItems = true,
            bool withNonEquipItems = true,
            bool withHotkeys = true,
            bool withQuests = true)
        {
            var reader = ExecuteReader("SELECT * FROM characters WHERE id=@id AND userId=@userId LIMIT 1",
                new SqliteParameter("@id", id),
                new SqliteParameter("@userId", userId));
            var result = new PlayerCharacterData();
            if (ReadCharacter(reader, out result))
            {
                this.InvokeInstanceDevExtMethods("ReadCharacter",
                    userId,
                    id,
                    withEquipWeapons,
                    withAttributes,
                    withSkills,
                    withBuffs,
                    withEquipItems,
                    withNonEquipItems,
                    withHotkeys,
                    withQuests);
                if (withEquipWeapons)
                    result.EquipWeapons = ReadCharacterEquipWeapons(id);
                if (withAttributes)
                    result.Attributes = ReadCharacterAttributes(id);
                if (withSkills)
                    result.Skills = ReadCharacterSkills(id);
                if (withBuffs)
                    result.Buffs = ReadCharacterBuffs(id);
                if (withEquipItems)
                    result.EquipItems = ReadCharacterEquipItems(id);
                if (withNonEquipItems)
                    result.NonEquipItems = ReadCharacterNonEquipItems(id);
                if (withHotkeys)
                    result.Hotkeys = ReadCharacterHotkeys(id);
                if (withQuests)
                    result.Quests = ReadCharacterQuests(id);
                return result;
            }
            return null;
        }

        public override List<PlayerCharacterData> ReadCharacters(string userId)
        {
            var result = new List<PlayerCharacterData>();
            var reader = ExecuteReader("SELECT id FROM characters WHERE userId=@userId ORDER BY updateAt DESC", new SqliteParameter("@userId", userId));
            while (reader.Read())
            {
                var characterId = reader.GetString("id");
                result.Add(ReadCharacter(userId, characterId, true, true, true, false, true, false, false, false));
            }
            return result;
        }

        public override void UpdateCharacter(IPlayerCharacterData character)
        {
            ExecuteNonQuery("BEGIN");
            ExecuteNonQuery("UPDATE characters SET " +
                "dataId=@dataId, " +
                "entityId=@entityId, " +
                "characterName=@characterName, " +
                "level=@level, " +
                "exp=@exp, " +
                "currentHp=@currentHp, " +
                "currentMp=@currentMp, " +
                "currentStamina=@currentStamina, " +
                "currentFood=@currentFood, " +
                "currentWater=@currentWater, " +
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
                "respawnPositionZ=@respawnPositionZ " +
                "WHERE id=@id",
                new SqliteParameter("@dataId", character.DataId),
                new SqliteParameter("@entityId", character.EntityId),
                new SqliteParameter("@characterName", character.CharacterName),
                new SqliteParameter("@level", character.Level),
                new SqliteParameter("@exp", character.Exp),
                new SqliteParameter("@currentHp", character.CurrentHp),
                new SqliteParameter("@currentMp", character.CurrentMp),
                new SqliteParameter("@currentStamina", character.CurrentStamina),
                new SqliteParameter("@currentFood", character.CurrentFood),
                new SqliteParameter("@currentWater", character.CurrentWater),
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
                new SqliteParameter("@id", character.Id));
            FillCharacterRelatesData(character);
            ExecuteNonQuery("END");
            this.InvokeInstanceDevExtMethods("UpdateCharacter", character);
        }

        public override void DeleteCharacter(string userId, string id)
        {
            var result = ExecuteScalar("SELECT COUNT(*) FROM characters WHERE id=@id AND userId=@userId",
                new SqliteParameter("@id", id),
                new SqliteParameter("@userId", userId));
            var count = result != null ? (long)result : 0;
            if (count > 0)
            {
                ExecuteNonQuery("BEGIN");
                ExecuteNonQuery("DELETE FROM characters WHERE id=@characterId", new SqliteParameter("@characterId", id));
                DeleteCharacterAttributes(id);
                DeleteCharacterBuffs(id);
                DeleteCharacterHotkeys(id);
                DeleteCharacterItems(id);
                DeleteCharacterQuests(id);
                DeleteCharacterSkills(id);
                ExecuteNonQuery("END");
                this.InvokeInstanceDevExtMethods("DeleteCharacter", userId, id);
            }
        }

        public override long FindCharacterName(string characterName)
        {
            var result = ExecuteScalar("SELECT COUNT(*) FROM characters WHERE characterName LIKE @characterName",
                new SqliteParameter("@characterName", characterName));
            return result != null ? (long)result : 0;
        }
    }
}
