using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadBuilding(MySQLRowsReader reader, out BuildingSaveData result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new BuildingSaveData();
                result.Id = reader.GetString("id");
                result.ParentId = reader.GetString("parentId");
                result.DataId = reader.GetInt32("dataId");
                result.CurrentHp = reader.GetInt32("currentHp");
                result.Position = new Vector3(reader.GetFloat("positionX"), reader.GetFloat("positionY"), reader.GetFloat("positionZ"));
                result.Rotation = Quaternion.Euler(reader.GetFloat("rotationX"), reader.GetFloat("rotationY"), reader.GetFloat("rotationZ"));
                result.CreatorId = reader.GetString("creatorId");
                result.CreatorName = reader.GetString("creatorName");
                return true;
            }
            result = new BuildingSaveData();
            return false;
        }

        public override async Task CreateBuilding(string mapName, BuildingSaveData saveData)
        {
            var connection = NewConnection();
            await connection.OpenAsync();
            await ExecuteNonQuery(connection, null, "INSERT INTO buildings (id, parentId, dataId, currentHp, mapName, positionX, positionY, positionZ, rotationX, rotationY, rotationZ, creatorId, creatorName) VALUES (@id, @parentId, @dataId, @currentHp, @mapName, @positionX, @positionY, @positionZ, @rotationX, @rotationY, @rotationZ, @creatorId, @creatorName)",
                new MySqlParameter("@id", saveData.Id),
                new MySqlParameter("@parentId", saveData.ParentId),
                new MySqlParameter("@dataId", saveData.DataId),
                new MySqlParameter("@currentHp", saveData.CurrentHp),
                new MySqlParameter("@mapName", mapName),
                new MySqlParameter("@positionX", saveData.Position.x),
                new MySqlParameter("@positionY", saveData.Position.y),
                new MySqlParameter("@positionZ", saveData.Position.z),
                new MySqlParameter("@rotationX", saveData.Rotation.eulerAngles.x),
                new MySqlParameter("@rotationY", saveData.Rotation.eulerAngles.y),
                new MySqlParameter("@rotationZ", saveData.Rotation.eulerAngles.z),
                new MySqlParameter("@creatorId", saveData.CreatorId),
                new MySqlParameter("@creatorName", saveData.CreatorName));
            connection.Close();
        }

        public override async Task<List<BuildingSaveData>> ReadBuildings(string mapName)
        {
            var result = new List<BuildingSaveData>();
            var reader = await ExecuteReader("SELECT * FROM buildings WHERE mapName=@mapName", new MySqlParameter("@mapName", mapName));
            BuildingSaveData tempBuilding;
            while (ReadBuilding(reader, out tempBuilding, false))
            {
                result.Add(tempBuilding);
            }
            return result;
        }

        public override async Task UpdateBuilding(string mapName, IBuildingSaveData building)
        {
            var connection = NewConnection();
            await connection.OpenAsync();
            await ExecuteNonQuery(connection, null, "UPDATE buildings SET " +
                "parentId=@parentId, " +
                "dataId=@dataId, " +
                "currentHp=@currentHp, " +
                "positionX=@positionX, " +
                "positionY=@positionY, " +
                "positionZ=@positionZ, " +
                "rotationX=@rotationX, " +
                "rotationY=@rotationY, " +
                "rotationZ=@rotationZ, " +
                "creatorId=@creatorId, " +
                "creatorName=@creatorName " +
                "WHERE id=@id AND mapName=@mapName",
                new MySqlParameter("@id", building.Id),
                new MySqlParameter("@parentId", building.ParentId),
                new MySqlParameter("@dataId", building.DataId),
                new MySqlParameter("@currentHp", building.CurrentHp),
                new MySqlParameter("@mapName", mapName),
                new MySqlParameter("@positionX", building.Position.x),
                new MySqlParameter("@positionY", building.Position.y),
                new MySqlParameter("@positionZ", building.Position.z),
                new MySqlParameter("@rotationX", building.Rotation.eulerAngles.x),
                new MySqlParameter("@rotationY", building.Rotation.eulerAngles.y),
                new MySqlParameter("@rotationZ", building.Rotation.eulerAngles.z),
                new MySqlParameter("@creatorId", building.CreatorId),
                new MySqlParameter("@creatorName", building.CreatorName));
            connection.Close();
        }

        public override async Task UpdateBuildings(string mapName, IEnumerable<IBuildingSaveData> buildings)
        {
            var tasks = new List<Task>();
            var connection = NewConnection();
            await connection.OpenAsync();
            foreach (var building in buildings)
            {
                tasks.Add(ExecuteNonQuery(connection, null, "UPDATE buildings SET " +
                    "parentId=@parentId, " +
                    "dataId=@dataId, " +
                    "currentHp=@currentHp, " +
                    "positionX=@positionX, " +
                    "positionY=@positionY, " +
                    "positionZ=@positionZ, " +
                    "rotationX=@rotationX, " +
                    "rotationY=@rotationY, " +
                    "rotationZ=@rotationZ, " +
                    "creatorId=@creatorId, " +
                    "creatorName=@creatorName " +
                    "WHERE id=@id AND mapName=@mapName",
                    new MySqlParameter("@id", building.Id),
                    new MySqlParameter("@parentId", building.ParentId),
                    new MySqlParameter("@dataId", building.DataId),
                    new MySqlParameter("@currentHp", building.CurrentHp),
                    new MySqlParameter("@mapName", mapName),
                    new MySqlParameter("@positionX", building.Position.x),
                    new MySqlParameter("@positionY", building.Position.y),
                    new MySqlParameter("@positionZ", building.Position.z),
                    new MySqlParameter("@rotationX", building.Rotation.eulerAngles.x),
                    new MySqlParameter("@rotationY", building.Rotation.eulerAngles.y),
                    new MySqlParameter("@rotationZ", building.Rotation.eulerAngles.z),
                    new MySqlParameter("@creatorId", building.CreatorId),
                    new MySqlParameter("@creatorName", building.CreatorName)));
            }
            await Task.WhenAll(tasks);
            connection.Close();
        }

        public override async Task DeleteBuilding(string mapName, string id)
        {
            var connection = NewConnection();
            await connection.OpenAsync();
            await ExecuteNonQuery(connection, null, "DELETE FROM buildings WHERE id=@id AND mapName=@mapName", new MySqlParameter("@id", id), new MySqlParameter("@mapName", mapName));
            connection.Close();
        }
    }
}