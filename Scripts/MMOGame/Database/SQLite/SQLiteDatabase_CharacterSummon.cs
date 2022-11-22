#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private bool ReadCharacterSummon(SqliteDataReader reader, out CharacterSummon result)
        {
            if (reader.Read())
            {
                result = new CharacterSummon();
                result.type = (SummonType)reader.GetByte(0);
                result.dataId = reader.GetInt32(1);
                result.summonRemainsDuration = reader.GetFloat(2);
                result.level = reader.GetInt16(3);
                result.exp = reader.GetInt32(4);
                result.currentHp = reader.GetInt32(5);
                result.currentMp = reader.GetInt32(6);
                return true;
            }
            result = CharacterSummon.Empty;
            return false;
        }

        public void CreateCharacterSummon(SqliteTransaction transaction, int idx, string characterId, CharacterSummon characterSummon)
        {
            ExecuteNonQuery(transaction, "INSERT INTO charactersummon (id, characterId, type, dataId, summonRemainsDuration, level, exp, currentHp, currentMp) VALUES (@id, @characterId, @type, @dataId, @summonRemainsDuration, @level, @exp, @currentHp, @currentMp)",
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
            ExecuteReader((reader) =>
            {
                CharacterSummon tempSummon;
                while (ReadCharacterSummon(reader, out tempSummon))
                {
                    result.Add(tempSummon);
                }
            }, "SELECT type, dataId, summonRemainsDuration, level, exp, currentHp, currentMp FROM charactersummon WHERE characterId=@characterId ORDER BY type DESC",
                new SqliteParameter("@characterId", characterId));
            return result;
        }

        public void DeleteCharacterSummons(SqliteTransaction transaction, string characterId)
        {
            ExecuteNonQuery(transaction, "DELETE FROM charactersummon WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
#endif