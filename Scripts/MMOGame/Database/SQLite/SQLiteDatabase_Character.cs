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
            string characterId = characterData.Id;
            DeleteCharacterAttributes(characterId);
            DeleteCharacterBuffs(characterId);
            DeleteCharacterHotkeys(characterId);
            DeleteCharacterItems(characterId);
            DeleteCharacterQuests(characterId);
            DeleteCharacterSkills(characterId);
            DeleteCharacterSkillUsages(characterId);
            DeleteCharacterSummons(characterId);

            CreateCharacterEquipWeapons(characterId, characterData.EquipWeapons);
            int i = 0;
            foreach (CharacterItem equipItem in characterData.EquipItems)
            {
                CreateCharacterEquipItem(i++, characterId, equipItem);
            }
            i = 0;
            foreach (CharacterItem nonEquipItem in characterData.NonEquipItems)
            {
                CreateCharacterNonEquipItem(i++, characterId, nonEquipItem);
            }
            i = 0;
            foreach (CharacterAttribute attribute in characterData.Attributes)
            {
                CreateCharacterAttribute(i++, characterId, attribute);
            }
            i = 0;
            foreach (CharacterSkill skill in characterData.Skills)
            {
                CreateCharacterSkill(i++, characterId, skill);
            }
            foreach (CharacterSkillUsage skillUsage in characterData.SkillUsages)
            {
                CreateCharacterSkillUsage(characterId, skillUsage);
            }
            i = 0;
            foreach (CharacterSummon summon in characterData.Summons)
            {
                CreateCharacterSummon(i++, characterId, summon);
            }
            i = 0;
            foreach (CharacterQuest quest in characterData.Quests)
            {
                CreateCharacterQuest(i++, characterId, quest);
            }
            foreach (CharacterBuff buff in characterData.Buffs)
            {
                CreateCharacterBuff(characterId, buff);
            }
            foreach (CharacterHotkey hotkey in characterData.Hotkeys)
            {
                CreateCharacterHotkey(characterId, hotkey);
            }
        }

        public override void CreateCharacter(string userId, IPlayerCharacterData characterData)
        {
            BeginTransaction();
            ExecuteNonQuery("INSERT INTO characters " +
                "(id, userId, dataId, entityId, factionId, characterName, level, exp, currentHp, currentMp, currentStamina, currentFood, currentWater, statPoint, skillPoint, gold, currentMapName, currentPositionX, currentPositionY, currentPositionZ, respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ) VALUES " +
                "(@id, @userId, @dataId, @entityId, @factionId, @characterName, @level, @exp, @currentHp, @currentMp, @currentStamina, @currentFood, @currentWater, @statPoint, @skillPoint, @gold, @currentMapName, @currentPositionX, @currentPositionY, @currentPositionZ, @respawnMapName, @respawnPositionX, @respawnPositionY, @respawnPositionZ)",
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
            this.InvokeInstanceDevExtMethods("CreateCharacter", userId, characterData);
            EndTransaction();
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
                result.FactionId = reader.GetInt32("factionId");
                result.CharacterName = reader.GetString("characterName");
                result.Level = (short)reader.GetInt32("level");
                result.Exp = reader.GetInt32("exp");
                result.CurrentHp = reader.GetInt32("currentHp");
                result.CurrentMp = reader.GetInt32("currentMp");
                result.CurrentStamina = reader.GetInt32("currentStamina");
                result.CurrentFood = reader.GetInt32("currentFood");
                result.CurrentWater = reader.GetInt32("currentWater");
                result.StatPoint = (short)reader.GetInt32("statPoint");
                result.SkillPoint = (short)reader.GetInt32("skillPoint");
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
            bool withSkillUsages = true,
            bool withBuffs = true,
            bool withEquipItems = true,
            bool withNonEquipItems = true,
            bool withSummons = true,
            bool withHotkeys = true,
            bool withQuests = true)
        {
            SQLiteRowsReader reader = ExecuteReader("SELECT * FROM characters WHERE id=@id AND userId=@userId LIMIT 1",
                new SqliteParameter("@id", id),
                new SqliteParameter("@userId", userId));
            PlayerCharacterData result = new PlayerCharacterData();
            if (ReadCharacter(reader, out result))
            {
                if (withEquipWeapons)
                    result.EquipWeapons = ReadCharacterEquipWeapons(id);
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
                // Return result
                return result;
            }
            return null;
        }

        public override List<PlayerCharacterData> ReadCharacters(string userId)
        {
            List<PlayerCharacterData> result = new List<PlayerCharacterData>();
            SQLiteRowsReader reader = ExecuteReader("SELECT id FROM characters WHERE userId=@userId ORDER BY updateAt DESC", new SqliteParameter("@userId", userId));
            while (reader.Read())
            {
                string characterId = reader.GetString("id");
                result.Add(ReadCharacter(userId, characterId, true, true, true, false, false, true, false, false, false, false));
            }
            return result;
        }

        public override void UpdateCharacter(IPlayerCharacterData character)
        {
            BeginTransaction();
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
                new SqliteParameter("@factionId", character.FactionId),
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
            this.InvokeInstanceDevExtMethods("UpdateCharacter", character);
            EndTransaction();
        }

        public override void DeleteCharacter(string userId, string id)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM characters WHERE id=@id AND userId=@userId",
                new SqliteParameter("@id", id),
                new SqliteParameter("@userId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                BeginTransaction();
                ExecuteNonQuery("DELETE FROM characters WHERE id=@characterId", new SqliteParameter("@characterId", id));
                DeleteCharacterAttributes(id);
                DeleteCharacterBuffs(id);
                DeleteCharacterHotkeys(id);
                DeleteCharacterItems(id);
                DeleteCharacterQuests(id);
                DeleteCharacterSkills(id);
                DeleteCharacterSkillUsages(id);
                DeleteCharacterSummons(id);
                this.InvokeInstanceDevExtMethods("DeleteCharacter", userId, id);
                EndTransaction();
            }
        }

        public override long FindCharacterName(string characterName)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM characters WHERE characterName LIKE @characterName",
                new SqliteParameter("@characterName", characterName));
            return result != null ? (long)result : 0;
        }

        public override List<SocialCharacterData> FindCharacters(string characterName)
        {
            List<SocialCharacterData> result = new List<SocialCharacterData>();
            SQLiteRowsReader reader = ExecuteReader("SELECT id, dataId, characterName, level FROM characters WHERE characterName LIKE @characterName",
                new SqliteParameter("@characterName", "%" + characterName + "%"));
            SocialCharacterData socialCharacterData;
            while (reader.Read())
            {
                // Get some required data, other data will be set at server side
                socialCharacterData = new SocialCharacterData();
                socialCharacterData.id = reader.GetString("id");
                socialCharacterData.characterName = reader.GetString("characterName");
                socialCharacterData.dataId = reader.GetInt32("dataId");
                socialCharacterData.level = (short)reader.GetInt32("level");
                result.Add(socialCharacterData);
            }
            return result;
        }

        public override void CreateFriend(string id1, string id2)
        {
            DeleteFriend(id1, id2);
            ExecuteNonQuery("INSERT INTO friend " +
                "(characterId1, characterId2) VALUES " +
                "(@characterId1, @characterId2)",
                new SqliteParameter("@characterId1", id1),
                new SqliteParameter("@characterId2", id2));
        }

        public override void DeleteFriend(string id1, string id2)
        {
            ExecuteNonQuery("DELETE FROM friend WHERE " +
                "characterId1 LIKE @characterId1 AND " +
                "characterId2 LIKE @characterId2",
                new SqliteParameter("@characterId1", id1),
                new SqliteParameter("@characterId2", id2));
        }

        public override List<SocialCharacterData> ReadFriends(string id1)
        {
            List<SocialCharacterData> result = new List<SocialCharacterData>();

            SQLiteRowsReader reader = ExecuteReader("SELECT characterId2 FROM friend WHERE characterId1=@id1",
                new SqliteParameter("@id1", id1));
            string characterId;
            SocialCharacterData socialCharacterData;
            SQLiteRowsReader reader2;
            while (reader.Read())
            {
                characterId = reader.GetString("characterId2");
                reader2 = ExecuteReader("SELECT id, dataId, characterName, level FROM characters WHERE id LIKE @id",
                    new SqliteParameter("@id", characterId));
                while (reader2.Read())
                {
                    // Get some required data, other data will be set at server side
                    socialCharacterData = new SocialCharacterData();
                    socialCharacterData.id = reader2.GetString("id");
                    socialCharacterData.characterName = reader2.GetString("characterName");
                    socialCharacterData.dataId = reader2.GetInt32("dataId");
                    socialCharacterData.level = (short)reader2.GetInt32("level");
                    result.Add(socialCharacterData);
                }
            }
            return result;
        }
    }
}
