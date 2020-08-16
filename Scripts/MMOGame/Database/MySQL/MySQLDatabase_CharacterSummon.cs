using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterSummon(MySqlDataReader reader, out CharacterSummon result)
        {
            if (reader.Read())
            {
                result = new CharacterSummon();
                result.type = (SummonType)reader.GetSByte("type");
                result.dataId = reader.GetInt32("dataId");
                result.summonRemainsDuration = reader.GetFloat("summonRemainsDuration");
                result.level = reader.GetInt16("level");
                result.exp = reader.GetInt32("exp");
                result.currentHp = reader.GetInt32("currentHp");
                result.currentMp = reader.GetInt32("currentMp");
                return true;
            }
            result = CharacterSummon.Empty;
            return false;
        }

        public async Task CreateCharacterSummon(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterSummon characterSummon)
        {
            await ExecuteNonQuery(connection, transaction, "INSERT INTO charactersummon (id, characterId, type, dataId, summonRemainsDuration, level, exp, currentHp, currentMp) VALUES (@id, @characterId, @type, @dataId, @summonRemainsDuration, @level, @exp, @currentHp, @currentMp)",
                new MySqlParameter("@id", characterId + "_" + characterSummon.type + "_" + idx),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@type", (byte)characterSummon.type),
                new MySqlParameter("@dataId", characterSummon.dataId),
                new MySqlParameter("@summonRemainsDuration", characterSummon.summonRemainsDuration),
                new MySqlParameter("@level", characterSummon.level),
                new MySqlParameter("@exp", characterSummon.exp),
                new MySqlParameter("@currentHp", characterSummon.currentHp),
                new MySqlParameter("@currentMp", characterSummon.currentMp));
        }

        public async Task<List<CharacterSummon>> ReadCharacterSummons(string characterId, List<CharacterSummon> result = null)
        {
            if (result == null)
                result = new List<CharacterSummon>();
            await ExecuteReader((reader) =>
            {
                CharacterSummon tempSummon;
                while (ReadCharacterSummon(reader, out tempSummon))
                {
                    result.Add(tempSummon);
                }
            }, "SELECT * FROM charactersummon WHERE characterId=@characterId ORDER BY type DESC",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        public async Task DeleteCharacterSummons(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            await ExecuteNonQuery(connection, transaction, "DELETE FROM charactersummon WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
