using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace Insthync.MMOG
{
    public partial class MySQLDatabase
    {
        private async void FillCharacterRelatesData(IPlayerCharacterData characterData)
        {
            // Delete all character then add all of them
            var characterId = characterData.Id;
            await Task.WhenAll(ExecuteNonQuery("DELETE FROM characterinventory WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId)),
                ExecuteNonQuery("DELETE FROM characterattribute WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId)),
                ExecuteNonQuery("DELETE FROM characterskill WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId)),
                ExecuteNonQuery("DELETE FROM characterbuff WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId)),
                ExecuteNonQuery("DELETE FROM characterhotkey WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId)),
                ExecuteNonQuery("DELETE FROM characterquest WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId)));

            var tasks = new List<Task>();
            tasks.Add(CreateCharacterEquipWeapons(characterId, characterData.EquipWeapons));
            foreach (var equipItem in characterData.EquipItems)
            {
                tasks.Add(CreateCharacterEquipItem(characterId, equipItem));
            }
            foreach (var nonEquipItem in characterData.NonEquipItems)
            {
                tasks.Add(CreateCharacterEquipItem(characterId, nonEquipItem));
            }
            foreach (var attribute in characterData.Attributes)
            {
                tasks.Add(CreateCharacterAttribute(characterId, attribute));
            }
            foreach (var skill in characterData.Skills)
            {
                tasks.Add(CreateCharacterSkill(characterId, skill));
            }
            foreach (var buff in characterData.Buffs)
            {
                tasks.Add(CreateCharacterBuff(characterId, buff));
            }
            foreach (var hotkey in characterData.Hotkeys)
            {
                tasks.Add(CreateCharacterHotkey(characterId, hotkey));
            }
            foreach (var quest in characterData.Quests)
            {
                tasks.Add(CreateCharacterQuest(characterId, quest));
            }
            await Task.WhenAll(tasks);
        }

        public override async Task CreateCharacter(string userId, PlayerCharacterData characterData)
        {
            await ExecuteNonQuery("INSERT INTO characters " +
                "(id, userId, databaseId, characterName, level, exp, currentHp, currentMp, currentStamina, currentFood, currentWater, statPoint, skillPoint, gold, currentMapName, currentPositionX, currentPositionY, currentPositionZ, respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ) VALUES " +
                "(@id, @userId, @databaseId, @characterName, @level, @exp, @currentHp, @currentMp, @currentStamina, @currentFood, @currentWater, @statPoint, @skillPoint, @gold, @currentMapName, @currentPositionX, @currentPositionY, @currentPositionZ, @respawnMapName, @respawnPositionX, @respawnPositionY, @respawnPositionZ)",
                new MySqlParameter("@id", characterData.Id),
                new MySqlParameter("@userId", userId),
                new MySqlParameter("@databaseId", characterData.DatabaseId),
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
            FillCharacterRelatesData(characterData);
        }

        private bool ReadCharacter(MySQLRowsReader reader, out PlayerCharacterData result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new PlayerCharacterData();
                result.Id = reader.GetString("id");
                result.DatabaseId = reader.GetString("databaseId");
                result.CharacterName = reader.GetString("characterName");
                result.Level = reader.GetInt32("level");
                result.Exp = reader.GetInt32("exp");
                result.CurrentHp = reader.GetInt32("currentHp");
                result.CurrentMp = reader.GetInt32("currentMp");
                result.CurrentStamina = reader.GetInt32("currentStamina");
                result.CurrentFood = reader.GetInt32("currentFood");
                result.CurrentWater = reader.GetInt32("currentWater");
                result.StatPoint = reader.GetInt32("statPoint");
                result.SkillPoint = reader.GetInt32("skillPoint");
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
            var reader = await ExecuteReader("SELECT * FROM characters WHERE id=@id LIMIT 1", new MySqlParameter("@id", id));
            var result = new PlayerCharacterData();
            if (ReadCharacter(reader, out result))
            {
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
                result.Add(await ReadCharacter(characterId, true, true, true, false, true, false, false, false));
            }
            return result;
        }

        public override async Task UpdateCharacter(PlayerCharacterData characterData)
        {
            await ExecuteNonQuery("UPDATE characters SET " +
                "databaseId=@databaseId, " +
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
                new MySqlParameter("@databaseId", characterData.DatabaseId),
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
            FillCharacterRelatesData(characterData);
        }

        public override async Task DeleteCharacter(string userId, string id)
        {
            var result = await ExecuteScalar("SELECT COUNT(*) FROM characters WHERE id=@id AND userId=@userId",
                new MySqlParameter("@id", id),
                new MySqlParameter("@userId", userId));
            var count = result != null ? (long)result : 0;
            if (count > 0)
            {
                await Task.WhenAll(ExecuteNonQuery("DELETE FROM characters WHERE id=@characterId", new MySqlParameter("@characterId", id)),
                    ExecuteNonQuery("DELETE FROM characterInventory WHERE characterId=@characterId", new MySqlParameter("@characterId", id)),
                    ExecuteNonQuery("DELETE FROM characterAttribute WHERE characterId=@characterId", new MySqlParameter("@characterId", id)),
                    ExecuteNonQuery("DELETE FROM characterSkill WHERE characterId=@characterId", new MySqlParameter("@characterId", id)),
                    ExecuteNonQuery("DELETE FROM characterBuff WHERE characterId=@characterId", new MySqlParameter("@characterId", id)),
                    ExecuteNonQuery("DELETE FROM characterHotkey WHERE characterId=@characterId", new MySqlParameter("@characterId", id)),
                    ExecuteNonQuery("DELETE FROM characterQuest WHERE characterId=@characterId", new MySqlParameter("@characterId", id)));
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
