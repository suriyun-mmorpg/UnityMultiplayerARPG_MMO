using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterSummon(MySQLRowsReader reader, out CharacterSummon result, bool resetReader = true)
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

        public void CreateCharacterSummon(MySqlConnection connection, MySqlTransaction transaction, string characterId, CharacterSummon characterSummon)
        {
            ExecuteNonQuery(connection, transaction, "INSERT INTO charactersummon (id, characterId, type, dataId, summonRemainsDuration, level, exp, currentHp, currentMp) VALUES (@id, @characterId, @type, @dataId, @summonRemainsDuration, @level, @exp, @currentHp, @currentMp)",
                new MySqlParameter("@id", characterId + "_" + characterSummon.type + "_" + characterSummon.dataId),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@type", (byte)characterSummon.type),
                new MySqlParameter("@dataId", characterSummon.dataId),
                new MySqlParameter("@summonRemainsDuration", characterSummon.summonRemainsDuration),
                new MySqlParameter("@level", characterSummon.level),
                new MySqlParameter("@exp", characterSummon.exp),
                new MySqlParameter("@currentHp", characterSummon.currentHp),
                new MySqlParameter("@currentMp", characterSummon.currentMp));
        }

        public List<CharacterSummon> ReadCharacterSummons(string characterId)
        {
            var result = new List<CharacterSummon>();
            var reader = ExecuteReader("SELECT * FROM charactersummon WHERE characterId=@characterId ORDER BY type DESC",
                new MySqlParameter("@characterId", characterId));
            CharacterSummon tempSummon;
            while (ReadCharacterSummon(reader, out tempSummon, false))
            {
                result.Add(tempSummon);
            }
            return result;
        }

        public void DeleteCharacterSummons(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuery(connection, transaction, "DELETE FROM charactersummon WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
