using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

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

        public override Task CreateCharacterSkill(string characterId, CharacterSkill characterSkill)
        {
            return CreateCharacterSkill(connection, characterId, characterSkill);
        }

        public async Task CreateCharacterSkill(MySqlConnection connection, string characterId, CharacterSkill characterSkill)
        {
            await ExecuteNonQuery(connection, "INSERT INTO characterskill (characterId, skillId, level, coolDownRemainsDuration) VALUES (@characterId, @skillId, @level, @coolDownRemainsDuration)",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@skillId", characterSkill.skillId),
                new MySqlParameter("@level", characterSkill.level),
                new MySqlParameter("@coolDownRemainsDuration", characterSkill.coolDownRemainsDuration));
        }

        public override async Task<CharacterSkill> ReadCharacterSkill(string characterId, string skillId)
        {
            var reader = await ExecuteReader("SELECT * FROM characterskill WHERE characterId=@characterId AND skillId=@skillId LIMIT 1",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@skillId", skillId));
            CharacterSkill result;
            ReadCharacterSkill(reader, out result);
            return result;
        }

        public override async Task<List<CharacterSkill>> ReadCharacterSkills(string characterId)
        {
            var result = new List<CharacterSkill>();
            var reader = await ExecuteReader("SELECT * FROM characterskill WHERE characterId=@characterId",
                new MySqlParameter("@characterId", characterId));
            CharacterSkill tempSkill;
            while (ReadCharacterSkill(reader, out tempSkill, false))
            {
                result.Add(tempSkill);
            }
            return result;
        }

        public override async Task UpdateCharacterSkill(string characterId, CharacterSkill characterSkill)
        {
            await ExecuteNonQuery("UPDATE characterskill SET level=@level, coolDownRemainsDuration=@coolDownRemainsDuration WHERE characterId=@characterId AND skillId=@skillId",
                new MySqlParameter("@level", characterSkill.level),
                new MySqlParameter("@coolDownRemainsDuration", characterSkill.coolDownRemainsDuration),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@skillId", characterSkill.skillId));
        }

        public override async Task DeleteCharacterSkill(string characterId, string skillId)
        {
            await ExecuteNonQuery("DELETE FROM characterskill WHERE characterId=@characterId AND skillId=@skillId",
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@skillId", skillId));
        }
    }
}
