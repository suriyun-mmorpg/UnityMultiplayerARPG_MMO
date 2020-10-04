#if UNITY_STANDALONE && !CLIENT_BUILD
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        public async UniTask CreateCharacterSkillUsage(MySqlConnection connection, MySqlTransaction transaction, string characterId, CharacterSkillUsage characterSkillUsage)
        {
            await ExecuteNonQuery(connection, transaction, "INSERT INTO characterskillusage (id, characterId, type, dataId, coolDownRemainsDuration) VALUES (@id, @characterId, @type, @dataId, @coolDownRemainsDuration)",
                new MySqlParameter("@id", characterId + "_" + characterSkillUsage.type + "_" + characterSkillUsage.dataId),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@type", (byte)characterSkillUsage.type),
                new MySqlParameter("@dataId", characterSkillUsage.dataId),
                new MySqlParameter("@coolDownRemainsDuration", characterSkillUsage.coolDownRemainsDuration));
        }

        public async UniTask<List<CharacterSkillUsage>> ReadCharacterSkillUsages(string characterId, List<CharacterSkillUsage> result = null)
        {
            if (result == null)
                result = new List<CharacterSkillUsage>();
            await ExecuteReader((reader) =>
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

        public async UniTask DeleteCharacterSkillUsages(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            await ExecuteNonQuery(connection, transaction, "DELETE FROM characterskillusage WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
#endif