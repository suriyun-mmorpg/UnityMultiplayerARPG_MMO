using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private Dictionary<int, int> ReadKillMonsters(string killMonsters)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            string[] splitSets = killMonsters.Split(';');
            foreach (string set in splitSets)
            {
                if (string.IsNullOrEmpty(set))
                    continue;
                string[] splitData = set.Split(':');
                if (splitData.Length != 2)
                    continue;
                result[int.Parse(splitData[0])] = int.Parse(splitData[1]);
            }
            return result;
        }

        private string WriteKillMonsters(Dictionary<int, int> killMonsters)
        {
            string result = "";
            foreach (KeyValuePair<int, int> keyValue in killMonsters)
            {
                result += keyValue.Key + ":" + keyValue.Value + ";";
            }
            return result;
        }

        private bool ReadCharacterQuest(MySqlDataReader reader, out CharacterQuest result)
        {
            if (reader.Read())
            {
                result = new CharacterQuest();
                result.dataId = reader.GetInt32("dataId");
                result.isComplete = reader.GetBoolean("isComplete");
                result.killedMonsters = ReadKillMonsters(reader.GetString("killedMonsters"));
                return true;
            }
            result = CharacterQuest.Empty;
            return false;
        }

        public async Task CreateCharacterQuest(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterQuest characterQuest)
        {
            await ExecuteNonQuery(connection, transaction, "INSERT INTO characterquest (id, idx, characterId, dataId, isComplete, killedMonsters) VALUES (@id, @idx, @characterId, @dataId, @isComplete, @killedMonsters)",
                new MySqlParameter("@id", characterId + "_" + idx),
                new MySqlParameter("@idx", idx),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterQuest.dataId),
                new MySqlParameter("@isComplete", characterQuest.isComplete),
                new MySqlParameter("@killedMonsters", WriteKillMonsters(characterQuest.killedMonsters)));
        }

        public async Task<List<CharacterQuest>> ReadCharacterQuests(string characterId, List<CharacterQuest> result = null)
        {
            if (result == null)
                result = new List<CharacterQuest>();
            await ExecuteReader((reader) =>
            {
                CharacterQuest tempQuest;
                while (ReadCharacterQuest(reader, out tempQuest))
                {
                    result.Add(tempQuest);
                }
            }, "SELECT * FROM characterquest WHERE characterId=@characterId ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        public async Task DeleteCharacterQuests(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            await ExecuteNonQuery(connection, transaction, "DELETE FROM characterquest WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
