using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private string GetStorageItemId(StorageType storageType, int storageDataId, string storageOwnerId, int idx)
        {
            return (byte)storageType + "_" + storageDataId + "_" + storageOwnerId + "_" + idx;

        }

        private void CreateStorageItem(int idx, StorageCharacterItem storageCharacterItem)
        {
            ExecuteNonQuery("INSERT INTO storageitem (id, idx, storageType, storageDataId, storageOwnerId, dataId, level, amount, durability, exp, lockRemainsDuration) VALUES (@id, @idx, @inventoryType, @characterId, @dataId, @level, @amount, @durability, @exp, @lockRemainsDuration)",
                new SqliteParameter("@id", GetStorageItemId(storageCharacterItem.storageType, storageCharacterItem.storageDataId, storageCharacterItem.storageOwnerId, idx)),
                new SqliteParameter("@idx", idx),
                new SqliteParameter("@storageType", (byte)storageCharacterItem.storageType),
                new SqliteParameter("@storageDataId", storageCharacterItem.storageDataId),
                new SqliteParameter("@storageOwnerId", storageCharacterItem.storageOwnerId),
                new SqliteParameter("@dataId", storageCharacterItem.characterItem.dataId),
                new SqliteParameter("@level", storageCharacterItem.characterItem.level),
                new SqliteParameter("@amount", storageCharacterItem.characterItem.amount),
                new SqliteParameter("@durability", storageCharacterItem.characterItem.durability),
                new SqliteParameter("@exp", storageCharacterItem.characterItem.exp),
                new SqliteParameter("@lockRemainsDuration", storageCharacterItem.characterItem.lockRemainsDuration));
        }

        private bool ReadStorageItem(SQLiteRowsReader reader, out StorageCharacterItem result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new StorageCharacterItem();
                result.storageType = (StorageType)reader.GetSByte("storageType");
                result.storageDataId = reader.GetInt32("storageDataId");
                result.storageOwnerId = reader.GetString("storageOwnerId");
                CharacterItem tempCharacterItem = new CharacterItem();
                tempCharacterItem.dataId = reader.GetInt32("dataId");
                tempCharacterItem.level = (short)reader.GetInt32("level");
                tempCharacterItem.amount = (short)reader.GetInt32("amount");
                tempCharacterItem.durability = reader.GetFloat("durability");
                tempCharacterItem.exp = reader.GetInt32("exp");
                tempCharacterItem.lockRemainsDuration = reader.GetFloat("lockRemainsDuration");
                result.characterItem = tempCharacterItem;
                return true;
            }
            result = StorageCharacterItem.Empty;
            return false;
        }

        public override List<StorageCharacterItem> ReadStorageItems(StorageType storageType, int storageDataId, string storageOwnerId)
        {
            List<StorageCharacterItem> result = new List<StorageCharacterItem>();
            SQLiteRowsReader reader = ExecuteReader("SELECT * FROM storageitem WHERE storageType=@storageType AND storageDataId=@storageDataId AND storageOwnerId=@storageOwnerId ORDER BY idx ASC",
                new SqliteParameter("@storageType", (byte)storageType),
                new SqliteParameter("@storageDataId", storageDataId),
                new SqliteParameter("@storageOwnerId", storageOwnerId));
            StorageCharacterItem tempInventory;
            while (ReadStorageItem(reader, out tempInventory, false))
            {
                result.Add(tempInventory);
            }
            return result;
        }

        public override void UpdateStorageItems(StorageType storageType, int storageDataId, string storageOwnerId, List<CharacterItem> characterItems)
        {
            BeginTransaction();
            DeleteStorageItems(storageType, storageDataId, storageOwnerId);
            StorageCharacterItem tempStorageItem;
            for (int i = 0; i < characterItems.Count; ++i)
            {
                tempStorageItem = new StorageCharacterItem();
                tempStorageItem.storageType = storageType;
                tempStorageItem.storageDataId = storageDataId;
                tempStorageItem.storageOwnerId = storageOwnerId;
                tempStorageItem.characterItem = characterItems[i];
                CreateStorageItem(i, tempStorageItem);
            }
            EndTransaction();
        }

        public void DeleteStorageItems(StorageType storageType, int storageDataId, string storageOwnerId)
        {
            ExecuteNonQuery("DELETE FROM storageitem WHERE storageType=@storageType AND storageDataId=@storageDataId AND storageOwnerId=@storageOwnerId",
                new SqliteParameter("@storageType", (byte)storageType),
                new SqliteParameter("@storageDataId", storageDataId),
                new SqliteParameter("@storageOwnerId", storageOwnerId));
        }
    }
}
