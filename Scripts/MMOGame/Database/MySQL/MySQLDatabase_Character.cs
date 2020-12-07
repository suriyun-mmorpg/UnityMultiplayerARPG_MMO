#if UNITY_STANDALONE && !CLIENT_BUILD
using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;
using MySqlConnector;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private async UniTask FillCharacterAttributes(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            MySqlTransaction transaction = await connection.BeginTransactionAsync();
            try
            {
                await DeleteCharacterAttributes(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Attributes.Count; ++i)
                {
                    await CreateCharacterAttribute(connection, transaction, i, characterData.Id, characterData.Attributes[i]);
                }
                await transaction.CommitAsync();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing attributes of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                await transaction.RollbackAsync();
            }
            await transaction.DisposeAsync();
            await connection.CloseAsync();
        }

        private async UniTask FillCharacterBuffs(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            MySqlTransaction transaction = await connection.BeginTransactionAsync();
            try
            {
                await DeleteCharacterBuffs(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Buffs.Count; ++i)
                {
                    await CreateCharacterBuff(connection, transaction, characterData.Id, characterData.Buffs[i]);
                }
                await transaction.CommitAsync();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing buffs of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                await transaction.RollbackAsync();
            }
            await transaction.DisposeAsync();
            await connection.CloseAsync();
        }

        private async UniTask FillCharacterHotkeys(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            MySqlTransaction transaction = await connection.BeginTransactionAsync();
            try
            {
                await DeleteCharacterHotkeys(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Hotkeys.Count; ++i)
                {
                    await CreateCharacterHotkey(connection, transaction, characterData.Id, characterData.Hotkeys[i]);
                }
                await transaction.CommitAsync();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing hotkeys of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                await transaction.RollbackAsync();
            }
            await transaction.DisposeAsync();
            await connection.CloseAsync();
        }

        private async UniTask FillCharacterItems(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            MySqlTransaction transaction = await connection.BeginTransactionAsync();
            try
            {
                await DeleteCharacterItems(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.SelectableWeaponSets.Count; ++i)
                {
                    await CreateCharacterEquipWeapons(connection, transaction, (byte)i, characterData.Id, characterData.SelectableWeaponSets[i]);
                }
                for (i = 0; i < characterData.EquipItems.Count; ++i)
                {
                    await CreateCharacterEquipItem(connection, transaction, i, characterData.Id, characterData.EquipItems[i]);
                }
                for (i = 0; i < characterData.NonEquipItems.Count; ++i)
                {
                    await CreateCharacterNonEquipItem(connection, transaction, i, characterData.Id, characterData.NonEquipItems[i]);
                }
                await transaction.CommitAsync();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing items of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                await transaction.RollbackAsync();
            }
            await transaction.DisposeAsync();
            await connection.CloseAsync();
        }

        private async UniTask FillCharacterQuests(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            MySqlTransaction transaction = await connection.BeginTransactionAsync();
            try
            {
                await DeleteCharacterQuests(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Quests.Count; ++i)
                {
                    await CreateCharacterQuest(connection, transaction, i, characterData.Id, characterData.Quests[i]);
                }
                await transaction.CommitAsync();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing quests of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                await transaction.RollbackAsync();
            }
            await transaction.DisposeAsync();
            await connection.CloseAsync();
        }

        private async UniTask FillCharacterCurrencies(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            MySqlTransaction transaction = await connection.BeginTransactionAsync();
            try
            {
                await DeleteCharacterCurrencies(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Currencies.Count; ++i)
                {
                    await CreateCharacterCurrency(connection, transaction, i, characterData.Id, characterData.Currencies[i]);
                }
                await transaction.CommitAsync();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing currencies of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                await transaction.RollbackAsync();
            }
            await transaction.DisposeAsync();
            await connection.CloseAsync();
        }

        private async UniTask FillCharacterSkills(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            MySqlTransaction transaction = await connection.BeginTransactionAsync();
            try
            {
                await DeleteCharacterSkills(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Skills.Count; ++i)
                {
                    await CreateCharacterSkill(connection, transaction, i, characterData.Id, characterData.Skills[i]);
                }
                await transaction.CommitAsync();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing skills of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                await transaction.RollbackAsync();
            }
            await transaction.DisposeAsync();
            await connection.CloseAsync();
        }

        private async UniTask FillCharacterSkillUsages(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            MySqlTransaction transaction = await connection.BeginTransactionAsync();
            try
            {
                await DeleteCharacterSkillUsages(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.SkillUsages.Count; ++i)
                {
                    await CreateCharacterSkillUsage(connection, transaction, characterData.Id, characterData.SkillUsages[i]);
                }
                await transaction.CommitAsync();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing skill usages of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                await transaction.RollbackAsync();
            }
            await transaction.DisposeAsync();
            await connection.CloseAsync();
        }

        private async UniTask FillCharacterSummons(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            await OpenConnection(connection);
            MySqlTransaction transaction = await connection.BeginTransactionAsync();
            try
            {
                await DeleteCharacterSummons(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Summons.Count; ++i)
                {
                    await CreateCharacterSummon(connection, transaction, i, characterData.Id, characterData.Summons[i]);
                }
                await transaction.CommitAsync();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing skill usages of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                await transaction.RollbackAsync();
            }
            await transaction.DisposeAsync();
            await connection.CloseAsync();
        }

        private async UniTask FillCharacterRelatesData(IPlayerCharacterData characterData)
        {
            await UniTask.WhenAll(FillCharacterAttributes(characterData),
                FillCharacterCurrencies(characterData),
                FillCharacterBuffs(characterData),
                FillCharacterHotkeys(characterData),
                FillCharacterItems(characterData),
                FillCharacterQuests(characterData),
                FillCharacterSkills(characterData),
                FillCharacterSkillUsages(characterData),
                FillCharacterSummons(characterData));
        }

        public override async UniTask CreateCharacter(string userId, IPlayerCharacterData characterData)
        {
            await ExecuteNonQuery("INSERT INTO characters " +
                "(id, userId, dataId, entityId, factionId, characterName, level, exp, currentHp, currentMp, currentStamina, currentFood, currentWater, equipWeaponSet, statPoint, skillPoint, gold, currentMapName, currentPositionX, currentPositionY, currentPositionZ, currentRotationX, currentRotationY, currentRotationZ, respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ, mountDataId) VALUES " +
                "(@id, @userId, @dataId, @entityId, @factionId, @characterName, @level, @exp, @currentHp, @currentMp, @currentStamina, @currentFood, @currentWater, @equipWeaponSet, @statPoint, @skillPoint, @gold, @currentMapName, @currentPositionX, @currentPositionY, @currentPositionZ, @currentRotationX, @currentRotationY, @currentRotationZ, @respawnMapName, @respawnPositionX, @respawnPositionY, @respawnPositionZ, @mountDataId)",
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
                new MySqlParameter("@currentRotationX", characterData.CurrentRotation.x),
                new MySqlParameter("@currentRotationY", characterData.CurrentRotation.y),
                new MySqlParameter("@currentRotationZ", characterData.CurrentRotation.z),
                new MySqlParameter("@respawnMapName", characterData.RespawnMapName),
                new MySqlParameter("@respawnPositionX", characterData.RespawnPosition.x),
                new MySqlParameter("@respawnPositionY", characterData.RespawnPosition.y),
                new MySqlParameter("@respawnPositionZ", characterData.RespawnPosition.z),
                new MySqlParameter("@mountDataId", characterData.MountDataId));
            await FillCharacterRelatesData(characterData);
            this.InvokeInstanceDevExtMethods("CreateCharacter", userId, characterData);
        }

        private bool ReadCharacter(MySqlDataReader reader, out PlayerCharacterData result)
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
                result.CurrentRotation = new Vector3(
                    reader.GetFloat(24),
                    reader.GetFloat(25),
                    reader.GetFloat(26));
                result.RespawnMapName = reader.GetString(27);
                result.RespawnPosition = new Vector3(
                    reader.GetFloat(28),
                    reader.GetFloat(29),
                    reader.GetFloat(30));
                result.MountDataId = reader.GetInt32(31);
                result.LastUpdate = (int)(reader.GetDateTime(32).Ticks / System.TimeSpan.TicksPerMillisecond);
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
            bool withQuests = true,
            bool withCurrencies = true)
        {
            PlayerCharacterData result = null;
            await ExecuteReader((reader) =>
            {
                ReadCharacter(reader, out result);
            }, "SELECT " +
                "id, dataId, entityId, factionId, characterName, level, exp, " +
                "currentHp, currentMp, currentStamina, currentFood, currentWater, " +
                "equipWeaponSet, statPoint, skillPoint, gold, partyId, guildId, guildRole, sharedGuildExp, " +
                "currentMapName, currentPositionX, currentPositionY, currentPositionZ, currentRotationX, currentRotationY, currentRotationZ," +
                "respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ," +
                "mountDataId, updateAt FROM characters WHERE id=@id LIMIT 1",
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
                List<UniTask> tasks = new List<UniTask>();
                if (withEquipWeapons)
                    tasks.Add(ReadCharacterEquipWeapons(id, selectableWeaponSets));
                if (withAttributes)
                    tasks.Add(ReadCharacterAttributes(id, attributes));
                if (withSkills)
                    tasks.Add(ReadCharacterSkills(id, skills));
                if (withSkillUsages)
                    tasks.Add(ReadCharacterSkillUsages(id, skillUsages));
                if (withBuffs)
                    tasks.Add(ReadCharacterBuffs(id, buffs));
                if (withEquipItems)
                    tasks.Add(ReadCharacterEquipItems(id, equipItems));
                if (withNonEquipItems)
                    tasks.Add(ReadCharacterNonEquipItems(id, nonEquipItems));
                if (withSummons)
                    tasks.Add(ReadCharacterSummons(id, summons));
                if (withHotkeys)
                    tasks.Add(ReadCharacterHotkeys(id, hotkeys));
                if (withQuests)
                    tasks.Add(ReadCharacterQuests(id, quests));
                if (withCurrencies)
                    tasks.Add(ReadCharacterCurrencies(id, currencies));
                await UniTask.WhenAll(tasks);
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

        public override async UniTask<List<PlayerCharacterData>> ReadCharacters(string userId)
        {
            List<PlayerCharacterData> result = new List<PlayerCharacterData>();
            List<string> characterIds = new List<string>();
            await ExecuteReader((reader) =>
            {
                while (reader.Read())
                {
                    characterIds.Add(reader.GetString(0));
                }
            }, "SELECT id FROM characters WHERE userId=@userId ORDER BY updateAt DESC", new MySqlParameter("@userId", userId));
            foreach (string characterId in characterIds)
            {
                result.Add(await ReadCharacter(characterId, true, true, true, false, false, true, false, false, false, false));
            }
            return result;
        }

        public override async UniTask UpdateCharacter(IPlayerCharacterData character)
        {
            await ExecuteNonQuery("UPDATE characters SET " +
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
                new MySqlParameter("@currentRotationX", character.CurrentRotation.x),
                new MySqlParameter("@currentRotationY", character.CurrentRotation.y),
                new MySqlParameter("@currentRotationZ", character.CurrentRotation.z),
                new MySqlParameter("@respawnMapName", character.RespawnMapName),
                new MySqlParameter("@respawnPositionX", character.RespawnPosition.x),
                new MySqlParameter("@respawnPositionY", character.RespawnPosition.y),
                new MySqlParameter("@respawnPositionZ", character.RespawnPosition.z),
                new MySqlParameter("@mountDataId", character.MountDataId),
                new MySqlParameter("@id", character.Id));
            await FillCharacterRelatesData(character);
            this.InvokeInstanceDevExtMethods("UpdateCharacter", character);
        }

        public override async UniTask DeleteCharacter(string userId, string id)
        {
            object result = await ExecuteScalar("SELECT COUNT(*) FROM characters WHERE id=@id AND userId=@userId",
                new MySqlParameter("@id", id),
                new MySqlParameter("@userId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                MySqlConnection connection = NewConnection();
                await OpenConnection(connection);
                MySqlTransaction transaction = await connection.BeginTransactionAsync();
                try
                {
                    await ExecuteNonQuery(connection, transaction, "DELETE FROM characters WHERE id=@characterId", new MySqlParameter("@characterId", id));
                    await DeleteCharacterAttributes(connection, transaction, id);
                    await DeleteCharacterCurrencies(connection, transaction, id);
                    await DeleteCharacterBuffs(connection, transaction, id);
                    await DeleteCharacterHotkeys(connection, transaction, id);
                    await DeleteCharacterItems(connection, transaction, id);
                    await DeleteCharacterQuests(connection, transaction, id);
                    await DeleteCharacterSkills(connection, transaction, id);
                    await DeleteCharacterSkillUsages(connection, transaction, id);
                    await DeleteCharacterSummons(connection, transaction, id);
                    await transaction.CommitAsync();
                }
                catch (System.Exception ex)
                {
                    Logging.LogError(ToString(), "Transaction, Error occurs while deleting character: " + id);
                    Logging.LogException(ToString(), ex);
                    await transaction.RollbackAsync();
                }
                await transaction.DisposeAsync();
                await connection.CloseAsync();
                this.InvokeInstanceDevExtMethods("DeleteCharacter", userId, id);
            }
        }

        public override async UniTask<long> FindCharacterName(string characterName)
        {
            object result = await ExecuteScalar("SELECT COUNT(*) FROM characters WHERE characterName LIKE @characterName",
                new MySqlParameter("@characterName", characterName));
            return result != null ? (long)result : 0;
        }

        public override async UniTask<string> GetIdByCharacterName(string characterName)
        {
            object result = await ExecuteScalar("SELECT id FROM characters WHERE characterName LIKE @characterName LIMIT 1",
                new MySqlParameter("@characterName", characterName));
            return result != null ? (string)result : string.Empty;
        }

        public override async UniTask<string> GetUserIdByCharacterName(string characterName)
        {
            object result = await ExecuteScalar("SELECT userId FROM characters WHERE characterName LIKE @characterName LIMIT 1",
                new MySqlParameter("@characterName", characterName));
            return result != null ? (string)result : string.Empty;
        }

        public override async UniTask<List<SocialCharacterData>> FindCharacters(string characterName)
        {
            List<SocialCharacterData> result = new List<SocialCharacterData>();
            await ExecuteReader((reader) =>
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
                new MySqlParameter("@characterName", "%" + characterName + "%"));
            return result;
        }

        public override async UniTask CreateFriend(string id1, string id2)
        {
            await DeleteFriend(id1, id2);
            await ExecuteNonQuery("INSERT INTO friend " +
                "(characterId1, characterId2) VALUES " +
                "(@characterId1, @characterId2)",
                new MySqlParameter("@characterId1", id1),
                new MySqlParameter("@characterId2", id2));
        }

        public override async UniTask DeleteFriend(string id1, string id2)
        {
            await ExecuteNonQuery("DELETE FROM friend WHERE " +
                "characterId1 LIKE @characterId1 AND " +
                "characterId2 LIKE @characterId2",
                new MySqlParameter("@characterId1", id1),
                new MySqlParameter("@characterId2", id2));
        }

        public override async UniTask<List<SocialCharacterData>> ReadFriends(string id1)
        {
            List<SocialCharacterData> result = new List<SocialCharacterData>();
            List<string> characterIds = new List<string>();
            await ExecuteReader((reader) =>
            {
                while (reader.Read())
                {
                    characterIds.Add(reader.GetString(0));
                }
            }, "SELECT characterId2 FROM friend WHERE characterId1=@id1",
                new MySqlParameter("@id1", id1));
            SocialCharacterData socialCharacterData;
            foreach (string characterId in characterIds)
            {
                await ExecuteReader((reader) =>
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
                }, "SELECT id, dataId, characterName, level FROM characters WHERE BINARY id = @id",
                    new MySqlParameter("@id", characterId));
            }
            return result;
        }
    }
}
#endif