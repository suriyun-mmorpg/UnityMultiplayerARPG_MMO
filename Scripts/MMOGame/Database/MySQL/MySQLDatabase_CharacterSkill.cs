using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterSkill(MySqlDataReader reader, out CharacterSkill result)
        {
            if (reader.Read())
            {
                result = new CharacterSkill();
                result.dataId = reader.GetInt32(0);
                result.level = reader.GetInt16(1);
                return true;
            }
            result = CharacterSkill.Empty;
            return false;
        }

        public async Task CreateCharacterSkill(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterSkill characterSkill)
        {
            await ExecuteNonQuery(connection, transaction, "INSERT INTO characterskill (id, idx, characterId, dataId, level) VALUES (@id, @idx, @characterId, @dataId, @level)",
                new MySqlParameter("@id", characterId + "_" + idx),
                new MySqlParameter("@idx", idx),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterSkill.dataId),
                new MySqlParameter("@level", characterSkill.level));
        }

        public async Task<List<CharacterSkill>> ReadCharacterSkills(string characterId, List<CharacterSkill> result = null)
        {
            if (result == null)
                result = new List<CharacterSkill>();
            await ExecuteReader((reader) =>
            {
                CharacterSkill tempSkill;
                while (ReadCharacterSkill(reader, out tempSkill))
                {
                    result.Add(tempSkill);
                }
            }, "SELECT dataId, level FROM characterskill WHERE characterId=@characterId ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        public async Task DeleteCharacterSkills(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            await ExecuteNonQuery(connection, transaction, "DELETE FROM characterskill WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
