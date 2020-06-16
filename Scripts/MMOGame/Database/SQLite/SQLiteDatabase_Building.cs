using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private bool ReadBuilding(SQLiteRowsReader reader, out BuildingSaveData result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new BuildingSaveData();
                result.Id = reader.GetString("id");
                result.ParentId = reader.GetString("parentId");
                result.EntityId = reader.GetInt32("entityId");
                result.CurrentHp = reader.GetInt32("currentHp");
                result.Position = new Vector3(reader.GetFloat("positionX"), reader.GetFloat("positionY"), reader.GetFloat("positionZ"));
                result.Rotation = Quaternion.Euler(reader.GetFloat("rotationX"), reader.GetFloat("rotationY"), reader.GetFloat("rotationZ"));
                result.IsLocked = reader.GetBoolean("isLocked");
                result.LockPassword = reader.GetString("lockPassword");
                result.CreatorId = reader.GetString("creatorId");
                result.CreatorName = reader.GetString("creatorName");
                return true;
            }
            result = new BuildingSaveData();
            return false;
        }

        public override void CreateBuilding(string mapName, IBuildingSaveData saveData)
        {
            ExecuteNonQuery("INSERT INTO buildings (id, parentId, entityId, currentHp, mapName, positionX, positionY, positionZ, rotationX, rotationY, rotationZ, creatorId, creatorName) VALUES (@id, @parentId, @entityId, @currentHp, @mapName, @positionX, @positionY, @positionZ, @rotationX, @rotationY, @rotationZ, @creatorId, @creatorName)",
                new SqliteParameter("@id", saveData.Id),
                new SqliteParameter("@parentId", saveData.ParentId),
                new SqliteParameter("@entityId", saveData.EntityId),
                new SqliteParameter("@currentHp", saveData.CurrentHp),
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

        public override List<BuildingSaveData> ReadBuildings(string mapName)
        {
            List<BuildingSaveData> result = new List<BuildingSaveData>();
            SQLiteRowsReader reader = ExecuteReader("SELECT * FROM buildings WHERE mapName=@mapName", new SqliteParameter("@mapName", mapName));
            BuildingSaveData tempBuilding;
            while (ReadBuilding(reader, out tempBuilding, false))
            {
                result.Add(tempBuilding);
            }
            return result;
        }

        public override void UpdateBuilding(string mapName, IBuildingSaveData building)
        {
            ExecuteNonQuery("UPDATE buildings SET " +
                "parentId=@parentId, " +
                "entityId=@entityId, " +
                "currentHp=@currentHp, " +
                "positionX=@positionX, " +
                "positionY=@positionY, " +
                "positionZ=@positionZ, " +
                "rotationX=@rotationX, " +
                "rotationY=@rotationY, " +
                "rotationZ=@rotationZ, " +
                "isLocked=@isLocked, " +
                "lockPassword=@lockPassword, " +
                "creatorId=@creatorId, " +
                "creatorName=@creatorName " +
                "WHERE id=@id AND mapName=@mapName",
                new SqliteParameter("@id", building.Id),
                new SqliteParameter("@parentId", building.ParentId),
                new SqliteParameter("@entityId", building.EntityId),
                new SqliteParameter("@currentHp", building.CurrentHp),
                new SqliteParameter("@mapName", mapName),
                new SqliteParameter("@positionX", building.Position.x),
                new SqliteParameter("@positionY", building.Position.y),
                new SqliteParameter("@positionZ", building.Position.z),
                new SqliteParameter("@rotationX", building.Rotation.eulerAngles.x),
                new SqliteParameter("@rotationY", building.Rotation.eulerAngles.y),
                new SqliteParameter("@rotationZ", building.Rotation.eulerAngles.z),
                new SqliteParameter("@isLocked", building.IsLocked),
                new SqliteParameter("@lockPassword", building.LockPassword),
                new SqliteParameter("@creatorId", building.CreatorId),
                new SqliteParameter("@creatorName", building.CreatorName));
        }

        public override void DeleteBuilding(string mapName, string id)
        {
            ExecuteNonQuery("DELETE FROM buildings WHERE id=@id AND mapName=@mapName", new SqliteParameter("@id", id), new SqliteParameter("@mapName", mapName));
        }
    }
}