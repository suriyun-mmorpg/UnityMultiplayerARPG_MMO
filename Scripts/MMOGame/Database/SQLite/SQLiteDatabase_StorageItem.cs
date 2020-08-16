﻿using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private void CreateStorageItem(int idx, StorageType storageType, string storageOwnerId, CharacterItem characterItem)
        {
            ExecuteNonQuery("INSERT INTO storageitem (id, idx, storageType, storageOwnerId, dataId, level, amount, durability, exp, lockRemainsDuration, ammo, sockets) VALUES (@id, @idx, @storageType, @storageOwnerId, @dataId, @level, @amount, @durability, @exp, @lockRemainsDuration, @ammo, @sockets)",
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
                new SqliteParameter("@ammo", characterItem.ammo),
                new SqliteParameter("@sockets", WriteSockets(characterItem.sockets)));
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
                result.ammo = reader.GetInt16(6);
                result.sockets = ReadSockets(reader.GetString(7));
                return true;
            }
            result = CharacterItem.Empty;
            return false;
        }

        public override async Task<List<CharacterItem>> ReadStorageItems(StorageType storageType, string storageOwnerId)
        {
            await Task.Yield();
            List<CharacterItem> result = new List<CharacterItem>();
            ExecuteReader((reader) =>
            {
                CharacterItem tempInventory;
                while (ReadStorageItem(reader, out tempInventory))
                {
                    result.Add(tempInventory);
                }
            }, "SELECT dataId, level, amount, durability, exp, lockRemainsDuration, ammo, sockets FROM storageitem WHERE storageType=@storageType AND storageOwnerId=@storageOwnerId ORDER BY idx ASC",
                new SqliteParameter("@storageType", (byte)storageType),
                new SqliteParameter("@storageOwnerId", storageOwnerId));
            return result;
        }

        public override async Task UpdateStorageItems(StorageType storageType, string storageOwnerId, IList<CharacterItem> characterItems)
        {
            await Task.Yield();
            BeginTransaction();
            DeleteStorageItems(storageType, storageOwnerId);
            for (int i = 0; i < characterItems.Count; ++i)
            {
                CreateStorageItem(i, storageType, storageOwnerId, characterItems[i]);
            }
            EndTransaction();
        }

        public void DeleteStorageItems(StorageType storageType, string storageOwnerId)
        {
            ExecuteNonQuery("DELETE FROM storageitem WHERE storageType=@storageType AND storageOwnerId=@storageOwnerId",
                new SqliteParameter("@storageType", (byte)storageType),
                new SqliteParameter("@storageOwnerId", storageOwnerId));
        }
    }
}
