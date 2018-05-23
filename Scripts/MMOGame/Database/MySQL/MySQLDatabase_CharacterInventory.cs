using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace Insthync.MMOG
{
    public partial class MySQLDatabase
    {
        private void CreateCharacterItem(string characterId, InventoryType inventoryType, CharacterItem characterItem)
        {
            ExecuteNonQuery("INSERT INTO characterInventory (id, inventoryType, characterId, itemId, level, amount) VALUES (@id, @inventoryType, @characterId, @itemId, @level, @amount)",
                new MySqlParameter("@id", characterItem.id),
                new MySqlParameter("@inventoryType", inventoryType),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@itemId", characterItem.itemId),
                new MySqlParameter("@level", characterItem.level),
                new MySqlParameter("@amount", characterItem.amount));
        }

        private bool ReadCharacterItem(MySQLRowsReader reader, out CharacterItem result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterItem();
                result.id = reader.GetString("id");
                result.itemId = reader.GetString("itemId");
                result.level = reader.GetInt32("level");
                result.amount = reader.GetInt32("amount");
                return true;
            }
            result = CharacterItem.Empty;
            return false;
        }

        private CharacterItem ReadCharacterItem(string characterId, string id)
        {
            var reader = ExecuteReader("SELECT * FROM characterInventory WHERE characterId=@characterId AND id=@id LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@id", id));
            CharacterItem result;
            ReadCharacterItem(reader, out result);
            return result;
        }

        private List<CharacterItem> ReadCharacterItems(string characterId, InventoryType inventoryType)
        {
            var result = new List<CharacterItem>();
            var reader = ExecuteReader("SELECT * FROM characterInventory WHERE characterId=@characterId AND inventoryType=@inventoryType",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", inventoryType));
            CharacterItem tempInventory;
            while (ReadCharacterItem(reader, out tempInventory, false))
            {
                result.Add(tempInventory);
            }
            return result;
        }

        private void UpdateCharacterItem(string characterId, CharacterItem characterItem)
        {
            ExecuteNonQuery("UPDATE characterInventory SET itemId=@itemId, level=@level, amount=@amount WHERE id=@id AND characterId=@characterId",
                new MySqlParameter("@itemId", characterItem.itemId),
                new MySqlParameter("@level", characterItem.level),
                new MySqlParameter("@amount", characterItem.amount),
                new MySqlParameter("@id", characterItem.id),
                new MySqlParameter("@characterId", characterId));
        }

        private void DeleteCharacterItem(string characterId, string id)
        {
            ExecuteNonQuery("DELETE FROM characterInventory WHERE id=@id AND characterId=@characterId)",
                new MySqlParameter("@id", id),
                new MySqlParameter("@characterId", characterId));
        }

        public override EquipWeapons ReadCharacterEquipWeapons(string characterId)
        {
            var result = new EquipWeapons();
            // Right hand weapon
            var reader = ExecuteReader("SELECT * FROM characterInventory WHERE characterId=@characterId AND inventoryType=@inventoryType LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", InventoryType.EquipWeaponRight));
            CharacterItem rightWeapon;
            if (ReadCharacterItem(reader, out rightWeapon))
                result.rightHand = rightWeapon;
            // Left hand weapon
            reader = ExecuteReader("SELECT * FROM characterInventory WHERE characterId=@characterId AND inventoryType=@inventoryType LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", InventoryType.EquipWeaponLeft));
            CharacterItem leftWeapon;
            if (ReadCharacterItem(reader, out leftWeapon))
                result.leftHand = leftWeapon;
            return result;
        }

        public override void CreateCharacterEquipWeapons(string characterId, EquipWeapons equipWeapons)
        {
            CreateCharacterItem(characterId, InventoryType.EquipWeaponRight, equipWeapons.rightHand);
            CreateCharacterItem(characterId, InventoryType.EquipWeaponLeft, equipWeapons.leftHand);
        }

        public override void UpdateCharacterEquipWeapons(string characterId, EquipWeapons equipWeapons)
        {
            DeleteCharacterEquipWeapons(characterId);
            CreateCharacterEquipWeapons(characterId, equipWeapons);
        }

        public override void DeleteCharacterEquipWeapons(string characterId)
        {
            ExecuteNonQuery("DELETE FROM characterInventory WHERE characterId=@characterId AND (inventoryType=@inventoryTypeRight OR inventoryType=@inventoryTypeLeft)",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryTypeRight", InventoryType.EquipWeaponRight),
                new MySqlParameter("@inventoryTypeLeft", InventoryType.EquipWeaponLeft));
        }

        public override void CreateCharacterEquipItem(string characterId, CharacterItem characterItem)
        {
            CreateCharacterItem(characterId, InventoryType.EquipItems, characterItem);
        }

        public override CharacterItem ReadCharacterEquipItem(string characterId, string id)
        {
            return ReadCharacterItem(characterId, id);
        }

        public override List<CharacterItem> ReadCharacterEquipItems(string characterId)
        {
            return ReadCharacterItems(characterId, InventoryType.EquipItems);
        }

        public override void UpdateCharacterEquipItem(string characterId, CharacterItem characterItem)
        {
            UpdateCharacterItem(characterId, characterItem);
        }

        public override void DeleteCharacterEquipItem(string characterId, string id)
        {
            DeleteCharacterItem(characterId, id);
        }

        public override void CreateCharacterNonEquipItem(string characterId, CharacterItem characterItem)
        {
            CreateCharacterItem(characterId, InventoryType.NonEquipItems, characterItem);
        }

        public override CharacterItem ReadCharacterNonEquipItem(string characterId, string id)
        {
            return ReadCharacterItem(characterId, id);
        }

        public override List<CharacterItem> ReadCharacterNonEquipItems(string characterId)
        {
            return ReadCharacterItems(characterId, InventoryType.NonEquipItems);
        }

        public override void UpdateCharacterNonEquipItem(string characterId, CharacterItem characterItem)
        {
            UpdateCharacterItem(characterId, characterItem);
        }

        public override void DeleteCharacterNonEquipItem(string characterId, string id)
        {
            DeleteCharacterItem(characterId, id);
        }
    }
}
