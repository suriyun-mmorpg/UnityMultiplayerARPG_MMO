using System.Collections.Generic;
using UnityEngine;
using MySqlConnector;
using System.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadBuilding(MySqlDataReader reader, out BuildingSaveData result)
        {
            if (reader.Read())
            {
                result = new BuildingSaveData();
                result.Id = reader.GetString("id");
                result.ParentId = reader.GetString("parentId");
                result.EntityId = reader.GetInt32("entityId");
                result.CurrentHp = reader.GetInt32("currentHp");
                result.RemainsLifeTime = reader.GetFloat("remainsLifeTime");
                result.IsLocked = reader.GetBoolean("isLocked");
                result.LockPassword = reader.GetString("lockPassword");
                result.CreatorId = reader.GetString("creatorId");
                result.CreatorName = reader.GetString("creatorName");
                result.Position = new Vector3(reader.GetFloat("positionX"), reader.GetFloat("positionY"), reader.GetFloat("positionZ"));
                result.Rotation = Quaternion.Euler(reader.GetFloat("rotationX"), reader.GetFloat("rotationY"), reader.GetFloat("rotationZ"));
                return true;
            }
            result = new BuildingSaveData();
            return false;
        }

        public override async Task CreateBuilding(string mapName, IBuildingSaveData saveData)
        {
            MySqlConnection connection = NewConnection();
            await connection.OpenAsync();
            await ExecuteNonQuery(connection, null, "INSERT INTO buildings (id, parentId, entityId, currentHp, remainsLifeTime, mapName, positionX, positionY, positionZ, rotationX, rotationY, rotationZ, creatorId, creatorName) VALUES (@id, @parentId, @entityId, @currentHp, @remainsLifeTime, @mapName, @positionX, @positionY, @positionZ, @rotationX, @rotationY, @rotationZ, @creatorId, @creatorName)",
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
                new MySqlParameter("@creatorName", saveData.CreatorName));
            await connection.CloseAsync();
        }

        public override async Task<List<BuildingSaveData>> ReadBuildings(string mapName)
        {
            List<BuildingSaveData> result = new List<BuildingSaveData>();
            await ExecuteReader((reader) =>
            {
                BuildingSaveData tempBuilding;
                while (ReadBuilding(reader, out tempBuilding))
                {
                    result.Add(tempBuilding);
                }
            }, "SELECT * FROM buildings WHERE mapName=@mapName", new MySqlParameter("@mapName", mapName));
            return result;
        }

        public override async Task UpdateBuilding(string mapName, IBuildingSaveData building)
        {
            MySqlConnection connection = NewConnection();
            await connection.OpenAsync();
            await ExecuteNonQuery(connection, null, "UPDATE buildings SET " +
                "parentId=@parentId, " +
                "entityId=@entityId, " +
                "currentHp=@currentHp, " +
                "remainsLifeTime=@remainsLifeTime, " +
                "isLocked=@isLocked, " +
                "lockPassword=@lockPassword, " +
                "creatorId=@creatorId, " +
                "creatorName=@creatorName " +
                "positionX=@positionX, " +
                "positionY=@positionY, " +
                "positionZ=@positionZ, " +
                "rotationX=@rotationX, " +
                "rotationY=@rotationY, " +
                "rotationZ=@rotationZ, " +
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
                new MySqlParameter("@positionX", building.Position.x),
                new MySqlParameter("@positionY", building.Position.y),
                new MySqlParameter("@positionZ", building.Position.z),
                new MySqlParameter("@rotationX", building.Rotation.eulerAngles.x),
                new MySqlParameter("@rotationY", building.Rotation.eulerAngles.y),
                new MySqlParameter("@rotationZ", building.Rotation.eulerAngles.z));
            await connection.CloseAsync();
        }

        public override async Task DeleteBuilding(string mapName, string id)
        {
            MySqlConnection connection = NewConnection();
            await connection.OpenAsync();
            await ExecuteNonQuery(connection, null, "DELETE FROM buildings WHERE id=@id AND mapName=@mapName", new MySqlParameter("@id", id), new MySqlParameter("@mapName", mapName));
            await connection.CloseAsync();
        }
    }
}