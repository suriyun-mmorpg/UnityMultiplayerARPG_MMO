using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private bool ReadCharacterSummon(SQLiteRowsReader reader, out CharacterSummon result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterSummon();
                result.type = (SummonType)reader.GetSByte("type");
                result.dataId = reader.GetInt32("dataId");
                result.summonRemainsDuration = reader.GetFloat("summonRemainsDuration");
                result.level = (short)reader.GetInt32("level");
                result.exp = reader.GetInt32("exp");
                result.currentHp = reader.GetInt32("currentHp");
                result.currentMp = reader.GetInt32("currentMp");
                return true;
            }
            result = CharacterSummon.Empty;
            return false;
        }

        public void CreateCharacterSummon(int idx, string characterId, CharacterSummon characterSummon)
        {
            ExecuteNonQuery("INSERT INTO charactersummon (id, characterId, type, dataId, summonRemainsDuration, level, exp, currentHp, currentMp) VALUES (@id, @characterId, @type, @dataId, @summonRemainsDuration, @level, @exp, @currentHp, @currentMp)",
                new SqliteParameter("@id", characterId + "_" + characterSummon.type + "_" + idx),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@type", (byte)characterSummon.type),
                new SqliteParameter("@dataId", characterSummon.dataId),
                new SqliteParameter("@summonRemainsDuration", characterSummon.summonRemainsDuration),
                new SqliteParameter("@level", characterSummon.level),
                new SqliteParameter("@exp", characterSummon.exp),
                new SqliteParameter("@currentHp", characterSummon.currentHp),
                new SqliteParameter("@currentMp", characterSummon.currentMp));
        }

        public List<CharacterSummon> ReadCharacterSummons(string characterId)
        {
            List<CharacterSummon> result = new List<CharacterSummon>();
            SQLiteRowsReader reader = ExecuteReader("SELECT * FROM charactersummon WHERE characterId=@characterId ORDER BY type DESC",
                new SqliteParameter("@characterId", characterId));
            CharacterSummon tempSummon;
            while (ReadCharacterSummon(reader, out tempSummon, false))
            {
                result.Add(tempSummon);
            }
            return result;
        }

        public void DeleteCharacterSummons(string characterId)
        {
            ExecuteNonQuery("DELETE FROM charactersummon WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
