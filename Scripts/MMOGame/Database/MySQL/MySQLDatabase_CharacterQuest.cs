#if UNITY_SERVER || !MMO_BUILD
using System.Collections.Generic;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterQuest(MySqlDataReader reader, out CharacterQuest result)
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

        public void CreateCharacterQuest(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterQuest characterQuest)
        {
            ExecuteNonQuerySync(connection, transaction, "INSERT INTO characterquest (id, idx, characterId, dataId, isComplete, isTracking, killedMonsters, completedTasks) VALUES (@id, @idx, @characterId, @dataId, @isComplete, @isTracking, @killedMonsters, @completedTasks)",
                new MySqlParameter("@id", characterId + "_" + idx),
                new MySqlParameter("@idx", idx),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterQuest.dataId),
                new MySqlParameter("@isComplete", characterQuest.isComplete),
                new MySqlParameter("@isTracking", characterQuest.isTracking),
                new MySqlParameter("@killedMonsters", characterQuest.WriteKilledMonsters()),
                new MySqlParameter("@completedTasks", characterQuest.WriteCompletedTasks()));
        }

        public List<CharacterQuest> ReadCharacterQuests(string characterId, List<CharacterQuest> result = null)
        {
            if (result == null)
                result = new List<CharacterQuest>();
            ExecuteReaderSync((reader) =>
            {
                CharacterQuest tempQuest;
                while (ReadCharacterQuest(reader, out tempQuest))
                {
                    result.Add(tempQuest);
                }
            }, "SELECT dataId, isComplete, isTracking, killedMonsters, completedTasks FROM characterquest WHERE characterId=@characterId ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        public void DeleteCharacterQuests(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuerySync(connection, transaction, "DELETE FROM characterquest WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
#endif