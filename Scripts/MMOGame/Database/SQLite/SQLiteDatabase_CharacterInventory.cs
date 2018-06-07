using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Threading.Tasks;

namespace Insthync.MMOG
{
    public partial class SQLiteDatabase
    {
        private async Task CreateCharacterItem(string characterId, InventoryType inventoryType, CharacterItem characterItem)
        {
            await ExecuteNonQuery("INSERT INTO characterinventory (id, inventoryType, characterId, itemId, level, amount) VALUES (@id, @inventoryType, @characterId, @itemId, @level, @amount)",
                new SqliteParameter("@id", characterItem.id),
                new SqliteParameter("@inventoryType", inventoryType),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@itemId", characterItem.itemId),
                new SqliteParameter("@level", characterItem.level),
                new SqliteParameter("@amount", characterItem.amount));
        }

        private bool ReadCharacterItem(SQLiteRowsReader reader, out CharacterItem result, bool resetReader = true)
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
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@id", id));
            CharacterItem result;
            ReadCharacterItem(reader, out result);
            return result;
        }

        private async Task<List<CharacterItem>> ReadCharacterItems(string characterId, InventoryType inventoryType)
        {
            var result = new List<CharacterItem>();
            var reader = await ExecuteReader("SELECT * FROM characterinventory WHERE characterId=@characterId AND inventoryType=@inventoryType",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@inventoryType", inventoryType));
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
                new SqliteParameter("@itemId", characterItem.itemId),
                new SqliteParameter("@level", characterItem.level),
                new SqliteParameter("@amount", characterItem.amount),
                new SqliteParameter("@id", characterItem.id),
                new SqliteParameter("@characterId", characterId));
        }

        private async Task DeleteCharacterItem(string characterId, string id)
        {
            await ExecuteNonQuery("DELETE FROM characterinventory WHERE id=@id AND characterId=@characterId)",
                new SqliteParameter("@id", id),
                new SqliteParameter("@characterId", characterId));
        }

        public override async Task<EquipWeapons> ReadCharacterEquipWeapons(string characterId)
        {
            var result = new EquipWeapons();
            // Right hand weapon
            var reader = await ExecuteReader("SELECT * FROM characterinventory WHERE characterId=@characterId AND inventoryType=@inventoryType LIMIT 1",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@inventoryType", InventoryType.EquipWeaponRight));
            CharacterItem rightWeapon;
            if (ReadCharacterItem(reader, out rightWeapon))
                result.rightHand = rightWeapon;
            // Left hand weapon
            reader = await ExecuteReader("SELECT * FROM characterinventory WHERE characterId=@characterId AND inventoryType=@inventoryType LIMIT 1",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@inventoryType", InventoryType.EquipWeaponLeft));
            CharacterItem leftWeapon;
            if (ReadCharacterItem(reader, out leftWeapon))
                result.leftHand = leftWeapon;
            return result;
        }

        public override async Task CreateCharacterEquipWeapons(string characterId, EquipWeapons equipWeapons)
        {
            await CreateCharacterItem(characterId, InventoryType.EquipWeaponRight, equipWeapons.rightHand);
            await CreateCharacterItem(characterId, InventoryType.EquipWeaponLeft, equipWeapons.leftHand);
        }

        public override async Task UpdateCharacterEquipWeapons(string characterId, EquipWeapons equipWeapons)
        {
            await DeleteCharacterEquipWeapons(characterId);
            await CreateCharacterEquipWeapons(characterId, equipWeapons);
        }

        public override async Task DeleteCharacterEquipWeapons(string characterId)
        {
            await ExecuteNonQuery("DELETE FROM characterinventory WHERE characterId=@characterId AND (inventoryType=@inventoryTypeRight OR inventoryType=@inventoryTypeLeft)",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@inventoryTypeRight", InventoryType.EquipWeaponRight),
                new SqliteParameter("@inventoryTypeLeft", InventoryType.EquipWeaponLeft));
        }

        public override async Task CreateCharacterEquipItem(string characterId, CharacterItem characterItem)
        {
            await CreateCharacterItem(characterId, InventoryType.EquipItems, characterItem);
        }

        public override async Task<CharacterItem> ReadCharacterEquipItem(string characterId, string id)
        {
            return await ReadCharacterItem(characterId, id);
        }

        public override async Task<List<CharacterItem>> ReadCharacterEquipItems(string characterId)
        {
            return await ReadCharacterItems(characterId, InventoryType.EquipItems);
        }

        public override async Task UpdateCharacterEquipItem(string characterId, CharacterItem characterItem)
        {
            await UpdateCharacterItem(characterId, characterItem);
        }

        public override async Task DeleteCharacterEquipItem(string characterId, string id)
        {
            await DeleteCharacterItem(characterId, id);
        }

        public override async Task CreateCharacterNonEquipItem(string characterId, CharacterItem characterItem)
        {
            await CreateCharacterItem(characterId, InventoryType.NonEquipItems, characterItem);
        }

        public override async Task<CharacterItem> ReadCharacterNonEquipItem(string characterId, string id)
        {
            return await ReadCharacterItem(characterId, id);
        }

        public override async Task<List<CharacterItem>> ReadCharacterNonEquipItems(string characterId)
        {
            return await ReadCharacterItems(characterId, InventoryType.NonEquipItems);
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
