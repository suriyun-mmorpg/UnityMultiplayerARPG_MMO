#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using System.Collections.Generic;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private void CreateCharacterItem(MySqlConnection connection, MySqlTransaction transaction, HashSet<string> insertedIds, int idx, string characterId, InventoryType inventoryType, CharacterItem characterItem)
        {
            string id = characterItem.id;
            if (insertedIds.Contains(id))
            {
                LogWarning(LogTag, $"Item {id}, inventory type {inventoryType}, for character {characterId}, already inserted");
                return;
            }
            if (string.IsNullOrEmpty(characterItem.id))
                return;
            insertedIds.Add(id);
            ExecuteNonQuerySync(connection, transaction, "INSERT INTO characteritem (id, idx, inventoryType, characterId, dataId, level, amount, equipSlotIndex, durability, exp, lockRemainsDuration, expireTime, randomSeed, ammo, sockets) VALUES (@id, @idx, @inventoryType, @characterId, @dataId, @level, @amount, @equipSlotIndex, @durability, @exp, @lockRemainsDuration, @expireTime, @randomSeed, @ammo, @sockets)",
                new MySqlParameter("@id", characterItem.id),
                new MySqlParameter("@idx", idx),
                new MySqlParameter("@inventoryType", (byte)inventoryType),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterItem.dataId),
                new MySqlParameter("@level", characterItem.level),
                new MySqlParameter("@amount", characterItem.amount),
                new MySqlParameter("@equipSlotIndex", characterItem.equipSlotIndex),
                new MySqlParameter("@durability", characterItem.durability),
                new MySqlParameter("@exp", characterItem.exp),
                new MySqlParameter("@lockRemainsDuration", characterItem.lockRemainsDuration),
                new MySqlParameter("@expireTime", characterItem.expireTime),
                new MySqlParameter("@randomSeed", characterItem.randomSeed),
                new MySqlParameter("@ammo", characterItem.ammo),
                new MySqlParameter("@sockets", characterItem.WriteSockets()));
        }

        private bool ReadCharacterItem(MySqlDataReader reader, out CharacterItem result)
        {
            if (reader.Read())
            {
                result = new CharacterItem();
                result.id = reader.GetString(0);
                result.dataId = reader.GetInt32(1);
                result.level = reader.GetInt32(2);
                result.amount = reader.GetInt32(3);
                result.equipSlotIndex = reader.GetByte(4);
                result.durability = reader.GetFloat(5);
                result.exp = reader.GetInt32(6);
                result.lockRemainsDuration = reader.GetFloat(7);
                result.expireTime = reader.GetInt64(8);
                result.randomSeed = reader.GetInt32(9);
                result.ammo = reader.GetInt32(10);
                result.ReadSockets(reader.GetString(11));
                return true;
            }
            result = CharacterItem.Empty;
            return false;
        }

        private List<CharacterItem> ReadCharacterItems(string characterId, InventoryType inventoryType, List<CharacterItem> result = null)
        {
            if (result == null)
                result = new List<CharacterItem>();
            ExecuteReaderSync((reader) =>
            {
                CharacterItem tempInventory;
                while (ReadCharacterItem(reader, out tempInventory))
                {
                    result.Add(tempInventory);
                }
            }, "SELECT id, dataId, level, amount, equipSlotIndex, durability, exp, lockRemainsDuration, expireTime, randomSeed, ammo, sockets FROM characteritem WHERE characterId=@characterId AND inventoryType=@inventoryType ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", (byte)inventoryType));
            return result;
        }

        public List<EquipWeapons> ReadCharacterEquipWeapons(string characterId, List<EquipWeapons> result = null)
        {
            if (result == null)
                result = new List<EquipWeapons>();
            ExecuteReaderSync((reader) =>
            {
                CharacterItem tempInventory;
                byte equipWeaponSet;
                InventoryType inventoryType;
                while (ReadCharacterItem(reader, out tempInventory))
                {
                    equipWeaponSet = reader.GetByte(12);
                    inventoryType = (InventoryType)reader.GetByte(13);
                    // Fill weapon sets if needed
                    while (result.Count <= equipWeaponSet)
                        result.Add(new EquipWeapons());
                    // Get equip weapon set
                    if (inventoryType == InventoryType.EquipWeaponRight)
                        result[equipWeaponSet].rightHand = tempInventory;
                    if (inventoryType == InventoryType.EquipWeaponLeft)
                        result[equipWeaponSet].leftHand = tempInventory;
                }
            }, "SELECT id, dataId, level, amount, equipSlotIndex, durability, exp, lockRemainsDuration, expireTime, randomSeed, ammo, sockets, idx, inventoryType FROM characteritem WHERE characterId=@characterId AND (inventoryType=@inventoryType1 OR inventoryType=@inventoryType2) ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType1", (byte)InventoryType.EquipWeaponRight),
                new MySqlParameter("@inventoryType2", (byte)InventoryType.EquipWeaponLeft));
            return result;
        }

        public void CreateCharacterEquipWeapons(MySqlConnection connection, MySqlTransaction transaction, HashSet<string> insertedIds, int equipWeaponSet, string characterId, EquipWeapons equipWeapons)
        {
            CreateCharacterItem(connection, transaction, insertedIds, equipWeaponSet, characterId, InventoryType.EquipWeaponRight, equipWeapons.rightHand);
            CreateCharacterItem(connection, transaction, insertedIds, equipWeaponSet, characterId, InventoryType.EquipWeaponLeft, equipWeapons.leftHand);
        }

        public void CreateCharacterEquipItem(MySqlConnection connection, MySqlTransaction transaction, HashSet<string> insertedIds, int idx, string characterId, CharacterItem characterItem)
        {
            CreateCharacterItem(connection, transaction, insertedIds, idx, characterId, InventoryType.EquipItems, characterItem);
        }

        public List<CharacterItem> ReadCharacterEquipItems(string characterId, List<CharacterItem> result = null)
        {
            return ReadCharacterItems(characterId, InventoryType.EquipItems, result);
        }

        public void CreateCharacterNonEquipItem(MySqlConnection connection, MySqlTransaction transaction, HashSet<string> insertedIds, int idx, string characterId, CharacterItem characterItem)
        {
            CreateCharacterItem(connection, transaction, insertedIds, idx, characterId, InventoryType.NonEquipItems, characterItem);
        }

        public List<CharacterItem> ReadCharacterNonEquipItems(string characterId, List<CharacterItem> result = null)
        {
            return ReadCharacterItems(characterId, InventoryType.NonEquipItems, result);
        }

        public void DeleteCharacterItems(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuerySync(connection, transaction, "DELETE FROM characteritem WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
#endif