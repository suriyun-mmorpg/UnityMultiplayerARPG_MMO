#if UNITY_STANDALONE && !CLIENT_BUILD
using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using Mono.Data.Sqlite;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private void CreateStorageItem(SqliteTransaction transaction, int idx, StorageType storageType, string storageOwnerId, CharacterItem characterItem)
        {
            ExecuteNonQuery(transaction, "INSERT INTO storageitem (id, idx, storageType, storageOwnerId, dataId, level, amount, durability, exp, lockRemainsDuration, expireTime, randomSeed, ammo, sockets) VALUES (@id, @idx, @storageType, @storageOwnerId, @dataId, @level, @amount, @durability, @exp, @lockRemainsDuration, @expireTime, @randomSeed, @ammo, @sockets)",
                new SqliteParameter("@id", new StorageItemId(storageType, storageOwnerId, idx).GetId()),
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

        public override async UniTask<List<CharacterItem>> ReadStorageItems(StorageType storageType, string storageOwnerId)
        {
            await UniTask.Yield();
            List<CharacterItem> result = new List<CharacterItem>();
            ExecuteReader((reader) =>
            {
                CharacterItem tempInventory;
                while (ReadStorageItem(reader, out tempInventory))
                {
                    result.Add(tempInventory);
                }
            }, "SELECT dataId, level, amount, durability, exp, lockRemainsDuration, expireTime, randomSeed, ammo, sockets FROM storageitem WHERE storageType=@storageType AND storageOwnerId=@storageOwnerId ORDER BY idx ASC",
                new SqliteParameter("@storageType", (byte)storageType),
                new SqliteParameter("@storageOwnerId", storageOwnerId));
            return result;
        }

        public override async UniTask UpdateStorageItems(StorageType storageType, string storageOwnerId, IList<CharacterItem> characterItems)
        {
            await UniTask.Yield();
            SqliteTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteStorageItems(transaction, storageType, storageOwnerId);
                for (int i = 0; i < characterItems.Count; ++i)
                {
                    CreateStorageItem(transaction, i, storageType, storageOwnerId, characterItems[i]);
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