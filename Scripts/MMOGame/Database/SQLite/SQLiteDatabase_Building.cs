using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Threading.Tasks;

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
            await ExecuteNonQuery("INSERT INTO buildings (id, parentId, dataId, currentHp, mapName, positionX, positionY, positionZ, rotationX, rotationY, rotationZ, creatorId, creatorName) VALUES (@id, @parentId, @dataId, @currentHp, @mapName, @positionX, @positionY, @positionZ, @rotationX, @rotationY, @rotationZ, @creatorId, @creatorName)",
                new SqliteParameter("@id", saveData.Id),
                new SqliteParameter("@parentId", saveData.ParentId),
                new SqliteParameter("@dataId", saveData.DataId),
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

        public override async Task<List<BuildingSaveData>> ReadBuildings(string mapName)
        {
            var result = new List<BuildingSaveData>();
            var reader = await ExecuteReader("SELECT * FROM buildings WHERE mapName=@mapName", new SqliteParameter("@mapName", mapName));
            BuildingSaveData tempBuilding;
            while (ReadBuilding(reader, out tempBuilding, false))
            {
                result.Add(tempBuilding);
            }
            return result;
        }

        public override async Task UpdateBuilding(string mapName, IBuildingSaveData building)
        {
            await ExecuteNonQuery("UPDATE buildings SET " +
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
                new SqliteParameter("@id", building.Id),
                new SqliteParameter("@parentId", building.ParentId),
                new SqliteParameter("@dataId", building.DataId),
                new SqliteParameter("@currentHp", building.CurrentHp),
                new SqliteParameter("@mapName", mapName),
                new SqliteParameter("@positionX", building.Position.x),
                new SqliteParameter("@positionY", building.Position.y),
                new SqliteParameter("@positionZ", building.Position.z),
                new SqliteParameter("@rotationX", building.Rotation.eulerAngles.x),
                new SqliteParameter("@rotationY", building.Rotation.eulerAngles.y),
                new SqliteParameter("@rotationZ", building.Rotation.eulerAngles.z),
                new SqliteParameter("@creatorId", building.CreatorId),
                new SqliteParameter("@creatorName", building.CreatorName));
        }

        public override async Task UpdateBuildings(string mapName, IEnumerable<IBuildingSaveData> buildings)
        {
            var tasks = new List<Task>();
            await ExecuteNonQuery("UPDATE buildings SET " +
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
                new SqliteParameter("@id", saveData.Id),
                new SqliteParameter("@parentId", saveData.ParentId),
                new SqliteParameter("@dataId", saveData.DataId),
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

        public override async Task DeleteBuilding(string mapName, string id)
        {
            await ExecuteNonQuery("DELETE FROM buildings WHERE id=@id AND mapName=@mapName", new SqliteParameter("@id", id), new SqliteParameter("@mapName", mapName));
        }
    }
}