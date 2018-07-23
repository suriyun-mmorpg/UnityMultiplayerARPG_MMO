using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private async Task FillCharacterRelatesData(MySqlConnection connection, IPlayerCharacterData characterData)
        {
            // Delete all character then add all of them
            var characterId = characterData.Id;
            await DeleteCharacterAttributes(connection, characterId);
            await DeleteCharacterBuffs(connection, characterId);
            await DeleteCharacterHotkeys(connection, characterId);
            await DeleteCharacterItems(connection, characterId);
            await DeleteCharacterQuests(connection, characterId);
            await DeleteCharacterSkills(connection, characterId);

            await CreateCharacterEquipWeapons(connection, characterId, characterData.EquipWeapons);
            var i = 0;
            foreach (var equipItem in characterData.EquipItems)
            {
                await CreateCharacterEquipItem(connection, i++, characterId, equipItem);
            }
            i = 0;
            foreach (var nonEquipItem in characterData.NonEquipItems)
            {
                await CreateCharacterNonEquipItem(connection, i++, characterId, nonEquipItem);
            }
            i = 0;
            foreach (var attribute in characterData.Attributes)
            {
                await CreateCharacterAttribute(connection, i++, characterId, attribute);
            }
            i = 0;
            foreach (var skill in characterData.Skills)
            {
                await CreateCharacterSkill(connection, i++, characterId, skill);
            }
            i = 0;
            foreach (var quest in characterData.Quests)
            {
                await CreateCharacterQuest(connection, i++, characterId, quest);
            }
            foreach (var buff in characterData.Buffs)
            {
                await CreateCharacterBuff(connection, characterId, buff);
            }
            foreach (var hotkey in characterData.Hotkeys)
            {
                await CreateCharacterHotkey(connection, characterId, hotkey);
            }
        }

        public override async Task CreateCharacter(string userId, PlayerCharacterData characterData)
        {
            var connection = NewConnection();
            connection.Open();
            await ExecuteNonQuery(connection, "START TRANSACTION");
            await ExecuteNonQuery(connection, "INSERT INTO characters " +
                "(id, userId, dataId, characterName, level, exp, currentHp, currentMp, currentStamina, currentFood, currentWater, statPoint, skillPoint, gold, currentMapName, currentPositionX, currentPositionY, currentPositionZ, respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ) VALUES " +
                "(@id, @userId, @dataId, @characterName, @level, @exp, @currentHp, @currentMp, @currentStamina, @currentFood, @currentWater, @statPoint, @skillPoint, @gold, @currentMapName, @currentPositionX, @currentPositionY, @currentPositionZ, @respawnMapName, @respawnPositionX, @respawnPositionY, @respawnPositionZ)",
                new MySqlParameter("@id", characterData.Id),
                new MySqlParameter("@userId", userId),
                new MySqlParameter("@dataId", characterData.DataId),
                new MySqlParameter("@characterName", characterData.CharacterName),
                new MySqlParameter("@level", characterData.Level),
                new MySqlParameter("@exp", characterData.Exp),
                new MySqlParameter("@currentHp", characterData.CurrentHp),
                new MySqlParameter("@currentMp", characterData.CurrentMp),
                new MySqlParameter("@currentStamina", characterData.CurrentStamina),
                new MySqlParameter("@currentFood", characterData.CurrentFood),
                new MySqlParameter("@currentWater", characterData.CurrentWater),
                new MySqlParameter("@statPoint", characterData.StatPoint),
                new MySqlParameter("@skillPoint", characterData.SkillPoint),
                new MySqlParameter("@gold", characterData.Gold),
                new MySqlParameter("@currentMapName", characterData.CurrentMapName),
                new MySqlParameter("@currentPositionX", characterData.CurrentPosition.x),
                new MySqlParameter("@currentPositionY", characterData.CurrentPosition.y),
                new MySqlParameter("@currentPositionZ", characterData.CurrentPosition.z),
                new MySqlParameter("@respawnMapName", characterData.RespawnMapName),
                new MySqlParameter("@respawnPositionX", characterData.RespawnPosition.x),
                new MySqlParameter("@respawnPositionY", characterData.RespawnPosition.y),
                new MySqlParameter("@respawnPositionZ", characterData.RespawnPosition.z));
            await FillCharacterRelatesData(connection, characterData);
            await ExecuteNonQuery(connection, "COMMIT");
            connection.Close();
            this.InvokeClassAddOnMethods("CreateCharacter", userId, characterData);
        }

        private bool ReadCharacter(MySQLRowsReader reader, out PlayerCharacterData result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new PlayerCharacterData();
                result.Id = reader.GetString("id");
                result.DataId = reader.GetInt32("dataId");
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

        public override async Task<PlayerCharacterData> ReadCharacter(
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
            var reader = await ExecuteReader("SELECT * FROM characters WHERE id=@id AND userId=@userId LIMIT 1",
                new MySqlParameter("@id", id),
                new MySqlParameter("@userId", userId));
            var result = new PlayerCharacterData();
            if (ReadCharacter(reader, out result))
            {
                this.InvokeClassAddOnMethods("ReadCharacter",
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
                    result.EquipWeapons = await ReadCharacterEquipWeapons(id);
                if (withAttributes)
                    result.Attributes = await ReadCharacterAttributes(id);
                if (withSkills)
                    result.Skills = await ReadCharacterSkills(id);
                if (withBuffs)
                    result.Buffs = await ReadCharacterBuffs(id);
                if (withEquipItems)
                    result.EquipItems = await ReadCharacterEquipItems(id);
                if (withNonEquipItems)
                    result.NonEquipItems = await ReadCharacterNonEquipItems(id);
                if (withHotkeys)
                    result.Hotkeys = await ReadCharacterHotkeys(id);
                if (withQuests)
                    result.Quests = await ReadCharacterQuests(id);
                return result;
            }
            return null;
        }

        public override async Task<List<PlayerCharacterData>> ReadCharacters(string userId)
        {
            var result = new List<PlayerCharacterData>();
            var reader = await ExecuteReader("SELECT id FROM characters WHERE userId=@userId ORDER BY updateAt DESC", new MySqlParameter("@userId", userId));
            while (reader.Read())
            {
                var characterId = reader.GetString("id");
                result.Add(await ReadCharacter(userId, characterId, true, true, true, false, true, false, false, false));
            }
            return result;
        }

        public override async Task UpdateCharacter(IPlayerCharacterData characterData)
        {
            var connection = NewConnection();
            connection.Open();
            await ExecuteNonQuery(connection, "START TRANSACTION");
            await ExecuteNonQuery(connection, "UPDATE characters SET " +
                "dataId=@dataId, " +
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
                new MySqlParameter("@dataId", characterData.DataId),
                new MySqlParameter("@characterName", characterData.CharacterName),
                new MySqlParameter("@level", characterData.Level),
                new MySqlParameter("@exp", characterData.Exp),
                new MySqlParameter("@currentHp", characterData.CurrentHp),
                new MySqlParameter("@currentMp", characterData.CurrentMp),
                new MySqlParameter("@currentStamina", characterData.CurrentStamina),
                new MySqlParameter("@currentFood", characterData.CurrentFood),
                new MySqlParameter("@currentWater", characterData.CurrentWater),
                new MySqlParameter("@statPoint", characterData.StatPoint),
                new MySqlParameter("@skillPoint", characterData.SkillPoint),
                new MySqlParameter("@gold", characterData.Gold),
                new MySqlParameter("@currentMapName", characterData.CurrentMapName),
                new MySqlParameter("@currentPositionX", characterData.CurrentPosition.x),
                new MySqlParameter("@currentPositionY", characterData.CurrentPosition.y),
                new MySqlParameter("@currentPositionZ", characterData.CurrentPosition.z),
                new MySqlParameter("@respawnMapName", characterData.RespawnMapName),
                new MySqlParameter("@respawnPositionX", characterData.RespawnPosition.x),
                new MySqlParameter("@respawnPositionY", characterData.RespawnPosition.y),
                new MySqlParameter("@respawnPositionZ", characterData.RespawnPosition.z),
                new MySqlParameter("@id", characterData.Id));
            await FillCharacterRelatesData(connection, characterData);
            await ExecuteNonQuery(connection, "COMMIT");
            connection.Close();
            this.InvokeClassAddOnMethods("UpdateCharacter", characterData);
        }

        public override async Task DeleteCharacter(string userId, string id)
        {
            var result = await ExecuteScalar("SELECT COUNT(*) FROM characters WHERE id=@id AND userId=@userId",
                new MySqlParameter("@id", id),
                new MySqlParameter("@userId", userId));
            var count = result != null ? (long)result : 0;
            if (count > 0)
            {
                var connection = NewConnection();
                connection.Open();
                await ExecuteNonQuery(connection, "START TRANSACTION");
                await ExecuteNonQuery(connection, "DELETE FROM characters WHERE id=@characterId", new MySqlParameter("@characterId", id));
                await DeleteCharacterAttributes(connection, id);
                await DeleteCharacterBuffs(connection, id);
                await DeleteCharacterHotkeys(connection, id);
                await DeleteCharacterItems(connection, id);
                await DeleteCharacterQuests(connection, id);
                await DeleteCharacterSkills(connection, id);
                await ExecuteNonQuery(connection, "COMMIT");
                connection.Close();
                this.InvokeClassAddOnMethods("DeleteCharacter", userId, id);
            }
        }

        public override async Task<long> FindCharacterName(string characterName)
        {
            var result = await ExecuteScalar("SELECT COUNT(*) FROM characters WHERE characterName LIKE @characterName",
                new MySqlParameter("@characterName", characterName));
            return result != null ? (long)result : 0;
        }
    }
}
