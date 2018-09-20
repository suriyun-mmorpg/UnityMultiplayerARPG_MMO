using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace MultiplayerARPG.MMO
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
                result.dataId = reader.GetInt32("dataId");
                result.level = (short)reader.GetInt32("level");
                result.coolDownRemainsDuration = reader.GetFloat("coolDownRemainsDuration");
                return true;
            }
            result = CharacterSkill.Empty;
            return false;
        }

        public async Task CreateCharacterSkill(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterSkill characterSkill)
        {
            await ExecuteNonQuery(connection, transaction, "INSERT INTO characterskill (id, idx, characterId, dataId, level, coolDownRemainsDuration) VALUES (@id, @idx, @characterId, @dataId, @level, @coolDownRemainsDuration)",
                new MySqlParameter("@id", characterId + "_" + idx),
                new MySqlParameter("@idx", idx),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterSkill.dataId),
                new MySqlParameter("@level", characterSkill.level),
                new MySqlParameter("@coolDownRemainsDuration", characterSkill.coolDownRemainsDuration));
        }

        public async Task<List<CharacterSkill>> ReadCharacterSkills(string characterId)
        {
            var result = new List<CharacterSkill>();
            var reader = await ExecuteReader("SELECT * FROM characterskill WHERE characterId=@characterId ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId));
            CharacterSkill tempSkill;
            while (ReadCharacterSkill(reader, out tempSkill, false))
            {
                result.Add(tempSkill);
            }
            return result;
        }

        public async Task DeleteCharacterSkills(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            await ExecuteNonQuery(connection, transaction, "DELETE FROM characterskill WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
