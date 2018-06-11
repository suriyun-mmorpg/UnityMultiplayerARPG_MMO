using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace Insthync.MMOG
{
    public partial class MySQLDatabase
    {
        private async Task CreateCharacterItem(string characterId, InventoryType inventoryType, CharacterItem characterItem)
        {
            var connection = NewConnection();
            connection.Open();
            await CreateCharacterItem(connection, characterId, inventoryType, characterItem);
            connection.Close();
        }

        private async Task CreateCharacterItem(MySqlConnection connection, string characterId, InventoryType inventoryType, CharacterItem characterItem)
        {
            await ExecuteNonQuery(connection, "INSERT INTO characteritem (id, inventoryType, characterId, dataId, level, amount) VALUES (@id, @inventoryType, @characterId, @dataId, @level, @amount)",
                new MySqlParameter("@id", characterItem.id),
                new MySqlParameter("@inventoryType", inventoryType),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterItem.dataId),
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
                result.dataId = reader.GetInt32("dataId");
                result.level = reader.GetInt32("level");
                result.amount = reader.GetInt32("amount");
                return true;
            }
            result = CharacterItem.Empty;
            return false;
        }

        private async Task<List<CharacterItem>> ReadCharacterItems(string characterId, InventoryType inventoryType)
        {
            var result = new List<CharacterItem>();
            var reader = await ExecuteReader("SELECT * FROM characteritem WHERE characterId=@characterId AND inventoryType=@inventoryType",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", inventoryType));
            CharacterItem tempInventory;
            while (ReadCharacterItem(reader, out tempInventory, false))
            {
                result.Add(tempInventory);
            }
            return result;
        }

        public async Task<EquipWeapons> ReadCharacterEquipWeapons(string characterId)
        {
            var result = new EquipWeapons();
            // Right hand weapon
            var reader = await ExecuteReader("SELECT * FROM characteritem WHERE characterId=@characterId AND inventoryType=@inventoryType LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", InventoryType.EquipWeaponRight));
            CharacterItem rightWeapon;
            if (ReadCharacterItem(reader, out rightWeapon))
                result.rightHand = rightWeapon;
            // Left hand weapon
            reader = await ExecuteReader("SELECT * FROM characteritem WHERE characterId=@characterId AND inventoryType=@inventoryType LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", InventoryType.EquipWeaponLeft));
            CharacterItem leftWeapon;
            if (ReadCharacterItem(reader, out leftWeapon))
                result.leftHand = leftWeapon;
            return result;
        }

        public async Task CreateCharacterEquipWeapons(MySqlConnection connection, string characterId, EquipWeapons equipWeapons)
        {
            await CreateCharacterItem(connection, characterId, InventoryType.EquipWeaponRight, equipWeapons.rightHand);
            await CreateCharacterItem(connection, characterId, InventoryType.EquipWeaponLeft, equipWeapons.leftHand);
        }

        public Task CreateCharacterEquipItem(MySqlConnection connection, string characterId, CharacterItem characterItem)
        {
            return CreateCharacterItem(connection, characterId, InventoryType.EquipItems, characterItem);
        }

        public Task<List<CharacterItem>> ReadCharacterEquipItems(string characterId)
        {
            return ReadCharacterItems(characterId, InventoryType.EquipItems);
        }

        public Task CreateCharacterNonEquipItem(MySqlConnection connection, string characterId, CharacterItem characterItem)
        {
            return CreateCharacterItem(connection, characterId, InventoryType.NonEquipItems, characterItem);
        }

        public Task<List<CharacterItem>> ReadCharacterNonEquipItems(string characterId)
        {
            return ReadCharacterItems(characterId, InventoryType.NonEquipItems);
        }

        public async Task DeleteCharacterItems(MySqlConnection connection, string characterId)
        {
            await ExecuteNonQuery(connection, "DELETE FROM characteritem WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
