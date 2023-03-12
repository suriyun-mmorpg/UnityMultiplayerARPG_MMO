#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using System.Collections.Generic;
using Mono.Data.Sqlite;

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
                result.ExtraData = reader.GetString(9);
                result.PositionX = reader.GetFloat(10);
                result.PositionY = reader.GetFloat(11);
                result.PositionZ = reader.GetFloat(12);
                result.RotationX = reader.GetFloat(13);
                result.RotationY = reader.GetFloat(14);
                result.RotationZ = reader.GetFloat(15);
                return true;
            }
            result = new BuildingSaveData();
            return false;
        }

        public override void CreateBuilding(string mapName, IBuildingSaveData saveData)
        {
            ExecuteNonQuery("INSERT INTO buildings (id, parentId, entityId, currentHp, remainsLifeTime, mapName, positionX, positionY, positionZ, rotationX, rotationY, rotationZ, creatorId, creatorName, extraData) VALUES (@id, @parentId, @entityId, @currentHp, @remainsLifeTime, @mapName, @positionX, @positionY, @positionZ, @rotationX, @rotationY, @rotationZ, @creatorId, @creatorName, @extraData)",
                new SqliteParameter("@id", saveData.Id),
                new SqliteParameter("@parentId", saveData.ParentId),
                new SqliteParameter("@entityId", saveData.EntityId),
                new SqliteParameter("@currentHp", saveData.CurrentHp),
                new SqliteParameter("@remainsLifeTime", saveData.RemainsLifeTime),
                new SqliteParameter("@mapName", mapName),
                new SqliteParameter("@positionX", saveData.PositionX),
                new SqliteParameter("@positionY", saveData.PositionY),
                new SqliteParameter("@positionZ", saveData.PositionZ),
                new SqliteParameter("@rotationX", saveData.RotationX),
                new SqliteParameter("@rotationY", saveData.RotationY),
                new SqliteParameter("@rotationZ", saveData.RotationZ),
                new SqliteParameter("@creatorId", saveData.CreatorId),
                new SqliteParameter("@creatorName", saveData.CreatorName),
                new SqliteParameter("@extraData", saveData.ExtraData));
        }

        public override List<BuildingSaveData> ReadBuildings(string mapName)
        {
            List<BuildingSaveData> result = new List<BuildingSaveData>();
            ExecuteReader((reader) =>
            {
                BuildingSaveData tempBuilding;
                while (ReadBuilding(reader, out tempBuilding))
                {
                    result.Add(tempBuilding);
                }
            }, "SELECT id, parentId, entityId, currentHp, remainsLifeTime, isLocked, lockPassword, creatorId, creatorName, extraData, positionX, positionY, positionZ, rotationX, rotationY, rotationZ FROM buildings WHERE mapName=@mapName", new SqliteParameter("@mapName", mapName));
            return result;
        }

        public override void UpdateBuilding(string mapName, IBuildingSaveData building)
        {
            ExecuteNonQuery("UPDATE buildings SET " +
                "parentId=@parentId, " +
                "entityId=@entityId, " +
                "currentHp=@currentHp, " +
                "remainsLifeTime=@remainsLifeTime, " +
                "isLocked=@isLocked, " +
                "lockPassword=@lockPassword, " +
                "creatorId=@creatorId, " +
                "creatorName=@creatorName, " +
                "extraData=@extraData, " +
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
                new SqliteParameter("@extraData", building.ExtraData),
                new SqliteParameter("@positionX", building.PositionX),
                new SqliteParameter("@positionY", building.PositionY),
                new SqliteParameter("@positionZ", building.PositionZ),
                new SqliteParameter("@rotationX", building.RotationX),
                new SqliteParameter("@rotationY", building.RotationY),
                new SqliteParameter("@rotationZ", building.RotationZ),
                new SqliteParameter("@mapName", mapName));
        }

        public override void DeleteBuilding(string mapName, string id)
        {
            ExecuteNonQuery("DELETE FROM buildings WHERE id=@id AND mapName=@mapName", new SqliteParameter("@id", id), new SqliteParameter("@mapName", mapName));
        }
    }
}
#endif