#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private bool ReadCharacterQuest(SqliteDataReader reader, out CharacterQuest result)
        {
            if (reader.Read())
            {
                result = new CharacterQuest();
                result.dataId = reader.GetInt32(0);
                result.isComplete = reader.GetBoolean(1);
                result.isTracking = reader.GetBoolean(2);
                result.ReadKilledMonsters(reader.GetString(3));
                result.ReadCompletedTasks(reader.GetString(4));
                return true;
            }
            result = CharacterQuest.Empty;
            return false;
        }

        public void CreateCharacterQuest(SqliteTransaction transaction, int idx, string characterId, CharacterQuest characterQuest)
        {
            ExecuteNonQuery(transaction, "INSERT INTO characterquest (id, idx, characterId, dataId, isComplete, isTracking, killedMonsters, completedTasks) VALUES (@id, @idx, @characterId, @dataId, @isComplete, @isTracking, @killedMonsters, @completedTasks)",
                new SqliteParameter("@id", characterId + "_" + idx),
                new SqliteParameter("@idx", idx),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@dataId", characterQuest.dataId),
                new SqliteParameter("@isComplete", characterQuest.isComplete),
                new SqliteParameter("@isTracking", characterQuest.isTracking),
                new SqliteParameter("@killedMonsters", characterQuest.WriteKilledMonsters()),
                new SqliteParameter("@completedTasks", characterQuest.WriteCompletedTasks()));
        }

        public List<CharacterQuest> ReadCharacterQuests(string characterId)
        {
            List<CharacterQuest> result = new List<CharacterQuest>();
            ExecuteReader((reader) =>
            {
                CharacterQuest tempQuest;
                while (ReadCharacterQuest(reader, out tempQuest))
                {
                    result.Add(tempQuest);
                }
            }, "SELECT dataId, isComplete, isTracking, killedMonsters, completedTasks FROM characterquest WHERE characterId=@characterId ORDER BY id ASC",
                new SqliteParameter("@characterId", characterId));
            return result;
        }

        public void DeleteCharacterQuests(SqliteTransaction transaction, string characterId)
        {
            ExecuteNonQuery(transaction, "DELETE FROM characterquest WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
#endif