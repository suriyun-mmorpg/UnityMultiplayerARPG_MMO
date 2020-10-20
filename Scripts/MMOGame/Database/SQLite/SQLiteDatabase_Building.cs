#if UNITY_STANDALONE && !CLIENT_BUILD
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private bool ReadBuilding(SqliteDataReader reader, out BuildingSaveData result)
        {
            if (reader.Read())
            {
                result = new BuildingSaveData();
                result.Id = reader.GetString(0);
                result.ParentId = reader.GetString(1);
                result.EntityId = reader.GetInt32(2);
                result.CurrentHp = reader.GetInt32(3);
                result.RemainsLifeTime = reader.GetFloat(4);
                result.IsLocked = reader.GetBoolean(5);
                result.LockPassword = reader.GetString(6);
                result.CreatorId = reader.GetString(7);
                result.CreatorName = reader.GetString(8);
                result.Position = new Vector3(reader.GetFloat(9), reader.GetFloat(10), reader.GetFloat(11));
                result.Rotation = Quaternion.Euler(reader.GetFloat(12), reader.GetFloat(13), reader.GetFloat(14));
                return true;
            }
            result = new BuildingSaveData();
            return false;
        }

        public override async UniTask CreateBuilding(string mapName, IBuildingSaveData saveData)
        {
            await UniTask.Yield();
            ExecuteNonQuery("INSERT INTO buildings (id, parentId, entityId, currentHp, remainsLifeTime, mapName, positionX, positionY, positionZ, rotationX, rotationY, rotationZ, creatorId, creatorName) VALUES (@id, @parentId, @entityId, @currentHp, @remainsLifeTime, @mapName, @positionX, @positionY, @positionZ, @rotationX, @rotationY, @rotationZ, @creatorId, @creatorName)",
                new SqliteParameter("@id", saveData.Id),
                new SqliteParameter("@parentId", saveData.ParentId),
                new SqliteParameter("@entityId", saveData.EntityId),
                new SqliteParameter("@currentHp", saveData.CurrentHp),
                new SqliteParameter("@remainsLifeTime", saveData.RemainsLifeTime),
                new SqliteParameter("@mapName", mapName),
                new SqliteParameter("@positionX", saveData.Position.x),
                new SqliteParameter("@positionY", saveData.Position.y),
                new SqliteParameter("@positionZ", saveData.Position.z),
                new SqliteParameter("@rotationX", saveData.Rotation.eulerAngles.x),
                new SqliteParameter("@rotationY", saveData.Rotation.eulerAngles.y),
                new SqliteParameter("@rotationZ", saveData.Rotation.eulerAngles.z),
                new SqliteParameter("@creatorId", saveData.CreatorId),
                new SqliteParameter("@creatorName", saveData.CreatorName));
        }

        public override async UniTask<List<BuildingSaveData>> ReadBuildings(string mapName)
        {
            await UniTask.Yield();
            List<BuildingSaveData> result = new List<BuildingSaveData>();
            ExecuteReader((reader) =>
            {
                BuildingSaveData tempBuilding;
                while (ReadBuilding(reader, out tempBuilding))
                {
                    result.Add(tempBuilding);
                }
            }, "SELECT id, parentId, entityId, currentHp, remainsLifeTime, isLocked, lockPassword, creatorId, creatorName, positionX, positionY, positionZ, rotationX, rotationY, rotationZ FROM buildings WHERE mapName=@mapName", new SqliteParameter("@mapName", mapName));
            return result;
        }

        public override async UniTask UpdateBuilding(string mapName, IBuildingSaveData building)
        {
            await UniTask.Yield();
            ExecuteNonQuery("UPDATE buildings SET " +
                "parentId=@parentId, " +
                "entityId=@entityId, " +
                "currentHp=@currentHp, " +
                "remainsLifeTime=@remainsLifeTime, " +
                "isLocked=@isLocked, " +
                "lockPassword=@lockPassword, " +
                "creatorId=@creatorId, " +
                "creatorName=@creatorName, " +
                "positionX=@positionX, " +
                "positionY=@positionY, " +
                "positionZ=@positionZ, " +
                "rotationX=@rotationX, " +
                "rotationY=@rotationY, " +
                "rotationZ=@rotationZ " +
                "WHERE id=@id AND mapName=@mapName",
                new SqliteParameter("@id", building.Id),
                new SqliteParameter("@parentId", building.ParentId),
                new SqliteParameter("@entityId", building.EntityId),
                new SqliteParameter("@currentHp", building.CurrentHp),
                new SqliteParameter("@remainsLifeTime", building.RemainsLifeTime),
                new SqliteParameter("@isLocked", building.IsLocked),
                new SqliteParameter("@lockPassword", building.LockPassword),
                new SqliteParameter("@creatorId", building.CreatorId),
                new SqliteParameter("@creatorName", building.CreatorName),
                new SqliteParameter("@positionX", building.Position.x),
                new SqliteParameter("@positionY", building.Position.y),
                new SqliteParameter("@positionZ", building.Position.z),
                new SqliteParameter("@rotationX", building.Rotation.eulerAngles.x),
                new SqliteParameter("@rotationY", building.Rotation.eulerAngles.y),
                new SqliteParameter("@rotationZ", building.Rotation.eulerAngles.z),
                new SqliteParameter("@mapName", mapName));
        }

        public override async UniTask DeleteBuilding(string mapName, string id)
        {
            await UniTask.Yield();
            ExecuteNonQuery("DELETE FROM buildings WHERE id=@id AND mapName=@mapName", new SqliteParameter("@id", id), new SqliteParameter("@mapName", mapName));
        }
    }
}
#endif