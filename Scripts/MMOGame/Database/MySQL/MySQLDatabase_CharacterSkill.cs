#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using System.Collections.Generic;
using Cysharp.Text;
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
                result.level = reader.GetInt32(1);
                return true;
            }
            result = CharacterSkill.Empty;
            return false;
        }

        public void CreateCharacterSkill(MySqlConnection connection, MySqlTransaction transaction, HashSet<string> insertedIds, string characterId, CharacterSkill characterSkill)
        {
            string id = ZString.Concat(characterId, "_", characterSkill.dataId);
            if (insertedIds.Contains(id))
            {
                LogWarning(LogTag, $"Skill {id}, for character {characterId}, already inserted");
                return;
            }
            insertedIds.Add(id);
            ExecuteNonQuerySync(connection, transaction, "INSERT INTO characterskill (id, characterId, dataId, level) VALUES (@id, @characterId, @dataId, @level)",
                new MySqlParameter("@id", id),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterSkill.dataId),
                new MySqlParameter("@level", characterSkill.level));
        }

        public List<CharacterSkill> ReadCharacterSkills(string characterId, List<CharacterSkill> result = null)
        {
            if (result == null)
                result = new List<CharacterSkill>();
            ExecuteReaderSync((reader) =>
            {
                CharacterSkill tempSkill;
                while (ReadCharacterSkill(reader, out tempSkill))
                {
                    result.Add(tempSkill);
                }
            }, "SELECT dataId, level FROM characterskill WHERE characterId=@characterId ORDER BY id ASC",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        public void DeleteCharacterSkills(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuerySync(connection, transaction, "DELETE FROM characterskill WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
#endif