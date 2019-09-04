using System.Collections.Generic;
using MySql.Data.MySqlClient;

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

        private void CreateCharacterItem(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, InventoryType inventoryType, CharacterItem characterItem)
        {
            if (string.IsNullOrEmpty(characterItem.id))
                return;

            ExecuteNonQuery(connection, transaction, "INSERT INTO characteritem (id, idx, inventoryType, characterId, dataId, level, amount, equipSlotIndex, durability, exp, lockRemainsDuration, ammo, sockets) VALUES (@id, @idx, @inventoryType, @characterId, @dataId, @level, @amount, @equipSlotIndex, @durability, @exp, @lockRemainsDuration, @ammo, @sockets)",
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

        private bool ReadCharacterItem(MySQLRowsReader reader, out CharacterItem result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterItem();
                result.id = reader.GetString("id");
                result.dataId = reader.GetInt32("dataId");
                result.level = (short)reader.GetInt32("level");
                result.amount = (short)reader.GetInt32("amount");
                result.equipSlotIndex = (byte)reader.GetInt32("equipSlotIndex");
                result.durability = reader.GetFloat("durability");
                result.exp = reader.GetInt32("exp");
                result.lockRemainsDuration = reader.GetFloat("lockRemainsDuration");
                result.ammo = (short)reader.GetInt32("ammo");
                result.sockets = ReadSockets(reader.GetString("sockets"));
                return true;
            }
            result = CharacterItem.Empty;
            return false;
        }

        private List<CharacterItem> ReadCharacterItems(string characterId, InventoryType inventoryType)
        {
            List<CharacterItem> result = new List<CharacterItem>();
            MySQLRowsReader reader = ExecuteReader("SELECT * FROM characteritem WHERE characterId=@characterId AND inventoryType=@inventoryType ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", (byte)inventoryType));
            CharacterItem tempInventory;
            while (ReadCharacterItem(reader, out tempInventory, false))
            {
                result.Add(tempInventory);
            }
            return result;
        }

        public List<EquipWeapons> ReadCharacterEquipWeapons(string characterId)
        {
            List<EquipWeapons> result = new List<EquipWeapons>();

            MySQLRowsReader reader = ExecuteReader("SELECT * FROM characteritem WHERE characterId=@characterId AND (inventoryType=@inventoryType1 OR inventoryType=@inventoryType2) ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType1", (byte)InventoryType.EquipWeaponRight),
                new MySqlParameter("@inventoryType2", (byte)InventoryType.EquipWeaponLeft));

            CharacterItem tempInventory;
            byte equipWeaponSet;
            InventoryType inventoryType;
            while (ReadCharacterItem(reader, out tempInventory, false))
            {
                equipWeaponSet = (byte)reader.GetInt32("idx");
                inventoryType = (InventoryType)reader.GetInt32("inventoryType");
                // Fill weapon sets if needed
                while (result.Count <= equipWeaponSet)
                    result.Add(new EquipWeapons());
                // Get equip weapon set
                if (inventoryType == InventoryType.EquipWeaponRight)
                    result[equipWeaponSet].rightHand = tempInventory;
                if (inventoryType == InventoryType.EquipWeaponLeft)
                    result[equipWeaponSet].leftHand = tempInventory;
            }
            return result;
        }

        public void CreateCharacterEquipWeapons(MySqlConnection connection, MySqlTransaction transaction, byte equipWeaponSet, string characterId, EquipWeapons equipWeapons)
        {
            CreateCharacterItem(connection, transaction, equipWeaponSet, characterId, InventoryType.EquipWeaponRight, equipWeapons.rightHand);
            CreateCharacterItem(connection, transaction, equipWeaponSet, characterId, InventoryType.EquipWeaponLeft, equipWeapons.leftHand);
        }

        public void CreateCharacterEquipItem(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterItem characterItem)
        {
            CreateCharacterItem(connection, transaction, idx, characterId, InventoryType.EquipItems, characterItem);
        }

        public List<CharacterItem> ReadCharacterEquipItems(string characterId)
        {
            return ReadCharacterItems(characterId, InventoryType.EquipItems);
        }

        public void CreateCharacterNonEquipItem(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterItem characterItem)
        {
            CreateCharacterItem(connection, transaction, idx, characterId, InventoryType.NonEquipItems, characterItem);
        }

        public List<CharacterItem> ReadCharacterNonEquipItems(string characterId)
        {
            return ReadCharacterItems(characterId, InventoryType.NonEquipItems);
        }

        public void DeleteCharacterItems(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuery(connection, transaction, "DELETE FROM characteritem WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
