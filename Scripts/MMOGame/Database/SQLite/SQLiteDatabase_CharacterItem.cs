using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
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

        private void CreateCharacterItem(int idx, string characterId, InventoryType inventoryType, CharacterItem characterItem)
        {
            if (string.IsNullOrEmpty(characterItem.id))
                return;

            ExecuteNonQuery("INSERT INTO characteritem (id, idx, inventoryType, characterId, dataId, level, amount, equipSlotIndex, durability, exp, lockRemainsDuration, ammo, sockets) VALUES (@id, @idx, @inventoryType, @characterId, @dataId, @level, @amount, @equipSlotIndex, @durability, @exp, @lockRemainsDuration, @ammo, @sockets)",
                new SqliteParameter("@id", characterItem.id),
                new SqliteParameter("@idx", idx),
                new SqliteParameter("@inventoryType", (byte)inventoryType),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@dataId", characterItem.dataId),
                new SqliteParameter("@level", characterItem.level),
                new SqliteParameter("@amount", characterItem.amount),
                new SqliteParameter("@equipSlotIndex", characterItem.equipSlotIndex),
                new SqliteParameter("@durability", characterItem.durability),
                new SqliteParameter("@exp", characterItem.exp),
                new SqliteParameter("@lockRemainsDuration", characterItem.lockRemainsDuration),
                new SqliteParameter("@ammo", characterItem.ammo),
                new SqliteParameter("@sockets", WriteSockets(characterItem.sockets)));
        }

        private bool ReadCharacterItem(SQLiteRowsReader reader, out CharacterItem result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

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

        private List<CharacterItem> ReadCharacterItems(string characterId, InventoryType inventoryType)
        {
            List<CharacterItem> result = new List<CharacterItem>();
            SQLiteRowsReader reader = ExecuteReader("SELECT * FROM characteritem WHERE characterId=@characterId AND inventoryType=@inventoryType ORDER BY idx ASC",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@inventoryType", (byte)inventoryType));
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

            SQLiteRowsReader reader = ExecuteReader("SELECT * FROM characteritem WHERE characterId=@characterId AND (inventoryType=@inventoryType1 OR inventoryType=@inventoryType2) ORDER BY idx ASC",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@inventoryType1", (byte)InventoryType.EquipWeaponRight),
                new SqliteParameter("@inventoryType2", (byte)InventoryType.EquipWeaponLeft));

            CharacterItem tempInventory;
            byte equipWeaponSet;
            InventoryType inventoryType;
            while (ReadCharacterItem(reader, out tempInventory, false))
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
            return result;
        }

        public void CreateCharacterEquipWeapons(byte equipWeaponSet, string characterId, EquipWeapons equipWeapons)
        {
            CreateCharacterItem(equipWeaponSet, characterId, InventoryType.EquipWeaponRight, equipWeapons.rightHand);
            CreateCharacterItem(equipWeaponSet, characterId, InventoryType.EquipWeaponLeft, equipWeapons.leftHand);
        }

        public void CreateCharacterEquipItem(int idx, string characterId, CharacterItem characterItem)
        {
            CreateCharacterItem(idx, characterId, InventoryType.EquipItems, characterItem);
        }

        public List<CharacterItem> ReadCharacterEquipItems(string characterId)
        {
            return ReadCharacterItems(characterId, InventoryType.EquipItems);
        }

        public void CreateCharacterNonEquipItem(int idx, string characterId, CharacterItem characterItem)
        {
            CreateCharacterItem(idx, characterId, InventoryType.NonEquipItems, characterItem);
        }

        public List<CharacterItem> ReadCharacterNonEquipItems(string characterId)
        {
            return ReadCharacterItems(characterId, InventoryType.NonEquipItems);
        }

        public void DeleteCharacterItems(string characterId)
        {
            ExecuteNonQuery("DELETE FROM characteritem WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
