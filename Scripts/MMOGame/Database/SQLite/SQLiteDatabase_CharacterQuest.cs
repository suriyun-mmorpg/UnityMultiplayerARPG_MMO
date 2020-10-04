#if UNITY_STANDALONE && !CLIENT_BUILD
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
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

        private bool ReadCharacterQuest(SqliteDataReader reader, out CharacterQuest result)
        {
            if (reader.Read())
            {
                result = new CharacterQuest();
                result.dataId = reader.GetInt32(0);
                result.isComplete = reader.GetBoolean(1);
                result.killedMonsters = ReadKillMonsters(reader.GetString(2));
                return true;
            }
            result = CharacterQuest.Empty;
            return false;
        }

        public void CreateCharacterQuest(SqliteTransaction transaction, int idx, string characterId, CharacterQuest characterQuest)
        {
            ExecuteNonQuery(transaction, "INSERT INTO characterquest (id, idx, characterId, dataId, isComplete, killedMonsters) VALUES (@id, @idx, @characterId, @dataId, @isComplete, @killedMonsters)",
                new SqliteParameter("@id", characterId + "_" + idx),
                new SqliteParameter("@idx", idx),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@dataId", characterQuest.dataId),
                new SqliteParameter("@isComplete", characterQuest.isComplete),
                new SqliteParameter("@killedMonsters", WriteKillMonsters(characterQuest.killedMonsters)));
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
            }, "SELECT dataId, isComplete, killedMonsters FROM characterquest WHERE characterId=@characterId ORDER BY idx ASC",
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