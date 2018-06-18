using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Threading.Tasks;

namespace Insthync.MMOG
{
    public partial class SQLiteDatabase
    {
        private async Task CreateCharacterItem(int idx, string characterId, InventoryType inventoryType, CharacterItem characterItem)
        {
            await ExecuteNonQuery("INSERT INTO characteritem (id, idx, inventoryType, characterId, dataId, level, amount) VALUES (@id, @idx, @inventoryType, @characterId, @dataId, @level, @amount)",
                new SqliteParameter("@id", characterId + "_" + (byte)inventoryType + "_" + idx),
                new SqliteParameter("@idx", idx),
                new SqliteParameter("@inventoryType", (byte)inventoryType),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@dataId", characterItem.dataId),
                new SqliteParameter("@level", characterItem.level),
                new SqliteParameter("@amount", characterItem.amount),
                new SqliteParameter("@durability", characterItem.durability));
        }

        private bool ReadCharacterItem(SQLiteRowsReader reader, out CharacterItem result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterItem();
                result.dataId = reader.GetInt32("dataId");
                result.level = reader.GetInt16("level");
                result.amount = reader.GetInt16("amount");
                result.durability = reader.GetFloat("durability");
                return true;
            }
            result = CharacterItem.Empty;
            return false;
        }

        private async Task<List<CharacterItem>> ReadCharacterItems(string characterId, InventoryType inventoryType)
        {
            var result = new List<CharacterItem>();
            var reader = await ExecuteReader("SELECT * FROM characteritem WHERE characterId=@characterId AND inventoryType=@inventoryType ORDER BY idx ASC",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@inventoryType", inventoryType));
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
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@inventoryType", InventoryType.EquipWeaponRight));
            CharacterItem rightWeapon;
            if (ReadCharacterItem(reader, out rightWeapon))
                result.rightHand = rightWeapon;
            // Left hand weapon
            reader = await ExecuteReader("SELECT * FROM characteritem WHERE characterId=@characterId AND inventoryType=@inventoryType LIMIT 1",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@inventoryType", InventoryType.EquipWeaponLeft));
            CharacterItem leftWeapon;
            if (ReadCharacterItem(reader, out leftWeapon))
                result.leftHand = leftWeapon;
            return result;
        }

        public async Task CreateCharacterEquipWeapons(string characterId, EquipWeapons equipWeapons)
        {
            await CreateCharacterItem(0, characterId, InventoryType.EquipWeaponRight, equipWeapons.rightHand);
            await CreateCharacterItem(0, characterId, InventoryType.EquipWeaponLeft, equipWeapons.leftHand);
        }

        public async Task CreateCharacterEquipItem(int idx, string characterId, CharacterItem characterItem)
        {
            await CreateCharacterItem(idx, characterId, InventoryType.EquipItems, characterItem);
        }

        public Task<List<CharacterItem>> ReadCharacterEquipItems(string characterId)
        {
            return ReadCharacterItems(characterId, InventoryType.EquipItems);
        }

        public Task CreateCharacterNonEquipItem(int idx, string characterId, CharacterItem characterItem)
        {
            return CreateCharacterItem(idx, characterId, InventoryType.NonEquipItems, characterItem);
        }

        public Task<List<CharacterItem>> ReadCharacterNonEquipItems(string characterId)
        {
            return ReadCharacterItems(characterId, InventoryType.NonEquipItems);
        }

        public async Task DeleteCharacterItems(string characterId)
        {
            await ExecuteNonQuery("DELETE FROM characteritem WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
