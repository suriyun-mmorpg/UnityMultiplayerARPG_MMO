#if UNITY_SERVER || !MMO_BUILD
using System.Collections.Generic;
using MySqlConnector;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private void CreateStorageItem(MySqlConnection connection, MySqlTransaction transaction, int idx, StorageType storageType, string storageOwnerId, CharacterItem characterItem)
        {
            ExecuteNonQuerySync(connection, transaction, "INSERT INTO storageitem (id, idx, storageType, storageOwnerId, dataId, level, amount, durability, exp, lockRemainsDuration, expireTime, randomSeed, ammo, sockets) VALUES (@id, @idx, @storageType, @storageOwnerId, @dataId, @level, @amount, @durability, @exp, @lockRemainsDuration, @expireTime, @randomSeed, @ammo, @sockets)",
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
                new MySqlParameter("@expireTime", characterItem.expireTime),
                new MySqlParameter("@randomSeed", characterItem.randomSeed),
                new MySqlParameter("@ammo", characterItem.ammo),
                new MySqlParameter("@sockets", characterItem.WriteSockets()));
        }

        private bool ReadStorageItem(MySqlDataReader reader, out CharacterItem result)
        {
            if (reader.Read())
            {
                result = new CharacterItem();
                result.dataId = reader.GetInt32(0);
                result.level = reader.GetInt16(1);
                result.amount = reader.GetInt16(2);
                result.durability = reader.GetFloat(3);
                result.exp = reader.GetInt32(4);
                result.lockRemainsDuration = reader.GetFloat(5);
                result.expireTime = reader.GetInt64(6);
                result.randomSeed = reader.GetInt16(7);
                result.ammo = reader.GetInt16(8);
                result.ReadSockets(reader.GetString(9));
                return true;
            }
            result = CharacterItem.Empty;
            return false;
        }

        public override List<CharacterItem> ReadStorageItems(StorageType storageType, string storageOwnerId)
        {
            List<CharacterItem> result = new List<CharacterItem>();
            ExecuteReaderSync((reader) =>
            {
                CharacterItem tempInventory;
                while (ReadStorageItem(reader, out tempInventory))
                {
                    result.Add(tempInventory);
                }
            }, "SELECT dataId, level, amount, durability, exp, lockRemainsDuration, expireTime, randomSeed, ammo, sockets FROM storageitem WHERE storageType=@storageType AND storageOwnerId=@storageOwnerId ORDER BY idx ASC",
                new MySqlParameter("@storageType", (byte)storageType),
                new MySqlParameter("@storageOwnerId", storageOwnerId));
            return result;
        }

        public override void UpdateStorageItems(StorageType storageType, string storageOwnerId, List<CharacterItem> characterItems)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteStorageItems(connection, transaction, storageType, storageOwnerId);
                int i;
                for (i = 0; i < characterItems.Count; ++i)
                {
                    CreateStorageItem(connection, transaction, i, storageType, storageOwnerId, characterItems[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing storage items");
                Logging.LogException(ToString(), ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }

        public void DeleteStorageItems(MySqlConnection connection, MySqlTransaction transaction, StorageType storageType, string storageOwnerId)
        {
            ExecuteNonQuerySync(connection, transaction, "DELETE FROM storageitem WHERE storageType=@storageType AND storageOwnerId=@storageOwnerId",
                new MySqlParameter("@storageType", (byte)storageType),
                new MySqlParameter("@storageOwnerId", storageOwnerId));
        }
    }
}
#endif