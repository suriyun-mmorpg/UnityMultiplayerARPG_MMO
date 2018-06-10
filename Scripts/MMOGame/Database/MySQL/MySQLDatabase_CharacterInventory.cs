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
            await ExecuteNonQuery(connection, "INSERT INTO characterinventory (id, inventoryType, characterId, itemId, level, amount) VALUES (@id, @inventoryType, @characterId, @itemId, @level, @amount)",
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

        private async Task<CharacterItem> ReadCharacterItem(string characterId, string id)
        {
            var reader = await ExecuteReader("SELECT * FROM characterinventory WHERE characterId=@characterId AND id=@id LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@id", id));
            CharacterItem result;
            ReadCharacterItem(reader, out result);
            return result;
        }

        private async Task<List<CharacterItem>> ReadCharacterItems(string characterId, InventoryType inventoryType)
        {
            var result = new List<CharacterItem>();
            var reader = await ExecuteReader("SELECT * FROM characterinventory WHERE characterId=@characterId AND inventoryType=@inventoryType",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", inventoryType));
            CharacterItem tempInventory;
            while (ReadCharacterItem(reader, out tempInventory, false))
            {
                result.Add(tempInventory);
            }
            return result;
        }

        private async Task UpdateCharacterItem(string characterId, CharacterItem characterItem)
        {
            await ExecuteNonQuery("UPDATE characterinventory SET itemId=@itemId, level=@level, amount=@amount WHERE id=@id AND characterId=@characterId",
                new MySqlParameter("@itemId", characterItem.itemId),
                new MySqlParameter("@level", characterItem.level),
                new MySqlParameter("@amount", characterItem.amount),
                new MySqlParameter("@id", characterItem.id),
                new MySqlParameter("@characterId", characterId));
        }

        private async Task DeleteCharacterItem(string characterId, string id)
        {
            await ExecuteNonQuery("DELETE FROM characterinventory WHERE id=@id AND characterId=@characterId)",
                new MySqlParameter("@id", id),
                new MySqlParameter("@characterId", characterId));
        }

        public override async Task<EquipWeapons> ReadCharacterEquipWeapons(string characterId)
        {
            var result = new EquipWeapons();
            // Right hand weapon
            var reader = await ExecuteReader("SELECT * FROM characterinventory WHERE characterId=@characterId AND inventoryType=@inventoryType LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", InventoryType.EquipWeaponRight));
            CharacterItem rightWeapon;
            if (ReadCharacterItem(reader, out rightWeapon))
                result.rightHand = rightWeapon;
            // Left hand weapon
            reader = await ExecuteReader("SELECT * FROM characterinventory WHERE characterId=@characterId AND inventoryType=@inventoryType LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryType", InventoryType.EquipWeaponLeft));
            CharacterItem leftWeapon;
            if (ReadCharacterItem(reader, out leftWeapon))
                result.leftHand = leftWeapon;
            return result;
        }

        public override async Task CreateCharacterEquipWeapons(string characterId, EquipWeapons equipWeapons)
        {
            var connection = NewConnection();
            connection.Open();
            await CreateCharacterEquipWeapons(connection, characterId, equipWeapons);
            connection.Close();
        }

        public async Task CreateCharacterEquipWeapons(MySqlConnection connection, string characterId, EquipWeapons equipWeapons)
        {
            await CreateCharacterItem(connection, characterId, InventoryType.EquipWeaponRight, equipWeapons.rightHand);
            await CreateCharacterItem(connection, characterId, InventoryType.EquipWeaponLeft, equipWeapons.leftHand);
        }

        public override async Task UpdateCharacterEquipWeapons(string characterId, EquipWeapons equipWeapons)
        {
            var connection = NewConnection();
            connection.Open();
            await UpdateCharacterEquipWeapons(connection, characterId, equipWeapons);
            connection.Close();
        }

        public async Task UpdateCharacterEquipWeapons(MySqlConnection connection, string characterId, EquipWeapons equipWeapons)
        {
            await DeleteCharacterEquipWeapons(connection, characterId);
            await CreateCharacterEquipWeapons(connection, characterId, equipWeapons);
        }

        public override async Task DeleteCharacterEquipWeapons(string characterId)
        {
            var connection = NewConnection();
            connection.Open();
            await ExecuteNonQuery(connection, characterId);
            connection.Close();
        }

        public async Task DeleteCharacterEquipWeapons(MySqlConnection connection, string characterId)
        {
            await ExecuteNonQuery(connection, "DELETE FROM characterinventory WHERE characterId=@characterId AND (inventoryType=@inventoryTypeRight OR inventoryType=@inventoryTypeLeft)",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@inventoryTypeRight", InventoryType.EquipWeaponRight),
                new MySqlParameter("@inventoryTypeLeft", InventoryType.EquipWeaponLeft));
        }

        public override async Task CreateCharacterEquipItem(string characterId, CharacterItem characterItem)
        {
            await CreateCharacterItem(characterId, InventoryType.EquipItems, characterItem);
        }

        public Task CreateCharacterEquipItem(MySqlConnection connection, string characterId, CharacterItem characterItem)
        {
            return CreateCharacterItem(connection, characterId, InventoryType.EquipItems, characterItem);
        }

        public override Task<CharacterItem> ReadCharacterEquipItem(string characterId, string id)
        {
            return ReadCharacterItem(characterId, id);
        }

        public override Task<List<CharacterItem>> ReadCharacterEquipItems(string characterId)
        {
            return ReadCharacterItems(characterId, InventoryType.EquipItems);
        }

        public override async Task UpdateCharacterEquipItem(string characterId, CharacterItem characterItem)
        {
            await UpdateCharacterItem(characterId, characterItem);
        }

        public override async Task DeleteCharacterEquipItem(string characterId, string id)
        {
            await DeleteCharacterItem(characterId, id);
        }

        public override Task CreateCharacterNonEquipItem(string characterId, CharacterItem characterItem)
        {
            return CreateCharacterItem(characterId, InventoryType.NonEquipItems, characterItem);
        }

        public Task CreateCharacterNonEquipItem(MySqlConnection connection, string characterId, CharacterItem characterItem)
        {
            return CreateCharacterItem(connection, characterId, InventoryType.NonEquipItems, characterItem);
        }

        public override Task<CharacterItem> ReadCharacterNonEquipItem(string characterId, string id)
        {
            return ReadCharacterItem(characterId, id);
        }

        public override Task<List<CharacterItem>> ReadCharacterNonEquipItems(string characterId)
        {
            return ReadCharacterItems(characterId, InventoryType.NonEquipItems);
        }

        public override async Task UpdateCharacterNonEquipItem(string characterId, CharacterItem characterItem)
        {
            await UpdateCharacterItem(characterId, characterItem);
        }

        public override async Task DeleteCharacterNonEquipItem(string characterId, string id)
        {
            await DeleteCharacterItem(characterId, id);
        }
    }
}
