using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private string GetStorageItemId(StorageType storageType, int storageDataId, string storageOwnerId, int idx)
        {
            return (byte)storageType + "_" + storageDataId + "_" + storageOwnerId + "_" + idx;

        }

        private void CreateStorageItem(MySqlConnection connection, MySqlTransaction transaction, int idx, StorageCharacterItem storageCharacterItem)
        {
            ExecuteNonQuery(connection, transaction, "INSERT INTO storageitem (id, idx, storageType, storageDataId, storageOwnerId, dataId, level, amount, durability, exp, lockRemainsDuration) VALUES (@id, @idx, @inventoryType, @characterId, @dataId, @level, @amount, @durability, @exp, @lockRemainsDuration)",
                new MySqlParameter("@id", GetStorageItemId(storageCharacterItem.storageType, storageCharacterItem.storageDataId, storageCharacterItem.storageOwnerId, idx)),
                new MySqlParameter("@idx", idx),
                new MySqlParameter("@storageType", (byte)storageCharacterItem.storageType),
                new MySqlParameter("@storageDataId", storageCharacterItem.storageDataId),
                new MySqlParameter("@storageOwnerId", storageCharacterItem.storageOwnerId),
                new MySqlParameter("@dataId", storageCharacterItem.characterItem.dataId),
                new MySqlParameter("@level", storageCharacterItem.characterItem.level),
                new MySqlParameter("@amount", storageCharacterItem.characterItem.amount),
                new MySqlParameter("@durability", storageCharacterItem.characterItem.durability),
                new MySqlParameter("@exp", storageCharacterItem.characterItem.exp),
                new MySqlParameter("@lockRemainsDuration", storageCharacterItem.characterItem.lockRemainsDuration));
        }

        private bool ReadStorageItem(MySQLRowsReader reader, out StorageCharacterItem result, bool resetReader = true)
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
            MySQLRowsReader reader = ExecuteReader("SELECT * FROM storageitem WHERE storageType=@storageType AND storageDataId=@storageDataId AND storageOwnerId=@storageOwnerId ORDER BY idx ASC",
                new MySqlParameter("@storageType", (byte)storageType),
                new MySqlParameter("@storageDataId", storageDataId),
                new MySqlParameter("@storageOwnerId", storageOwnerId));
            StorageCharacterItem tempInventory;
            while (ReadStorageItem(reader, out tempInventory, false))
            {
                result.Add(tempInventory);
            }
            return result;
        }

        public override void UpdateStorageItems(StorageType storageType, int storageDataId, string storageOwnerId, List<CharacterItem> characterItems)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteStorageItems(connection, transaction, storageType, storageDataId, storageOwnerId);
                StorageCharacterItem tempStorageItem;
                for (int i = 0; i < characterItems.Count; ++i)
                {
                    tempStorageItem = new StorageCharacterItem();
                    tempStorageItem.storageType = storageType;
                    tempStorageItem.storageDataId = storageDataId;
                    tempStorageItem.storageOwnerId = storageOwnerId;
                    tempStorageItem.characterItem = characterItems[i];
                    CreateStorageItem(connection, transaction, i, tempStorageItem);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Transaction, Error occurs while replacing storage items");
                Debug.LogException(ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        public void DeleteStorageItems(MySqlConnection connection, MySqlTransaction transaction, StorageType storageType, int storageDataId, string storageOwnerId)
        {
            ExecuteNonQuery(connection, transaction, "DELETE FROM storageitem WHERE storageType=@storageType AND storageDataId=@storageDataId AND storageOwnerId=@storageOwnerId",
                new MySqlParameter("@storageType", (byte)storageType),
                new MySqlParameter("@storageDataId", storageDataId),
                new MySqlParameter("@storageOwnerId", storageOwnerId));
        }
    }
}
