using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
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

        private bool ReadCharacterQuest(SQLiteRowsReader reader, out CharacterQuest result, bool resetReader = true)
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

        public void CreateCharacterQuest(string characterId, CharacterQuest characterQuest)
        {
            ExecuteNonQuery("INSERT INTO characterquest (characterId, dataId, isComplete, killedMonsters) VALUES (@characterId, @dataId, @isComplete, @killedMonsters)",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@dataId", characterQuest.dataId),
                new SqliteParameter("@isComplete", characterQuest.isComplete ? 1 : 0),
                new SqliteParameter("@killedMonsters", WriteKillMonsters(characterQuest.killedMonsters)));
        }

        public List<CharacterQuest> ReadCharacterQuests(string characterId)
        {
            var result = new List<CharacterQuest>();
            var reader = ExecuteReader("SELECT * FROM characterquest WHERE characterId=@characterId",
                new SqliteParameter("@characterId", characterId));
            CharacterQuest tempQuest;
            while (ReadCharacterQuest(reader, out tempQuest, false))
            {
                result.Add(tempQuest);
            }
            return result;
        }

        public void DeleteCharacterQuests(string characterId)
        {
            ExecuteNonQuery("DELETE FROM characterquest WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
