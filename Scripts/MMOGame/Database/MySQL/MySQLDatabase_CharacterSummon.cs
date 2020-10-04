#if UNITY_STANDALONE && !CLIENT_BUILD
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        public async UniTask CreateCharacterSummon(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterSummon characterSummon)
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

        public async UniTask<List<CharacterSummon>> ReadCharacterSummons(string characterId, List<CharacterSummon> result = null)
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
            }, "SELECT type, dataId, summonRemainsDuration, level, exp, currentHp, currentMp FROM charactersummon WHERE characterId=@characterId ORDER BY type DESC",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        public async UniTask DeleteCharacterSummons(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            await ExecuteNonQuery(connection, transaction, "DELETE FROM charactersummon WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
#endif