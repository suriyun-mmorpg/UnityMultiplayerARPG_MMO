using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace Insthync.MMOG
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterQuest(MySQLRowsReader reader, out CharacterQuest result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterQuest();
                result.questId = reader.GetString("questId");
                result.isComplete = reader.GetBoolean("isComplete");
                var killMonsters = reader.GetString("killMonsters");
                var killMonstersDict = new Dictionary<string, int>();
                var splitSets = killMonsters.Split(';');
                foreach (var set in splitSets)
                {
                    var splitData = set.Split(':');
                    killMonstersDict[splitData[0]] = int.Parse(splitData[1]);
                }
                result.killedMonsters = killMonstersDict;
                return true;
            }
            result = CharacterQuest.Empty;
            return false;
        }

        public override CharacterQuest ReadCharacterQuest(string characterId, string questId)
        {
            var reader = ExecuteReader("SELECT * FROM characterQuest WHERE characterId=@characterId AND questId=@questId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@questId", questId));
            CharacterQuest result;
            ReadCharacterQuest(reader, out result);
            return result;
        }

        public override List<CharacterQuest> ReadCharacterQuests(string characterId)
        {
            var result = new List<CharacterQuest>();
            var reader = ExecuteReader("SELECT * FROM characterQuest WHERE characterId=@characterId",
                new MySqlParameter("@characterId", characterId));
            CharacterQuest tempQuest;
            while (ReadCharacterQuest(reader, out tempQuest, false))
            {
                result.Add(tempQuest);
            }
            return result;
        }
    }
}
