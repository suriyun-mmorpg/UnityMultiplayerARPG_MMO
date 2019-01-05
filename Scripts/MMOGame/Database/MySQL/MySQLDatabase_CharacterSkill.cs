using System.Collections.Generic;
using MySql.Data.MySqlClient;

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
                return true;
            }
            result = CharacterSkill.Empty;
            return false;
        }

        public void CreateCharacterSkill(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterSkill characterSkill)
        {
            ExecuteNonQuery(connection, transaction, "INSERT INTO characterskill (id, idx, characterId, dataId, level) VALUES (@id, @idx, @characterId, @dataId, @level)",
                new MySqlParameter("@id", characterId + "_" + idx),
                new MySqlParameter("@idx", idx),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterSkill.dataId),
                new MySqlParameter("@level", characterSkill.level));
        }

        public List<CharacterSkill> ReadCharacterSkills(string characterId)
        {
            List<CharacterSkill> result = new List<CharacterSkill>();
            MySQLRowsReader reader = ExecuteReader("SELECT * FROM characterskill WHERE characterId=@characterId ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId));
            CharacterSkill tempSkill;
            while (ReadCharacterSkill(reader, out tempSkill, false))
            {
                result.Add(tempSkill);
            }
            return result;
        }

        public void DeleteCharacterSkills(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuery(connection, transaction, "DELETE FROM characterskill WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
