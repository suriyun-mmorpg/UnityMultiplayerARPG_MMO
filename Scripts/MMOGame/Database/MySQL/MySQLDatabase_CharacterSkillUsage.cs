using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterSkillUsage(MySQLRowsReader reader, out CharacterSkillUsage result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterSkillUsage();
                result.type = (SkillUsageType)reader.GetSByte("type");
                result.dataId = reader.GetInt32("dataId");
                result.coolDownRemainsDuration = reader.GetFloat("coolDownRemainsDuration");
                result.isSummoned = reader.GetBoolean("isSummoned");
                result.currentSummonedHp = reader.GetInt32("currentSummonedHp");
                result.currentSummonedMp = reader.GetInt32("currentSummonedMp");
                return true;
            }
            result = CharacterSkillUsage.Empty;
            return false;
        }

        public void CreateCharacterSkillUsage(MySqlConnection connection, MySqlTransaction transaction, string characterId, CharacterSkillUsage characterSkillUsage)
        {
            ExecuteNonQuery(connection, transaction, "INSERT INTO characterskillusage (id, characterId, type, dataId, coolDownRemainsDuration, isSummoned, currentSummonedHp, currentSummonedMp) VALUES (@id, @characterId, @type, @dataId, @coolDownRemainsDuration, @isSummoned, @currentSummonedHp, @currentSummonedMp)",
                new MySqlParameter("@id", characterId + "_" + characterSkillUsage.type + "_" + characterSkillUsage.dataId),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@type", (byte)characterSkillUsage.type),
                new MySqlParameter("@dataId", characterSkillUsage.dataId),
                new MySqlParameter("@coolDownRemainsDuration", characterSkillUsage.coolDownRemainsDuration),
                new MySqlParameter("@isSummoned", characterSkillUsage.isSummoned),
                new MySqlParameter("@currentSummonedHp", characterSkillUsage.currentSummonedHp),
                new MySqlParameter("@currentSummonedMp", characterSkillUsage.currentSummonedMp));
        }

        public List<CharacterSkillUsage> ReadCharacterSkillUsages(string characterId)
        {
            var result = new List<CharacterSkillUsage>();
            var reader = ExecuteReader("SELECT * FROM characterskillusage WHERE characterId=@characterId ORDER BY coolDownRemainsDuration ASC",
                new MySqlParameter("@characterId", characterId));
            CharacterSkillUsage tempSkillUsage;
            while (ReadCharacterSkillUsage(reader, out tempSkillUsage, false))
            {
                result.Add(tempSkillUsage);
            }
            return result;
        }

        public void DeleteCharacterSkillUsages(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuery(connection, transaction, "DELETE FROM characterskillusage WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
