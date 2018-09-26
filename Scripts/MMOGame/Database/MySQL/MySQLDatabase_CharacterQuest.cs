using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private Dictionary<int, int> ReadKillMonsters(string killMonsters)
        {
            var result = new Dictionary<int, int>();
            var splitSets = killMonsters.Split(';');
            foreach (var set in splitSets)
            {
                if (string.IsNullOrEmpty(set))
                    continue;
                var splitData = set.Split(':');
                if (splitData.Length != 2)
                    continue;
                result[int.Parse(splitData[0])] = int.Parse(splitData[1]);
            }
            return result;
        }

        private string WriteKillMonsters(Dictionary<int, int> killMonsters)
        {
            var result = "";
            foreach (var keyValue in killMonsters)
            {
                result += keyValue.Key + ":" + keyValue.Value + ";";
            }
            return result;
        }

        private bool ReadCharacterQuest(MySQLRowsReader reader, out CharacterQuest result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

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

        public void CreateCharacterQuest(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterQuest characterQuest)
        {
            ExecuteNonQuery(connection, transaction, "INSERT INTO characterquest (id, idx, characterId, dataId, isComplete, killedMonsters) VALUES (@id, @idx, @characterId, @dataId, @isComplete, @killedMonsters)",
                new MySqlParameter("@id", characterId + "_" + idx),
                new MySqlParameter("@idx", idx),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterQuest.dataId),
                new MySqlParameter("@isComplete", characterQuest.isComplete),
                new MySqlParameter("@killedMonsters", WriteKillMonsters(characterQuest.killedMonsters)));
        }

        public List<CharacterQuest> ReadCharacterQuests(string characterId)
        {
            var result = new List<CharacterQuest>();
            var reader = ExecuteReader("SELECT * FROM characterquest WHERE characterId=@characterId ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId));
            CharacterQuest tempQuest;
            while (ReadCharacterQuest(reader, out tempQuest, false))
            {
                result.Add(tempQuest);
            }
            return result;
        }

        public void DeleteCharacterQuests(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuery(connection, transaction, "DELETE FROM characterquest WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
