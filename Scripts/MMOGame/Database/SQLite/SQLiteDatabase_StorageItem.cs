using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private void CreateStorageItem(int idx, StorageCharacterItem storageCharacterItem)
        {
            ExecuteNonQuery("INSERT INTO storageitem (id, idx, storageType, storageDataId, storageOwnerId, dataId, level, amount, durability, exp, lockRemainsDuration) VALUES (@id, @idx, @inventoryType, @characterId, @dataId, @level, @amount, @durability, @exp, @lockRemainsDuration)",
                new SqliteParameter("@id", new StorageItemId(storageCharacterItem.storageType, storageCharacterItem.storageDataId, storageCharacterItem.storageOwnerId, idx).GetId()),
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

        private bool ReadStorageItem(SQLiteRowsReader reader, out CharacterItem result, bool resetReader = true)
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
                return true;
            }
            result = CharacterItem.Empty;
            return false;
        }

        public override List<CharacterItem> ReadStorageItems(StorageType storageType, int storageDataId, string storageOwnerId)
        {
            List<CharacterItem> result = new List<CharacterItem>();
            SQLiteRowsReader reader = ExecuteReader("SELECT * FROM storageitem WHERE storageType=@storageType AND storageDataId=@storageDataId AND storageOwnerId=@storageOwnerId ORDER BY idx ASC",
                new SqliteParameter("@storageType", (byte)storageType),
                new SqliteParameter("@storageDataId", storageDataId),
                new SqliteParameter("@storageOwnerId", storageOwnerId));
            CharacterItem tempInventory;
            while (ReadStorageItem(reader, out tempInventory, false))
            {
                result.Add(tempInventory);
            }
            return result;
        }

        public override void UpdateStorageItems(StorageType storageType, int storageDataId, string storageOwnerId, IList<CharacterItem> characterItems)
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
