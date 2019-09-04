using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private bool ReadCharacterSkill(SQLiteRowsReader reader, out CharacterSkill result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterSkill();
                result.dataId = reader.GetInt32("dataId");
                result.level = reader.GetInt16("level");
                return true;
            }
            result = CharacterSkill.Empty;
            return false;
        }

        public void CreateCharacterSkill(int idx, string characterId, CharacterSkill characterSkill)
        {
            ExecuteNonQuery("INSERT INTO characterskill (id, idx, characterId, dataId, level, coolDownRemainsDuration) VALUES (@id, @idx, @characterId, @dataId, @level, 0)",
                new SqliteParameter("@id", characterId + "_" + idx),
                new SqliteParameter("@idx", idx),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@dataId", characterSkill.dataId),
                new SqliteParameter("@level", characterSkill.level));
        }

        public List<CharacterSkill> ReadCharacterSkills(string characterId)
        {
            List<CharacterSkill> result = new List<CharacterSkill>();
            SQLiteRowsReader reader = ExecuteReader("SELECT * FROM characterskill WHERE characterId=@characterId ORDER BY idx ASC",
                new SqliteParameter("@characterId", characterId));
            CharacterSkill tempSkill;
            while (ReadCharacterSkill(reader, out tempSkill, false))
            {
                result.Add(tempSkill);
            }
            return result;
        }

        public void DeleteCharacterSkills(string characterId)
        {
            ExecuteNonQuery("DELETE FROM characterskill WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
