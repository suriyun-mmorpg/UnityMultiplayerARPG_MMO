#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using System.Collections.Generic;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private void CreateStorageItem(MySqlConnection connection, MySqlTransaction transaction, HashSet<string> insertedIds, int idx, StorageType storageType, string storageOwnerId, CharacterItem characterItem)
        {
            string id = characterItem.id;
            if (insertedIds.Contains(id))
            {
                LogWarning(LogTag, $"Storage item {id}, storage type {storageType}, owner {storageOwnerId}, already inserted");
                return;
            }
            if (string.IsNullOrEmpty(characterItem.id))
                return;
            insertedIds.Add(id);
            ExecuteNonQuerySync(connection, transaction, "INSERT INTO storageitem (id, idx, storageType, storageOwnerId, dataId, level, amount, durability, exp, lockRemainsDuration, expireTime, randomSeed, ammo, sockets) VALUES (@id, @idx, @storageType, @storageOwnerId, @dataId, @level, @amount, @durability, @exp, @lockRemainsDuration, @expireTime, @randomSeed, @ammo, @sockets)",
                new MySqlParameter("@id", characterItem.id),
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
                result.id = reader.GetString(0);
                result.dataId = reader.GetInt32(1);
                result.level = reader.GetInt32(2);
                result.amount = reader.GetInt32(3);
                result.durability = reader.GetFloat(4);
                result.exp = reader.GetInt32(5);
                result.lockRemainsDuration = reader.GetFloat(6);
                result.expireTime = reader.GetInt64(7);
                result.randomSeed = reader.GetInt32(8);
                result.ammo = reader.GetInt32(9);
                result.ReadSockets(reader.GetString(10));
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
            }, "SELECT id, dataId, level, amount, durability, exp, lockRemainsDuration, expireTime, randomSeed, ammo, sockets FROM storageitem WHERE storageType=@storageType AND storageOwnerId=@storageOwnerId ORDER BY idx ASC",
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
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterItems.Count; ++i)
                {
                    CreateStorageItem(connection, transaction, insertedIds, i, storageType, storageOwnerId, characterItems[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing storage items");
                LogException(LogTag, ex);
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