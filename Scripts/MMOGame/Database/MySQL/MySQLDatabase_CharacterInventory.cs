using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace Insthync.MMOG
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterInventory(MySQLRowsReader reader, out CharacterItem result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterItem();
                result.id = reader.GetInt64("id").ToString();
                result.itemId = reader.GetString("itemId");
                result.level = reader.GetInt32("level");
                result.amount = reader.GetInt32("amount");
                return true;
            }
            result = CharacterItem.Empty;
            return false;
        }

        public override CharacterItem ReadCharacterEquipWeapon(string id)
        {
            var reader = ExecuteReader("SELECT * FROM characterInventory WHERE id=@id AND (inventoryType=@inventoryTypeRight OR inventoryType=@inventoryTypeLeft) LIMIT 1",
                new MySqlParameter("@id", id),
                new MySqlParameter("@inventoryTypeRight", InventoryType.EquipWeaponRight),
                new MySqlParameter("@inventoryTypeLeft", InventoryType.EquipWeaponLeft));
            CharacterItem result;
            ReadCharacterInventory(reader, out result);
            return result;
        }

        public override EquipWeapons ReadCharacterEquipWeapons(string characterId)
        {
            var result = new EquipWeapons();
            // Right hand weapon
            var reader = ExecuteReader("SELECT * FROM characterInventory WHERE characterId=@characterId AND inventoryType=@inventoryType LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", InventoryType.EquipWeaponRight));
            CharacterItem rightWeapon;
            if (ReadCharacterInventory(reader, out rightWeapon))
                result.rightHand = rightWeapon;
            // Left hand weapon
            reader = ExecuteReader("SELECT * FROM characterInventory WHERE characterId=@characterId AND inventoryType=@inventoryType LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", InventoryType.EquipWeaponLeft));
            CharacterItem leftWeapon;
            if (ReadCharacterInventory(reader, out leftWeapon))
                result.leftHand = leftWeapon;
            return result;
        }

        public override CharacterItem ReadCharacterEquipItem(string inventoryId)
        {
            var reader = ExecuteReader("SELECT * FROM characterInventory WHERE inventoryId=@inventoryId AND inventoryType=@inventoryType LIMIT 1",
                new MySqlParameter("@inventoryId", inventoryId),
                new MySqlParameter("@inventoryType", InventoryType.EquipItems));
            CharacterItem result;
            ReadCharacterInventory(reader, out result);
            return result;
        }

        public override List<CharacterItem> ReadCharacterEquipItems(string characterId)
        {
            var result = new List<CharacterItem>();
            var reader = ExecuteReader("SELECT * FROM characterInventory WHERE characterId=@characterId AND inventoryType=@inventoryType",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", InventoryType.EquipItems));
            CharacterItem tempInventory;
            while (ReadCharacterInventory(reader, out tempInventory, false))
            {
                result.Add(tempInventory);
            }
            return result;
        }

        public override CharacterItem ReadCharacterNonEquipItem(string id)
        {
            var reader = ExecuteReader("SELECT * FROM characterInventory WHERE id=@id AND inventoryType=@inventoryType LIMIT 1",
                new MySqlParameter("@id", id),
                new MySqlParameter("@inventoryType", InventoryType.NonEquipItems));
            CharacterItem result;
            ReadCharacterInventory(reader, out result);
            return result;
        }

        public override List<CharacterItem> ReadCharacterNonEquipItems(string characterId)
        {
            var result = new List<CharacterItem>();
            var reader = ExecuteReader("SELECT * FROM characterInventory WHERE characterId=@characterId AND inventoryType=@inventoryType",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", InventoryType.NonEquipItems));
            CharacterItem tempInventory;
            while (ReadCharacterInventory(reader, out tempInventory, false))
            {
                result.Add(tempInventory);
            }
            return result;
        }
    }
}
