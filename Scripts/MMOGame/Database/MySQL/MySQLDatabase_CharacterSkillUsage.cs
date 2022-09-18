#if UNITY_EDITOR || UNITY_SERVER || !MMO_BUILD
using System.Collections.Generic;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterSkillUsage(MySqlDataReader reader, out CharacterSkillUsage result)
        {
            if (reader.Read())
            {
                result = new CharacterSkillUsage();
                result.type = (SkillUsageType)reader.GetByte(0);
                result.dataId = reader.GetInt32(1);
                result.coolDownRemainsDuration = reader.GetFloat(2);
                return true;
            }
            result = CharacterSkillUsage.Empty;
            return false;
        }

        public void CreateCharacterSkillUsage(MySqlConnection connection, MySqlTransaction transaction, string characterId, CharacterSkillUsage characterSkillUsage)
        {
            ExecuteNonQuerySync(connection, transaction, "INSERT INTO characterskillusage (id, characterId, type, dataId, coolDownRemainsDuration) VALUES (@id, @characterId, @type, @dataId, @coolDownRemainsDuration)",
                new MySqlParameter("@id", characterId + "_" + characterSkillUsage.type + "_" + characterSkillUsage.dataId),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@type", (byte)characterSkillUsage.type),
                new MySqlParameter("@dataId", characterSkillUsage.dataId),
                new MySqlParameter("@coolDownRemainsDuration", characterSkillUsage.coolDownRemainsDuration));
        }

        public List<CharacterSkillUsage> ReadCharacterSkillUsages(string characterId, List<CharacterSkillUsage> result = null)
        {
            if (result == null)
                result = new List<CharacterSkillUsage>();
            ExecuteReaderSync((reader) =>
            {
                CharacterSkillUsage tempSkillUsage;
                while (ReadCharacterSkillUsage(reader, out tempSkillUsage))
                {
                    result.Add(tempSkillUsage);
                }
            }, "SELECT type, dataId, coolDownRemainsDuration FROM characterskillusage WHERE characterId=@characterId ORDER BY coolDownRemainsDuration ASC",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        public void DeleteCharacterSkillUsages(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuerySync(connection, transaction, "DELETE FROM characterskillusage WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
#endif