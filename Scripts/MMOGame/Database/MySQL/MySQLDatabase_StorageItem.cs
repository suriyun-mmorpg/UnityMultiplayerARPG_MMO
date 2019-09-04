using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private void CreateStorageItem(MySqlConnection connection, MySqlTransaction transaction, int idx, StorageType storageType, string storageOwnerId, CharacterItem characterItem)
        {
            ExecuteNonQuery(connection, transaction, "INSERT INTO storageitem (id, idx, storageType, storageOwnerId, dataId, level, amount, durability, exp, lockRemainsDuration, ammo, sockets) VALUES (@id, @idx, @storageType, @storageOwnerId, @dataId, @level, @amount, @durability, @exp, @lockRemainsDuration, @ammo, @sockets)",
                new MySqlParameter("@id", new StorageItemId(storageType, storageOwnerId, idx).GetId()),
                new MySqlParameter("@idx", idx),
                new MySqlParameter("@storageType", (byte)storageType),
                new MySqlParameter("@storageOwnerId", storageOwnerId),
                new MySqlParameter("@dataId", characterItem.dataId),
                new MySqlParameter("@level", characterItem.level),
                new MySqlParameter("@amount", characterItem.amount),
                new MySqlParameter("@durability", characterItem.durability),
                new MySqlParameter("@exp", characterItem.exp),
                new MySqlParameter("@lockRemainsDuration", characterItem.lockRemainsDuration),
                new MySqlParameter("@ammo", characterItem.ammo),
                new MySqlParameter("@sockets", WriteSockets(characterItem.sockets)));
        }

        private bool ReadStorageItem(MySQLRowsReader reader, out CharacterItem result, bool resetReader = true)
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
                result.exp = reader.GetInt32("exp");
                result.lockRemainsDuration = reader.GetFloat("lockRemainsDuration");
                result.ammo = reader.GetInt16("ammo");
                result.sockets = ReadSockets(reader.GetString("sockets"));
                return true;
            }
            result = CharacterItem.Empty;
            return false;
        }

        public override List<CharacterItem> ReadStorageItems(StorageType storageType, string storageOwnerId)
        {
            List<CharacterItem> result = new List<CharacterItem>();
            MySQLRowsReader reader = ExecuteReader("SELECT * FROM storageitem WHERE storageType=@storageType AND storageOwnerId=@storageOwnerId ORDER BY idx ASC",
                new MySqlParameter("@storageType", (byte)storageType),
                new MySqlParameter("@storageOwnerId", storageOwnerId));
            CharacterItem tempInventory;
            while (ReadStorageItem(reader, out tempInventory, false))
            {
                result.Add(tempInventory);
            }
            return result;
        }

        public override void UpdateStorageItems(StorageType storageType, string storageOwnerId, IList<CharacterItem> characterItems)
        {
            MySqlConnection connection = NewConnection();
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteStorageItems(connection, transaction, storageType, storageOwnerId);
                for (int i = 0; i < characterItems.Count; ++i)
                {
                    CreateStorageItem(connection, transaction, i, storageType, storageOwnerId, characterItems[i]);
                }
                transaction.Commit();
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

        public void DeleteStorageItems(MySqlConnection connection, MySqlTransaction transaction, StorageType storageType, string storageOwnerId)
        {
            ExecuteNonQuery(connection, transaction, "DELETE FROM storageitem WHERE storageType=@storageType AND storageOwnerId=@storageOwnerId",
                new MySqlParameter("@storageType", (byte)storageType),
                new MySqlParameter("@storageOwnerId", storageOwnerId));
        }
    }
}
