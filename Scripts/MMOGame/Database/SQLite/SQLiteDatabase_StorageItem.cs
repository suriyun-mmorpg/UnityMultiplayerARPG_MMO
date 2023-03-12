#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using Mono.Data.Sqlite;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private void CreateStorageItem(SqliteTransaction transaction, HashSet<string> insertedIds, int idx, StorageType storageType, string storageOwnerId, CharacterItem characterItem)
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
            ExecuteNonQuery(transaction, "INSERT INTO storageitem (id, idx, storageType, storageOwnerId, dataId, level, amount, durability, exp, lockRemainsDuration, expireTime, randomSeed, ammo, sockets) VALUES (@id, @idx, @storageType, @storageOwnerId, @dataId, @level, @amount, @durability, @exp, @lockRemainsDuration, @expireTime, @randomSeed, @ammo, @sockets)",
                new SqliteParameter("@id", characterItem.id),
                new SqliteParameter("@idx", idx),
                new SqliteParameter("@storageType", (byte)storageType),
                new SqliteParameter("@storageOwnerId", storageOwnerId),
                new SqliteParameter("@dataId", characterItem.dataId),
                new SqliteParameter("@level", characterItem.level),
                new SqliteParameter("@amount", characterItem.amount),
                new SqliteParameter("@durability", characterItem.durability),
                new SqliteParameter("@exp", characterItem.exp),
                new SqliteParameter("@lockRemainsDuration", characterItem.lockRemainsDuration),
                new SqliteParameter("@expireTime", characterItem.expireTime),
                new SqliteParameter("@randomSeed", characterItem.randomSeed),
                new SqliteParameter("@ammo", characterItem.ammo),
                new SqliteParameter("@sockets", characterItem.WriteSockets()));
        }

        private bool ReadStorageItem(SqliteDataReader reader, out CharacterItem result)
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
            ExecuteReader((reader) =>
            {
                CharacterItem tempInventory;
                while (ReadStorageItem(reader, out tempInventory))
                {
                    result.Add(tempInventory);
                }
            }, "SELECT id, dataId, level, amount, durability, exp, lockRemainsDuration, expireTime, randomSeed, ammo, sockets FROM storageitem WHERE storageType=@storageType AND storageOwnerId=@storageOwnerId ORDER BY idx ASC",
                new SqliteParameter("@storageType", (byte)storageType),
                new SqliteParameter("@storageOwnerId", storageOwnerId));
            return result;
        }

        public override void UpdateStorageItems(StorageType storageType, string storageOwnerId, List<CharacterItem> characterItems)
        {
            SqliteTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteStorageItems(transaction, storageType, storageOwnerId);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterItems.Count; ++i)
                {
                    CreateStorageItem(transaction, insertedIds, i, storageType, storageOwnerId, characterItems[i]);
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
        }

        public void DeleteStorageItems(SqliteTransaction transaction, StorageType storageType, string storageOwnerId)
        {
            ExecuteNonQuery(transaction, "DELETE FROM storageitem WHERE storageType=@storageType AND storageOwnerId=@storageOwnerId",
                new SqliteParameter("@storageType", (byte)storageType),
                new SqliteParameter("@storageOwnerId", storageOwnerId));
        }
    }
}
#endif