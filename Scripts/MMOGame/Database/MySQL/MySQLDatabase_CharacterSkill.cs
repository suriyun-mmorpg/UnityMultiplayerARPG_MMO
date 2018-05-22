using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace Insthync.MMOG
{
    public partial class MySQLDatabase
    {

        public override CharacterSkill ReadCharacterSkill(string characterId, string skillId)
        {
            var reader = ExecuteReader("SELECT level, coolDownRemainsDuration FROM characterSkill WHERE characterId=@characterId AND skillId=@skillId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@skillId", skillId));
            if (reader.Read())
            {
                var result = new CharacterSkill();
                result.skillId = skillId;
                result.level = reader.GetInt32(0);
                result.coolDownRemainsDuration = reader.GetFloat(1);
                return result;
            }
            return CharacterSkill.Empty;
        }

        public override List<CharacterSkill> ReadCharacterSkills(string characterId)
        {
            throw new System.NotImplementedException();
        }
    }
}
