using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private List<int> ReadSockets(string sockets)
        {
            List<int> result = new List<int>();
            string[] splitTexts = sockets.Split(';');
            foreach (string text in splitTexts)
            {
                if (string.IsNullOrEmpty(text))
                    continue;
                result.Add(int.Parse(text));
            }
            return result;
        }

        private string WriteSockets(List<int> killMonsters)
        {
            string result = "";
            foreach (int killMonster in killMonsters)
            {
                result += killMonster + ";";
            }
            return result;
        }

        private async Task CreateCharacterItem(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, InventoryType inventoryType, CharacterItem characterItem)
        {
            if (string.IsNullOrEmpty(characterItem.id))
                return;

            await ExecuteNonQuery(connection, transaction, "INSERT INTO characteritem (id, idx, inventoryType, characterId, dataId, level, amount, equipSlotIndex, durability, exp, lockRemainsDuration, ammo, sockets) VALUES (@id, @idx, @inventoryType, @characterId, @dataId, @level, @amount, @equipSlotIndex, @durability, @exp, @lockRemainsDuration, @ammo, @sockets)",
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
                new MySqlParameter("@ammo", characterItem.ammo),
                new MySqlParameter("@sockets", WriteSockets(characterItem.sockets)));
        }

        private bool ReadCharacterItem(MySqlDataReader reader, out CharacterItem result)
        {
            if (reader.Read())
            {
                result = new CharacterItem();
                result.id = reader.GetString("id");
                result.dataId = reader.GetInt32("dataId");
                result.level = reader.GetInt16("level");
                result.amount = reader.GetInt16("amount");
                result.equipSlotIndex = reader.GetByte("equipSlotIndex");
                result.durability = reader.GetFloat("durability");
                result.exp = reader.GetInt32("exp");
                result.lockRemainsDuration = reader.GetFloat("lockRemainsDuration");
                result.ammo = reader.GetInt16("ammo");
                result.sockets = ReadSockets(reader.GetString("sockets"));
                return true;
            }
            result = CharacterItem.Empty;
            return false;
        }

        private async Task<List<CharacterItem>> ReadCharacterItems(string characterId, InventoryType inventoryType, List<CharacterItem> result = null)
        {
            if (result == null)
                result = new List<CharacterItem>();
            await ExecuteReader((reader) =>
            {
                CharacterItem tempInventory;
                while (ReadCharacterItem(reader, out tempInventory))
                {
                    result.Add(tempInventory);
                }
            }, "SELECT * FROM characteritem WHERE characterId=@characterId AND inventoryType=@inventoryType ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", (byte)inventoryType));
            return result;
        }

        public async Task<List<EquipWeapons>> ReadCharacterEquipWeapons(string characterId, List<EquipWeapons> result = null)
        {
            if (result == null)
                result = new List<EquipWeapons>();
            await ExecuteReader((reader) =>
            {
                CharacterItem tempInventory;
                byte equipWeaponSet;
                InventoryType inventoryType;
                while (ReadCharacterItem(reader, out tempInventory))
                {
                    equipWeaponSet = reader.GetByte("idx");
                    inventoryType = (InventoryType)reader.GetSByte("inventoryType");
                    // Fill weapon sets if needed
                    while (result.Count <= equipWeaponSet)
                        result.Add(new EquipWeapons());
                    // Get equip weapon set
                    if (inventoryType == InventoryType.EquipWeaponRight)
                        result[equipWeaponSet].rightHand = tempInventory;
                    if (inventoryType == InventoryType.EquipWeaponLeft)
                        result[equipWeaponSet].leftHand = tempInventory;
                }
            }, "SELECT * FROM characteritem WHERE characterId=@characterId AND (inventoryType=@inventoryType1 OR inventoryType=@inventoryType2) ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType1", (byte)InventoryType.EquipWeaponRight),
                new MySqlParameter("@inventoryType2", (byte)InventoryType.EquipWeaponLeft));
            return result;
        }

        public async Task CreateCharacterEquipWeapons(MySqlConnection connection, MySqlTransaction transaction, byte equipWeaponSet, string characterId, EquipWeapons equipWeapons)
        {
            await Task.WhenAll(
                CreateCharacterItem(connection, transaction, equipWeaponSet, characterId, InventoryType.EquipWeaponRight, equipWeapons.rightHand),
                CreateCharacterItem(connection, transaction, equipWeaponSet, characterId, InventoryType.EquipWeaponLeft, equipWeapons.leftHand));
        }

        public async Task CreateCharacterEquipItem(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterItem characterItem)
        {
            await CreateCharacterItem(connection, transaction, idx, characterId, InventoryType.EquipItems, characterItem);
        }

        public async Task<List<CharacterItem>> ReadCharacterEquipItems(string characterId, List<CharacterItem> result = null)
        {
            return await ReadCharacterItems(characterId, InventoryType.EquipItems, result);
        }

        public async Task CreateCharacterNonEquipItem(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterItem characterItem)
        {
            await CreateCharacterItem(connection, transaction, idx, characterId, InventoryType.NonEquipItems, characterItem);
        }

        public async Task<List<CharacterItem>> ReadCharacterNonEquipItems(string characterId, List<CharacterItem> result = null)
        {
            return await ReadCharacterItems(characterId, InventoryType.NonEquipItems, result);
        }

        public async Task DeleteCharacterItems(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            await ExecuteNonQuery(connection, transaction, "DELETE FROM characteritem WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
