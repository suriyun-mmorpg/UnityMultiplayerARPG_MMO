#if UNITY_STANDALONE && !CLIENT_BUILD
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private bool ReadCharacterAttribute(MySqlDataReader reader, out CharacterAttribute result)
        {
            if (reader.Read())
            {
                result = new CharacterAttribute();
                result.dataId = reader.GetInt32(0);
                result.amount = reader.GetInt16(1);
                return true;
            }
            result = CharacterAttribute.Empty;
            return false;
        }

        public async UniTask CreateCharacterAttribute(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, CharacterAttribute characterAttribute)
        {
            await ExecuteNonQuery(connection, transaction, "INSERT INTO characterattribute (id, idx, characterId, dataId, amount) VALUES (@id, @idx, @characterId, @dataId, @amount)",
                new MySqlParameter("@id", characterId + "_" + idx),
                new MySqlParameter("@idx", idx),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterAttribute.dataId),
                new MySqlParameter("@amount", characterAttribute.amount));
        }

        public async UniTask<List<CharacterAttribute>> ReadCharacterAttributes(string characterId, List<CharacterAttribute> result = null)
        {
            if (result == null)
                result = new List<CharacterAttribute>();
            await ExecuteReader((reader) =>
            {
                CharacterAttribute tempAttribute;
                while (ReadCharacterAttribute(reader, out tempAttribute))
                {
                    result.Add(tempAttribute);
                }
            }, "SELECT dataId, amount FROM characterattribute WHERE characterId=@characterId ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        public async UniTask DeleteCharacterAttributes(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            await ExecuteNonQuery(connection, transaction, "DELETE FROM characterattribute WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
#endif