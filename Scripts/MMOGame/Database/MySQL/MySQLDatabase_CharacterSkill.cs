using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace Insthync.MMOG
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterSkill(MySQLRowsReader reader, out CharacterSkill result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterSkill();
                result.skillId = reader.GetString("skillId");
                result.level = reader.GetInt32("level");
                result.coolDownRemainsDuration = reader.GetFloat("coolDownRemainsDuration");
                return true;
            }
            result = CharacterSkill.Empty;
            return false;
        }

        public override CharacterSkill ReadCharacterSkill(string characterId, string skillId)
        {
            var reader = ExecuteReader("SELECT * FROM characterSkill WHERE characterId=@characterId AND skillId=@skillId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@skillId", skillId));
            CharacterSkill result;
            ReadCharacterSkill(reader, out result);
            return result;
        }

        public override List<CharacterSkill> ReadCharacterSkills(string characterId)
        {
            var result = new List<CharacterSkill>();
            var reader = ExecuteReader("SELECT * FROM characterSkill WHERE characterId=@characterId",
                new MySqlParameter("@characterId", characterId));
            CharacterSkill tempSkill;
            while (ReadCharacterSkill(reader, out tempSkill, false))
            {
                result.Add(tempSkill);
            }
            return result;
        }
    }
}
