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
            ExecuteNonQuery(connection, transaction, "INSERT INTO characteritem (id, idx, inventoryType, characterId, dataId, level, amount, durability, exp, lockRemainsDuration, ammo, sockets) VALUES (@id, @idx, @inventoryType, @characterId, @dataId, @level, @amount, @durability, @exp, @lockRemainsDuration, @ammo, @sockets)",
                new MySqlParameter("@id", characterId + "_" + (byte)inventoryType + "_" + idx),
                new MySqlParameter("@idx", idx),
                new MySqlParameter("@inventoryType", (byte)inventoryType),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterItem.dataId),
                new MySqlParameter("@level", characterItem.level),
                new MySqlParameter("@amount", characterItem.amount),
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
                result.dataId = reader.GetInt32("dataId");
                result.level = (short)reader.GetInt32("level");
                result.amount = (short)reader.GetInt32("amount");
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

        public EquipWeapons ReadCharacterEquipWeapons(string characterId)
        {
            EquipWeapons result = new EquipWeapons();
            // Right hand weapon
            MySQLRowsReader reader = ExecuteReader("SELECT * FROM characteritem WHERE characterId=@characterId AND inventoryType=@inventoryType LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", (byte)InventoryType.EquipWeaponRight));
            CharacterItem rightWeapon;
            if (ReadCharacterItem(reader, out rightWeapon))
                result.rightHand = rightWeapon;
            // Left hand weapon
            reader = ExecuteReader("SELECT * FROM characteritem WHERE characterId=@characterId AND inventoryType=@inventoryType LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", (byte)InventoryType.EquipWeaponLeft));
            CharacterItem leftWeapon;
            if (ReadCharacterItem(reader, out leftWeapon))
                result.leftHand = leftWeapon;
            return result;
        }

        public void CreateCharacterEquipWeapons(MySqlConnection connection, MySqlTransaction transaction, string characterId, EquipWeapons equipWeapons)
        {
            CreateCharacterItem(connection, transaction, 0, characterId, InventoryType.EquipWeaponRight, equipWeapons.rightHand);
            CreateCharacterItem(connection, transaction, 0, characterId, InventoryType.EquipWeaponLeft, equipWeapons.leftHand);
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
