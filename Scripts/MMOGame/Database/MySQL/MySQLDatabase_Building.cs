#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
using System.Collections.Generic;
using UnityEngine;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadBuilding(MySqlDataReader reader, out BuildingSaveData result)
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
                result.Position = new Vector3(reader.GetFloat(10), reader.GetFloat(11), reader.GetFloat(12));
                result.Rotation = Quaternion.Euler(reader.GetFloat(13), reader.GetFloat(14), reader.GetFloat(15));
                return true;
            }
            result = new BuildingSaveData();
            return false;
        }

        public override void CreateBuilding(string mapName, IBuildingSaveData saveData)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            ExecuteNonQuerySync(connection, null, "INSERT INTO buildings (id, parentId, entityId, currentHp, remainsLifeTime, mapName, positionX, positionY, positionZ, rotationX, rotationY, rotationZ, creatorId, creatorName, extraData) VALUES (@id, @parentId, @entityId, @currentHp, @remainsLifeTime, @mapName, @positionX, @positionY, @positionZ, @rotationX, @rotationY, @rotationZ, @creatorId, @creatorName, @extraData)",
                new MySqlParameter("@id", saveData.Id),
                new MySqlParameter("@parentId", saveData.ParentId),
                new MySqlParameter("@entityId", saveData.EntityId),
                new MySqlParameter("@currentHp", saveData.CurrentHp),
                new MySqlParameter("@remainsLifeTime", saveData.RemainsLifeTime),
                new MySqlParameter("@mapName", mapName),
                new MySqlParameter("@positionX", saveData.Position.x),
                new MySqlParameter("@positionY", saveData.Position.y),
                new MySqlParameter("@positionZ", saveData.Position.z),
                new MySqlParameter("@rotationX", saveData.Rotation.eulerAngles.x),
                new MySqlParameter("@rotationY", saveData.Rotation.eulerAngles.y),
                new MySqlParameter("@rotationZ", saveData.Rotation.eulerAngles.z),
                new MySqlParameter("@creatorId", saveData.CreatorId),
                new MySqlParameter("@creatorName", saveData.CreatorName),
                new MySqlParameter("@extraData", saveData.ExtraData));
            connection.Close();
        }

        public override List<BuildingSaveData> ReadBuildings(string mapName)
        {
            List<BuildingSaveData> result = new List<BuildingSaveData>();
            ExecuteReaderSync((reader) =>
            {
                BuildingSaveData tempBuilding;
                while (ReadBuilding(reader, out tempBuilding))
                {
                    result.Add(tempBuilding);
                }
            }, "SELECT id, parentId, entityId, currentHp, remainsLifeTime, isLocked, lockPassword, creatorId, creatorName, extraData, positionX, positionY, positionZ, rotationX, rotationY, rotationZ FROM buildings WHERE mapName=@mapName", new MySqlParameter("@mapName", mapName));
            return result;
        }

        public override void UpdateBuilding(string mapName, IBuildingSaveData building)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            ExecuteNonQuerySync(connection, null, "UPDATE buildings SET " +
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
                new MySqlParameter("@id", building.Id),
                new MySqlParameter("@parentId", building.ParentId),
                new MySqlParameter("@entityId", building.EntityId),
                new MySqlParameter("@currentHp", building.CurrentHp),
                new MySqlParameter("@remainsLifeTime", building.RemainsLifeTime),
                new MySqlParameter("@mapName", mapName),
                new MySqlParameter("@isLocked", building.IsLocked),
                new MySqlParameter("@lockPassword", building.LockPassword),
                new MySqlParameter("@creatorId", building.CreatorId),
                new MySqlParameter("@creatorName", building.CreatorName),
                new MySqlParameter("@extraData", building.ExtraData),
                new MySqlParameter("@positionX", building.Position.x),
                new MySqlParameter("@positionY", building.Position.y),
                new MySqlParameter("@positionZ", building.Position.z),
                new MySqlParameter("@rotationX", building.Rotation.eulerAngles.x),
                new MySqlParameter("@rotationY", building.Rotation.eulerAngles.y),
                new MySqlParameter("@rotationZ", building.Rotation.eulerAngles.z));
            connection.Close();
        }

        public override void DeleteBuilding(string mapName, string id)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            ExecuteNonQuerySync(connection, null, "DELETE FROM buildings WHERE id=@id AND mapName=@mapName", new MySqlParameter("@id", id), new MySqlParameter("@mapName", mapName));
            connection.Close();
        }
    }
}
#endif