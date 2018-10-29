using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private bool ReadCharacterSkillUsage(SQLiteRowsReader reader, out CharacterSkillUsage result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterSkillUsage();
                result.type = (SkillUsageType)reader.GetSByte("type");
                result.dataId = reader.GetInt32("dataId");
                result.coolDownRemainsDuration = reader.GetFloat("coolDownRemainsDuration");
                return true;
            }
            result = CharacterSkillUsage.Empty;
            return false;
        }

        public void CreateCharacterSkillUsage(string characterId, CharacterSkillUsage characterSkillUsage)
        {
            ExecuteNonQuery("INSERT INTO characterskillusage (characterId, type, dataId, coolDownRemainsDuration) VALUES (@characterId, @type, @dataId, @coolDownRemainsDuration)",
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@type", (byte)characterSkillUsage.type),
                new SqliteParameter("@dataId", characterSkillUsage.dataId),
                new SqliteParameter("@coolDownRemainsDuration", characterSkillUsage.coolDownRemainsDuration));
        }

        public List<CharacterSkillUsage> ReadCharacterSkillUsages(string characterId)
        {
            var result = new List<CharacterSkillUsage>();
            var reader = ExecuteReader("SELECT * FROM characterskillusage WHERE characterId=@characterId ORDER BY coolDownRemainsDuration ASC",
                new SqliteParameter("@characterId", characterId));
            CharacterSkillUsage tempSkillUsage;
            while (ReadCharacterSkillUsage(reader, out tempSkillUsage, false))
            {
                result.Add(tempSkillUsage);
            }
            return result;
        }

        public void DeleteCharacterSkillUsages(string characterId)
        {
            ExecuteNonQuery("DELETE FROM characterskillusage WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
