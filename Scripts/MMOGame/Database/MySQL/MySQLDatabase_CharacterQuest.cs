#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using System.Collections.Generic;
using Cysharp.Text;
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

        public void CreateCharacterQuest(MySqlConnection connection, MySqlTransaction transaction, HashSet<string> insertedIds, string characterId, CharacterQuest characterQuest)
        {
            string id = ZString.Concat(characterId, "_", characterQuest.dataId);
            if (insertedIds.Contains(id))
            {
                LogWarning(LogTag, $"Quest {id}, for character {characterId}, already inserted");
                return;
            }
            insertedIds.Add(id);
            ExecuteNonQuerySync(connection, transaction, "INSERT INTO characterquest (id, characterId, dataId, isComplete, isTracking, killedMonsters, completedTasks) VALUES (@id, @characterId, @dataId, @isComplete, @isTracking, @killedMonsters, @completedTasks)",
                new MySqlParameter("@id", id),
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
            }, "SELECT dataId, isComplete, isTracking, killedMonsters, completedTasks FROM characterquest WHERE characterId=@characterId ORDER BY id ASC",
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