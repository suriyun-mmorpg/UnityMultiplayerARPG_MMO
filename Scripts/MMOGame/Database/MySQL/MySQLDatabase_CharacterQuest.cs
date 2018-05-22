using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace Insthync.MMOG
{
    public partial class MySQLDatabase
    {

        public override CharacterQuest ReadCharacterQuest(string characterId, string questId)
        {
            var reader = ExecuteReader("SELECT isComplete, killMonsters FROM characterQuest WHERE characterId=@characterId AND questId=@questId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@questId", questId));
            if (reader.Read())
            {
                var result = new CharacterQuest();
                result.questId = questId;
                result.isComplete = reader.GetBoolean(0);
                var killMonsters = reader.GetString(1);
                var killMonstersDict = new Dictionary<string, int>();
                var splitSets = killMonsters.Split(';');
                foreach (var set in splitSets)
                {
                    var splitData = set.Split(':');
                    killMonstersDict[splitData[0]] = int.Parse(splitData[1]);
                }
                result.killedMonsters = killMonstersDict;
                return result;
            }
            return CharacterQuest.Empty;
        }

        public override List<CharacterQuest> ReadCharacterQuests(string characterId)
        {
            var result = new List<CharacterQuest>();
            var reader = ExecuteReader("SELECT questId FROM characterQuest WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
            while (reader.Read())
            {
                var questId = reader.GetString(0);
                result.Add(ReadCharacterQuest(characterId, questId));
            }
            return result;
        }
    }
}
