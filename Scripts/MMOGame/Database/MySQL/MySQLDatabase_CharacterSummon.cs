#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using System.Collections.Generic;
using Cysharp.Text;
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
                result.level = reader.GetInt32(3);
                result.exp = reader.GetInt32(4);
                result.currentHp = reader.GetInt32(5);
                result.currentMp = reader.GetInt32(6);
                return true;
            }
            result = CharacterSummon.Empty;
            return false;
        }

        public void CreateCharacterSummon(MySqlConnection connection, MySqlTransaction transaction, HashSet<string> insertedIds, int idx, string characterId, CharacterSummon characterSummon)
        {
            string id = ZString.Concat(characterId, "_", idx);
            if (insertedIds.Contains(id))
            {
                LogWarning(LogTag, $"Summon {id}, for character {characterId}, already inserted");
                return;
            }
            insertedIds.Add(id);
            ExecuteNonQuerySync(connection, transaction, "INSERT INTO charactersummon (id, characterId, type, dataId, summonRemainsDuration, level, exp, currentHp, currentMp) VALUES (@id, @characterId, @type, @dataId, @summonRemainsDuration, @level, @exp, @currentHp, @currentMp)",
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

        public List<CharacterSummon> ReadCharacterSummons(string characterId, List<CharacterSummon> result = null)
        {
            if (result == null)
                result = new List<CharacterSummon>();
            ExecuteReaderSync((reader) =>
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

        public void DeleteCharacterSummons(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuerySync(connection, transaction, "DELETE FROM charactersummon WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
#endif