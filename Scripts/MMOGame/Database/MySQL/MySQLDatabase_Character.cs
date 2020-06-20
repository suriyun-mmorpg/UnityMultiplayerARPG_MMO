using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private void FillCharacterAttributes(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterAttributes(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Attributes.Count; ++i)
                {
                    CreateCharacterAttribute(connection, transaction, i, characterData.Id, characterData.Attributes[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing attributes of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterBuffs(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterBuffs(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Buffs.Count; ++i)
                {
                    CreateCharacterBuff(connection, transaction, characterData.Id, characterData.Buffs[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing buffs of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterHotkeys(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterHotkeys(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Hotkeys.Count; ++i)
                {
                    CreateCharacterHotkey(connection, transaction, characterData.Id, characterData.Hotkeys[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing hotkeys of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterItems(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterItems(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.SelectableWeaponSets.Count; ++i)
                {
                    CreateCharacterEquipWeapons(connection, transaction, (byte)i, characterData.Id, characterData.SelectableWeaponSets[i]);
                }
                for (i = 0; i < characterData.EquipItems.Count; ++i)
                {
                    CreateCharacterEquipItem(connection, transaction, i, characterData.Id, characterData.EquipItems[i]);
                }
                for (i = 0; i < characterData.NonEquipItems.Count; ++i)
                {
                    CreateCharacterNonEquipItem(connection, transaction, i, characterData.Id, characterData.NonEquipItems[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing items of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterQuests(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterQuests(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Quests.Count; ++i)
                {
                    CreateCharacterQuest(connection, transaction, i, characterData.Id, characterData.Quests[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing quests of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterSkills(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterSkills(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Skills.Count; ++i)
                {
                    CreateCharacterSkill(connection, transaction, i, characterData.Id, characterData.Skills[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing skills of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterSkillUsages(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterSkillUsages(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.SkillUsages.Count; ++i)
                {
                    CreateCharacterSkillUsage(connection, transaction, characterData.Id, characterData.SkillUsages[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing skill usages of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterSummons(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterSummons(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Summons.Count; ++i)
                {
                    CreateCharacterSummon(connection, transaction, i, characterData.Id, characterData.Summons[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing skill usages of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        private void FillCharacterRelatesData(IPlayerCharacterData characterData)
        {
            FillCharacterAttributes(characterData);
            FillCharacterBuffs(characterData);
            FillCharacterHotkeys(characterData);
            FillCharacterItems(characterData);
            FillCharacterQuests(characterData);
            FillCharacterSkills(characterData);
            FillCharacterSkillUsages(characterData);
            FillCharacterSummons(characterData);
        }

        public override void CreateCharacter(string userId, IPlayerCharacterData characterData)
        {
            ExecuteNonQuery("INSERT INTO characters " +
                "(id, userId, dataId, entityId, factionId, characterName, level, exp, currentHp, currentMp, currentStamina, currentFood, currentWater, equipWeaponSet, statPoint, skillPoint, gold, currentMapName, currentPositionX, currentPositionY, currentPositionZ, respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ, mountDataId) VALUES " +
                "(@id, @userId, @dataId, @entityId, @factionId, @characterName, @level, @exp, @currentHp, @currentMp, @currentStamina, @currentFood, @currentWater, @equipWeaponSet, @statPoint, @skillPoint, @gold, @currentMapName, @currentPositionX, @currentPositionY, @currentPositionZ, @respawnMapName, @respawnPositionX, @respawnPositionY, @respawnPositionZ, @mountDataId)",
                new MySqlParameter("@id", characterData.Id),
                new MySqlParameter("@userId", userId),
                new MySqlParameter("@dataId", characterData.DataId),
                new MySqlParameter("@entityId", characterData.EntityId),
                new MySqlParameter("@factionId", characterData.FactionId),
                new MySqlParameter("@characterName", characterData.CharacterName),
                new MySqlParameter("@level", characterData.Level),
                new MySqlParameter("@exp", characterData.Exp),
                new MySqlParameter("@currentHp", characterData.CurrentHp),
                new MySqlParameter("@currentMp", characterData.CurrentMp),
                new MySqlParameter("@currentStamina", characterData.CurrentStamina),
                new MySqlParameter("@currentFood", characterData.CurrentFood),
                new MySqlParameter("@currentWater", characterData.CurrentWater),
                new MySqlParameter("@equipWeaponSet", characterData.EquipWeaponSet),
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
                new MySqlParameter("@mountDataId", characterData.MountDataId));
            FillCharacterRelatesData(characterData);
            this.InvokeInstanceDevExtMethods("CreateCharacter", userId, characterData);
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
                result.EntityId = reader.GetInt32("entityId");
                result.FactionId = reader.GetInt32("factionId");
                result.CharacterName = reader.GetString("characterName");
                result.Level = reader.GetInt16("level");
                result.Exp = reader.GetInt32("exp");
                result.CurrentHp = reader.GetInt32("currentHp");
                result.CurrentMp = reader.GetInt32("currentMp");
                result.CurrentStamina = reader.GetInt32("currentStamina");
                result.CurrentFood = reader.GetInt32("currentFood");
                result.CurrentWater = reader.GetInt32("currentWater");
                result.EquipWeaponSet = reader.GetByte("equipWeaponSet");
                result.StatPoint = reader.GetInt16("statPoint");
                result.SkillPoint = reader.GetInt16("skillPoint");
                result.Gold = reader.GetInt32("gold");
                result.PartyId = reader.GetInt32("partyId");
                result.GuildId = reader.GetInt32("guildId");
                result.GuildRole = reader.GetByte("guildRole");
                result.SharedGuildExp = reader.GetInt32("sharedGuildExp");
                result.CurrentMapName = reader.GetString("currentMapName");
                result.CurrentPosition = new Vector3(reader.GetFloat("currentPositionX"), reader.GetFloat("currentPositionY"), reader.GetFloat("currentPositionZ"));
                result.RespawnMapName = reader.GetString("respawnMapName");
                result.RespawnPosition = new Vector3(reader.GetFloat("respawnPositionX"), reader.GetFloat("respawnPositionY"), reader.GetFloat("respawnPositionZ"));
                result.MountDataId = reader.GetInt32("mountDataId");
                result.LastUpdate = (int)(reader.GetDateTime("updateAt").Ticks / System.TimeSpan.TicksPerMillisecond);
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
            bool withQuests = true)
        {
            MySQLRowsReader reader = ExecuteReader("SELECT * FROM characters WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", id));
            PlayerCharacterData result = new PlayerCharacterData();
            if (ReadCharacter(reader, out result))
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
            MySQLRowsReader reader = ExecuteReader("SELECT id FROM characters WHERE userId=@userId ORDER BY updateAt DESC", new MySqlParameter("@userId", userId));
            while (reader.Read())
            {
                string characterId = reader.GetString("id");
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
                "respawnMapName=@respawnMapName, " +
                "respawnPositionX=@respawnPositionX, " +
                "respawnPositionY=@respawnPositionY, " +
                "respawnPositionZ=@respawnPositionZ, " +
                "mountDataId=@mountDataId " +
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
                new MySqlParameter("@currentPositionX", character.CurrentPosition.x),
                new MySqlParameter("@currentPositionY", character.CurrentPosition.y),
                new MySqlParameter("@currentPositionZ", character.CurrentPosition.z),
                new MySqlParameter("@respawnMapName", character.RespawnMapName),
                new MySqlParameter("@respawnPositionX", character.RespawnPosition.x),
                new MySqlParameter("@respawnPositionY", character.RespawnPosition.y),
                new MySqlParameter("@respawnPositionZ", character.RespawnPosition.z),
                new MySqlParameter("@mountDataId", character.MountDataId),
                new MySqlParameter("@id", character.Id));
            FillCharacterRelatesData(character);
            this.InvokeInstanceDevExtMethods("UpdateCharacter", character);
        }

        public override void DeleteCharacter(string userId, string id)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM characters WHERE id=@id AND userId=@userId",
                new MySqlParameter("@id", id),
                new MySqlParameter("@userId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                MySqlConnection connection = NewConnection();
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    ExecuteNonQuery(connection, transaction, "DELETE FROM characters WHERE id=@characterId", new MySqlParameter("@characterId", id));
                    DeleteCharacterAttributes(connection, transaction, id);
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
                    Logging.LogError(ToString(), "Transaction, Error occurs while deleting character: " + id);
                    Logging.LogException(ToString(), ex);
                    transaction.Rollback();
                }
                transaction.Dispose();
                connection.Close();
                this.InvokeInstanceDevExtMethods("DeleteCharacter", userId, id);
            }
        }

        public override long FindCharacterName(string characterName)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM characters WHERE characterName LIKE @characterName",
                new MySqlParameter("@characterName", characterName));
            return result != null ? (long)result : 0;
        }

        public override List<SocialCharacterData> FindCharacters(string characterName)
        {
            List<SocialCharacterData> result = new List<SocialCharacterData>();
            MySQLRowsReader reader = ExecuteReader("SELECT id, dataId, characterName, level FROM characters WHERE characterName LIKE @characterName LIMIT 0, 20",
                new MySqlParameter("@characterName", "%" + characterName + "%"));
            SocialCharacterData socialCharacterData;
            while (reader.Read())
            {
                // Get some required data, other data will be set at server side
                socialCharacterData = new SocialCharacterData();
                socialCharacterData.id = reader.GetString("id");
                socialCharacterData.characterName = reader.GetString("characterName");
                socialCharacterData.dataId = reader.GetInt32("dataId");
                socialCharacterData.level = reader.GetInt16("level");
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
                new MySqlParameter("@characterId1", id1),
                new MySqlParameter("@characterId2", id2));
        }

        public override void DeleteFriend(string id1, string id2)
        {
            ExecuteNonQuery("DELETE FROM friend WHERE " +
                "characterId1 LIKE @characterId1 AND " +
                "characterId2 LIKE @characterId2",
                new MySqlParameter("@characterId1", id1),
                new MySqlParameter("@characterId2", id2));
        }

        public override List<SocialCharacterData> ReadFriends(string id1)
        {
            List<SocialCharacterData> result = new List<SocialCharacterData>();

            MySQLRowsReader reader = ExecuteReader("SELECT characterId2 FROM friend WHERE characterId1=@id1",
                new MySqlParameter("@id1", id1));
            string characterId;
            SocialCharacterData socialCharacterData;
            MySQLRowsReader reader2;
            while (reader.Read())
            {
                characterId = reader.GetString("characterId2");
                reader2 = ExecuteReader("SELECT id, dataId, characterName, level FROM characters WHERE BINARY id = @id",
                    new MySqlParameter("@id", characterId));
                while (reader2.Read())
                {
                    // Get some required data, other data will be set at server side
                    socialCharacterData = new SocialCharacterData();
                    socialCharacterData.id = reader2.GetString("id");
                    socialCharacterData.characterName = reader2.GetString("characterName");
                    socialCharacterData.dataId = reader2.GetInt32("dataId");
                    socialCharacterData.level = reader2.GetInt16("level");
                    result.Add(socialCharacterData);
                }
            }
            return result;
        }
    }
}
